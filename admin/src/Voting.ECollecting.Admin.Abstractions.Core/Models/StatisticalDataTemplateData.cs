// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Admin.Abstractions.Core.Models;

public record StatisticalDataTemplateData(
    HashSet<Guid> CollectionIds,
    string Description,
    Guid? DecreeId = null);
