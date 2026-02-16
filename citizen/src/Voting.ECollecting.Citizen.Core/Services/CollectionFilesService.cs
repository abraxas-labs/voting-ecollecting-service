// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voting.ECollecting.Citizen.Abstractions.Adapter.Data;
using Voting.ECollecting.Citizen.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Citizen.Abstractions.Core.Services;
using Voting.ECollecting.Citizen.Core.Configuration;
using Voting.ECollecting.Citizen.Core.Permissions;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;
using Voting.ECollecting.Shared.Domain.Models;
using Voting.ECollecting.Shared.Domain.Queries;
using Voting.Lib.Common.Files;
using IPermissionService = Voting.ECollecting.Citizen.Abstractions.Adapter.ELogin.IPermissionService;

namespace Voting.ECollecting.Citizen.Core.Services;

public class CollectionFilesService : ICollectionFilesService
{
    private readonly IFileRepository _fileRepository;
    private readonly CoreAppConfig _config;
    private readonly IDataContext _dataContext;
    private readonly IPermissionService _permissionService;
    private readonly ICollectionRepository _collectionRepository;
    private readonly IFileService _fileService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ICollectionSignatureSheetGenerationService _collectionSignatureSheetGenerationService;
    private readonly IElectronicSignaturesProtocolGenerator _electronicSignaturesProtocolGenerator;
    private readonly IAccessControlListDoiService _accessControlListDoiService;
    private readonly IInitiativeSubTypeRepository _initiativeSubTypeRepository;

    public CollectionFilesService(
        IFileRepository fileRepository,
        CoreAppConfig config,
        IDataContext dataContext,
        IPermissionService permissionService,
        ICollectionRepository collectionRepository,
        IFileService fileService,
        IServiceProvider serviceProvider,
        ICollectionSignatureSheetGenerationService collectionSignatureSheetGenerationService,
        IElectronicSignaturesProtocolGenerator electronicSignaturesProtocolGenerator,
        IAccessControlListDoiService accessControlListDoiService,
        IInitiativeSubTypeRepository initiativeSubTypeRepository)
    {
        _fileRepository = fileRepository;
        _config = config;
        _dataContext = dataContext;
        _permissionService = permissionService;
        _collectionRepository = collectionRepository;
        _fileService = fileService;
        _serviceProvider = serviceProvider;
        _collectionSignatureSheetGenerationService = collectionSignatureSheetGenerationService;
        _electronicSignaturesProtocolGenerator = electronicSignaturesProtocolGenerator;
        _accessControlListDoiService = accessControlListDoiService;
        _initiativeSubTypeRepository = initiativeSubTypeRepository;
    }

    public async Task DeleteImage(Guid collectionId)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var collection = await _collectionRepository.Query()
                             .WhereCanEdit(_permissionService)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Id == collectionId)
                         ?? throw new EntityNotFoundException(nameof(CollectionBaseEntity), collectionId);

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

        await transaction.CommitAsync();
    }

    public async Task DeleteLogo(Guid collectionId)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var collection = await _collectionRepository.Query()
                             .WhereCanEdit(_permissionService)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Id == collectionId)
                         ?? throw new EntityNotFoundException(nameof(CollectionBaseEntity), collectionId);

        if (!collection.LogoId.HasValue)
        {
            return;
        }

        await _fileRepository.Query()
            .Where(x => x.Id == collection.LogoId)
            .ExecuteDeleteAsync();

        _permissionService.SetModified(collection);
        collection.LogoId = null;
        await _dataContext.SaveChangesAsync();

        await transaction.CommitAsync();
    }

    public async Task UpdateImage(Guid id, Stream image, string? contentType, string? fileName, CancellationToken ct)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var collection = await _collectionRepository.Query()
                             .WhereCanEdit(_permissionService)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Id == id, ct)
                         ?? throw new EntityNotFoundException(nameof(CollectionBaseEntity), id);

        var oldImageFileId = collection.ImageId;

        collection.ImageId = null;
        collection.Image = await _fileService.Validate(image, contentType, fileName, _config.AllowedImageFileExtensions, ct: ct);
        _permissionService.SetModified(collection);
        _permissionService.SetCreated(collection.Image);
        await _dataContext.SaveChangesAsync();

        if (oldImageFileId.HasValue)
        {
            await _fileRepository.Query()
                .Where(x => x.Id == oldImageFileId)
                .ExecuteDeleteAsync(ct);
        }

        await transaction.CommitAsync(ct);
    }

    public async Task<FileEntity> GetImage(Guid id)
    {
        // file is small, ok to buffer in memory.
        return await _collectionRepository.Query()
                   .WhereCanReadOrIsPastRegistered(_permissionService)
                   .Where(x => x.Id == id)
                   .Include(x => x.Image!.Content)
                   .Select(x => x.Image!)
                   .FirstOrDefaultAsync()
               ?? throw new EntityNotFoundException(nameof(CollectionBaseEntity), id);
    }

    public async Task UpdateLogo(Guid id, Stream logo, string? contentType, string? fileName, CancellationToken ct)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var collection = await _collectionRepository.Query()
                             .WhereCanEdit(_permissionService)
                             .Include(x => x.Logo)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Id == id, ct)
                         ?? throw new EntityNotFoundException(nameof(CollectionBaseEntity), id);

        var oldLogoFileId = collection.LogoId;
        collection.LogoId = null;
        collection.Logo = await _fileService.Validate(logo, contentType, fileName, _config.AllowedImageFileExtensions, ct: ct);
        _permissionService.SetModified(collection);
        _permissionService.SetCreated(collection.Logo);
        await _dataContext.SaveChangesAsync();

        if (oldLogoFileId.HasValue)
        {
            await _fileRepository.Query()
                .Where(x => x.Id == oldLogoFileId)
                .ExecuteDeleteAsync(ct);
        }

        await transaction.CommitAsync(ct);
    }

    public async Task<FileEntity> GetLogo(Guid id)
    {
        // file is small, ok to buffer in memory.
        return await _collectionRepository.Query()
                   .WhereCanReadOrIsPastRegistered(_permissionService)
                   .Where(x => x.Id == id)
                   .Include(x => x.Logo!.Content)
                   .Select(x => x.Logo!)
                   .FirstOrDefaultAsync()
               ?? throw new EntityNotFoundException(nameof(CollectionBaseEntity), id);
    }

    public async Task SetSignatureSheetTemplateGenerated(Guid id, CollectionType collectionType)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var collection = await _collectionRepository.Query()
                             .WhereCanEditSignatureSheetTemplate(_permissionService)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(nameof(CollectionBaseEntity), id);

        var oldSignatureSheetTemplateId = collection.SignatureSheetTemplateId;
        collection.SignatureSheetTemplateId = null;
        collection.SignatureSheetTemplateGenerated = true;
        _permissionService.SetModified(collection);
        await _dataContext.SaveChangesAsync();

        if (oldSignatureSheetTemplateId.HasValue)
        {
            await _fileRepository.Query()
                .Where(x => x.Id == oldSignatureSheetTemplateId.Value)
                .ExecuteDeleteAsync();
        }

        await GenerateSignatureSheetTemplate(collection);
        await transaction.CommitAsync();
    }

    public async Task<FileEntity> GetSignatureSheetTemplate(Guid id, bool requireEnabledForSubmission)
    {
        var query = _collectionRepository.Query();
        query = requireEnabledForSubmission
            ? query.WhereIsEnabledForCollection()
            : query.WhereCanRead(_permissionService);

        // file is small, ok to buffer in memory.
        return await query
                   .Where(x => x.Id == id)
                   .Include(x => x.SignatureSheetTemplate!.Content)
                   .Select(x => x.SignatureSheetTemplate!)
                   .FirstOrDefaultAsync()
               ?? throw new EntityNotFoundException(nameof(CollectionBaseEntity), id);
    }

    public async Task DeleteSignatureSheetTemplate(Guid id)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var collection = await _collectionRepository.Query()
                             .WhereCanDeleteSignatureSheetTemplate(_permissionService)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(nameof(CollectionBaseEntity), id);

        if (!collection.SignatureSheetTemplateId.HasValue)
        {
            return;
        }

        await _fileRepository.Query()
            .Where(x => x.Id == collection.SignatureSheetTemplateId.Value)
            .ExecuteDeleteAsync();

        collection.SignatureSheetTemplateId = null;
        _permissionService.SetModified(collection);

        await _dataContext.SaveChangesAsync();

        await transaction.CommitAsync();
    }

    public async Task UpdateSignatureSheetTemplate(Guid id, Stream file, string? contentType, string? fileName, CancellationToken ct)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var collection = await _collectionRepository.Query()
                             .WhereCanEditSignatureSheetTemplate(_permissionService)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Id == id, ct)
                         ?? throw new EntityNotFoundException(nameof(CollectionBaseEntity), id);

        var oldSignatureSheetTemplateId = collection.SignatureSheetTemplateId;
        collection.SignatureSheetTemplateGenerated = false;
        collection.SignatureSheetTemplateId = null;
        collection.SignatureSheetTemplate = await _fileService.Validate(file, contentType, fileName, _config.AllowedSignatureSheetFileExtensions, ct: ct);
        _permissionService.SetModified(collection);
        _permissionService.SetCreated(collection.SignatureSheetTemplate);
        await _dataContext.SaveChangesAsync();

        if (oldSignatureSheetTemplateId.HasValue)
        {
            await _fileRepository.Query()
                .Where(x => x.Id == oldSignatureSheetTemplateId.Value)
                .ExecuteDeleteAsync(ct);
        }

        await transaction.CommitAsync(ct);
    }

    public async Task GenerateSignatureSheetTemplatePreview(Guid id, CollectionType collectionType)
    {
        var collection = await _collectionRepository.Query()
                             .WhereCanGenerateSignatureSheetTemplatePreview(_permissionService)
                             .Include(x => x.SignatureSheetTemplate)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(nameof(CollectionBaseEntity), id);

        // don't pass the cancellation token to finish generating the preview
        // even if the user stops the request (e.g. navigates to another page)
        await using var transaction = await _dataContext.BeginTransaction();
        await GenerateSignatureSheetTemplate(collection);
        await transaction.CommitAsync();
    }

    public async Task<IFile> GetElectronicSignaturesProtocol(Guid collectionId, CancellationToken cancellationToken)
    {
        var collection = await _collectionRepository.Query()
            .WhereCanGetElectronicSignaturesProtocol(_permissionService)
            .Include(x => x.Municipalities)
            .Include(x => x.CollectionCount)
            .FirstOrDefaultAsync(x => x.Id == collectionId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(CollectionBaseEntity), collectionId);

        var accessControlListDoi = await _accessControlListDoiService.GetAccessControlListDoiWithChildren(collection.Bfs!);
        InitiativeSubTypeEntity? subType = null;
        if (collection is InitiativeEntity { SubTypeId: not null })
        {
            subType = await _initiativeSubTypeRepository.Query()
                .FirstOrDefaultAsync(x => x.Id == ((InitiativeEntity)collection).SubTypeId, cancellationToken);
        }

        var templateData = new ECollectingProtocolTemplateData(
            [collection],
            accessControlListDoi,
            collection.Description,
            collection.DomainOfInfluenceType!.Value,
            collection.Type == CollectionType.Referendum,
            subType);
        return await _electronicSignaturesProtocolGenerator.GenerateFileModel(templateData, cancellationToken);
    }

    internal async Task GenerateSignatureSheetTemplate(CollectionBaseEntity collection)
    {
        var validationService = _serviceProvider.GetRequiredKeyedService<ICollectionValidationService>(collection.Type);
        var validationResult = validationService.ValidateGeneralInformation(collection);
        if (!validationResult.IsValid)
        {
            throw new ValidationException("Collection must have all required general information.");
        }

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
}
