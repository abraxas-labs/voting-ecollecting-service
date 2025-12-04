// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Citizen.Abstractions.Core.Services;

public interface ICollectionValidationService
{
    ValidationResult ValidateGeneralInformation(CollectionBaseEntity collection);

    Task<ValidationSummary> ValidateForSubmission(CollectionBaseEntity collection);
}
