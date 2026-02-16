// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Voting.ECollecting.Citizen.Abstractions.Core.Services;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.Lib.Grpc;

namespace Voting.ECollecting.Citizen.Api.Grpc.Services;

public class AuthGrpcService : AuthService.AuthServiceBase
{
    private readonly ICollectionPermissionService _collectionPermissionService;

    public AuthGrpcService(ICollectionPermissionService collectionPermissionService)
    {
        _collectionPermissionService = collectionPermissionService;
    }

    [Authorize]
    public override async Task<Empty> TrackLogin(Empty request, ServerCallContext context)
    {
        await _collectionPermissionService.UpdateIamInfo();
        return ProtobufEmpty.Instance;
    }
}
