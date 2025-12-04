// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Ecollecting.Shared.V1.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Voting.ECollecting.Citizen.Abstractions.Core.Services;
using Voting.ECollecting.Citizen.Abstractions.Core.Services.Signature;
using Voting.ECollecting.Citizen.Api.Grpc.Mappings;
using Voting.ECollecting.Citizen.Domain.Authorization;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Models;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Proto.Citizen.Services.V1.Responses;
using Voting.Lib.Common;
using Voting.Lib.Grpc;

namespace Voting.ECollecting.Citizen.Api.Grpc.Services;

public class ReferendumGrpcService : ReferendumService.ReferendumServiceBase
{
    private readonly IReferendumService _referendumService;
    private readonly IReferendumSignService _referendumSignService;

    public ReferendumGrpcService(IReferendumService referendumService, IReferendumSignService referendumSignService)
    {
        _referendumService = referendumService;
        _referendumSignService = referendumSignService;
    }

    [CreateCollectionPolicy]
    public override async Task<IdValue> Create(CreateReferendumRequest request, ServerCallContext context)
    {
        var id = await _referendumService.Create(request.Description, GuidParser.ParseNullable(request.DecreeId));
        return new IdValue { Id = id.ToString() };
    }

    [CreateCollectionPolicy]
    public override async Task<IdValue> SetInPreparation(
        SetReferendumInPreparationRequest request,
        ServerCallContext context)
    {
        var id = await _referendumService.SetInPreparation(request.ReferendumNumber);
        return new IdValue { Id = id.ToString() };
    }

    public override async Task<Referendum> Get(GetReferendumRequest request, ServerCallContext context)
    {
        var referendum = await _referendumService.Get(Guid.Parse(request.Id), request.IncludeIsSigned);
        return Mapper.MapToReferendum(referendum);
    }

    public override async Task<Empty> Update(UpdateReferendumRequest request, ServerCallContext context)
    {
        await _referendumService.Update(
            Guid.Parse(request.Id),
            request.Description,
            request.Reason,
            Mapper.MapToCollectionAddress(request.Address),
            request.MembersCommittee,
            request.Link);
        return ProtobufEmpty.Instance;
    }

    public override async Task<Empty> Submit(SubmitReferendumRequest request, ServerCallContext context)
    {
        await _referendumService.Submit(GuidParser.Parse(request.Id));
        return ProtobufEmpty.Instance;
    }

    public override async Task<ListMyReferendumsResponse> ListMy(ListMyReferendumsRequest request, ServerCallContext context)
    {
        var (decrees, referendumsWithoutDecree) = await _referendumService.ListMy();
        return Mapper.MapToListMyReferendumsResponse(decrees, referendumsWithoutDecree);
    }

    public override async Task<ListDecreesEligibleForReferendumResponse> ListDecreesEligibleForReferendum(
        ListDecreesEligibleForReferendumRequest request,
        ServerCallContext context)
    {
        var doiTypes = Mapper.MapToDomainOfInfluenceTypes(request.Types_).ToHashSet();
        var decrees = await _referendumService.ListDecreesEligibleForReferendumByDoiType(doiTypes, request.Bfs, request.IncludeReferendums);
        return Mapper.MapToListDecreesEligibleForReferendumResponse(decrees);
    }

    public override async Task<Empty> UpdateDecree(UpdateReferendumDecreeRequest request, ServerCallContext context)
    {
        await _referendumService.UpdateDecree(Guid.Parse(request.Id), Guid.Parse(request.DecreeId));
        return ProtobufEmpty.Instance;
    }

    [SignCollectionPolicy]
    public override async Task<Empty> Sign(SignReferendumRequest request, ServerCallContext context)
    {
        await _referendumSignService.Sign(Guid.Parse(request.Id));
        return ProtobufEmpty.Instance;
    }
}
