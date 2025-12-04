// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.Lib.Database.Models;
using IVotingStimmregisterPersonInfo = Voting.ECollecting.Admin.Domain.Models.IVotingStimmregisterPersonInfo;

namespace Voting.ECollecting.Admin.Abstractions.Core.Services;

public interface ICollectionSignatureSheetService
{
    Task<Page<CollectionSignatureSheet>> List(
        Guid collectionId,
        IReadOnlySet<DateTime>? attestedAt,
        IReadOnlyCollection<CollectionSignatureSheetState> states,
        Pageable? pageable,
        CollectionSignatureSheetSort sort = CollectionSignatureSheetSort.Number,
        SortDirection sortDirection = SortDirection.Ascending);

    Task<IReadOnlyCollection<DateTime>> ListAttestedAt(Guid collectionId);

    Task Delete(Guid collectionId, Guid sheetId);

    Task<CollectionSignatureSheetNumberInfo> ReserveNumber(Guid collectionId);

    Task TryReleaseNumber(Guid collectionId, int number);

    Task<CollectionSignatureSheetEntity> Add(Guid collectionId, int number, DateTime receivedAt, int signatureCountTotal);

    Task Update(Guid collectionId, Guid sheetId, DateTime receivedAt, int signatureCountTotal);

    Task<Stream> Attest(Guid collectionId, IReadOnlySet<Guid> signatureSheetIds);

    Task<CollectionSignatureSheet> Get(Guid collectionId, Guid sheetId);

    Task<Page<CollectionSignatureSheetCandidate>> SearchPersonCandidates(
        CollectionType collectionType,
        Guid collectionId,
        Guid sheetId,
        VotingStimmregisterPersonFilterData filter,
        Pageable? pageable = null,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<IVotingStimmregisterPersonInfo>> ListCitizens(Guid collectionId, Guid sheetId);

    Task AddCitizen(CollectionType collectionType, Guid collectionId, Guid sheetId, Guid personRegisterId);

    Task RemoveCitizen(Guid collectionId, Guid sheetId, Guid personRegisterId);

    Task<SignatureSheetStateChangeResult> Submit(Guid collectionId, Guid sheetId);

    Task<SignatureSheetStateChangeResult> Unsubmit(Guid collectionId, Guid sheetId);

    Task<SignatureSheetStateChangeResult> Discard(Guid collectionId, Guid sheetId);

    Task<SignatureSheetStateChangeResult> Restore(Guid collectionId, Guid sheetId);

    Task<SignatureSheetConfirmResult> Confirm(SignatureSheetConfirmRequest request);

    Task<List<CollectionSignatureSheetEntity>> ListSamples(Guid collectionId);

    Task<List<CollectionSignatureSheetEntity>> AddSamples(Guid collectionId, int signatureSheetsCount);
}
