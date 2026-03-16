// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Core.Configuration;
using Voting.Lib.Iam.SecondFactor.Configuration;
using Voting.Lib.UserNotifications;

namespace Voting.ECollecting.Admin.Core.Configuration;

public class CoreAppConfig
{
    /// <summary>
    /// Gets or sets the smtp config.
    /// </summary>
    public SmtpConfig Smtp { get; set; } = new();

    /// <summary>
    /// Gets or sets the imports config.
    /// </summary>
    public ImportConfig Import { get; set; } = new();

    /// <summary>
    /// Gets or sets the user notification sender job config.
    /// </summary>
    public UserNotificationsJobConfig UserNotificationsJob { get; set; } = new();

    /// <summary>
    /// Gets or sets the initiative committee member expiry job config.
    /// </summary>
    public InitiativeCommitteeMemberExpiryJobConfig InitiativeCommitteeMemberExpiryJob { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection permission expiry job config.
    /// </summary>
    public CollectionPermissionExpiryJobConfig CollectionPermissionExpiryJob { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection cleanup job config.
    /// </summary>
    public CollectionCleanupJobConfig CollectionCleanupJob { get; set; } = new();

    /// <summary>
    /// Gets or sets public urls.
    /// </summary>
    public UrlConfig Urls { get; set; } = new();

    /// <summary>
    /// Gets or sets the KMS config.
    /// </summary>
    public KmsConfig Kms { get; set; } = new();

    public int InitiativeCommitteeMinApprovedMembersCount { get; set; } = 15;

    /// <summary>
    /// Gets or sets the backup certificate config.
    /// </summary>
    public BackupCertificateConfig BackupCertificate { get; set; } = new();

    /// <summary>
    /// Gets or sets second factor transaction config.
    /// </summary>
    public SecondFactorTransactionConfig SecondFactorTransaction { get; set; } = new();

    public HashSet<string> AllowedImageFileExtensions { get; set; } = new();

    /// <summary>
    /// Gets or sets the csv config.
    /// </summary>
    public CsvConfig Csv { get; set; } = new();
}
