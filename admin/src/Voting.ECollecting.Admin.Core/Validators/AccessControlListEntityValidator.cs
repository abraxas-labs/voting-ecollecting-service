// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentValidation;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Admin.Core.Validators;

/// <summary>
/// Validator for the <see cref="AccessControlListDoiEntity"/> for validation reports.
/// </summary>
internal class AccessControlListEntityValidator : AbstractValidator<AccessControlListDoiEntity>
{
    private const int MaxBfsStringLength = 8;
    private const int MaxStringLength = 150;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccessControlListEntityValidator"/> class with fluent validation rule set.
    /// </summary>
    internal AccessControlListEntityValidator()
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
        RuleFor(v => v.Type).IsInEnum().NotEqual(AclDomainOfInfluenceType.Unspecified);
        RuleFor(v => v.Canton).IsInEnum();
    }
}
