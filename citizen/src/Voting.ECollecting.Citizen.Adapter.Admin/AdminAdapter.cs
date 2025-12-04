// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Abstractions.Adapter.Admin;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.Lib.Grpc;

namespace Voting.ECollecting.Citizen.Adapter.Admin;

public class AdminAdapter : IAdminAdapter
{
    private readonly CollectionService.CollectionServiceClient _client;

    public AdminAdapter(CollectionService.CollectionServiceClient client)
    {
        _client = client;
    }

    public async Task NotifyPreparingForCollection()
    {
        await _client.NotifyPreparingForCollectionAsync(ProtobufEmpty.Instance);
    }
}
