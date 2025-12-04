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
using Voting.Lib.Grpc;
using DomainOfInfluence = Voting.ECollecting.Proto.Admin.Services.V1.Models.DomainOfInfluence;

namespace Voting.ECollecting.Admin.Api.Grpc.Services;

[HumanUser]
public class DomainOfInfluenceGrpcService : DomainOfInfluenceService.DomainOfInfluenceServiceBase
{
    private readonly IDomainOfInfluenceService _domainOfInfluenceService;
    private readonly IDomainOfInfluenceFilesService _domainOfInfluenceFilesService;

    public DomainOfInfluenceGrpcService(IDomainOfInfluenceService domainOfInfluenceService, IDomainOfInfluenceFilesService domainOfInfluenceFilesService)
    {
        _domainOfInfluenceService = domainOfInfluenceService;
        _domainOfInfluenceFilesService = domainOfInfluenceFilesService;
    }

    public override async Task<ListDomainOfInfluencesResponse> List(
        ListDomainOfInfluencesRequest request,
        ServerCallContext context)
    {
        var doiTypes = Mapper.MapToDomainOfInfluenceTypes(request.Types_).ToHashSet();
        var domainOfInfluences = await _domainOfInfluenceService.List(
            request.ECollectingEnabled,
            doiTypes.Count == 0 ? null : doiTypes,
            request.IncludeChildren);
        return Mapper.MapToListDomainOfInfluencesResponse(domainOfInfluences);
    }

    public override async Task<ListDomainOfInfluenceOwnTypesResponse> ListOwnTypes(
        ListDomainOfInfluenceOwnTypesRequest request,
        ServerCallContext context)
    {
        var types = await _domainOfInfluenceService.ListOwnTypes();
        return Mapper.MapToListDomainOfInfluenceTypesResponse(types);
    }

    public override async Task<DomainOfInfluence> Get(GetDomainOfInfluenceRequest request, ServerCallContext context)
    {
        var domainOfInfluence = await _domainOfInfluenceService.Get(request.Bfs);
        return Mapper.MapToDomainOfInfluence(domainOfInfluence);
    }

    [Stammdatenverwalter]
    public override async Task<Empty> Update(UpdateDomainOfInfluenceRequest request, ServerCallContext context)
    {
        var updateReq = Mapper.MapToDomainOfInfluenceUpdate(request);
        await _domainOfInfluenceService.Update(request.Bfs, updateReq);
        return ProtobufEmpty.Instance;
    }

    [Stammdatenverwalter]
    public override async Task<Empty> RemoveLogo(RemoveDomainOfInfluenceLogoRequest request, ServerCallContext context)
    {
        await _domainOfInfluenceFilesService.DeleteLogo(request.Bfs);
        return ProtobufEmpty.Instance;
    }
}
