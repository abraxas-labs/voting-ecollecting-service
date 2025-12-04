// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq.Expressions;
using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
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

public abstract class BaseGrpcTest<TService> : GrpcAuthorizationBaseTest<TestApplicationFactory, TestStartup>
    where TService : ClientBase<TService>
{
    private readonly Lazy<TService> _apiNotifyClient;

    private readonly Lazy<TService> _ctSgStammdatenverwalterClient;
    private readonly Lazy<TService> _ctSgKontrollzeichenerfasserClient;
    private readonly Lazy<TService> _ctSgKontrollzeichenloescherClient;
    private readonly Lazy<TService> _ctSgStichprobenverwalterClient;

    private readonly Lazy<TService> _muSgStammdatenverwalterClient;
    private readonly Lazy<TService> _muGoldachStammdatenverwalterClient;
    private readonly Lazy<TService> _muSgKontrollzeichenerfasserClient;
    private readonly Lazy<TService> _muSgKontrollzeichenloescherClient;
    private readonly Lazy<TService> _muGoldachKontrollzeichenerfasserClient;
    private readonly Lazy<TService> _muGoldachKontrollzeichenloescherClient;
    private readonly Lazy<TService> _muSgStichprobenverwalterClient;
    private readonly Lazy<TService> _muGoldachStichprobenverwalterClient;
    private readonly Lazy<TService> _ctSgZertifikatsverwalterClient;
    private readonly Lazy<TService> _muSgZertifikatsverwalterClient;

    protected BaseGrpcTest(TestApplicationFactory factory)
        : base(factory)
    {
        ResetDb();
        GetService<SecondFactorTransactionServiceMock>().Reset();

        _apiNotifyClient = new Lazy<TService>(() =>
            CreateService(CreateGrpcChannel(tenant: MockedTenantIds.Abraxas, roles: Roles.ApiNotify)));

        _ctSgStammdatenverwalterClient = new Lazy<TService>(() =>
            CreateService(CreateGrpcChannel(tenant: MockedTenantIds.KTSG, roles: Roles.Stammdatenverwalter)));

        _muSgStammdatenverwalterClient = new Lazy<TService>(() =>
            CreateService(CreateGrpcChannel(tenant: MockedTenantIds.MUSG, roles: Roles.Stammdatenverwalter)));

        _muGoldachStammdatenverwalterClient = new Lazy<TService>(() =>
            CreateService(CreateGrpcChannel(tenant: MockedTenantIds.MUGOLDACH, roles: Roles.Stammdatenverwalter)));

        _ctSgKontrollzeichenerfasserClient = new Lazy<TService>(() =>
            CreateService(CreateGrpcChannel(tenant: MockedTenantIds.KTSG, roles: Roles.Kontrollzeichenerfasser)));

        _ctSgKontrollzeichenloescherClient = new Lazy<TService>(() =>
            CreateService(CreateGrpcChannel(tenant: MockedTenantIds.KTSG, roles: Roles.Kontrollzeichenloescher)));

        _muSgKontrollzeichenerfasserClient = new Lazy<TService>(() =>
            CreateService(CreateGrpcChannel(tenant: MockedTenantIds.MUSG, roles: Roles.Kontrollzeichenerfasser)));

        _muSgKontrollzeichenloescherClient = new Lazy<TService>(() =>
            CreateService(CreateGrpcChannel(tenant: MockedTenantIds.MUSG, roles: Roles.Kontrollzeichenloescher)));

        _muGoldachKontrollzeichenerfasserClient = new Lazy<TService>(() =>
            CreateService(CreateGrpcChannel(tenant: MockedTenantIds.MUGOLDACH, roles: Roles.Kontrollzeichenerfasser)));

        _muGoldachKontrollzeichenloescherClient = new Lazy<TService>(() =>
            CreateService(CreateGrpcChannel(tenant: MockedTenantIds.MUGOLDACH, roles: Roles.Kontrollzeichenloescher)));

        _ctSgZertifikatsverwalterClient = new Lazy<TService>(() =>
            CreateService(CreateGrpcChannel(tenant: MockedTenantIds.KTSG, roles: Roles.Zertifikatsverwalter)));

        _muSgZertifikatsverwalterClient = new Lazy<TService>(() =>
            CreateService(CreateGrpcChannel(tenant: MockedTenantIds.MUSG, roles: Roles.Zertifikatsverwalter)));

        _ctSgStichprobenverwalterClient = new Lazy<TService>(() =>
            CreateService(CreateGrpcChannel(tenant: MockedTenantIds.KTSG, roles: Roles.Stichprobenverwalter)));

        _muSgStichprobenverwalterClient = new Lazy<TService>(() =>
            CreateService(CreateGrpcChannel(tenant: MockedTenantIds.MUSG, roles: Roles.Stichprobenverwalter)));

        _muGoldachStichprobenverwalterClient = new Lazy<TService>(() =>
            CreateService(CreateGrpcChannel(tenant: MockedTenantIds.MUGOLDACH, roles: Roles.Stichprobenverwalter)));
    }

    protected TService ApiNotifyClient => _apiNotifyClient.Value;

    protected TService CtSgStammdatenverwalterClient => _ctSgStammdatenverwalterClient.Value;

    protected TService MuSgStammdatenverwalterClient => _muSgStammdatenverwalterClient.Value;

    protected TService MuGoldachStammdatenverwalterClient => _muGoldachStammdatenverwalterClient.Value;

    protected TService CtSgKontrollzeichenerfasserClient => _ctSgKontrollzeichenerfasserClient.Value;

    protected TService CtSgKontrollzeichenloescherClient => _ctSgKontrollzeichenloescherClient.Value;

    protected TService MuSgKontrollzeichenerfasserClient => _muSgKontrollzeichenerfasserClient.Value;

    protected TService MuSgKontrollzeichenloescherClient => _muSgKontrollzeichenloescherClient.Value;

    protected TService MuGoldachKontrollzeichenerfasserClient => _muGoldachKontrollzeichenerfasserClient.Value;

    protected TService MuGoldachKontrollzeichenloescherClient => _muGoldachKontrollzeichenloescherClient.Value;

    protected TService CtSgZertifikatsverwalterClient => _ctSgZertifikatsverwalterClient.Value;

    protected TService MuSgZertifikatsverwalterClient => _muSgZertifikatsverwalterClient.Value;

    protected TService CtSgStichprobenverwalterClient => _ctSgStichprobenverwalterClient.Value;

    protected TService MuSgStichprobenverwalterClient => _muSgStichprobenverwalterClient.Value;

    protected TService MuGoldachStichprobenverwalterClient => _muGoldachStichprobenverwalterClient.Value;

    protected Task<TResult> RunOnDb<TResult>(Func<MigrationDataContext, Task<TResult>> action)
        => RunScoped(action);

    protected Task RunOnDb(Func<MigrationDataContext, Task> action)
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

    protected void ResetUserNotificationSender(bool failAttempts = false)
    {
        var senderMock = GetService<UserNotificationSenderMock>();
        senderMock.Sent.Clear();
        senderMock.FailSendAttempts = failAttempts;
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

    protected abstract IEnumerable<string> AuthorizedRoles();

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        return Roles
            .All()
            .Append(NoRole)
            .Except(AuthorizedRoles());
    }

    private TService CreateService(GrpcChannel channel)
    {
        return (TService)Activator.CreateInstance(typeof(TService), channel)!;
    }
}
