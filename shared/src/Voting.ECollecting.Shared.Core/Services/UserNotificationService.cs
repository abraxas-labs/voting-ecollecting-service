// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Voting.ECollecting.Shared.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Core.Configuration;
using Voting.ECollecting.Shared.Core.Exceptions;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Models;
using Voting.Lib.Common;
using Voting.Lib.Common.Files;
using Voting.Lib.UserNotifications;

namespace Voting.ECollecting.Shared.Core.Services;

public class UserNotificationService : IUserNotificationService
{
    private readonly IPermissionService _permissionService;
    private readonly IUserNotificationRepository _userNotificationRepository;
    private readonly IAccessControlListDoiRepository _accessControlListDoiRepository;
    private readonly UserNotificationsConfig _config;
    private readonly ILogger<UserNotificationService> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly IUserNotificationSender _userNotificationSender;
    private readonly IServiceProvider _serviceProvider;

    public UserNotificationService(
        IPermissionService permissionService,
        IUserNotificationRepository userNotificationRepository,
        IAccessControlListDoiRepository accessControlListDoiRepository,
        UserNotificationsConfig config,
        ILogger<UserNotificationService> logger,
        TimeProvider timeProvider,
        IUserNotificationSender userNotificationSender,
        IServiceProvider serviceProvider)
    {
        _permissionService = permissionService;
        _userNotificationRepository = userNotificationRepository;
        _accessControlListDoiRepository = accessControlListDoiRepository;
        _config = config;
        _logger = logger;
        _timeProvider = timeProvider;
        _userNotificationSender = userNotificationSender;
        _serviceProvider = serviceProvider;
    }

    public async Task ScheduleNotification(
        CollectionBaseEntity collection,
        UserNotificationType type)
    {
        var recipients = await BuildRecipients(collection);
        var notifications = recipients.Select(r => new UserNotificationEntity
        {
            State = UserNotificationState.Pending,
            RecipientEMail = r.EMail,
            TemplateBag = new UserNotificationTemplateBag
            {
                CollectionId = collection.Id,
                CollectionType = collection.Type,
                CollectionName = collection.Description,
                NotificationType = type,
                RecipientIsCitizen = r.IsCitizen,
            },
        });
        await _userNotificationRepository.CreateRange(notifications);
    }

    public async Task SendUserNotification(
        string email,
        bool recipientIsCitizen,
        UserNotificationType type,
        DecreeEntity? decree = null,
        CollectionBaseEntity? collection = null,
        IFile[]? attachments = null,
        UrlToken? permissionToken = null,
        UrlToken? initiativeCommitteeMembershipToken = null,
        AccessibilityMessage? accessibilityMessage = null,
        CancellationToken cancellationToken = default)
    {
        var userNotification = new UserNotificationEntity
        {
            RecipientEMail = email,
            TemplateBag = new UserNotificationTemplateBag
            {
                DecreeId = decree?.Id,
                DecreeName = decree?.Description,
                CollectionId = collection?.Id,
                CollectionName = collection?.Description ?? string.Empty,
                CollectionType = collection?.Type,
                NotificationType = type,
                RecipientIsCitizen = recipientIsCitizen,
                PermissionToken = permissionToken,
                InitiativeCommitteeMembershipToken = initiativeCommitteeMembershipToken,
                AccessibilityMessage = accessibilityMessage,
            },
        };

        try
        {
            var renderer = _serviceProvider.GetRequiredKeyedService<IUserNotificationRenderer>(userNotification.TemplateBag.NotificationType);
            var message = renderer.Render(userNotification);
            if (attachments != null)
            {
                message = message with { Attachments = attachments };
            }

            await _userNotificationSender.Send(message, cancellationToken);

            userNotification.State = UserNotificationState.Sent;
            userNotification.SentTimestamp = _timeProvider.GetUtcNowDateTime();
            await _userNotificationRepository.Create(userNotification);
            _logger.LogInformation("User notification {id} sent.", userNotification.Id);
        }
        catch (Exception ex)
        {
            userNotification.FailureCounter = 1;
            userNotification.LastError = $"{ex.GetType().Name}: {ex.Message}{Environment.NewLine}{Environment.NewLine}{ex.StackTrace}";
            userNotification.State = UserNotificationState.Failed;
            userNotification.SentTimestamp = _timeProvider.GetUtcNowDateTime();

            await _userNotificationRepository.Create(userNotification);
            _logger.LogError(ex, "Sending user notification {id} failed.", userNotification.Id);
            throw new SendUserNotificationFailedException(userNotification.Id, ex);
        }
    }

    private async Task<HashSet<(string EMail, bool IsCitizen)>> BuildRecipients(CollectionBaseEntity collection)
    {
        var recipients = new HashSet<(string EMail, bool IsCitizen)>();

        foreach (var recipientAddress in _config.AdditionalRecipientMailAddresses)
        {
            recipients.Add((recipientAddress, false));
        }

        if (!string.IsNullOrWhiteSpace(collection.Bfs))
        {
            var doiEmail = await _accessControlListDoiRepository.Query()
                .Where(x => x.Bfs == collection.Bfs)
                .Select(x => x.ECollectingEmail)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(doiEmail))
            {
                recipients.Add((doiEmail, false));
            }
        }

        if (collection.AuditInfo.CreatedByEmail != null)
        {
            recipients.Add((collection.AuditInfo.CreatedByEmail, true));
        }

        Debug.Assert(collection.Permissions != null, "Collection permissions need to be loaded to send notifications");
        var deputies = collection.Permissions!.Where(x => x is { Accepted: true, Role: CollectionPermissionRole.Deputy });
        foreach (var deputy in deputies)
        {
            recipients.Add((deputy.Email, true));
        }

        if (_permissionService.UserEmail != null)
        {
            recipients.Remove((_permissionService.UserEmail, true));
            recipients.Remove((_permissionService.UserEmail, false));
        }

        return recipients;
    }
}
