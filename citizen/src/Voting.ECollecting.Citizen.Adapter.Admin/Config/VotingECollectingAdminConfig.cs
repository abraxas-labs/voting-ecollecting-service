// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Grpc.Configuration;
using Voting.Lib.Iam.TokenHandling.ServiceToken;

namespace Voting.ECollecting.Citizen.Adapter.Admin.Config;

public class VotingECollectingAdminConfig
{
#if !RELEASE
    /// <summary>
    /// Gets or sets a value indicating whether the mock should be enabled.
    /// Not availalbe in RELEASE builds.
    /// </summary>
    public bool EnableMock { get; set; }
#endif

    /// <summary>
    /// Gets or sets the api endpoint for the Admin service.
    /// </summary>
    public Uri? ApiEndpoint { get; set; }

    /// <summary>
    /// Gets or sets which grpc mode to use.
    /// </summary>
    public GrpcMode Mode { get; set; } = GrpcMode.GrpcWebText;

    /// <summary>
    /// Gets or sets the tenant which is used to access the Admin service.
    /// </summary>
    public string Tenant { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identity provider configuration.
    /// </summary>
    public SecureConnectServiceAccountOptions IdpServiceAccount { get; set; } = new();
}
