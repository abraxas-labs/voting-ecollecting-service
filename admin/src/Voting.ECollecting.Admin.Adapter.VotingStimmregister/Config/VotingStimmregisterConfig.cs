// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Grpc.Configuration;
using Voting.Lib.Iam.TokenHandling.OnBehalfToken;

namespace Voting.ECollecting.Admin.Adapter.VotingStimmregister.Config;

public class VotingStimmregisterConfig
{
#if !RELEASE
    /// <summary>
    /// Gets or sets a value indicating whether the mock should be enabled.
    /// Not availalbe in RELEASE builds.
    /// </summary>
    public bool EnableMock { get; set; }
#endif

    /// <summary>
    /// Gets or sets the api endpoint for VOTING Stimmregister.
    /// </summary>
    public Uri? ApiEndpoint { get; set; }

    /// <summary>
    /// Gets or sets which grpc mode to use.
    /// </summary>
    public GrpcMode Mode { get; set; } = GrpcMode.GrpcWebText;

    /// <summary>
    /// Gets or sets the on-behalf token configuration.
    /// </summary>
    public SecureConnectOnBehalfOptions OnBehalf { get; set; } = new();
}
