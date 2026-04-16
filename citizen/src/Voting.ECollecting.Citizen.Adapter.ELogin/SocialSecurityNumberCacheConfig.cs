// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Citizen.Adapter.ELogin;

public class SocialSecurityNumberCacheConfig
{
    public long SizeLimit { get; set; } = 100_000;

    public TimeSpan ExpirationTime { get; set; } = TimeSpan.FromMinutes(5);
}
