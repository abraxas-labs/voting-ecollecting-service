// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Core.Resources;
using Voting.ECollecting.Admin.Domain.Models;

namespace Voting.ECollecting.Admin.Core.Extensions;

internal static class InitiativeCommitteeMemberFieldExtensions
{
    public static string ToLocalizedString(this InitiativeCommitteeMemberFields fields)
    {
        var localizedNames = Enum.GetValues<InitiativeCommitteeMemberFields>()
            .Where(f => f != InitiativeCommitteeMemberFields.None && fields.HasFlag(f))
            .Select(f => Strings.ResourceManager.GetString($"InitiativeCommitteeMemberFields.{f}") ?? f.ToString());

        return string.Join(", ", localizedNames);
    }
}
