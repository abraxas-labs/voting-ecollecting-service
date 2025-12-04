// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Citizen.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Citizen.Abstractions.Core.Services;
using Voting.ECollecting.Citizen.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using DomainEnums = Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Citizen.Core.Services.Validation;

public abstract class CollectionValidationService : ICollectionValidationService
{
    private readonly ICollectionPermissionRepository _collectionPermissionRepository;

    protected CollectionValidationService(ICollectionPermissionRepository collectionPermissionRepository)
    {
        _collectionPermissionRepository = collectionPermissionRepository;
    }

    public virtual ValidationResult ValidateGeneralInformation(CollectionBaseEntity collection)
    {
        var generalInformationNotNull = !string.IsNullOrEmpty(collection.Description) && collection.Address.IsComplete;
        return new ValidationResult(DomainEnums.Validation.GeneralInformationNotNull, generalInformationNotNull);
    }

    public virtual async Task<ValidationSummary> ValidateForSubmission(CollectionBaseEntity collection)
    {
        var hasDeputyOnCollection = await _collectionPermissionRepository.Query()
            .AnyAsync(x =>
                x.CollectionId == collection.Id && x.State == DomainEnums.CollectionPermissionState.Accepted &&
                x.Role == DomainEnums.CollectionPermissionRole.Deputy);

        return new ValidationSummary([
            new ValidationResult(DomainEnums.Validation.HasDeputyPermissions, hasDeputyOnCollection),
            ValidateGeneralInformation(collection),
        ]);
    }
}
