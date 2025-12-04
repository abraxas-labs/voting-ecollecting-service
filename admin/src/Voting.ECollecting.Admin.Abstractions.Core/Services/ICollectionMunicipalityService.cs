// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.Abstractions.Core.Services;

public interface ICollectionMunicipalityService
{
    Task<List<CollectionMunicipalityEntity>> List(Guid collectionId);

    Task SetLocked(Guid collectionId, string bfs, bool locked);

    Task<SubmitMunicipalitySignatureSheetsResult> SubmitSignatureSheets(Guid collectionId, string bfs);

    Task<List<CollectionSignatureSheet>> ListSignatureSheets(Guid collectionId, string bfs);
}
