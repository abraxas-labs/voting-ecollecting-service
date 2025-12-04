// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.IO.Compression;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.Admin.WebService.Integration.Tests.Mocks;
using Voting.ECollecting.Shared.Migrations;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Models;
using Voting.ECollecting.Shared.Test.Utils;
using Voting.Lib.Testing;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests;

public abstract class BaseRestTest : RestAuthorizationBaseTest<TestApplicationFactory, TestStartup>
{
    private const string ZipExtension = ".zip";

    private readonly Lazy<HttpClient> _ctStammdatenVerwalterClient;
    private readonly Lazy<HttpClient> _muSgStammdatenVerwalterClient;
    private readonly Lazy<HttpClient> _muGoldachStammdatenVerwalterClient;
    private readonly Lazy<HttpClient> _muSgKontrollzeichenerstellerClient;
    private readonly Lazy<HttpClient> _muGoldachKontrollzeichenerstellerClient;
    private readonly Lazy<HttpClient> _ctSgZertifikatsverwalterClient;
    private readonly Lazy<HttpClient> _muSgZertifikatsverwalterClient;

    protected BaseRestTest(TestApplicationFactory factory)
        : base(factory)
    {
        ResetDb();

        _ctStammdatenVerwalterClient = new Lazy<HttpClient>(() => CreateHttpClient(tenant: MockedTenantIds.KTSG, roles: Roles.Stammdatenverwalter));
        _muSgStammdatenVerwalterClient = new Lazy<HttpClient>(() => CreateHttpClient(tenant: MockedTenantIds.MUSG, roles: Roles.Stammdatenverwalter));
        _muGoldachStammdatenVerwalterClient = new Lazy<HttpClient>(() => CreateHttpClient(tenant: MockedTenantIds.MUGOLDACH, roles: Roles.Stammdatenverwalter));
        _muSgKontrollzeichenerstellerClient = new Lazy<HttpClient>(() => CreateHttpClient(tenant: MockedTenantIds.MUSG, roles: Roles.Kontrollzeichenerfasser));
        _muGoldachKontrollzeichenerstellerClient = new Lazy<HttpClient>(() => CreateHttpClient(tenant: MockedTenantIds.MUGOLDACH, roles: Roles.Kontrollzeichenerfasser));
        _ctSgZertifikatsverwalterClient = new Lazy<HttpClient>(() => CreateHttpClient(tenant: MockedTenantIds.KTSG, roles: Roles.Zertifikatsverwalter));
        _muSgZertifikatsverwalterClient = new Lazy<HttpClient>(() => CreateHttpClient(tenant: MockedTenantIds.MUSG, roles: Roles.Zertifikatsverwalter));
    }

    protected HttpClient CtStammdatenverwalterClient => _ctStammdatenVerwalterClient.Value;

    protected HttpClient MuSgStammdatenverwalterClient => _muSgStammdatenVerwalterClient.Value;

    protected HttpClient MuGoldachStammdatenverwalterClient => _muGoldachStammdatenVerwalterClient.Value;

    protected HttpClient MuSgKontrollzeichenerstellerClient => _muSgKontrollzeichenerstellerClient.Value;

    protected HttpClient MuGoldachKontrollzeichenerstellerClient => _muGoldachKontrollzeichenerstellerClient.Value;

    protected HttpClient CtSgZertifikatsverwalterClient => _ctSgZertifikatsverwalterClient.Value;

    protected HttpClient MuSgZertifikatsverwalterClient => _muSgZertifikatsverwalterClient.Value;

    protected Task RunOnDb(Func<MigrationDataContext, Task> action)
        => RunScoped(action);

    protected Task<TResult> RunOnDb<TResult>(Func<MigrationDataContext, Task<TResult>> action)
        => RunScoped(action);

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

    protected void ResetDb()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MigrationDataContext>();
        DatabaseUtil.Truncate(db);
    }

    protected abstract IEnumerable<string> AuthorizedRoles();

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        return Roles
            .All()
            .Append(NoRole)
            .Except(AuthorizedRoles());
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

    protected async Task<IReadOnlyDictionary<string, string>> AssertZipDownloadAsStringEntries(Func<Task<HttpResponseMessage>> apiCall, string expectedFileName)
    {
        var resp = await AssertDownload(apiCall, MediaTypeNames.Application.Zip, ZipExtension, expectedFileName);
        using var archive = new ZipArchive(await resp.Content.ReadAsStreamAsync());

        var result = new Dictionary<string, string>();
        foreach (var entry in archive.Entries.OrderBy(x => x.FullName))
        {
            using var reader = new StreamReader(entry.Open(), Encoding.UTF8, leaveOpen: false);
            result.Add(entry.FullName, await reader.ReadToEndAsync());
        }

        return result;
    }

    private async Task<HttpResponseMessage> AssertDownload(Func<Task<HttpResponseMessage>> apiCall, string expectedMediaType, string fileExtension, string expectedFileName)
    {
        var response = await AssertStatus(apiCall, HttpStatusCode.OK);

        response.Content.Headers.ContentType!.MediaType.Should().Be(expectedMediaType);

        var contentDisposition = response.Content.Headers.ContentDisposition;
        contentDisposition!.FileNameStar.Should().EndWith(fileExtension);
        contentDisposition.FileNameStar.Should().Be(expectedFileName);
        contentDisposition.DispositionType.Should().Be("attachment");

        return response;
    }
}
