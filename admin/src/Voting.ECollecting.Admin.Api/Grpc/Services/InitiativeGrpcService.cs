// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Ecollecting.Shared.V1.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Api.Grpc.Mappings;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Models;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Proto.Admin.Services.V1.Responses;
using Voting.Lib.Common;
using Voting.Lib.Grpc;

namespace Voting.ECollecting.Admin.Api.Grpc.Services;

public class InitiativeGrpcService : InitiativeService.InitiativeServiceBase
{
    private readonly IInitiativeService _initiativeService;
    private readonly IInitiativeAdmissibilityDecisionService _initiativeAdmissibilityDecisionService;
    private readonly IInitiativeCommitteeService _initiativeCommitteeService;

    public InitiativeGrpcService(
        IInitiativeService initiativeService,
        IInitiativeCommitteeService initiativeCommitteeService,
        IInitiativeAdmissibilityDecisionService initiativeAdmissibilityDecisionService)
    {
        _initiativeService = initiativeService;
        _initiativeCommitteeService = initiativeCommitteeService;
        _initiativeAdmissibilityDecisionService = initiativeAdmissibilityDecisionService;
    }

    [Stammdatenverwalter]
    public override async Task<ListInitiativeSubTypesResponse> ListSubTypes(
        ListInitiativeSubTypesRequest request,
        ServerCallContext context)
    {
        var subTypes = await _initiativeService.ListSubTypes();
        return Mapper.MapToListInitiativeSubTypesResponse(subTypes);
    }

    [HumanUser]
    public override async Task<ListInitiativesResponse> List(ListInitiativesRequest request, ServerCallContext context)
    {
        var doiTypes = Mapper.MapToDomainOfInfluenceTypes(request.Types_).ToHashSet();
        var initiatives = await _initiativeService.ListByDoiType(doiTypes, request.Bfs);
        return Mapper.MapToListInitiativesResponse(initiatives);
    }

    [HumanUser]
    public override async Task<Initiative> Get(GetInitiativeRequest request, ServerCallContext context)
    {
        var initiative = await _initiativeService.Get(GuidParser.Parse(request.Id));
        return Mapper.MapToInitiative(initiative);
    }

    [Stammdatenverwalter]
    public override async Task<Empty> FinishCorrection(FinishCorrectionInitiativeRequest request, ServerCallContext context)
    {
        await _initiativeService.FinishCorrection(GuidParser.Parse(request.Id));
        return ProtobufEmpty.Instance;
    }

    [Stammdatenverwalter]
    public override async Task<Empty> SetCollectionPeriod(SetCollectionPeriodInitiativeRequest request, ServerCallContext context)
    {
        await _initiativeService.SetCollectionPeriod(
            GuidParser.Parse(request.Id),
            Mapper.MapToDateOnly(request.CollectionStartDate),
            Mapper.MapToDateOnly(request.CollectionEndDate));
        return ProtobufEmpty.Instance;
    }

    [Stammdatenverwalter]
    public override async Task<Empty> Enable(EnableInitiativeRequest request, ServerCallContext context)
    {
        await _initiativeService.Enable(
            GuidParser.Parse(request.Id),
            Mapper.MapToNullableDateOnly(request.CollectionStartDate),
            Mapper.MapToNullableDateOnly(request.CollectionEndDate));
        return ProtobufEmpty.Instance;
    }

    [Stammdatenverwalter]
    public override async Task<InitiativeCommittee> GetCommittee(
        GetInitiativeCommitteeRequest request,
        ServerCallContext context)
    {
        var committee = await _initiativeCommitteeService.GetCommittee(Guid.Parse(request.Id));
        return Mapper.MapToInitiativeCommittee(committee);
    }

    [Stammdatenverwalter]
    public override async Task<Empty> ResetCommitteeMember(ResetCommitteeMemberRequest request, ServerCallContext context)
    {
        await _initiativeCommitteeService.ResetCommitteeMember(
            GuidParser.Parse(request.InitiativeId),
            GuidParser.Parse(request.Id));
        return ProtobufEmpty.Instance;
    }

    [Stammdatenverwalter]
    public override async Task<VerifyInitiativeCommitteeMemberResponse> VerifyCommitteeMember(
        VerifyCommitteeMemberRequest request,
        ServerCallContext context)
    {
        var personInfo = await _initiativeCommitteeService.VerifyCommitteeMember(
            GuidParser.Parse(request.InitiativeId),
            GuidParser.Parse(request.Id));

        return Mapper.MapToVerifyInitiativeCommitteeMemberResponse(personInfo);
    }

    [Stammdatenverwalter]
    public override async Task<Empty> ApproveCommitteeMember(ApproveCommitteeMemberRequest request, ServerCallContext context)
    {
        await _initiativeCommitteeService.ApproveCommitteeMember(
            GuidParser.Parse(request.InitiativeId),
            GuidParser.Parse(request.Id));
        return ProtobufEmpty.Instance;
    }

    [Stammdatenverwalter]
    public override async Task<Empty> RejectCommitteeMember(RejectCommitteeMemberRequest request, ServerCallContext context)
    {
        await _initiativeCommitteeService.RejectCommitteeMember(
            GuidParser.Parse(request.InitiativeId),
            GuidParser.Parse(request.Id));
        return ProtobufEmpty.Instance;
    }

    [Stammdatenverwalter]
    public override async Task<Empty> UpdateCommitteeMember(UpdateCommitteeMemberRequest request, ServerCallContext context)
    {
        await _initiativeCommitteeService.UpdateCommitteeMember(
            GuidParser.Parse(request.InitiativeId),
            GuidParser.Parse(request.Id),
            Mapper.MapToUpdateCommitteeMemberParams(request));
        return ProtobufEmpty.Instance;
    }

    [Stammdatenverwalter]
    public override async Task<Empty> CameAbout(CameAboutInitiativeRequest request, ServerCallContext context)
    {
        await _initiativeService.CameAbout(
            GuidParser.Parse(request.InitiativeId),
            Mapper.MapToDateOnly(request.SensitiveDataExpiryDate));
        return ProtobufEmpty.Instance;
    }

    [Stammdatenverwalter]
    public override async Task<Empty> CameNotAbout(CameNotAboutInitiativeRequest request, ServerCallContext context)
    {
        await _initiativeService.CameNotAbout(
            GuidParser.Parse(request.InitiativeId),
            Mapper.MapToCollectionCameNotAboutReason(request.Reason),
            Mapper.MapToDateOnly(request.SensitiveDataExpiryDate));
        return ProtobufEmpty.Instance;
    }

    [Stammdatenverwalter]
    public override async Task<Empty> Update(UpdateInitiativeRequest request, ServerCallContext context)
    {
        await _initiativeService.Update(
            GuidParser.Parse(request.Id),
            Mapper.MapToUpdateInitiativeParams(request));
        return ProtobufEmpty.Instance;
    }

    [Stammdatenverwalter]
    public override async Task<ListEligibleForAdmissibilityDecisionResponse> ListEligibleForAdmissibilityDecision(
        ListEligibleForAdmissibilityDecisionRequest request,
        ServerCallContext context)
    {
        var data = await _initiativeAdmissibilityDecisionService.ListEligibleForAdmissibilityDecision();
        return Mapper.MapToListEligibleForAdmissibilityDecisionResponse(data);
    }

    [Stammdatenverwalter]
    public override async Task<ListAdmissibilityDecisionsResponse> ListAdmissibilityDecisions(
        ListAdmissibilityDecisionsRequest request,
        ServerCallContext context)
    {
        var data = await _initiativeAdmissibilityDecisionService.ListAdmissibilityDecisions();
        return Mapper.MapToListAdmissibilityDecisionsResponse(data);
    }

    [Stammdatenverwalter]
    public override async Task<Empty> DeleteAdmissibilityDecision(
        DeleteAdmissibilityDecisionRequest request,
        ServerCallContext context)
    {
        await _initiativeAdmissibilityDecisionService.DeleteAdmissibilityDecision(GuidParser.Parse(request.Id));
        return ProtobufEmpty.Instance;
    }

    [Stammdatenverwalter]
    public override async Task<Empty> CreateLinkedAdmissibilityDecision(
        CreateLinkedAdmissibilityDecisionRequest request,
        ServerCallContext context)
    {
        await _initiativeAdmissibilityDecisionService.CreateLinkedAdmissibilityDecision(
            GuidParser.Parse(request.InitiativeId),
            request.GovernmentDecisionNumber,
            Mapper.MapToAdmissibilityDecisionState(request.AdmissibilityDecisionState));
        return ProtobufEmpty.Instance;
    }

    [Stammdatenverwalter]
    public override async Task<IdValue> CreateWithAdmissibilityDecision(
        CreateInitiativeWithAdmissibilityDecisionRequest request,
        ServerCallContext context)
    {
        var reqParams = Mapper.MapToCreateInitiativeParams(request);
        var initiativeId = await _initiativeAdmissibilityDecisionService.CreateWithAdmissibilityDecision(reqParams);
        return new IdValue { Id = initiativeId.ToString() };
    }

    [Stammdatenverwalter]
    public override async Task<Empty> UpdateAdmissibilityDecision(
        UpdateAdmissibilityDecisionRequest request,
        ServerCallContext context)
    {
        await _initiativeAdmissibilityDecisionService.UpdateAdmissibilityDecision(
            GuidParser.Parse(request.InitiativeId),
            request.GovernmentDecisionNumber,
            Mapper.MapToAdmissibilityDecisionState(request.AdmissibilityDecisionState));
        return ProtobufEmpty.Instance;
    }

    [Stammdatenverwalter]
    public override async Task<Empty> ReturnForCorrection(ReturnInitiativeForCorrectionRequest request, ServerCallContext context)
    {
        await _initiativeService.ReturnForCorrection(
            GuidParser.Parse(request.Id),
            Mapper.MapToInitiativeLockedFields(request.LockedFields));
        return ProtobufEmpty.Instance;
    }

    [Kontrollzeichenloescher]
    public override async Task<Empty> SetSensitiveDataExpiryDate(
        SetInitiativeSensitiveDataExpiryDateRequest request,
        ServerCallContext context)
    {
        var date = Mapper.MapToDateOnly(request.SensitiveDataExpiryDate);
        await _initiativeService.SetSensitiveDataExpiryDate(GuidParser.Parse(request.InitiativeId), date);
        return ProtobufEmpty.Instance;
    }

    [Kontrollzeichenloescher]
    public override async Task<SecondFactorTransaction> PrepareDelete(
        PrepareDeleteInitiativeRequest request,
        ServerCallContext context)
    {
        var transaction = await _initiativeService.PrepareDelete(GuidParser.Parse(request.InitiativeId));
        return Mapper.MapSecondFactorTransaction(transaction);
    }

    [Kontrollzeichenloescher]
    public override async Task<Empty> Delete(DeleteInitiativeRequest request, ServerCallContext context)
    {
        await _initiativeService.Delete(
            GuidParser.Parse(request.InitiativeId),
            GuidParser.Parse(request.SecondFactorTransactionId),
            context.CancellationToken);
        return ProtobufEmpty.Instance;
    }
}
