// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Api.Grpc.Mappings;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Proto.Admin.Services.V1.Responses;
using Voting.Lib.Common;
using Voting.Lib.Grpc;

namespace Voting.ECollecting.Admin.Api.Grpc.Services;

[Stichprobenverwalter]
public class CollectionMunicipalityGrpcService : CollectionMunicipalityService.CollectionMunicipalityServiceBase
{
    private readonly ICollectionMunicipalityService _collectionMunicipalityService;

    public CollectionMunicipalityGrpcService(ICollectionMunicipalityService collectionMunicipalityService)
    {
        _collectionMunicipalityService = collectionMunicipalityService;
    }

    public override async Task<ListCollectionMunicipalitiesResponse> List(
        ListCollectionMunicipalitiesRequest request,
        ServerCallContext context)
    {
        var collectionId = GuidParser.Parse(request.CollectionId);
        var collectionMunicipalities = await _collectionMunicipalityService.List(collectionId);
        return Mapper.MapToListCollectionMunicipalitiesResponse(collectionMunicipalities);
    }

    public override async Task<Empty> Unlock(
        UnlockCollectionMunicipalityRequest request,
        ServerCallContext context)
    {
        await _collectionMunicipalityService.SetLocked(GuidParser.Parse(request.CollectionId), request.Bfs, false);
        return ProtobufEmpty.Instance;
    }

    public override async Task<Empty> Lock(
        LockCollectionMunicipalityRequest request,
        ServerCallContext context)
    {
        await _collectionMunicipalityService.SetLocked(GuidParser.Parse(request.CollectionId), request.Bfs, true);
        return ProtobufEmpty.Instance;
    }

    public override async Task<SubmitCollectionMunicipalitySignatureSheetsResponse> SubmitSignatureSheets(
        SubmitCollectionMunicipalitySignatureSheetsRequest request,
        ServerCallContext context)
    {
        var result = await _collectionMunicipalityService.SubmitSignatureSheets(GuidParser.Parse(request.CollectionId), request.Bfs);
        return Mapper.MapToSubmitCollectionMunicipalitySignatureSheetsResponse(result);
    }

    public override async Task<ListCollectionMunicipalitySignatureSheetsResponse> ListSignatureSheets(
        ListCollectionMunicipalitySignatureSheetsRequest request, ServerCallContext context)
    {
        var sheets = await _collectionMunicipalityService.ListSignatureSheets(GuidParser.Parse(request.CollectionId), request.Bfs);
        return Mapper.MapToListCollectionMunicipalitySignatureSheetsResponse(sheets);
    }
}
