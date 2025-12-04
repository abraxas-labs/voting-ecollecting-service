// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Models;

namespace Voting.ECollecting.Citizen.Abstractions.Core.Services;

public interface IUserNotificationService
{
    Task SendAccessibilityMessage(AccessibilityMessage message);
}
