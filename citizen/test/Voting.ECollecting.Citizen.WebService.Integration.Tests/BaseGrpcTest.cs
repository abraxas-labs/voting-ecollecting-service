// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq.Expressions;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voting.ECollecting.Citizen.Core.Configuration;
using Voting.ECollecting.Citizen.WebService.Integration.Tests.Mocks;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Migrations;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Models;
using Voting.ECollecting.Shared.Test.Utils;
using Voting.Lib.Database.Models;
using Voting.Lib.Testing;
using Voting.Lib.UserNotifications;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests;

public abstract class BaseGrpcTest<TService> : GrpcApiBaseTest<TestApplicationFactory, TestStartup>
    where TService : ClientBase<TService>
{
    private readonly Lazy<TService> _client;
    private readonly Lazy<TService> _authenticatedClient;
    private readonly Lazy<TService> _authenticatedNoPermissionClient;
    private readonly Lazy<TService> _deputyClient;
    private readonly Lazy<TService> _deputyNotAcceptedClient;
    private readonly Lazy<TService> _readerClient;
    private readonly Lazy<TService> _readerNotAcceptedClient;

    protected BaseGrpcTest(TestApplicationFactory factory)
        : base(factory)
    {
        ResetDb();

        _client = new Lazy<TService>(() => CreateService(CreateGrpcChannel(authorize: false)));
        _authenticatedClient = new Lazy<TService>(() => CreateService(CreateGrpcChannel(authorize: true)));
        _authenticatedNoPermissionClient = new Lazy<TService>(() => CreateService(CreateGrpcChannel(authorize: true, userId: CitizenAuthMockDefaults.NoPermissionUserId)));
        _deputyClient = new Lazy<TService>(() => CreateService(CreateGrpcChannel(authorize: true, userId: CitizenAuthMockDefaults.DeputyUserId)));
        _deputyNotAcceptedClient = new Lazy<TService>(() => CreateService(CreateGrpcChannel(authorize: true, userId: CitizenAuthMockDefaults.DeputyNotAcceptedUserId)));
        _readerClient = new Lazy<TService>(() => CreateService(CreateGrpcChannel(authorize: true, userId: CitizenAuthMockDefaults.ReaderUserId)));
        _readerNotAcceptedClient = new Lazy<TService>(() => CreateService(CreateGrpcChannel(authorize: true, userId: CitizenAuthMockDefaults.ReaderNotAcceptedUserId)));
    }

    protected TService Client => _client.Value;

    protected TService AuthenticatedClient => _authenticatedClient.Value;

    protected TService AuthenticatedNoPermissionClient => _authenticatedNoPermissionClient.Value;

    protected TService DeputyClient => _deputyClient.Value;

    protected TService DeputyNotAcceptedClient => _deputyNotAcceptedClient.Value;

    protected TService ReaderClient => _readerClient.Value;

    protected TService ReaderNotAcceptedClient => _readerNotAcceptedClient.Value;

    protected List<UserNotification> SentUserNotifications => GetService<UserNotificationSenderMock>().Sent;

    protected TService CreateCitizenClient(
        string userId = CitizenAuthMockDefaults.CitizenUserId,
        string acrValue = "",
        string email = CitizenAuthMockDefaults.UserCitizenTestEMail,
        string ssn = "")
    {
        return CreateService(Factory.CreateGrpcChannel(
            true,
            null,
            userId,
            [],
            [
                (CitizenAuthMockDefaults.UserAcrHeaderName, acrValue),
                (CitizenAuthMockDefaults.UserEMailHeaderName, email),
                (CitizenAuthMockDefaults.UserSocialSecurityNumberHeaderName, ssn)
            ]));
    }

    protected Task<TResult> RunOnDb<TResult>(Func<MigrationDataContext, Task<TResult>> action)
        => RunScoped(action);

    protected Task RunOnDb(Func<MigrationDataContext, Task> action)
        => RunScoped(action);

    protected async Task<TEntity> GetEntity<TEntity>(Guid id)
        where TEntity : BaseEntity
    {
        return await RunOnDb(db => db.Set<TEntity>().SingleAsync(x => x.Id == id));
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

    protected void ResetDb()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MigrationDataContext>();

        DatabaseUtil.Truncate(db);
    }

    protected async Task WithOnlyCtDomainOfInfluenceTypeEnabled(Func<Task> action)
        => await WithEnabledDomainOfInfluenceTypes([DomainOfInfluenceType.Ct], action);

    protected async Task WithEnabledDomainOfInfluenceTypes(HashSet<DomainOfInfluenceType> types, Func<Task> action)
    {
        var config = GetService<CoreAppConfig>();
        var oldEnabledDoiTypes = config.EnabledDomainOfInfluenceTypes;

        try
        {
            config.EnabledDomainOfInfluenceTypes = types;
            await action();
        }
        finally
        {
            config.EnabledDomainOfInfluenceTypes = oldEnabledDoiTypes;
        }
    }

    protected void ResetUserNotificationSender(bool failAttempts = false)
    {
        var senderMock = GetService<UserNotificationSenderMock>();
        senderMock.Sent.Clear();
        senderMock.FailSendAttempts = failAttempts;
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

    private TService CreateService(GrpcChannel channel)
    {
        return (TService)Activator.CreateInstance(typeof(TService), channel)!;
    }
}
