// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Voting.ECollecting.Citizen.Abstractions.Core.Services;
using Voting.ECollecting.Citizen.Api.Grpc.Mappings;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Proto.Citizen.Services.V1.Responses;

namespace Voting.ECollecting.Citizen.Api.Grpc.Services;

public class DomainOfInfluenceGrpcService : DomainOfInfluenceService.DomainOfInfluenceServiceBase
{
    private readonly IDomainOfInfluenceService _domainOfInfluenceService;

    public DomainOfInfluenceGrpcService(IDomainOfInfluenceService domainOfInfluenceService)
    {
        _domainOfInfluenceService = domainOfInfluenceService;
    }

    [AllowAnonymous]
    public override async Task<ListDomainOfInfluencesResponse> List(
        ListDomainOfInfluencesRequest request,
        ServerCallContext context)
    {
        var doiTypes = Mapper.MapToDomainOfInfluenceTypes(request.Types_).ToHashSet();
        var domainOfInfluences = await _domainOfInfluenceService.List(
            request.ECollectingEnabled,
            doiTypes.Count == 0 ? null : doiTypes);
        return Mapper.MapToListDomainOfInfluencesResponse(domainOfInfluences);
    }

    [AllowAnonymous]
    public override Task<ListDomainOfInfluenceTypesResponse> ListTypes(
        ListDomainOfInfluenceTypesRequest request,
        ServerCallContext context)
    {
        var doiTypes = _domainOfInfluenceService.ListDomainOfInfluenceTypes();
        return Task.FromResult(new ListDomainOfInfluenceTypesResponse
        {
            DomainOfInfluenceTypes = { Mapper.MapDomainOfInfluenceTypes(doiTypes) },
        });
    }
}
