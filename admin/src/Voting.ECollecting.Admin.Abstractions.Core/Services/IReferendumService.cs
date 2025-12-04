// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Admin.Abstractions.Core.Services;

public interface IReferendumService
{
    Task<Dictionary<DomainOfInfluenceType, List<Decree>>> ListDecreesByDoiType(IReadOnlySet<DomainOfInfluenceType>? doiTypes, string? bfs);

    Task<Referendum> Get(Guid id);

    Task<Guid> Create(Guid decreeId, string description, CollectionAddress collectionAddress);
}
