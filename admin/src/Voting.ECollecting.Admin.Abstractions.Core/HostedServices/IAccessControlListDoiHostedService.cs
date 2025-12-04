// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.Extensions.Hosting;

namespace Voting.ECollecting.Admin.Abstractions.Core.HostedServices;

/// <summary>
/// The hosted services is responsible to synchronize domain of influence (DOI) based
/// access control list (ACL) data on a time-based execution defined by a cron schedule expression.
/// ACL data is required to determine the access level for a specific security context based on the requesting user.
/// </summary>
public interface IAccessControlListDoiHostedService : IHostedService;
