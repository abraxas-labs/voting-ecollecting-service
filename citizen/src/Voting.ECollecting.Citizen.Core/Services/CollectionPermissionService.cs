// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Npgsql;
using Voting.ECollecting.Citizen.Abstractions.Adapter.Data;
using Voting.ECollecting.Citizen.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Citizen.Abstractions.Core.Services;
using Voting.ECollecting.Citizen.Core.Configuration;
using Voting.ECollecting.Citizen.Core.Exceptions;
using Voting.ECollecting.Citizen.Core.Mappings;
using Voting.ECollecting.Citizen.Core.Permissions;
using Voting.ECollecting.Citizen.Domain.Models;
using Voting.ECollecting.Citizen.Domain.Queries;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;
using Voting.ECollecting.Shared.Domain.Models;
using Voting.ECollecting.Shared.Domain.Queries;
using Voting.Lib.Common;
using IPermissionService = Voting.ECollecting.Citizen.Abstractions.Adapter.ELogin.IPermissionService;
using IUserNotificationService = Voting.ECollecting.Shared.Abstractions.Core.Services.IUserNotificationService;

namespace Voting.ECollecting.Citizen.Core.Services;

// collections can still be edited even if collection is not in pre recorded / in preparation state anymore.
public class CollectionPermissionService : ICollectionPermissionService
{
    private const string IamUserIdUniqueConstraintName = "IX_CollectionPermissions_CollectionId_IamUserId";

    private readonly CoreAppConfig _config;
    private readonly IUserNotificationService _userNotificationService;
    private readonly ICollectionPermissionRepository _collectionPermissionRepository;
    private readonly IPermissionService _permissionService;
    private readonly ICollectionRepository _collectionRepository;
    private readonly IDataContext _dataContext;
    private readonly TimeProvider _timeProvider;

    public CollectionPermissionService(
        IUserNotificationService userNotificationService,
        ICollectionPermissionRepository collectionPermissionRepository,
        IPermissionService permissionService,
        ICollectionRepository collectionRepository,
        CoreAppConfig config,
        IDataContext dataContext,
        TimeProvider timeProvider)
    {
        _userNotificationService = userNotificationService;
        _collectionPermissionRepository = collectionPermissionRepository;
        _permissionService = permissionService;
        _collectionRepository = collectionRepository;
        _config = config;
        _dataContext = dataContext;
        _timeProvider = timeProvider;
    }

    public async Task<List<CollectionPermission>> ListPermissions(Guid collectionId)
    {
        var permissionEntities = await _collectionRepository.Query()
            .Where(x => x.Id == collectionId)
            .WhereCanReadPermissions(_permissionService)
            .SelectMany(x => x.Permissions!)
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToListAsync();
        var permissions = Mapper.MapToCollectionPermissions(permissionEntities);
        SetUserPermissions(permissions);
        return permissions;
    }

    public async Task<Guid> CreatePermission(
        Guid collectionId,
        string firstName,
        string lastName,
        string email,
        CollectionPermissionRole role,
        CancellationToken ct)
    {
        var collection = await _collectionRepository.Query()
                             .WhereCanEditPermissions(_permissionService)
                             .FirstOrDefaultAsync(x => x.Id == collectionId, ct)
                         ?? throw new EntityNotFoundException(nameof(CollectionBaseEntity), collectionId);

        var permission = await CreatePermissionInternal(collection, firstName, lastName, email, role);
        await _userNotificationService.SendUserNotification(
            email,
            true,
            UserNotificationType.PermissionAdded,
            new UserNotificationContext(Collection: collection, PermissionToken: permission.Token),
            cancellationToken: ct);
        return permission.Id;
    }

    public async Task DeletePermission(Guid id)
    {
        if (await _collectionRepository.Query()
                .WhereCanEditPermissions(_permissionService)
                .SelectMany(x => x.Permissions!)
                .AnyAsync(x => x.Id == id && x.IamUserId == _permissionService.UserId))
        {
            throw new CannotDeleteOwnPermissionException();
        }

        var existing = await _collectionRepository.Query()
            .WhereCanEditPermissions(_permissionService)
            .SelectMany(x => x.Permissions!)
            .WhereCanDeletePermission(_permissionService)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new EntityNotFoundException(nameof(CollectionPermissionEntity), id);

        await _collectionPermissionRepository.AuditedDelete(existing);
    }

    public async Task ResendPermission(Guid id, CancellationToken ct)
    {
        var collection = await _collectionRepository.Query()
                             .AsTracking()
                             .WhereCanEditPermissions(_permissionService)
                             .WhereHasPendingOrRejectedOrExpiredPermission(id)
                             .IncludePendingOrRejectedOrExpiredPermission(id)
                             .FirstOrDefaultAsync(ct)
                         ?? throw new EntityNotFoundException(nameof(CollectionPermissionEntity), id);

        var permission = collection.Permissions!.Single();
        permission.State = CollectionPermissionState.Pending;
        permission.Token = UrlToken.New();
        permission.TokenExpiry = _timeProvider.GetUtcNowDateTime() + _config.PermissionTokenLifetime;
        await _dataContext.SaveChangesAsync();

        await _userNotificationService.SendUserNotification(
            permission.Email,
            true,
            UserNotificationType.PermissionAdded,
            new UserNotificationContext(Collection: collection, PermissionToken: permission.Token),
            cancellationToken: ct);
    }

    public async Task<PendingCollectionPermission> GetPendingByTokenInclCollection(UrlToken token)
    {
        return await _collectionPermissionRepository.Query()
                   .WhereIsPendingAndCanReadWithToken(_permissionService, token)
                   .Select(x => new PendingCollectionPermission(
                       x.CollectionId,
                       x.Collection.Type,
                       x.Collection.Description,
                       x.AuditInfo.CreatedByName,
                       x.LastName,
                       x.FirstName,
                       x.Role,
                       _config.Acr.AcceptPermission))
                   .FirstOrDefaultAsync()
               ?? throw new EntityNotFoundException(nameof(CollectionPermissionEntity), token);
    }

    public async Task AcceptByToken(UrlToken token)
    {
        var permission = await _collectionPermissionRepository.Query()
                             .WhereIsPendingAndCanReadWithToken(_permissionService, token)
                             .AsTracking()
                             .FirstOrDefaultAsync()
                         ?? throw new EntityNotFoundException(nameof(CollectionPermissionEntity), token);

        if (!await _collectionRepository
                .Query()
                .WhereIsNotEndedAndNotAborted()
                .AnyAsync(x => x.Id == permission.CollectionId))
        {
            throw new EntityNotFoundException(nameof(CollectionBaseEntity), permission.CollectionId);
        }

        _permissionService.RequireEmail(permission.Email);
        permission.IamUserId = _permissionService.UserId;
        permission.IamLastName = _permissionService.UserLastName;
        permission.IamFirstName = _permissionService.UserFirstName;
        permission.State = CollectionPermissionState.Accepted;
        permission.Token = null;
        permission.TokenExpiry = null;
        _permissionService.SetModified(permission);

        try
        {
            await _dataContext.SaveChangesAsync();
        }
        catch (Exception e) when (e.InnerException is PostgresException { ConstraintName: IamUserIdUniqueConstraintName })
        {
            throw new UserHasAlreadyAPermissionException();
        }
    }

    public async Task RejectByToken(UrlToken token)
    {
        var permission = await _collectionPermissionRepository.Query()
                             .WhereIsPendingAndCanReadWithToken(_permissionService, token)
                             .AsTracking()
                             .FirstOrDefaultAsync()
                         ?? throw new EntityNotFoundException(nameof(CollectionPermissionEntity), token);

        if (!await _collectionRepository
                .Query()
                .WhereIsNotEndedAndNotAborted()
                .AnyAsync(x => x.Id == permission.CollectionId))
        {
            throw new EntityNotFoundException(nameof(CollectionBaseEntity), permission.CollectionId);
        }

        permission.State = CollectionPermissionState.Rejected;
        permission.TokenExpiry = null;
        permission.AuditInfo.ModifiedAt = _timeProvider.GetUtcNowDateTime();
        await _dataContext.SaveChangesAsync();
    }

    public async Task UpdateIamInfo()
    {
        if (string.IsNullOrWhiteSpace(_permissionService.UserEmail)
            || !_permissionService.UserEmailVerified)
        {
            return;
        }

        await _collectionPermissionRepository.AuditedUpdateRange(BuildQuery, UpdateAction);
        return;

        IQueryable<CollectionPermissionEntity> BuildQuery(IQueryable<CollectionPermissionEntity> q)
            => q.Where(x =>
                x.IamUserId == _permissionService.UserId
                && (x.Email != _permissionService.UserEmail
                    || x.IamFirstName != _permissionService.UserFirstName
                    || x.IamLastName != _permissionService.UserLastName));

        void UpdateAction(CollectionPermissionEntity p)
        {
            _permissionService.SetModified(p);
            p.Email = _permissionService.UserEmail!;
            p.IamFirstName = _permissionService.UserFirstName;
            p.IamLastName = _permissionService.UserLastName;
        }
    }

    internal async Task<CollectionPermissionEntity> CreatePermissionInternal(
        CollectionBaseEntity collection,
        string firstName,
        string lastName,
        string email,
        CollectionPermissionRole role)
    {
        if (role is CollectionPermissionRole.Owner)
        {
            throw new CannotAddOwnerPermissionException();
        }

        if (string.Equals(collection.AuditInfo.CreatedByEmail, email, StringComparison.Ordinal))
        {
            throw new CannotAddOwnerPermissionException();
        }

        // this is a simple check to avoid duplicate permissions
        // this unique check is not mission forced and therefore not enforced by the DB.
        // duplicates can still be created by concurrent requests and/or users updating their emails in the IAM.
        var permissionAlreadyExists = await _collectionPermissionRepository.Query()
            .AnyAsync(x => x.CollectionId == collection.Id && x.Email == email);
        if (permissionAlreadyExists)
        {
            throw new CollectionPermissionAlreadyExistsException(collection.Id, email);
        }

        var permission = new CollectionPermissionEntity
        {
            CollectionId = collection.Id,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Role = role,
            Token = UrlToken.New(),
            TokenExpiry = _timeProvider.GetUtcNowDateTime() + _config.PermissionTokenLifetime,
        };

        _permissionService.SetCreated(permission);
        await _collectionPermissionRepository.Create(permission);
        return permission;
    }

    private void SetUserPermissions(IEnumerable<CollectionPermission> permissions)
    {
        foreach (var permission in permissions)
        {
            permission.UserPermissions = CollectionPermissionPermissions.Build(permission);
        }
    }
}
