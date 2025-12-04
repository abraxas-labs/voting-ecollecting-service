// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Core.Permissions;
using Voting.ECollecting.Admin.Core.Resources;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Exceptions;
using IPermissionService = Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam.IPermissionService;

namespace Voting.ECollecting.Admin.Core.Services;

public class CollectionFilesService : ICollectionFilesService
{
    private readonly IFileRepository _fileRepository;
    private readonly IDataContext _dataContext;
    private readonly IPermissionService _permissionService;
    private readonly ICollectionMessageRepository _collectionMessageRepository;
    private readonly IUserNotificationService _coreUserNotificationService;
    private readonly ICollectionRepository _collectionRepository;
    private readonly ICollectionSignatureSheetGenerationService _collectionSignatureSheetGenerationService;

    public CollectionFilesService(
        IFileRepository fileRepository,
        IDataContext dataContext,
        IPermissionService permissionService,
        ICollectionMessageRepository collectionMessageRepository,
        IUserNotificationService coreUserNotificationService,
        ICollectionRepository collectionRepository,
        ICollectionSignatureSheetGenerationService collectionSignatureSheetGenerationService)
    {
        _fileRepository = fileRepository;
        _dataContext = dataContext;
        _permissionService = permissionService;
        _collectionMessageRepository = collectionMessageRepository;
        _coreUserNotificationService = coreUserNotificationService;
        _collectionRepository = collectionRepository;
        _collectionSignatureSheetGenerationService = collectionSignatureSheetGenerationService;
    }

    public async Task DeleteImage(Guid collectionId)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var collection = await _collectionRepository.Query()
                             .WhereCanEdit(_permissionService)
                             .AsTracking()
                             .Include(x => x.Permissions)
                             .FirstOrDefaultAsync(x => x.Id == collectionId)
                         ?? throw new EntityNotFoundException(typeof(CollectionBaseEntity), collectionId);

        if (!collection.ImageId.HasValue)
        {
            return;
        }

        await _fileRepository.Query()
            .Where(x => x.Id == collection.ImageId)
            .ExecuteDeleteAsync();

        _permissionService.SetModified(collection);
        collection.ImageId = null;
        await _dataContext.SaveChangesAsync();

        if (collection.SignatureSheetTemplateGenerated)
        {
            collection.Image = null;
            await GenerateSignatureSheetTemplate(collection);
        }

        await AddFileDeletedMessage(collection, Strings.UserNotification_ImageDeleted);

        await transaction.CommitAsync();
    }

    public async Task DeleteLogo(Guid collectionId)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var collection = await _collectionRepository.Query()
                             .WhereCanEdit(_permissionService)
                             .AsTracking()
                             .Include(x => x.Permissions)
                             .FirstOrDefaultAsync(x => x.Id == collectionId)
                         ?? throw new EntityNotFoundException(typeof(CollectionBaseEntity), collectionId);

        if (!collection.LogoId.HasValue)
        {
            return;
        }

        await _fileRepository.Query()
            .Where(x => x.Id == collection.LogoId)
            .ExecuteDeleteAsync();

        collection.LogoId = null;
        _permissionService.SetModified(collection);
        await _dataContext.SaveChangesAsync();

        if (collection.SignatureSheetTemplateGenerated)
        {
            collection.Logo = null;
            await GenerateSignatureSheetTemplate(collection);
        }

        await AddFileDeletedMessage(collection, Strings.UserNotification_LogoDeleted);

        await transaction.CommitAsync();
    }

    public async Task<FileEntity> GetImage(Guid collectionId)
    {
        // file is small, ok to buffer in memory.
        return await _collectionRepository.Query()
                   .WhereCanRead(_permissionService)
                   .Where(x => x.Id == collectionId)
                   .Include(x => x.Image!.Content)
                   .Select(x => x.Image!)
                   .FirstOrDefaultAsync()
               ?? throw new EntityNotFoundException(nameof(CollectionBaseEntity), collectionId);
    }

    public async Task<FileEntity> GetLogo(Guid collectionId)
    {
        // file is small, ok to buffer in memory.
        return await _collectionRepository.Query()
                   .WhereCanRead(_permissionService)
                   .Where(x => x.Id == collectionId)
                   .Include(x => x.Logo!.Content)
                   .Select(x => x.Logo!)
                   .FirstOrDefaultAsync()
               ?? throw new EntityNotFoundException(nameof(CollectionBaseEntity), collectionId);
    }

    public async Task<FileEntity> GetSignatureSheetTemplate(Guid collectionId)
    {
        // file is small, ok to buffer in memory.
        return await _collectionRepository.Query()
                   .WhereCanRead(_permissionService)
                   .Where(x => x.Id == collectionId)
                   .Include(x => x.SignatureSheetTemplate!.Content)
                   .Select(x => x.SignatureSheetTemplate!)
                   .FirstOrDefaultAsync()
               ?? throw new EntityNotFoundException(nameof(CollectionBaseEntity), collectionId);
    }

    public async Task DeleteSignatureSheetTemplate(Guid collectionId)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var collection = await _collectionRepository.Query()
                             .WhereCanEdit(_permissionService)
                             .AsTracking()
                             .Include(x => x.Permissions)
                             .FirstOrDefaultAsync(x => x.Id == collectionId)
                         ?? throw new EntityNotFoundException(typeof(CollectionBaseEntity), collectionId);

        if (!collection.SignatureSheetTemplateId.HasValue)
        {
            return;
        }

        await GenerateSignatureSheetTemplate(collection);
        collection.SignatureSheetTemplateGenerated = true;

        _permissionService.SetModified(collection);
        await _dataContext.SaveChangesAsync();

        await AddFileDeletedMessage(collection, Strings.UserNotification_SignatureSheetDeleted);

        await transaction.CommitAsync();
    }

    private async Task GenerateSignatureSheetTemplate(CollectionBaseEntity collection)
    {
        var oldSignatureSheetTemplateId = collection.SignatureSheetTemplateId;

        collection.SignatureSheetTemplateId = null;
        collection.SignatureSheetTemplate = await _collectionSignatureSheetGenerationService.GenerateSignatureSheetFile(collection.Id, collection.Type);

        _permissionService.SetCreated(collection.SignatureSheetTemplate);
        await _dataContext.SaveChangesAsync();

        if (oldSignatureSheetTemplateId.HasValue)
        {
            await _fileRepository.Query()
                .Where(x => x.Id == oldSignatureSheetTemplateId.Value)
                .ExecuteDeleteAsync();
        }
    }

    private async Task AddFileDeletedMessage(CollectionBaseEntity collection, string content)
    {
        var msg = new CollectionMessageEntity { Content = content, CollectionId = collection.Id };
        _permissionService.SetCreated(msg);
        await _collectionMessageRepository.Create(msg);
        await _coreUserNotificationService.ScheduleNotification(collection, UserNotificationType.MessageAdded);
    }
}
