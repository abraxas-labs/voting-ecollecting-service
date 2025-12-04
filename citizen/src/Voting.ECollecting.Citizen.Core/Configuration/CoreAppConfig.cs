// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Enums;
using Voting.Lib.MalwareScanner.Configuration;
using Voting.Lib.UserNotifications;

namespace Voting.ECollecting.Citizen.Core.Configuration;

public class CoreAppConfig
{
    private TimeZoneInfo? _timeZoneInfo;

    public HashSet<DomainOfInfluenceType> EnabledDomainOfInfluenceTypes { get; set; } = [];

    /// <summary>
    /// Gets or sets the malware scanner configuration.
    /// </summary>
    public MalwareScannerConfig MalwareScanner { get; set; } = new();

    public HashSet<string> AllowedImageFileExtensions { get; set; } = new();

    public HashSet<string> AllowedSignatureSheetFileExtensions { get; set; } = new();

    public HashSet<string> AllowedCommitteeListFileFileExtensions { get; set; } = new();

    public string CommitteeListFileNameSuffixDateFormat { get; set; } = "_yyyyMMdd-HHmm";

    public string TimeZone { get; set; } = "Europe/Zurich";

    public TimeZoneInfo TimeZoneInfo => _timeZoneInfo ??= TimeZoneInfo.FindSystemTimeZoneById(TimeZone);

    public SmtpConfig Smtp { get; set; } = new();

    public int MaxAllowedReferendumsPerDecree { get; set; }

    public int InitiativeCommitteeMinApprovedMembersCount { get; set; } = 15;

    public int InitiativeCommitteeMaxApprovedMembersCount { get; set; } = 27;

    /// <summary>
    /// Gets or sets the acr (authentication context class reference) config.
    /// </summary>
    public AcrConfig Acr { get; set; } = new();

    public TimeSpan PermissionTokenLifetime { get; set; } = TimeSpan.FromHours(72);

    public TimeSpan InitiativeCommitteeMemberTokenLifetime { get; set; } = TimeSpan.FromHours(72);

    /// <summary>
    /// Gets or sets the Kms config.
    /// </summary>
    public KmsConfig Kms { get; set; } = new();

    /// <summary>
    /// Gets or sets accessibility feedback email.
    /// </summary>
    public string AccessibilityEmail { get; set; } = "voting@abraxas.ch";
}
