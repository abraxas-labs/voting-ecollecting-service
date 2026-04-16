// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Voting.ECollecting.Citizen.Adapter.ELogin;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.ELoginTests;

public class SocialSecurityNumberCacheTest
{
    private const string TestUserId = "test-user-123";

    // Valid AHV-N13 numbers used in the stimmregister mock
    private const string TestSsn = "756.0051.1442.43";
    private const string UpdatedSsn = "756.2147.7416.95";

    private static readonly DateTimeOffset _testAuthTime = new(2024, 6, 15, 10, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetSocialSecurityNumberWithAllowCacheReturnsCachedValueOnSecondCall()
    {
        var callCount = 0;
        var permissionService = CreatePermissionService(
            _ =>
            {
                callCount++;
                return Task.FromResult<string?>(TestSsn);
            });
        InitPermissionService(permissionService, TestUserId, _testAuthTime);

        var ssn1 = await permissionService.GetSocialSecurityNumber(allowCache: true);
        var ssn2 = await permissionService.GetSocialSecurityNumber(allowCache: true);

        ssn1.Should().Be(TestSsn);
        ssn2.Should().Be(TestSsn);
        callCount.Should().Be(1, "PersonServiceClient should only be called once when the SSN is cached");
    }

    [Fact]
    public async Task GetSocialSecurityNumberWithoutAllowCacheAlwaysCallsPersonServiceClient()
    {
        var callCount = 0;
        var permissionService = CreatePermissionService(
            _ =>
            {
                callCount++;
                return Task.FromResult<string?>(TestSsn);
            });
        InitPermissionService(permissionService, TestUserId, _testAuthTime);

        await permissionService.GetSocialSecurityNumber(allowCache: false);
        await permissionService.GetSocialSecurityNumber(allowCache: false);

        callCount.Should().Be(2, "PersonServiceClient must be called on every request when allowCache is false (used at signature submission and committee member invitation)");
    }

    [Fact]
    public async Task GetSocialSecurityNumberWithoutAllowCacheStillPopulatesCache()
    {
        var callCount = 0;
        var permissionService = CreatePermissionService(
            _ =>
            {
                callCount++;
                return Task.FromResult<string?>(TestSsn);
            });
        InitPermissionService(permissionService, TestUserId, _testAuthTime);

        // allowCache: false → should fetch and store in cache
        var ssn1 = await permissionService.GetSocialSecurityNumber(allowCache: false);

        // allowCache: true → should return from cache, no additional call
        var ssn2 = await permissionService.GetSocialSecurityNumber(allowCache: true);

        ssn1.Should().Be(TestSsn);
        ssn2.Should().Be(TestSsn);
        callCount.Should().Be(1, "The cache should be populated after a non-cache fetch, so the second call hits the cache");
    }

    [Fact]
    public async Task GetSocialSecurityNumberCacheMissWhenSameUserHasDifferentAuthTime()
    {
        var callCount = 0;
        var cache = new SocialSecurityNumberCache(new SocialSecurityNumberCacheConfig(), new FakeTimeProvider());
        var permissionService1 = CreatePermissionService(
            _ =>
            {
                callCount++;
                return Task.FromResult<string?>(TestSsn);
            },
            cache);
        var permissionService2 = CreatePermissionService(
            _ =>
            {
                callCount++;
                return Task.FromResult<string?>(UpdatedSsn);
            },
            cache);

        InitPermissionService(permissionService1, TestUserId, _testAuthTime);
        InitPermissionService(permissionService2, TestUserId, _testAuthTime.AddSeconds(1));

        // Populate cache for user with authTime
        await permissionService1.GetSocialSecurityNumber(allowCache: true);

        // Same user but different auth_time should cause a cache miss
        var ssn = await permissionService2.GetSocialSecurityNumber(allowCache: true);

        ssn.Should().Be(UpdatedSsn);
        callCount.Should().Be(2, "Different auth_time produces a different cache key, so the SSN must be re-fetched");
    }

    [Fact]
    public async Task GetSocialSecurityNumberCacheMissWhenDifferentUserHasSameAuthTime()
    {
        var callCount = 0;
        var cache = new SocialSecurityNumberCache(new SocialSecurityNumberCacheConfig(), new FakeTimeProvider());
        var permissionService1 = CreatePermissionService(
            _ =>
            {
                callCount++;
                return Task.FromResult<string?>(TestSsn);
            },
            cache);
        var permissionService2 = CreatePermissionService(
            _ =>
            {
                callCount++;
                return Task.FromResult<string?>(UpdatedSsn);
            },
            cache);

        InitPermissionService(permissionService1, TestUserId, _testAuthTime);
        InitPermissionService(permissionService2, "other-user-456", _testAuthTime);

        // Populate cache for user 1
        await permissionService1.GetSocialSecurityNumber(allowCache: true);

        // Different user with same auth_time should cause a cache miss
        var ssn = await permissionService2.GetSocialSecurityNumber(allowCache: true);

        ssn.Should().Be(UpdatedSsn);
        callCount.Should().Be(2, "Different sub (userId) produces a different cache key, so the SSN must be re-fetched");
    }

    [Fact]
    public async Task GetSocialSecurityNumberNullSsnIsNotCached()
    {
        var callCount = 0;
        var permissionService = CreatePermissionService(
            _ =>
            {
                callCount++;
                return Task.FromResult<string?>(null);
            });
        InitPermissionService(permissionService, TestUserId, _testAuthTime);

        var ssn1 = await permissionService.GetSocialSecurityNumber(allowCache: true);
        var ssn2 = await permissionService.GetSocialSecurityNumber(allowCache: true);

        ssn1.Should().BeNull();
        ssn2.Should().BeNull();
        callCount.Should().Be(2, "A null SSN must not be cached; the service must re-fetch on every call");
    }

    [Fact]
    public async Task GetSocialSecurityNumberWithConfiguredExpirationIsRespected()
    {
        var callCount = 0;
        var fakeTimeProvider = new FakeTimeProvider();
        var permissionService = CreatePermissionService(
            _ =>
            {
                callCount++;
                return Task.FromResult<string?>(TestSsn);
            },
            config: new SocialSecurityNumberCacheConfig { ExpirationTime = TimeSpan.FromMinutes(5) },
            timeProvider: fakeTimeProvider);
        InitPermissionService(permissionService, TestUserId, _testAuthTime);

        // Populate cache
        await permissionService.GetSocialSecurityNumber(allowCache: true);
        callCount.Should().Be(1);

        // Advance the clock past expiration
        fakeTimeProvider.Advance(TimeSpan.FromMinutes(6));

        // Cache entry should have expired
        var ssn = await permissionService.GetSocialSecurityNumber(allowCache: true);

        ssn.Should().Be(TestSsn);
        callCount.Should().Be(2, "After configured expiration time, the cache entry should be evicted and re-fetched");
    }

    private static PermissionService CreatePermissionService(
        Func<string, Task<string?>> getSsnForUserId,
        SocialSecurityNumberCache? cache = null,
        SocialSecurityNumberCacheConfig? config = null,
        TimeProvider? timeProvider = null)
    {
        var handler = new FakePersonServiceHandler(getSsnForUserId);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test-elogin/") };
        var personServiceClient = new PersonServiceClient(httpClient);
        var resolvedCache = cache ?? new SocialSecurityNumberCache(config ?? new SocialSecurityNumberCacheConfig(), timeProvider ?? new FakeTimeProvider());
        return new PermissionService(TimeProvider.System, resolvedCache, personServiceClient);
    }

    private static void InitPermissionService(PermissionService service, string userId, DateTimeOffset authTime)
    {
        service.Init(
            userId,
            "Test User",
            "test@example.com",
            true,
            "Test",
            "User",
            authTime);
    }

    private sealed class FakePersonServiceHandler(Func<string, Task<string?>> getSsnForUserId) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // URL pattern: data/public/v1/person/{userId}
            var userId = request.RequestUri!.Segments.Last();
            var ssn = await getSsnForUserId(userId);
            var personInfo = new { SocialSecurityNumber = ssn };
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(personInfo),
            };
        }
    }
}
