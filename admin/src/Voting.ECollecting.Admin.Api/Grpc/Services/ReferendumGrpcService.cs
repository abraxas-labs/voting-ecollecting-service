// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Ecollecting.Shared.V1.Models;
using Grpc.Core;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Api.Grpc.Mappings;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Models;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Proto.Admin.Services.V1.Responses;
using Voting.Lib.Common;

namespace Voting.ECollecting.Admin.Api.Grpc.Services;

public class ReferendumGrpcService : ReferendumService.ReferendumServiceBase
{
    private readonly IReferendumService _referendumService;

    public ReferendumGrpcService(IReferendumService referendumService)
    {
        _referendumService = referendumService;
    }

    [HumanUser]
    public override async Task<Referendum> Get(GetReferendumRequest request, ServerCallContext context)
    {
        var referendum = await _referendumService.Get(GuidParser.Parse(request.Id));
        return Mapper.MapToReferendum(referendum);
    }

    [HumanUser]
    public override async Task<ListReferendumDecreesResponse> ListDecrees(
        ListReferendumDecreesRequest request,
        ServerCallContext context)
    {
        var doiTypes = Mapper.MapToDomainOfInfluenceTypes(request.Types_).ToHashSet();
        var decrees = await _referendumService.ListDecreesByDoiType(doiTypes, request.Bfs);
        return Mapper.MapToListReferendumDecreesResponse(decrees);
    }

    [StammdatenverwalterOrKontrollzeichenerfasser]
    public override async Task<IdValue> Create(CreateReferendumRequest request, ServerCallContext context)
    {
        var id = await _referendumService.Create(GuidParser.Parse(request.DecreeId), request.Description, Mapper.MapToCollectionAddress(request.Address));
        return new IdValue { Id = id.ToString() };
    }
}
