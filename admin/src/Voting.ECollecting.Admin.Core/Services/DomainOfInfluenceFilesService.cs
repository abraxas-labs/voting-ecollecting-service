// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Core.Configuration;
using Voting.ECollecting.Admin.Core.Permissions;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Exceptions;
using IPermissionService = Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam.IPermissionService;

namespace Voting.ECollecting.Admin.Core.Services;

public class DomainOfInfluenceFilesService : IDomainOfInfluenceFilesService
{
    private readonly IDataContext _dataContext;
    private readonly IFileRepository _fileRepository;
    private readonly IDomainOfInfluenceRepository _domainOfInfluenceRepository;
    private readonly IPermissionService _permissionService;
    private readonly IFileService _fileService;
    private readonly CoreAppConfig _config;

    public DomainOfInfluenceFilesService(
        IDataContext dataContext,
        IFileRepository fileRepository,
        IDomainOfInfluenceRepository domainOfInfluenceRepository,
        IPermissionService permissionService,
        IFileService fileService,
        CoreAppConfig config)
    {
        _dataContext = dataContext;
        _fileRepository = fileRepository;
        _domainOfInfluenceRepository = domainOfInfluenceRepository;
        _permissionService = permissionService;
        _fileService = fileService;
        _config = config;
    }

    public async Task DeleteLogo(string bfs)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var domainOfInfluence = await _domainOfInfluenceRepository.Query()
                             .WhereCanEdit(_permissionService)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Bfs == bfs)
                         ?? throw new EntityNotFoundException(nameof(DomainOfInfluenceEntity), bfs);

        if (!domainOfInfluence.LogoId.HasValue)
        {
            return;
        }

        _permissionService.SetModified(domainOfInfluence);
        await _dataContext.SaveChangesAsync();

        if (domainOfInfluence.LogoId.HasValue)
        {
            await _fileRepository.Query()
                .Where(x => x.Id == domainOfInfluence.LogoId)
                .ExecuteDeleteAsync();
        }

        await transaction.CommitAsync();
    }

    public async Task UpdateLogo(
        string bfs,
        Stream logo,
        string? contentType,
        string? fileName,
        CancellationToken ct)
    {
        await using var transaction = await _dataContext.BeginTransaction(ct);

        var domainOfInfluence = await _domainOfInfluenceRepository.Query()
                             .WhereCanEdit(_permissionService)
                             .Include(x => x.Logo)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Bfs == bfs, ct)
                         ?? throw new EntityNotFoundException(nameof(DomainOfInfluenceEntity), bfs);

        var oldLogoFileId = domainOfInfluence.LogoId;
        domainOfInfluence.LogoId = null;
        domainOfInfluence.Logo = await _fileService.Validate(logo, contentType, fileName, _config.AllowedImageFileExtensions, ct: ct);
        _permissionService.SetModified(domainOfInfluence);
        _permissionService.SetCreated(domainOfInfluence.Logo);
        await _dataContext.SaveChangesAsync();

        if (oldLogoFileId.HasValue)
        {
            await _fileRepository.Query()
                .Where(x => x.Id == oldLogoFileId)
                .ExecuteDeleteAsync(ct);
        }

        await transaction.CommitAsync(ct);
    }

    public async Task<FileEntity> GetLogo(string bfs)
    {
        // file is small, ok to buffer in memory.
        return await _domainOfInfluenceRepository.Query()
                   .WhereCanAccessOwnBfsOrChildrenOrParents(_permissionService)
                   .Where(x => x.Bfs == bfs)
                   .Include(x => x.Logo!.Content)
                   .Select(x => x.Logo!)
                   .FirstOrDefaultAsync()
               ?? throw new EntityNotFoundException(nameof(DomainOfInfluenceEntity), bfs);
    }
}
