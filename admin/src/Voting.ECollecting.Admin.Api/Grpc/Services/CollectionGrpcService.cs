// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Ecollecting.Shared.V1.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Api.Grpc.Mappings;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Proto.Admin.Services.V1.Responses;
using Voting.Lib.Common;
using Voting.Lib.Grpc;
using CollectionService = Voting.ECollecting.Proto.Admin.Services.V1.CollectionService;

namespace Voting.ECollecting.Admin.Api.Grpc.Services;

public class CollectionGrpcService : CollectionService.CollectionServiceBase
{
    private readonly ICollectionService _collectionService;
    private readonly ICollectionFilesService _collectionFilesService;

    public CollectionGrpcService(
        ICollectionService collectionService,
        ICollectionFilesService collectionFilesService)
    {
        _collectionService = collectionService;
        _collectionFilesService = collectionFilesService;
    }

    [Stammdatenverwalter]
    public override async Task<ListCollectionMessagesResponse> ListMessages(ListCollectionMessagesRequest request, ServerCallContext context)
    {
        var (messages, informalReviewRequested) = await _collectionService.ListMessages(GuidParser.Parse(request.CollectionId));
        return Mapper.MapToCollectionMessagesResponse(messages, informalReviewRequested);
    }

    [Stammdatenverwalter]
    public override async Task<IdValue> AddMessage(AddCollectionMessageRequest request, ServerCallContext context)
    {
        var createdMessage = await _collectionService.AddMessage(GuidParser.Parse(request.CollectionId), request.Content);
        return Mapper.MapToIdValue(createdMessage);
    }

    [Stammdatenverwalter]
    public override async Task<CollectionMessage> FinishInformalReview(FinishInformalReviewRequest request, ServerCallContext context)
    {
        var message = await _collectionService.FinishInformalReview(GuidParser.Parse(request.CollectionId));
        return Mapper.MapToCollectionMessage(message);
    }

    [Authorize(Roles = Roles.ApiNotify)]
    public override async Task<Empty> NotifyPreparingForCollection(Empty request, ServerCallContext context)
    {
        await _collectionService.NotifyPreparingForCollection();
        return ProtobufEmpty.Instance;
    }

    [Kontrollzeichenloescher]
    public override async Task<ListCollectionsForDeletionResponse> ListForDeletion(
        ListCollectionsForDeletionRequest request,
        ServerCallContext context)
    {
        var doiTypes = Mapper.MapToDomainOfInfluenceTypes(request.Types_).ToHashSet();
        var filter = Mapper.MapToCollectionControlSignFilter(request.Filter);
        var groups = await _collectionService.ListForDeletionByDoiType(doiTypes, request.Bfs, filter);
        return Mapper.MapToListCollectionsForDeletionResponse(groups);
    }

    [Stammdatenverwalter]
    public override async Task<Empty> DeleteWithdrawn(DeleteWithdrawnCollectionRequest request, ServerCallContext context)
    {
        await _collectionService.DeleteWithdrawn(GuidParser.Parse(request.CollectionId));
        return ProtobufEmpty.Instance;
    }

    [Stammdatenverwalter]
    public override async Task<DeleteCollectionImageResponse> DeleteImage(DeleteCollectionImageRequest request, ServerCallContext context)
    {
        var generatedSignatureSheetTemplate = await _collectionFilesService.DeleteImage(GuidParser.Parse(request.CollectionId));
        return Mapper.MapToDeleteCollectionImageResponse(generatedSignatureSheetTemplate);
    }

    [Stammdatenverwalter]
    public override async Task<DeleteCollectionLogoResponse> DeleteLogo(DeleteCollectionLogoRequest request, ServerCallContext context)
    {
        var generatedSignatureSheetTemplate = await _collectionFilesService.DeleteLogo(GuidParser.Parse(request.CollectionId));
        return Mapper.MapToDeleteCollectionLogoResponse(generatedSignatureSheetTemplate);
    }

    [Stammdatenverwalter]
    public override async Task<DeleteSignatureSheetTemplateResponse> DeleteSignatureSheetTemplate(DeleteSignatureSheetTemplateRequest request, ServerCallContext context)
    {
        var generatedSignatureSheetTemplate = await _collectionFilesService.DeleteSignatureSheetTemplate(GuidParser.Parse(request.CollectionId));
        return Mapper.MapToDeleteSignatureSheetTemplateResponse(generatedSignatureSheetTemplate);
    }

    [Stammdatenverwalter]
    public override async Task<ListCollectionPermissionsResponse> ListPermissions(ListCollectionPermissionsRequest request, ServerCallContext context)
    {
        var permissions = await _collectionService.ListPermissions(GuidParser.Parse(request.CollectionId));
        return Mapper.MapToListCollectionPermissionsResponse(permissions);
    }

    [Stammdatenverwalter]
    public override async Task<SubmitSignatureSheetsResponse> SubmitSignatureSheets(
        SubmitSignatureSheetsRequest request,
        ServerCallContext context)
    {
        var userPermissions = await _collectionService.SubmitSignatureSheets(GuidParser.Parse(request.CollectionId));
        return Mapper.MapToSubmitSignatureSheetsResponse(userPermissions);
    }

    [Kontrollzeichenerfasser]
    public override async Task<Empty> SetCommitteeAddress(SetCommitteeAddressRequest request, ServerCallContext context)
    {
        var collectionId = GuidParser.Parse(request.CollectionId);
        var address = Mapper.MapToCollectionAddress(request.Address);
        await _collectionService.SetCommitteeAddress(collectionId, address);
        return ProtobufEmpty.Instance;
    }
}
