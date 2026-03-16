// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.Extensions.Hosting;

namespace Voting.ECollecting.Admin.Abstractions.Core.HostedServices;

/// <summary>
/// The hosted services is responsible to synchronize domain of influence (DOI) data on a time-based execution defined by a cron schedule expression.
/// </summary>
public interface IDomainOfInfluenceHostedService : IHostedService;
