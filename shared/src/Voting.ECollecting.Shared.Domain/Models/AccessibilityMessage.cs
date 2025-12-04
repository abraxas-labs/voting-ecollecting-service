// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Shared.Domain.Models;

public class AccessibilityMessage
{
    public AccessibilitySalutation? Salutation { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public AccessibilityCategory? Category { get; set; }

    public string Message { get; set; } = string.Empty;
}
