// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Ecollecting.Shared.V1.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Voting.ECollecting.Citizen.Abstractions.Core.Services;
using Voting.ECollecting.Citizen.Api.Grpc.Mappings;
using Voting.ECollecting.Citizen.Domain.Authorization;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Models;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Proto.Citizen.Services.V1.Responses;
using Voting.Lib.Common;
using Voting.Lib.Grpc;

namespace Voting.ECollecting.Citizen.Api.Grpc.Services;

public class CollectionGrpcService : CollectionService.CollectionServiceBase
{
    private readonly ICollectionService _collectionService;
    private readonly ICollectionFilesService _collectionFilesService;
    private readonly ICollectionPermissionService _collectionPermissionService;

    public CollectionGrpcService(
        ICollectionService collectionService,
        ICollectionFilesService collectionFilesService,
        ICollectionPermissionService collectionPermissionService)
    {
        _collectionService = collectionService;
        _collectionFilesService = collectionFilesService;
        _collectionPermissionService = collectionPermissionService;
    }

    [AllowAnonymous]
    public override async Task<ListCollectionsResponse> List(ListCollectionsRequest request, ServerCallContext context)
    {
        var doiTypes = Mapper.MapToDomainOfInfluenceTypes(request.Types_).ToHashSet();
        var state = Mapper.MapCollectionPeriodState(request.PeriodState);
        var collections = await _collectionService.ListByDoiType(state, doiTypes, request.Bfs);
        return Mapper.MapToListCollectionResponse(collections);
    }

    public override async Task<ListCollectionPermissionsResponse> ListPermissions(
        ListCollectionPermissionsRequest request,
        ServerCallContext context)
    {
        var permissions = await _collectionPermissionService.ListPermissions(Guid.Parse(request.CollectionId));
        return Mapper.MapToListCollectionPermissionsResponse(permissions);
    }

    [AcceptPermissionPolicy]
    public override async Task<Empty> AcceptPermissionByToken(AcceptCollectionPermissionRequest request, ServerCallContext context)
    {
        await _collectionPermissionService.AcceptByToken(request.Token);
        return ProtobufEmpty.Instance;
    }

    [AllowAnonymous]
    public override async Task<Empty> RejectPermissionByToken(RejectCollectionPermissionRequest request, ServerCallContext context)
    {
        await _collectionPermissionService.RejectByToken(request.Token);
        return ProtobufEmpty.Instance;
    }

    [AllowAnonymous]
    public override async Task<GetPendingCollectionPermissionResponse> GetPendingPermissionByToken(
        GetPendingCollectionPermissionByTokenRequest request,
        ServerCallContext context)
    {
        var permission = await _collectionPermissionService.GetPendingByTokenInclCollection(request.Token);
        return Mapper.MapToGetCollectionPermissionResponse(permission);
    }

    public override async Task<IdValue> CreatePermission(CreateCollectionPermissionRequest request, ServerCallContext context)
    {
        var id = await _collectionPermissionService.CreatePermission(
            Guid.Parse(request.CollectionId),
            request.FirstName,
            request.LastName,
            request.Email,
            Mapper.MapCollectionPermissionRole(request.Role),
            context.CancellationToken);
        return new IdValue { Id = id.ToString() };
    }

    public override async Task<Empty> DeletePermission(DeleteCollectionPermissionRequest request, ServerCallContext context)
    {
        await _collectionPermissionService.DeletePermission(Guid.Parse(request.Id));
        return ProtobufEmpty.Instance;
    }

    public override async Task<Empty> ResendPermission(ResendCollectionPermissionRequest request, ServerCallContext context)
    {
        await _collectionPermissionService.ResendPermission(Guid.Parse(request.Id), context.CancellationToken);
        return ProtobufEmpty.Instance;
    }

    public override async Task<ListCollectionMessagesResponse> ListMessages(ListCollectionMessagesRequest request, ServerCallContext context)
    {
        var collectionId = GuidParser.Parse(request.CollectionId);
        var (messages, informalReviewRequested) = await _collectionService.ListMessages(collectionId);
        return Mapper.MapToCollectionMessagesResponse(messages, informalReviewRequested);
    }

    public override async Task<IdValue> AddMessage(AddCollectionMessageRequest request, ServerCallContext context)
    {
        var collectionId = GuidParser.Parse(request.CollectionId);
        var createdMessage = await _collectionService.AddMessage(collectionId, request.Content);
        return Mapper.MapToIdValue(createdMessage);
    }

    public override async Task<Empty> DeleteImage(DeleteCollectionImageRequest request, ServerCallContext context)
    {
        await _collectionFilesService.DeleteImage(Guid.Parse(request.CollectionId));
        return ProtobufEmpty.Instance;
    }

    public override async Task<Empty> DeleteLogo(DeleteCollectionLogoRequest request, ServerCallContext context)
    {
        await _collectionFilesService.DeleteLogo(Guid.Parse(request.CollectionId));
        return ProtobufEmpty.Instance;
    }

    public override async Task<Empty> SetSignatureSheetTemplateGenerated(
        SetSignatureSheetTemplateGeneratedRequest request,
        ServerCallContext context)
    {
        var collectionType = Mapper.MapCollectionType(request.CollectionType);
        await _collectionFilesService.SetSignatureSheetTemplateGenerated(GuidParser.Parse(request.Id), collectionType);
        return ProtobufEmpty.Instance;
    }

    public override async Task<Empty> GenerateSignatureSheetTemplatePreview(
        GenerateSignatureSheetTemplatePreviewRequest request,
        ServerCallContext context)
    {
        var collectionType = Mapper.MapCollectionType(request.CollectionType);
        await _collectionFilesService.GenerateSignatureSheetTemplatePreview(GuidParser.Parse(request.Id), collectionType);
        return ProtobufEmpty.Instance;
    }

    public override async Task<Empty> DeleteSignatureSheetTemplate(
        DeleteSignatureSheetTemplateRequest request,
        ServerCallContext context)
    {
        await _collectionFilesService.DeleteSignatureSheetTemplate(GuidParser.Parse(request.Id));
        return ProtobufEmpty.Instance;
    }

    public override async Task<CollectionMessage> UpdateRequestInformalReview(
        UpdateRequestInformalReviewRequest request,
        ServerCallContext context)
    {
        var message = await _collectionService.UpdateRequestInformalReview(
            GuidParser.Parse(request.Id),
            request.RequestInformalReview);
        return Mapper.MapToCollectionMessage(message);
    }

    public override async Task<Empty> Withdraw(WithdrawCollectionRequest request, ServerCallContext context)
    {
        await _collectionService.Withdraw(GuidParser.Parse(request.Id));
        return ProtobufEmpty.Instance;
    }

    public override async Task<ValidationSummary> Validate(ValidateCollectionRequest request, ServerCallContext context)
    {
        var validationSummary = await _collectionService.Validate(GuidParser.Parse(request.Id));
        return Mapper.MapToValidationSummary(validationSummary);
    }
}
