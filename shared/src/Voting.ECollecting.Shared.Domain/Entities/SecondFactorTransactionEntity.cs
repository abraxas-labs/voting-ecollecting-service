// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Database.Models;

namespace Voting.ECollecting.Shared.Domain.Entities;

public class SecondFactorTransactionEntity : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public int PollCount { get; set; }

    public DateTime LastUpdatedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpireAt { get; set; }

    public string ActionIdHash { get; set; } = string.Empty;

    public List<string> ExternalTokenJwtIds { get; set; } = [];
}
