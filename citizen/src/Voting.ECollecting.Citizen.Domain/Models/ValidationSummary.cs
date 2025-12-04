// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;

namespace Voting.ECollecting.Citizen.Domain.Models;

public record ValidationSummary(IReadOnlyCollection<ValidationResult> ValidationResults)
{
    public bool IsValid => ValidationResults.All(x => x.IsValid);

    public void EnsureIsValid()
    {
        if (IsValid)
        {
            return;
        }

        var failedValidations = ValidationResults
            .Where(r => !r.IsValid)
            .Select(r => r.Validation);

        throw new ValidationException($"Validation failed for {string.Join(",", failedValidations)}");
    }
}
