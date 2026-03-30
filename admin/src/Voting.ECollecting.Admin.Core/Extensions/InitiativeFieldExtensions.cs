// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Core.Resources;
using Voting.ECollecting.Admin.Domain.Models;

namespace Voting.ECollecting.Admin.Core.Extensions;

internal static class InitiativeFieldExtensions
{
    public static string ToLocalizedString(this InitiativeFields fields)
    {
        var localizedNames = Enum.GetValues<InitiativeFields>()
            .Where(f => f != InitiativeFields.None && fields.HasFlag(f))
            .Select(f => Strings.ResourceManager.GetString($"InitiativeFields.{f}") ?? f.ToString());

        return string.Join(", ", localizedNames);
    }
}
