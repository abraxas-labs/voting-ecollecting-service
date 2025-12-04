// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Admin.Core.Import;

/// <summary>
/// Service for importing domain of influence (DOI) based access control list (ACL).
/// </summary>
public interface IAccessControlListImporter
{
    /// <summary>
    /// Import domain of influence based access control lists.
    /// </summary>
    /// <param name="allowedCantons">Only data related these cantons is allowed to be imported.</param>
    /// <param name="ignoredBfs">List of ignored BFS.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation on success.</returns>
    Task ImportAcl(
        IReadOnlySet<Canton> allowedCantons,
        IReadOnlySet<string> ignoredBfs);
}
