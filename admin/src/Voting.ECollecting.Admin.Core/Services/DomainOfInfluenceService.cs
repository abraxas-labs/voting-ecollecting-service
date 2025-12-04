// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Core.Mappings;
using Voting.ECollecting.Admin.Core.Permissions;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;

namespace Voting.ECollecting.Admin.Core.Services;

public class DomainOfInfluenceService : IDomainOfInfluenceService
{
    private static readonly IReadOnlySet<AclDomainOfInfluenceType> _supportedAclDoiTypes = Mapper.MapToAclDoiTypes(Enum.GetValues<DomainOfInfluenceType>()).ToHashSet();
    private readonly IAccessControlListDoiRepository _aclRepository;
    private readonly IDomainOfInfluenceRepository _doiRepository;
    private readonly IPermissionService _permissionService;
    private readonly IDataContext _dataContext;

    public DomainOfInfluenceService(
        IPermissionService permissionService,
        IAccessControlListDoiRepository aclRepository,
        IDomainOfInfluenceRepository doiRepository,
        IDataContext dataContext)
    {
        _permissionService = permissionService;
        _aclRepository = aclRepository;
        _doiRepository = doiRepository;
        _dataContext = dataContext;
    }

    public async Task<List<DomainOfInfluence>> List(
        bool? eCollectingEnabled,
        IReadOnlySet<DomainOfInfluenceType>? doiTypes,
        bool includeChildren)
    {
        var query = _aclRepository.Query()
            .Where(x => x.IsValid);

        if (includeChildren)
        {
            query = query.WhereCanAccessOwnBfsOrChildren(_permissionService);
        }
        else
        {
            query = query.WhereCanAccessOwnBfs(_permissionService);
        }

        if (eCollectingEnabled.HasValue)
        {
            query = query.Where(x => x.ECollectingEnabled == eCollectingEnabled.Value);
        }

        // limit to DomainOfInfluenceType if none are provided,
        // acl contain a lot of other types which are not supported in e-collecting.
        doiTypes ??= Enum.GetValues<DomainOfInfluenceType>().ToHashSet();
        var aclDoiTypes = Mapper.MapToAclDoiTypes(doiTypes).ToHashSet();
        query = query.Where(x => aclDoiTypes.Contains(x.Type));

        var aclDoiEntities = await query
            .OrderBy(x => x.Type)
            .ThenBy(x => x.Name)
            .ToListAsync();

        var dois = Mapper.MapToDomainOfInfluences(aclDoiEntities).ToList();
        var bfs = dois.Select(x => x.Bfs).ToHashSet();
        var doisByBfs = await _doiRepository.Query()
            .Where(x => bfs.Contains(x.Bfs))
            .Include(x => x.Logo)
            .ToDictionaryAsync(x => x.Bfs, x => x);

        foreach (var domainOfInfluence in dois)
        {
            if (doisByBfs.TryGetValue(domainOfInfluence.Bfs, out var doi))
            {
                Mapper.MapToDomainOfInfluence(doi, domainOfInfluence);
            }
        }

        return dois;
    }

    public async Task<List<DomainOfInfluenceType>> ListOwnTypes()
    {
        // directly accessing the field does not work with Npgsql / ef core (cannot be translated)
        // storing it in a local variable works.
        var supportedDoiTypes = _supportedAclDoiTypes;
        var types = await _aclRepository.Query()
            .Where(x => x.IsValid
                        && x.TenantId == _permissionService.TenantId
                        && supportedDoiTypes.Contains(x.Type))
            .OrderBy(x => x.Type)
            .Select(x => x.Type)
            .ToListAsync();
        return Mapper.MapToDoiTypes(types).ToList();
    }

    public async Task<DomainOfInfluence> Get(string bfs)
    {
        var aclDoiEntity = await _aclRepository.Query()
                               .WhereCanAccessOwnBfsOrChildren(_permissionService)
                               .FirstOrDefaultAsync(x => x.IsValid && x.Bfs == bfs)
                           ?? throw new EntityNotFoundException(nameof(AccessControlListDoiEntity), bfs);

        var doi = Mapper.MapToDomainOfInfluence(aclDoiEntity);
        var doiEntity = await _doiRepository.Query()
            .WhereCanAccessOwnBfsOrChildren(_permissionService)
            .FirstOrDefaultAsync(x => x.Bfs == bfs);
        if (doiEntity != null)
        {
            Mapper.MapToDomainOfInfluence(doiEntity, doi);
        }

        return doi;
    }

    public async Task Update(string bfs, UpdateDomainOfInfluenceRequest updateRequest)
    {
        var doi = await _doiRepository.Query()
            .WhereCanEdit(_permissionService)
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Bfs == bfs);

        if (doi == null)
        {
            var type = await _aclRepository.Query()
                           .WhereCanAccessOwnBfs(_permissionService)
                           .Where(x => x.Bfs == bfs)
                           .Select(x => (AclDomainOfInfluenceType?)x.Type)
                           .SingleOrDefaultAsync()
                       ?? throw new EntityNotFoundException(nameof(AccessControlListDoiEntity), new { bfs });

            doi = new DomainOfInfluenceEntity { Bfs = bfs, Type = Mapper.MapToDoiType(type) };
            _permissionService.SetCreated(doi);
            _dataContext.DomainOfInfluences.Add(doi);
        }

        Mapper.UpdateDomainOfInfluence(updateRequest, doi);
        if (doi.Id != Guid.Empty)
        {
            _permissionService.SetModified(doi);
        }

        await _dataContext.SaveChangesAsync();
    }
}
