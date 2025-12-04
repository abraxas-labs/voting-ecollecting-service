// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.AspNetCore.Authorization;
using Voting.ECollecting.Citizen.Core.Configuration;
using Voting.ECollecting.Citizen.Domain.Authorization;

namespace Voting.ECollecting.Citizen.WebService.Extensions;

internal static class AuthorizationBuilderExtensions
{
    public static AuthorizationBuilder AddAcrPolicies(this AuthorizationBuilder builder, AcrConfig acrConfig)
    {
        return builder
            .AddAcrPolicy(Policies.AcceptPermission, acrConfig.AcceptPermission)
            .AddAcrPolicy(Policies.AcceptInitiativeCommitteeMembership, acrConfig.AcceptInitiativeCommitteeMembership)
            .AddAcrPolicy(Policies.SignCollection, acrConfig.SignCollection)
            .AddAcrPolicy(Policies.CreateCollection, acrConfig.CreateCollection);
    }

    private static AuthorizationBuilder AddAcrPolicy(this AuthorizationBuilder builder, string policyName, IReadOnlySet<string> acceptedAcrs)
    {
        return builder.AddPolicy(policyName, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(ClaimTypes.Acr, acceptedAcrs);
        });
    }
}
