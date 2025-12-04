// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Abstractions.Core.Services;
using Voting.ECollecting.Citizen.Core.Configuration;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Models;

namespace Voting.ECollecting.Citizen.Core.Services.UserNotifications;

public class UserNotificationService : IUserNotificationService
{
    private readonly Shared.Abstractions.Core.Services.IUserNotificationService _coreUserNotificationService;
    private readonly CoreAppConfig _config;

    public UserNotificationService(Shared.Abstractions.Core.Services.IUserNotificationService coreUserNotificationService, CoreAppConfig config)
    {
        _coreUserNotificationService = coreUserNotificationService;
        _config = config;
    }

    public async Task SendAccessibilityMessage(AccessibilityMessage message)
    {
        await _coreUserNotificationService.SendUserNotification(
            _config.AccessibilityEmail,
            false,
            UserNotificationType.AccessibilityMessage,
            accessibilityMessage: message);
    }
}
