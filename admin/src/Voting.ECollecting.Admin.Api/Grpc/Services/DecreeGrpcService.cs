// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Ecollecting.Shared.V1.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Models;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Proto.Admin.Services.V1.Responses;
using Voting.Lib.Common;
using Voting.Lib.Grpc;
using Mapper = Voting.ECollecting.Admin.Api.Grpc.Mappings.Mapper;

namespace Voting.ECollecting.Admin.Api.Grpc.Services;

public class DecreeGrpcService : DecreeService.DecreeServiceBase
{
    private readonly IDecreeService _decreeService;

    public DecreeGrpcService(IDecreeService decreeService)
    {
        _decreeService = decreeService;
    }

    [Stammdatenverwalter]
    public override async Task<IdValue> Create(CreateDecreeRequest request, ServerCallContext context)
    {
        var decree = Mapper.MapToDecree(request);
        var id = await _decreeService.Create(decree);
        return new IdValue { Id = id.ToString() };
    }

    [Stammdatenverwalter]
    public override async Task<ListDecreesResponse> List(ListDecreesRequest request, ServerCallContext context)
    {
        var decrees = await _decreeService.List();
        return Mapper.MapToListDecreesResponse(decrees);
    }

    [Stammdatenverwalter]
    public override async Task<Empty> Update(UpdateDecreeRequest request, ServerCallContext context)
    {
        var decree = await _decreeService.GetForEdit(Guid.Parse(request.Id));
        Mapper.MapToDecree(request, decree);
        await _decreeService.Update(decree);
        return ProtobufEmpty.Instance;
    }

    [Stammdatenverwalter]
    public override async Task<Empty> DeletePublished(DeletePublishedDecreeRequest request, ServerCallContext context)
    {
        await _decreeService.DeletePublished(Guid.Parse(request.Id));
        return ProtobufEmpty.Instance;
    }

    [Stammdatenverwalter]
    public override async Task<Empty> CameAbout(CameAboutDecreeRequest request, ServerCallContext context)
    {
        await _decreeService.CameAbout(
            GuidParser.Parse(request.DecreeId),
            Mapper.MapToDateOnly(request.SensitiveDataExpiryDate));
        return ProtobufEmpty.Instance;
    }

    [Stammdatenverwalter]
    public override async Task<Empty> CameNotAbout(CameNotAboutDecreeRequest request, ServerCallContext context)
    {
        await _decreeService.CameNotAbout(
            GuidParser.Parse(request.DecreeId),
            Mapper.MapToCollectionCameNotAboutReason(request.Reason),
            Mapper.MapToDateOnly(request.SensitiveDataExpiryDate));
        return ProtobufEmpty.Instance;
    }

    [Kontrollzeichenloescher]
    public override async Task<SecondFactorTransaction> PrepareDelete(
        PrepareDeleteDecreeRequest request,
        ServerCallContext context)
    {
        var transaction = await _decreeService.PrepareDelete(GuidParser.Parse(request.DecreeId));
        return Mapper.MapSecondFactorTransaction(transaction);
    }

    [Kontrollzeichenloescher]
    public override async Task<Empty> Delete(
        DeleteDecreeRequest request,
        ServerCallContext context)
    {
        await _decreeService.Delete(
            GuidParser.Parse(request.DecreeId),
            GuidParser.Parse(request.SecondFactorTransactionId),
            context.CancellationToken);
        return ProtobufEmpty.Instance;
    }

    [Kontrollzeichenloescher]
    public override async Task<Empty> SetSensitiveDataExpiryDate(
        SetDecreeSensitiveDataExpiryDateRequest request,
        ServerCallContext context)
    {
        var date = Mapper.MapToDateOnly(request.SensitiveDataExpiryDate);
        await _decreeService.SetSensitiveDataExpiryDate(GuidParser.Parse(request.DecreeId), date);
        return ProtobufEmpty.Instance;
    }
}
