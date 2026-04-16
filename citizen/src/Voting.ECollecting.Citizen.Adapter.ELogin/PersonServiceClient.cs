// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Voting.Lib.Common;

namespace Voting.ECollecting.Citizen.Adapter.ELogin;

public class PersonServiceClient
{
    private readonly HttpClient _client;

    public PersonServiceClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<string?> GetPersonSsn(string userId)
    {
        var response = await _client.GetFromJsonAsync<PersonInfo>($"data/public/v1/person/{userId}");
        if (!string.IsNullOrEmpty(response?.SocialSecurityNumber) && !Ahvn13.IsValid(response.SocialSecurityNumber))
        {
            throw new ValidationException("Social security number is not valid.");
        }

        return response?.SocialSecurityNumber;
    }

    private sealed record PersonInfo(string? SocialSecurityNumber);
}
