// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Text.RegularExpressions;
using Voting.Lib.Common;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests;

public static class VerifyExtensions
{
    private static readonly Regex _pattern = new(
        $"{Regex.Escape(UrlToken.Prefix)}[A-Za-z0-9_-]{{{Base64Url.GetLength(64)}}}",
        RegexOptions.Compiled);

    public static SettingsTask ScrubUrlTokens(this SettingsTask task)
        => task.ScrubLinesWithReplace(x => _pattern.Replace(x, "<url-token>"));
}
