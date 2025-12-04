// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Ecollecting.Shared.V1.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Api.Grpc.Mappings;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Proto.Admin.Services.V1.Responses;
using Voting.Lib.Common;
using Voting.Lib.Grpc;
using CollectionSignatureSheet = Voting.ECollecting.Proto.Admin.Services.V1.Models.CollectionSignatureSheet;

namespace Voting.ECollecting.Admin.Api.Grpc.Services;

public class CollectionSignatureSheetGrpcService : CollectionSignatureSheetService.CollectionSignatureSheetServiceBase
{
    private readonly ICollectionSignatureSheetService _collectionSignatureSheetService;

    public CollectionSignatureSheetGrpcService(ICollectionSignatureSheetService collectionSignatureSheetService)
    {
        _collectionSignatureSheetService = collectionSignatureSheetService;
    }

    [Kontrollzeichenerfasser]
    public override async Task<ListSignatureSheetsResponse> List(
        ListSignatureSheetsRequest request,
        ServerCallContext context)
    {
        var sheets = await _collectionSignatureSheetService.List(
            GuidParser.Parse(request.CollectionId),
            Mapper.MapTimestampsToSet(request.AttestedAts),
            Mapper.MapToCollectionSignatureSheetStates(request.States),
            Mapper.MapToPageable(request.Pageable),
            Mapper.MapToCollectionSignatureSheetSort(request.Sort),
            Mapper.MapToSortDirection(request.SortDirection));
        return Mapper.MapToListCollectionSignatureSheetsResponse(sheets);
    }

    [Kontrollzeichenerfasser]
    public override async Task<ListSignatureSheetsAttestedAtResponse> ListAttestedAt(
        ListSignatureSheetsAttestedAtRequest request,
        ServerCallContext context)
    {
        var attestedAts = await _collectionSignatureSheetService.ListAttestedAt(GuidParser.Parse(request.CollectionId));
        return Mapper.MapToListSignatureSheetsAttestedAtResponse(attestedAts);
    }

    [Kontrollzeichenerfasser]
    public override async Task<Empty> Delete(DeleteSignatureSheetRequest request, ServerCallContext context)
    {
        await _collectionSignatureSheetService.Delete(
            GuidParser.Parse(request.CollectionId),
            GuidParser.Parse(request.SignatureSheetId));
        return ProtobufEmpty.Instance;
    }

    [Kontrollzeichenerfasser]
    public override async Task<ReserveSignatureSheetNumberResponse> ReserveNumber(
        ReserveSignatureSheetNumberRequest request,
        ServerCallContext context)
    {
        var info = await _collectionSignatureSheetService.ReserveNumber(GuidParser.Parse(request.CollectionId));
        return Mapper.MapToReserveSignatureSheetNumberResponse(info);
    }

    [KontrollzeichenerfasserOrStichprobenverwalter]
    public override async Task<CollectionSignatureSheet> Get(
        GetSignatureSheetRequest request,
        ServerCallContext context)
    {
        var info = await _collectionSignatureSheetService.Get(
            GuidParser.Parse(request.CollectionId),
            GuidParser.Parse(request.SignatureSheetId));
        return Mapper.MapToCollectionSignatureSheet(info);
    }

    [Kontrollzeichenerfasser]
    public override async Task<Empty> TryReleaseNumber(
        TryReleaseSignatureSheetNumberRequest request,
        ServerCallContext context)
    {
        await _collectionSignatureSheetService.TryReleaseNumber(GuidParser.Parse(request.CollectionId), request.Number);
        return ProtobufEmpty.Instance;
    }

    [Kontrollzeichenerfasser]
    public override async Task<IdValue> Add(AddSignatureSheetRequest request, ServerCallContext context)
    {
        var id = await _collectionSignatureSheetService.Add(
            GuidParser.Parse(request.CollectionId),
            request.Number,
            request.ReceivedAt.ToDateTime(),
            request.SignatureCountTotal);
        return Mapper.MapToIdValue(id);
    }

    [Kontrollzeichenerfasser]
    public override async Task<Empty> Update(
        UpdateSignatureSheetRequest request,
        ServerCallContext context)
    {
        await _collectionSignatureSheetService.Update(
            GuidParser.Parse(request.CollectionId),
            GuidParser.Parse(request.SignatureSheetId),
            request.ReceivedAt.ToDateTime(),
            request.SignatureCountTotal);
        return ProtobufEmpty.Instance;
    }

    [KontrollzeichenerfasserOrStichprobenverwalter]
    public override async Task<SearchSignatureSheetPersonCandidatesResponse> SearchPersonCandidates(
        SearchSignatureSheetPersonCandidatesRequest request,
        ServerCallContext context)
    {
        var filter = Mapper.MapToPersonFilterData(request);
        var candidates = await _collectionSignatureSheetService.SearchPersonCandidates(
            Mapper.MapCollectionType(request.CollectionType),
            GuidParser.Parse(request.CollectionId),
            GuidParser.Parse(request.SignatureSheetId),
            filter,
            Mapper.MapToPageable(request.Pageable),
            context.CancellationToken);
        return Mapper.MapToSearchSignatureSheetPersonsResponse(candidates);
    }

    [Kontrollzeichenerfasser]
    public override async Task<Empty> AddCitizen(
        AddSignatureSheetCitizenRequest request,
        ServerCallContext context)
    {
        await _collectionSignatureSheetService.AddCitizen(
            Mapper.MapCollectionType(request.CollectionType),
            GuidParser.Parse(request.CollectionId),
            GuidParser.Parse(request.SignatureSheetId),
            GuidParser.Parse(request.PersonRegisterId));
        return ProtobufEmpty.Instance;
    }

    [Kontrollzeichenerfasser]
    public override async Task<Empty> RemoveCitizen(
        RemoveSignatureSheetCitizenRequest request,
        ServerCallContext context)
    {
        await _collectionSignatureSheetService.RemoveCitizen(
            GuidParser.Parse(request.CollectionId),
            GuidParser.Parse(request.SignatureSheetId),
            GuidParser.Parse(request.PersonRegisterId));
        return ProtobufEmpty.Instance;
    }

    [KontrollzeichenerfasserOrStichprobenverwalter]
    public override async Task<ListSignatureSheetCitizensResponse> ListCitizens(
        ListSignatureSheetCitizensRequest request,
        ServerCallContext context)
    {
        var citizens = await _collectionSignatureSheetService.ListCitizens(
            GuidParser.Parse(request.CollectionId),
            GuidParser.Parse(request.SignatureSheetId));
        return Mapper.MapToListSignatureSheetCitizensResponse(citizens);
    }

    [Stichprobenverwalter]
    public override async Task<SubmitSignatureSheetResponse> Submit(SubmitSignatureSheetRequest request, ServerCallContext context)
    {
        var result = await _collectionSignatureSheetService.Submit(
            GuidParser.Parse(request.CollectionId),
            GuidParser.Parse(request.SignatureSheetId));
        return Mapper.MapToSubmitSignatureSheetResponse(result);
    }

    [Stichprobenverwalter]
    public override async Task<UnsubmitSignatureSheetResponse> Unsubmit(UnsubmitSignatureSheetRequest request, ServerCallContext context)
    {
        var result = await _collectionSignatureSheetService.Unsubmit(
            GuidParser.Parse(request.CollectionId),
            GuidParser.Parse(request.SignatureSheetId));
        return Mapper.MapToUnsubmitSignatureSheetResponse(result);
    }

    [Stichprobenverwalter]
    public override async Task<DiscardSignatureSheetResponse> Discard(DiscardSignatureSheetRequest request, ServerCallContext context)
    {
        var result = await _collectionSignatureSheetService.Discard(
            GuidParser.Parse(request.CollectionId),
            GuidParser.Parse(request.SignatureSheetId));
        return Mapper.MapToDiscardSignatureSheetResponse(result);
    }

    [Stichprobenverwalter]
    public override async Task<RestoreSignatureSheetResponse> Restore(RestoreSignatureSheetRequest request, ServerCallContext context)
    {
        var result = await _collectionSignatureSheetService.Restore(
            GuidParser.Parse(request.CollectionId),
            GuidParser.Parse(request.SignatureSheetId));
        return Mapper.MapToRestoreSignatureSheetResponse(result);
    }

    [Stichprobenverwalter]
    public override async Task<ConfirmSignatureSheetResponse> Confirm(ConfirmSignatureSheetRequest request, ServerCallContext context)
    {
        var result = await _collectionSignatureSheetService.Confirm(new SignatureSheetConfirmRequest(
            GuidParser.Parse(request.CollectionId),
            GuidParser.Parse(request.SignatureSheetId),
            Mapper.MapCollectionType(request.CollectionType),
            request.AddedPersonRegisterIds.Select(GuidParser.Parse).ToHashSet(),
            request.RemovedPersonRegisterIds.Select(GuidParser.Parse).ToHashSet(),
            request.SignatureCountTotal));
        return Mapper.MapToConfirmSignatureSheetResponse(result);
    }

    [Stichprobenverwalter]
    public override async Task<ListSignatureSheetSamplesResponse> ListSamples(ListSignatureSheetSamplesRequest request, ServerCallContext context)
    {
        var sheets = await _collectionSignatureSheetService.ListSamples(GuidParser.Parse(request.CollectionId));
        return Mapper.MapToListSignatureSheetSamplesResponse(sheets);
    }

    [Stichprobenverwalter]
    public override async Task<AddSignatureSheetSamplesResponse> AddSamples(AddSignatureSheetSamplesRequest request, ServerCallContext context)
    {
        var sheets = await _collectionSignatureSheetService.AddSamples(GuidParser.Parse(request.CollectionId), request.SignatureSheetsCount);
        return Mapper.MapToAddSignatureSheetSamplesResponse(sheets);
    }
}
