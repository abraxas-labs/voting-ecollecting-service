// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Citizen.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using DomainEnums = Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Citizen.Core.Services.Validation;

public class ReferendumValidationService : CollectionValidationService
{
    public ReferendumValidationService(ICollectionPermissionRepository collectionPermissionRepository)
        : base(collectionPermissionRepository)
    {
    }

    public override async Task<ValidationSummary> ValidateForSubmission(CollectionBaseEntity collection)
    {
        return new ValidationSummary([
            .. (await base.ValidateForSubmission(collection)).ValidationResults,
            new ValidationResult(DomainEnums.Validation.DecreeNotNull, ((ReferendumEntity)collection).DecreeId.HasValue),
        ]);
    }
}
