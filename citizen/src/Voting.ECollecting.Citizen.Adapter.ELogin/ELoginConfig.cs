// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Citizen.Adapter.ELogin;

public class ELoginConfig
{
    public Uri? ApiBaseUrl { get; set; }

    public SocialSecurityNumberCacheConfig SocialSecurityNumberCache { get; set; } = new();
}
