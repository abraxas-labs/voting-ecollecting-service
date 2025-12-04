// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Core.Exceptions;
using Voting.ECollecting.Admin.Core.Mappings;
using Voting.ECollecting.Admin.Core.Permissions;
using Voting.ECollecting.Admin.Core.Utils;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Admin.Domain.Queries;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;

namespace Voting.ECollecting.Admin.Core.Services;

public class ReferendumService : IReferendumService
{
    private readonly IDecreeRepository _decreeRepository;
    private readonly IReferendumRepository _referendumRepository;
    private readonly CollectionService _collectionService;
    private readonly DecreeService _decreeService;
    private readonly TimeProvider _timeProvider;
    private readonly IPermissionService _permissionService;
    private readonly IDataContext _dataContext;

    public ReferendumService(
        IDecreeRepository decreeRepository,
        IReferendumRepository referendumRepository,
        CollectionService collectionService,
        DecreeService decreeService,
        TimeProvider timeProvider,
        IPermissionService permissionService,
        IDataContext dataContext)
    {
        _decreeRepository = decreeRepository;
        _referendumRepository = referendumRepository;
        _collectionService = collectionService;
        _decreeService = decreeService;
        _timeProvider = timeProvider;
        _permissionService = permissionService;
        _dataContext = dataContext;
    }

    public async Task<Dictionary<DomainOfInfluenceType, List<Decree>>> ListDecreesByDoiType(IReadOnlySet<DomainOfInfluenceType>? doiTypes, string? bfs)
    {
        var query = _decreeRepository.Query().WhereCanReadOnReferendums(_permissionService);

        if (doiTypes?.Count > 0)
        {
            query = query.Where(d => doiTypes.Contains(d.DomainOfInfluenceType));
        }

        if (!string.IsNullOrEmpty(bfs))
        {
            query = query.Where(x => x.Bfs == bfs);
        }

        var decreesByDoiType = await query
            .Include<DecreeEntity, List<ReferendumEntity>>(x => x.Collections)
            .ThenIncludeMunicipalities(_permissionService.AclBfsLists)
            .Include<DecreeEntity, List<ReferendumEntity>>(x => x.Collections)
            .ThenInclude(x => x.CollectionCount)
            .GroupBy(x => x.DomainOfInfluenceType)
            .ToDictionaryAsync(x => x.Key, x => x.OrderByDescending(y => y.CollectionStartDate).ToList());

        var now = _timeProvider.GetUtcNowDateTime();
        return Enum.GetValues<DomainOfInfluenceType>()
            .Where(x => x != DomainOfInfluenceType.Unspecified)
            .OrderBy(x => x)
            .ToDictionary(x => x, x =>
            {
                var decrees = Mapper.MapToDecrees(decreesByDoiType.GetValueOrDefault(x) ?? []);

                foreach (var decree in decrees)
                {
                    _decreeService.SetPeriodStateAndUserPermissions(decree, now);

                    foreach (var referendum in decree.Referendums)
                    {
                        referendum.Decree = decree;
                        referendum.SetPeriodState(now);
                        _collectionService.LoadPermission(referendum);
                        _collectionService.SetCollectionCount(referendum);
                    }
                }

                return decrees;
            });
    }

    public async Task<Referendum> Get(Guid id)
    {
        var referendumEntity = await _referendumRepository.Query()
                                   .WhereCanRead(_permissionService)
                                   .Include(x => x.Decree)
                                   .Include(x => x.CollectionCount)
                                   .IncludeMunicipalities(_permissionService.AclBfsLists)

                                   // include files but not the file content
                                   .Include(x => x.Image)
                                   .Include(x => x.Logo)
                                   .Include(x => x.SignatureSheetTemplate)
                                   .FirstOrDefaultAsync(x => x.Id == id)
                               ?? throw new EntityNotFoundException(nameof(ReferendumEntity), id);
        var referendum = Mapper.MapToReferendum(referendumEntity);
        referendum.SetPeriodState(_timeProvider.GetUtcNowDateTime());
        _collectionService.LoadPermission(referendum);
        _collectionService.SetCollectionCount(referendum);
        return referendum;
    }

    public async Task<Guid> Create(Guid decreeId, string description, CollectionAddress collectionAddress)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var decree = await _decreeRepository.Query()
                         .WhereCanAddCollection(_permissionService)
                         .Include(x => x.Collections)
                         .FirstOrDefaultAsync(x => x.Id == decreeId)
                     ?? throw new EntityNotFoundException(nameof(DecreeEntity), decreeId);

        if (decree.Collections.Any(x => x.Description == description))
        {
            throw new CollectionAlreadyExistsException();
        }

        var existingNumbers = await _referendumRepository.Query().Select(x => x.Number).ToHashSetAsync();
        var number = RandomUtil.GenerateReferendumNumber(existingNumbers);
        var referendum = new ReferendumEntity
        {
            Description = description,
            DecreeId = decreeId,
            CollectionCount = new CollectionCountEntity(),
            SignatureSheetTemplateGenerated = true,
            Type = CollectionType.Referendum,
            IsElectronicSubmission = false,
            State = CollectionState.PreRecorded,
            Address = collectionAddress,
            Number = number,

            // decree data
            Bfs = decree.Bfs,
            DomainOfInfluenceType = decree.DomainOfInfluenceType,
            MaxElectronicSignatureCount = decree.MaxElectronicSignatureCount,
            CollectionStartDate = decree.CollectionStartDate,
            CollectionEndDate = decree.CollectionEndDate,
        };

        _permissionService.SetCreated(referendum);
        _permissionService.SetCreated(referendum.CollectionCount);
        await _referendumRepository.Create(referendum);

        await _collectionService.PrepareForCollection(referendum);
        await _dataContext.SaveChangesAsync();

        await transaction.CommitAsync();
        return referendum.Id;
    }
}
