// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Admin.WebService.Configuration;

/// <summary>
/// Port configuration options used to setup the listening ports for the application within the <see cref="Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions.ListenAnyIP"/>.
/// </summary>
public class PortConfig
{
    /// <summary>
    /// Gets or sets the listening port for HTTP requests.
    /// </summary>
    public ushort Http { get; set; } = 5000;

    /// <summary>
    /// Gets or sets the listening port for HTTP2 requests.
    /// </summary>
    public ushort Http2 { get; set; } = 5001;
}
