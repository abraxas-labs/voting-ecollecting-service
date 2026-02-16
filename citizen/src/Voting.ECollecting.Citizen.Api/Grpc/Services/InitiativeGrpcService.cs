// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Ecollecting.Shared.V1.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Voting.ECollecting.Citizen.Abstractions.Core.Services;
using Voting.ECollecting.Citizen.Abstractions.Core.Services.Signature;
using Voting.ECollecting.Citizen.Api.Grpc.Mappings;
using Voting.ECollecting.Citizen.Domain.Authorization;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Models;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Proto.Citizen.Services.V1.Responses;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.Lib.Common;
using Voting.Lib.Grpc;
using InitiativeCommittee = Voting.ECollecting.Proto.Citizen.Services.V1.Models.InitiativeCommittee;

namespace Voting.ECollecting.Citizen.Api.Grpc.Services;

public class InitiativeGrpcService : InitiativeService.InitiativeServiceBase
{
    private readonly IInitiativeService _initiativeService;
    private readonly IInitiativeSignService _initiativeSignService;
    private readonly IInitiativeCommitteeListService _initiativeCommitteeListService;
    private readonly IInitiativeCommitteeMemberService _initiativeCommitteeMemberService;

    public InitiativeGrpcService(
        IInitiativeService initiativeService,
        IInitiativeCommitteeListService initiativeCommitteeListService,
        IInitiativeCommitteeMemberService initiativeCommitteeMemberService,
        IInitiativeSignService initiativeSignService)
    {
        _initiativeService = initiativeService;
        _initiativeCommitteeListService = initiativeCommitteeListService;
        _initiativeCommitteeMemberService = initiativeCommitteeMemberService;
        _initiativeSignService = initiativeSignService;
    }

    public override async Task<ListInitiativesResponse> ListMy(ListMyInitiativesRequest request, ServerCallContext context)
    {
        var initiatives = await _initiativeService.ListMy();
        return Mapper.MapToListInitiativesResponse(initiatives);
    }

    public override async Task<ListInitiativeSubTypesResponse> ListSubTypes(
        ListInitiativeSubTypesRequest request,
        ServerCallContext context)
    {
        var subTypes = await _initiativeService.ListSubTypes();
        return Mapper.MapToListInitiativeSubTypesResponse(subTypes);
    }

    [CreateCollectionPolicy]
    public override async Task<IdValue> Create(CreateInitiativeRequest request, ServerCallContext context)
    {
        var id = await _initiativeService.Create(
            Mapper.MapDomainOfInfluenceType(request.DomainOfInfluenceType),
            request.Description,
            GuidParser.ParseNullable(request.SubTypeId),
            request.Bfs);
        return new IdValue { Id = id.ToString() };
    }

    [CreateCollectionPolicy]
    public override async Task<IdValue> SetInPreparation(
        SetInitiativeInPreparationRequest request,
        ServerCallContext context)
    {
        var id = await _initiativeService.SetInPreparation(request.SecureIdNumber);
        return new IdValue { Id = id.ToString() };
    }

    public override async Task<Initiative> Get(GetInitiativeRequest request, ServerCallContext context)
    {
        var initiative = await _initiativeService.Get(
            Guid.Parse(request.Id),
            request.IncludeCommitteeDescription,
            request.IncludeIsSigned);
        return Mapper.MapToInitiative(initiative);
    }

    public override async Task<Empty> Update(UpdateInitiativeRequest request, ServerCallContext context)
    {
        await _initiativeService.Update(
            Guid.Parse(request.Id),
            GuidParser.ParseNullable(request.SubTypeId),
            request.Description,
            request.Wording,
            request.Reason,
            Mapper.MapToCollectionAddress(request.Address),
            request.Link);
        return ProtobufEmpty.Instance;
    }

    public override async Task<InitiativeCommittee> GetCommittee(
        GetInitiativeCommitteeRequest request,
        ServerCallContext context)
    {
        var committee = await _initiativeCommitteeMemberService.GetCommittee(Guid.Parse(request.Id));
        return Mapper.MapToInitiativeCommittee(committee);
    }

    public override async Task<Empty> DeleteCommitteeList(DeleteCommitteeListRequest request, ServerCallContext context)
    {
        await _initiativeCommitteeListService.DeleteCommitteeList(Guid.Parse(request.InitiativeId), Guid.Parse(request.Id));
        return ProtobufEmpty.Instance;
    }

    public override async Task<IdValue> AddCommitteeMember(AddCommitteeMemberRequest request, ServerCallContext context)
    {
        var role = Mapper.MapToRole(request.Role);
        var entity = await _initiativeCommitteeMemberService.AddCommitteeMember(
            Mapper.MapToInitiativeCommitteeMember(request),
            role == CollectionPermissionRole.Unspecified ? null : role);
        return Mapper.MapToIdValue(entity);
    }

    public override async Task<Empty> DeleteCommitteeMember(DeleteCommitteeMemberRequest request, ServerCallContext context)
    {
        var id = Guid.Parse(request.Id);
        var initiativeId = Guid.Parse(request.InitiativeId);
        await _initiativeCommitteeMemberService.RemoveCommitteeMember(initiativeId, id);
        return ProtobufEmpty.Instance;
    }

    public override async Task<Empty> UpdateCommitteeMemberSort(
        UpdateCommitteeMemberSortRequest request,
        ServerCallContext context)
    {
        var id = Guid.Parse(request.Id);
        var initiativeId = Guid.Parse(request.InitiativeId);
        await _initiativeCommitteeMemberService.UpdateCommitteeMemberSort(initiativeId, id, request.NewIndex);
        return ProtobufEmpty.Instance;
    }

    public override async Task<Empty> UpdateCommitteeMember(
        UpdateCommitteeMemberRequest request,
        ServerCallContext context)
    {
        var role = Mapper.MapToRole(request.Role);
        var member = Mapper.MapToInitiativeCommitteeMember(request);
        await _initiativeCommitteeMemberService.UpdateCommitteeMember(
            member,
            role == CollectionPermissionRole.Unspecified ? null : role);
        return ProtobufEmpty.Instance;
    }

    public override async Task<Empty> UpdateCommitteeMemberPoliticalDuty(
        UpdateCommitteeMemberPoliticalDutyRequest request,
        ServerCallContext context)
    {
        var id = Guid.Parse(request.Id);
        var initiativeId = Guid.Parse(request.InitiativeId);
        await _initiativeCommitteeMemberService.UpdateCommitteeMemberPoliticalDuty(initiativeId, id, request.PoliticalDuty);
        return ProtobufEmpty.Instance;
    }

    public override async Task<Empty> ResendCommitteeMemberInvitation(
        ResendCommitteeMemberInvitationRequest request,
        ServerCallContext context)
    {
        var id = Guid.Parse(request.Id);
        var initiativeId = Guid.Parse(request.InitiativeId);
        await _initiativeCommitteeMemberService.ResendCommitteeMemberInvitation(initiativeId, id);
        return ProtobufEmpty.Instance;
    }

    public override async Task<Empty> Submit(SubmitInitiativeRequest request, ServerCallContext context)
    {
        await _initiativeService.Submit(GuidParser.Parse(request.Id));
        return ProtobufEmpty.Instance;
    }

    public override async Task<Empty> FlagForReview(FlagInitiativeForReviewRequest request, ServerCallContext context)
    {
        await _initiativeService.FlagForReview(GuidParser.Parse(request.Id));
        return ProtobufEmpty.Instance;
    }

    public override async Task<Empty> Register(RegisterInitiativeRequest request, ServerCallContext context)
    {
        await _initiativeService.Register(GuidParser.Parse(request.Id));
        return ProtobufEmpty.Instance;
    }

    [AllowAnonymous]
    public override async Task<GetPendingCommitteeMemberResponse> GetPendingCommitteeMembershipByToken(
        GetPendingCommitteeMembershipByTokenRequest request,
        ServerCallContext context)
    {
        var resp = await _initiativeCommitteeMemberService.GetPendingCommitteeMembershipByToken(request.Token);
        return Mapper.MapToPendingCommitteeMembership(resp);
    }

    [AcceptInitiativeCommitteeMembershipPolicy]
    public override async Task<AcceptCommitteeMembershipResponse> AcceptCommitteeMembershipByToken(
        AcceptCommitteeMembershipRequest request,
        ServerCallContext context)
    {
        var accepted = await _initiativeCommitteeMemberService.AcceptCommitteeMemberInvitation(request.Token);
        return new AcceptCommitteeMembershipResponse { Accepted = accepted };
    }

    [AllowAnonymous]
    public override async Task<Empty> RejectCommitteeMembershipByToken(
        RejectCommitteeMembershipRequest request,
        ServerCallContext context)
    {
        await _initiativeCommitteeMemberService.RejectCommitteeMemberInvitation(request.Token);
        return ProtobufEmpty.Instance;
    }

    [SignCollectionPolicy]
    public override async Task<Empty> Sign(SignInitiativeRequest request, ServerCallContext context)
    {
        await _initiativeSignService.Sign(GuidParser.Parse(request.Id));
        return ProtobufEmpty.Instance;
    }
}
