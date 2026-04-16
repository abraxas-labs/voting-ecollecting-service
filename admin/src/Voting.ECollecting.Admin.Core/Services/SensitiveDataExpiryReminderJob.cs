// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Models;
using Voting.Lib.Scheduler;

namespace Voting.ECollecting.Admin.Core.Services;

public class SensitiveDataExpiryReminderJob : IScheduledJob
{
    private readonly IDecreeRepository _decreeRepository;
    private readonly IInitiativeRepository _initiativeRepository;
    private readonly IDomainOfInfluenceRepository _domainOfInfluenceRepository;
    private readonly IUserNotificationService _userNotificationService;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<SensitiveDataExpiryReminderJob> _logger;

    public SensitiveDataExpiryReminderJob(
        IDecreeRepository decreeRepository,
        IInitiativeRepository initiativeRepository,
        IDomainOfInfluenceRepository domainOfInfluenceRepository,
        IUserNotificationService userNotificationService,
        TimeProvider timeProvider,
        ILogger<SensitiveDataExpiryReminderJob> logger)
    {
        _decreeRepository = decreeRepository;
        _initiativeRepository = initiativeRepository;
        _domainOfInfluenceRepository = domainOfInfluenceRepository;
        _userNotificationService = userNotificationService;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task Run(CancellationToken ct)
    {
        var today = _timeProvider.GetUtcTodayDateOnly();
        var decrees = await _decreeRepository.Query()
            .Where(x => x.SensitiveDataExpiryDate == today)
            .ToListAsync(ct);

        foreach (var decree in decrees)
        {
            _logger.LogInformation("Sensitive data expiry date reached for decree {DecreeId}.", decree.Id);

            var recipients = await _domainOfInfluenceRepository.Query()
                                 .Where(x => x.Bfs == decree.Bfs && x.Type == decree.DomainOfInfluenceType)
                                 .Select(x => x.NotificationEmails)
                                 .SingleOrDefaultAsync(ct)
                             ?? [];

            await _userNotificationService.SendUserNotifications(
                recipients,
                recipientsAreCitizen: false,
                UserNotificationType.SensitiveDataExpiryReminder,
                new UserNotificationContext(Decree: decree),
                cancellationToken: ct);
        }

        var initiatives = await _initiativeRepository.Query()
            .Where(x => x.SensitiveDataExpiryDate == today)
            .ToListAsync(ct);

        foreach (var initiative in initiatives)
        {
            _logger.LogInformation("Sensitive data expiry date reached for initiative {InitiativeId}.", initiative.Id);

            var recipients = await _domainOfInfluenceRepository.Query()
                                 .Where(x => x.Bfs == initiative.Bfs && x.Type == initiative.DomainOfInfluenceType!.Value)
                                 .Select(x => x.NotificationEmails)
                                 .SingleOrDefaultAsync(ct)
                             ?? [];

            await _userNotificationService.SendUserNotifications(
                recipients,
                recipientsAreCitizen: false,
                UserNotificationType.SensitiveDataExpiryReminder,
                new UserNotificationContext(Collection: initiative),
                cancellationToken: ct);
        }
    }
}
