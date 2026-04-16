// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Voting.ECollecting.Citizen.Abstractions.Adapter.ELogin;
using Voting.Lib.Common;
using Voting.Lib.Common.Cache;

namespace Voting.ECollecting.Citizen.Adapter.ELogin;

public class SocialSecurityNumberCache
{
    private readonly Cache<SocialSecurityNumberCacheEntry> _cache;

    public SocialSecurityNumberCache(SocialSecurityNumberCacheConfig config, TimeProvider timeProvider)
    {
        _cache = new Cache<SocialSecurityNumberCacheEntry>(
            new MemoryCache(new MemoryCacheOptions { SizeLimit = config.SizeLimit, Clock = new SystemClockAdapter(timeProvider) }),
            new CacheOptions<SocialSecurityNumberCacheEntry>
            {
                AbsoluteExpirationRelativeToNow = config.ExpirationTime,
            });
    }

    internal void TrySet(PermissionService permissionService, string? ssn)
    {
        // we only cache valid social security numbers.
        // if a person does not have a ssn, we recheck each time.
        if (ssn == null)
        {
            return;
        }

        if (TryBuildKey(permissionService, out var key))
        {
            _cache.Set(key, new SocialSecurityNumberCacheEntry(ssn));
        }
    }

    internal string? Get(IPermissionService userInfo)
    {
        return TryBuildKey(userInfo, out var key)
            ? _cache.Get(key)?.SocialSecurityNumber
            : null;
    }

    private static bool TryBuildKey(
        IPermissionService userInfo,
        [NotNullWhen(true)] out string? key)
    {
        if (!userInfo.IsAuthenticated)
        {
            key = null;
            return false;
        }

        var authTime = userInfo.AuthenticatedTime.ToUnixTimeSeconds();
        key = HashUtil.GetSHA256Hash($"ssn-{userInfo.UserId}-{authTime}");
        return true;
    }

    private sealed record SocialSecurityNumberCacheEntry(string? SocialSecurityNumber);

    // unfortunately, the memory cache still uses ISystemClock instead of TimeProvider.
    private sealed class SystemClockAdapter(TimeProvider timeProvider) : ISystemClock
    {
        public DateTimeOffset UtcNow => timeProvider.GetUtcNow();
    }
}
