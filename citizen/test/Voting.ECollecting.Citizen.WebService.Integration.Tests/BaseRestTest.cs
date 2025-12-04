// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voting.ECollecting.Citizen.WebService.Integration.Tests.Mocks;
using Voting.ECollecting.Shared.Migrations;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Models;
using Voting.ECollecting.Shared.Test.Utils;
using Voting.Lib.Testing;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests;

public abstract class BaseRestTest : RestApiBaseTest<TestApplicationFactory, TestStartup>
{
    private readonly Lazy<HttpClient> _client;
    private readonly Lazy<HttpClient> _authenticatedClient;
    private readonly Lazy<HttpClient> _authenticatedNoPermissionClient;
    private readonly Lazy<HttpClient> _deputyClient;
    private readonly Lazy<HttpClient> _deputyNotAcceptedClient;
    private readonly Lazy<HttpClient> _readerClient;
    private readonly Lazy<HttpClient> _readerNotAcceptedClient;

    protected BaseRestTest(TestApplicationFactory factory)
        : base(factory)
    {
        ResetDb();

        _client = new Lazy<HttpClient>(() => CreateHttpClient(false));
        _authenticatedClient = new Lazy<HttpClient>(() => CreateHttpClient());
        _authenticatedNoPermissionClient = new Lazy<HttpClient>(() => CreateHttpClient(userId: CitizenAuthMockDefaults.NoPermissionUserId));
        _deputyClient = new Lazy<HttpClient>(() => CreateHttpClient(userId: CitizenAuthMockDefaults.DeputyUserId));
        _deputyNotAcceptedClient = new Lazy<HttpClient>(() => CreateHttpClient(userId: CitizenAuthMockDefaults.DeputyNotAcceptedUserId));
        _readerClient = new Lazy<HttpClient>(() => CreateHttpClient(userId: CitizenAuthMockDefaults.ReaderUserId));
        _readerNotAcceptedClient = new Lazy<HttpClient>(() => CreateHttpClient(userId: CitizenAuthMockDefaults.ReaderNotAcceptedUserId));
    }

    protected HttpClient Client => _client.Value;

    protected HttpClient AuthenticatedClient => _authenticatedClient.Value;

    protected HttpClient AuthenticatedNoPermissionClient => _authenticatedNoPermissionClient.Value;

    protected HttpClient DeputyClient => _deputyClient.Value;

    protected HttpClient DeputyNotAcceptedClient => _deputyNotAcceptedClient.Value;

    protected HttpClient ReaderClient => _readerClient.Value;

    protected HttpClient ReaderNotAcceptedClient => _readerNotAcceptedClient.Value;

    protected HttpClient CreateCitizenClient(
        string userId = CitizenAuthMockDefaults.CitizenUserId,
        string acrValue = "",
        string email = CitizenAuthMockDefaults.UserCitizenTestEMail,
        string ssn = "")
    {
        return Factory.CreateHttpClient(
            true,
            null,
            userId,
            [],
            [
                (CitizenAuthMockDefaults.UserAcrHeaderName, acrValue),
                (CitizenAuthMockDefaults.UserEMailHeaderName, email),
                (CitizenAuthMockDefaults.UserSocialSecurityNumberHeaderName, ssn)
            ]);
    }

    protected Task RunOnDb(Func<MigrationDataContext, Task> action)
        => RunScoped(action);

    protected Task<TResult> RunOnDb<TResult>(Func<MigrationDataContext, Task<TResult>> action)
        => RunScoped(action);

    protected void ResetDb()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MigrationDataContext>();
        DatabaseUtil.Truncate(db);
    }

    protected Task ModifyDbEntities<TEntity>(Expression<Func<TEntity, bool>> predicate, Action<TEntity> modifier)
        where TEntity : class
    {
        return RunOnDb(async db =>
        {
            var set = db.Set<TEntity>();
            var entities = await set.AsTracking().Where(predicate).ToListAsync();

            foreach (var entity in entities)
            {
                modifier(entity);
            }

            await db.SaveChangesAsync();
        });
    }

    protected async Task AssertStatus(Func<Task<HttpResponseMessage>> action, HttpStatusCode code, string title, string detail)
    {
        using var resp = await action();
        resp.StatusCode.Should().Be(code);

        var problemDto = await resp.Content.ReadFromJsonAsync<ProblemDetails>()
                         ?? throw new InvalidOperationException("Response does not contain ProblemDetails");
        problemDto.Title.Should().Be(title);
        problemDto.Detail.Should().Be(detail);
    }

    protected async Task<AuditTrailEntriesResult> GetAuditTrailEntries()
    {
        var auditTrailEntries = await RunOnDb(db => db.AuditTrailEntries
            .OrderBy(e => e.AuditInfo.CreatedAt)
            .ToListAsync());

        var collectionCitizenLogAuditTrailEntries = await RunOnDb(db => db.CollectionCitizenLogAuditTrailEntries
            .OrderBy(e => e.AuditInfo.CreatedAt)
            .ToListAsync());

        return new(auditTrailEntries, collectionCitizenLogAuditTrailEntries);
    }

    protected async Task RunInAuditTrailTestScope(Func<Task> action)
    {
        PermissionServiceMock.HasTimestampIncrement = true;

        try
        {
            await action.Invoke();
        }
        finally
        {
            PermissionServiceMock.HasTimestampIncrement = false;
        }
    }
}
