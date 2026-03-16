// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Adapter.Data.Configuration;
using Voting.ECollecting.Admin.Adapter.VotingBasis.Configuration;
using Voting.ECollecting.Admin.Adapter.VotingIam.Configuration;
using Voting.ECollecting.Admin.Adapter.VotingStimmregister.Config;
using Voting.ECollecting.Admin.Core.Configuration;
using Voting.ECollecting.Shared.Core.Configuration;
using Voting.Lib.Common.Net;
using Voting.Lib.Iam.Configuration;
using Voting.Lib.MalwareScanner.Configuration;

namespace Voting.ECollecting.Admin.WebService.Configuration;

public class AppConfig : CoreAppConfig
{
    /// <summary>
    /// Gets or sets the CORS config options used within the <see cref="Voting.Lib.Common.DependencyInjection.ApplicationBuilderExtensions"/>
    /// to configure the CORS middleware from <see cref="Microsoft.AspNetCore.Builder.CorsMiddlewareExtensions"/>.
    /// </summary>
    public CorsConfig Cors { get; set; } = new();

    /// <summary>
    /// Gets or sets the Ports configuration with the listening ports for the application.
    /// </summary>
    public PortConfig Ports { get; set; } = new();

    /// <summary>
    /// Gets or sets the port configuration for the metric endpoint.
    /// </summary>
    public ushort MetricPort { get; set; } = 9090;

    /// <summary>
    /// Gets or sets the Database configuration.
    /// </summary>
    public DataConfig Database { get; set; } = new();

    /// <summary>
    /// Gets or sets the Database configuration which is used to migrate the database schema.
    /// The service itself runs with the configuration of <see cref="Database"/>.
    /// </summary>
    public DataConfig? MigrationDatabase { get; set; }

    public DataConfig MigrationDatabaseOrDatabase => MigrationDatabase ?? Database;

    /// <summary>
    /// Gets or sets the identity provider configuration.
    /// </summary>
    public VotingIamConfig SecureConnect { get; set; } = new();

    /// <summary>
    /// Gets or sets the VOTING Basis config.
    /// </summary>
    public VotingBasisConfig VotingBasis { get; set; } = new();

    /// <summary>
    /// Gets or sets the identity provider api.
    /// </summary>
    public Uri? SecureConnectApi { get; set; }

    /// <summary>
    /// Gets or sets the certificate pinning configuration.
    /// </summary>
    public CertificatePinningConfig CertificatePinning { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether detailed errors are enabled. Should not be enabled in production environments,
    /// as this could expose information about the internals of this service.
    /// </summary>
    public bool EnableDetailedErrors { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether grpc-web should be used or plain grpc.
    /// </summary>
    public bool EnableGrpcWeb { get; set; }

    /// <summary>
    /// Gets or sets a list of paths where language headers are getting ignored.
    /// </summary>
    public HashSet<string> LanguageHeaderIgnoredPaths { get; set; } =
    [
        "/healthz",
        "/metrics",
    ];

    /// <summary>
    /// Gets or sets a list of paths where access control list evaluations are getting ignored.
    /// </summary>
    public HashSet<string> AccessControlListEvaluationIgnoredPaths { get; set; } =
    [
        "/healthz",
        "/metrics",
    ];

    /// <summary>
    /// Gets or sets a time span for the prometheus adapter interval.
    /// </summary>
    public TimeSpan PrometheusAdapterInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets the health check names of all health checks which are considered as non mission-critical
    /// (if any of them is unhealthy the system may still operate but in a degraded state).
    /// These health checks are monitored separately.
    /// </summary>
    public HashSet<string> LowPriorityHealthCheckNames { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether server timing is enabled.
    /// Should only be enabled for troubleshooting the performance.
    /// </summary>
    public bool EnableServerTiming { get; set; }

    /// <summary>
    /// Gets or sets the auth store configuration.
    /// </summary>
    public AuthStoreConfig AuthStore { get; set; } = new();

    /// <summary>
    /// Gets or sets the documatrix config.
    /// </summary>
    public DmDocConfig DmDoc { get; set; } = new();

    /// <summary>
    /// Gets or sets the malware scanner configuration.
    /// </summary>
    public MalwareScannerConfig MalwareScanner { get; set; } = new();

    /// <summary>
    /// Gets or sets the VOTING Stimmregister config.
    /// </summary>
    public VotingStimmregisterConfig VotingStimmregister { get; set; } = new();
}
