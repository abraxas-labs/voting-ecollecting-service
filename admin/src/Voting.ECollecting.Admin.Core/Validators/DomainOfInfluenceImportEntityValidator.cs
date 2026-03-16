// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentValidation;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Admin.Core.Validators;

/// <summary>
/// Validator for the <see cref="DomainOfInfluenceEntity"/> for validation reports.
/// </summary>
internal class DomainOfInfluenceImportEntityValidator : AbstractValidator<DomainOfInfluenceEntity>
{
    private const int MaxBfsStringLength = 8;
    private const int MaxStringLength = 150;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainOfInfluenceImportEntityValidator"/> class with fluent validation rule set.
    /// </summary>
    internal DomainOfInfluenceImportEntityValidator()
    {
        InitializeRuleset();
    }

    /// <summary>
    /// Initializes the fluent validation rule set.
    /// </summary>
    private void InitializeRuleset()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(MaxStringLength);
        RuleFor(v => v.Bfs).NotEmpty().MaximumLength(MaxBfsStringLength);
        RuleFor(v => v.TenantName).NotEmpty().MaximumLength(MaxStringLength);
        RuleFor(v => v.TenantId).NotEmpty().MaximumLength(MaxStringLength);
        RuleFor(v => v.BasisType).IsInEnum().NotEqual(BasisDomainOfInfluenceType.Unspecified);
        RuleFor(v => v.Canton).IsInEnum();
    }
}
