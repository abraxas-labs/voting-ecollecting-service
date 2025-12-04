// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Database.Models;

namespace Voting.ECollecting.Shared.Domain.Entities;

public class UserNotificationEntity : BaseEntity
{
    public string RecipientEMail { get; set; } = string.Empty;

    public UserNotificationState State { get; set; }

    public DateTime? SentTimestamp { get; set; }

    public int FailureCounter { get; set; }

    public string? LastError { get; set; }

    public UserNotificationTemplateBag TemplateBag { get; set; } = new();
}
