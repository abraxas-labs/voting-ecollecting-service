// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Voting.ECollecting.Citizen.Api.Grpc.Mappings;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Grpc;
using IUserNotificationService = Voting.ECollecting.Citizen.Abstractions.Core.Services.IUserNotificationService;

namespace Voting.ECollecting.Citizen.Api.Grpc.Services;

public class AccessibilityGrpcService : AccessibilityService.AccessibilityServiceBase
{
    private readonly IUserNotificationService _userNotificationService;

    public AccessibilityGrpcService(
        IUserNotificationService userNotificationService)
    {
        _userNotificationService = userNotificationService;
    }

    [AllowAnonymous]
    public override async Task<Empty> SendMessage(SendAccessibilityMessageRequest request, ServerCallContext context)
    {
        await _userNotificationService.SendAccessibilityMessage(Mapper.MapToAccessibilityMessage(request));
        return ProtobufEmpty.Instance;
    }
}
