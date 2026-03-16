// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Core.Mappings;
using Voting.ECollecting.Admin.Core.Permissions;
using Voting.ECollecting.Admin.Core.Resources;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;
using IDomainOfInfluenceRepository = Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories.IDomainOfInfluenceRepository;

namespace Voting.ECollecting.Admin.Core.Services;

public class DomainOfInfluenceService : IDomainOfInfluenceService
{
    private readonly IDomainOfInfluenceRepository _doiRepository;
    private readonly IPermissionService _permissionService;
    private readonly IDataContext _dataContext;

    public DomainOfInfluenceService(
        IPermissionService permissionService,
        IDomainOfInfluenceRepository doiRepository,
        IDataContext dataContext)
    {
        _permissionService = permissionService;
        _doiRepository = doiRepository;
        _dataContext = dataContext;
    }

    public async Task<List<DomainOfInfluence>> List(
        bool? eCollectingEnabled,
        IReadOnlySet<DomainOfInfluenceType>? doiTypes,
        bool includeChildren)
    {
        var query = _doiRepository.Query()
            .Where(x => x.IsValid);

        query = includeChildren
            ? query.WhereCanAccessOwnBfsOrChildren(_permissionService)
            : query.WhereCanAccessOwnBfs(_permissionService);

        if (eCollectingEnabled.HasValue)
        {
            query = query.Where(x => x.ECollectingEnabled == eCollectingEnabled.Value);
        }

        // limit to support doi types if none are provided.
        query = doiTypes != null
            ? query.Where(x => doiTypes.Contains(x.Type))
            : query.Where(x => x.Type != DomainOfInfluenceType.Unspecified);

        var doiEntities = await query
            .Include(x => x.Logo) // include the logo reference, not the actual content
            .OrderBy(x => x.Type)
            .ThenBy(x => x.Name)
            .ToListAsync();

        // the MU's inherit the canton's max electronic signature percent
        if (doiEntities.Any(x => x.Type == DomainOfInfluenceType.Mu))
        {
            var quorumDoi = await _doiRepository.GetCanton();
            foreach (var doi in doiEntities.Where(x => x.Type == DomainOfInfluenceType.Mu))
            {
                doi.InitiativeMaxElectronicSignaturePercent = quorumDoi.InitiativeMaxElectronicSignaturePercent;
                doi.ReferendumMaxElectronicSignaturePercent = quorumDoi.ReferendumMaxElectronicSignaturePercent;
            }
        }

        return doiEntities.ConvertAll(BuildDomainOfInfluence);
    }

    public async Task<List<DomainOfInfluenceType>> ListOwnTypes()
    {
        return await _doiRepository.Query()
            .Where(x => x.IsValid
                        && x.TenantId == _permissionService.TenantId
                        && x.Type != DomainOfInfluenceType.Unspecified)
            .OrderBy(x => x.Type)
            .Select(x => x.Type)
            .OrderBy(x => x)
            .ToListAsync();
    }

    public async Task<DomainOfInfluence> Get(string bfs)
    {
        var doiEntity = await _doiRepository.Query()
                            .WhereCanAccessOwnBfsOrChildren(_permissionService)
                            .FirstOrDefaultAsync(x => x.IsValid && x.Bfs == bfs)
                        ?? throw new EntityNotFoundException(nameof(DomainOfInfluenceEntity), bfs);

        return BuildDomainOfInfluence(doiEntity);
    }

    public async Task Update(string bfs, UpdateDomainOfInfluenceRequest updateRequest)
    {
        var doi = await _doiRepository.Query()
            .WhereCanEdit(_permissionService)
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Bfs == bfs)
            ?? throw new EntityNotFoundException(nameof(DomainOfInfluenceEntity), bfs);

        ValidateUpdateRequest(updateRequest, doi);

        Mapper.UpdateDomainOfInfluence(updateRequest, doi);
        _permissionService.SetModified(doi);
        await _dataContext.SaveChangesAsync();
    }

    internal async Task<string> LoadDomainOfInfluenceName(DomainOfInfluenceType domainOfInfluenceType, string bfs)
    {
        return domainOfInfluenceType switch
        {
            DomainOfInfluenceType.Mu => await _doiRepository.GetNameByBfs(DomainOfInfluenceType.Mu, bfs),
            DomainOfInfluenceType.Ct => Strings.DomainOfInfluenceName_Ct,
            DomainOfInfluenceType.Ch => Strings.DomainOfInfluenceName_Ch,
            _ => throw new ArgumentOutOfRangeException(nameof(domainOfInfluenceType)),
        };
    }

    private void ValidateUpdateRequest(
        UpdateDomainOfInfluenceRequest req,
        DomainOfInfluenceEntity doi)
    {
        switch (doi.Type)
        {
            case DomainOfInfluenceType.Ch:
                if (req.Settings?.InitiativeMaxElectronicSignaturePercent != null)
                {
                    throw new ValidationException(
                        $"{nameof(req.Settings.InitiativeMaxElectronicSignaturePercent)} is not supported for {doi.Type}");
                }

                if (req.Settings?.InitiativeMinSignatureCount != null)
                {
                    throw new ValidationException(
                        $"{nameof(req.Settings.InitiativeMinSignatureCount)} is not supported for {doi.Type}");
                }

                break;

            case DomainOfInfluenceType.Ct:
                if (req.Settings?.InitiativeMinSignatureCount != null)
                {
                    throw new ValidationException(
                        $"{nameof(req.Settings.InitiativeMinSignatureCount)} is not supported for {doi.Type}");
                }

                break;
            case DomainOfInfluenceType.Mu:
                if (req.Settings?.InitiativeMaxElectronicSignaturePercent != null)
                {
                    throw new ValidationException(
                        $"{nameof(req.Settings.InitiativeMaxElectronicSignaturePercent)} is not supported for {doi.Type}");
                }

                if (req.Settings?.ReferendumMaxElectronicSignaturePercent != null)
                {
                    throw new ValidationException(
                        $"{nameof(req.Settings.ReferendumMaxElectronicSignaturePercent)} is not supported for {doi.Type}");
                }

                break;
        }
    }

    private DomainOfInfluence BuildDomainOfInfluence(DomainOfInfluenceEntity doiEntity)
    {
        var doi = Mapper.MapToDomainOfInfluence(doiEntity);
        doi.UserPermissions = DomainOfInfluencePermissions.Build(_permissionService, doi);
        return doi;
    }
}
