// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Core;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.Helpers;

public static class CallHelpers
{
    public static AsyncUnaryCall<TResponse> CreateAsyncUnaryCall<TResponse>(TResponse response)
    {
        return new AsyncUnaryCall<TResponse>(
            Task.FromResult(response),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });
    }
}
