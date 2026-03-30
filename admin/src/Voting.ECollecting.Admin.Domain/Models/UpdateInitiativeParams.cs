// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.Lib.Database.Models;

namespace Voting.ECollecting.Admin.Domain.Models;

public record UpdateInitiativeParams(
    Guid? SubTypeId,
    string Description,
    string Reason,
    MarkdownString Wording,
    CollectionAddress? Address);
