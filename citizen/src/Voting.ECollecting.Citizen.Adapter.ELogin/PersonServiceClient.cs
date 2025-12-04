// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Voting.ECollecting.Citizen.Abstractions.Adapter.ELogin;

namespace Voting.ECollecting.Citizen.Adapter.ELogin;

public class PersonServiceClient
{
    private readonly HttpClient _client;
    private readonly IServiceProvider _serviceProvider;

    public PersonServiceClient(HttpClient client, IServiceProvider serviceProvider)
    {
        _client = client;
        _serviceProvider = serviceProvider;
    }

    public async Task<string?> GetPersonSsn()
    {
        // lazy resolve permission service to avoid circular dependency
        var userId = _serviceProvider.GetRequiredService<IPermissionService>().UserId;
        var response = await _client.GetFromJsonAsync<PersonInfo>($"data/public/v1/person/{userId}");
        return response?.SocialSecurityNumber;
    }

    private sealed record PersonInfo(string? SocialSecurityNumber);
}
