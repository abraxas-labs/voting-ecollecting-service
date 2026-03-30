// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Core.Resources;
using Voting.ECollecting.Admin.Domain.Models;

namespace Voting.ECollecting.Admin.Core.Extensions;

internal static class ReferendumFieldExtensions
{
    public static string ToLocalizedString(this ReferendumFields fields)
    {
        var localizedNames = Enum.GetValues<ReferendumFields>()
            .Where(f => f != ReferendumFields.None && fields.HasFlag(f))
            .Select(f => Strings.ResourceManager.GetString($"ReferendumFields.{f}") ?? f.ToString());

        return string.Join(", ", localizedNames);
    }
}
