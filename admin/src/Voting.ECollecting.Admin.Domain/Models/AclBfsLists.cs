// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Admin.Domain.Models;

/// <summary>
/// BFS number access control lists for a single user.
/// </summary>
/// <param name="Bfs">The set of directly accessible BFS numbers of the current user.</param>
/// <param name="BfsMunicipalities">The set of directly accessible BFS numbers of municipalities of the current user.</param>
/// <param name="BfsMunicipalitiesInclParents">The set <see cref="BfsMunicipalities"/> plus parent bfs of directly accessible BFS numbers of municipalities of the current user. Basically the bfs numbers of all parents of <see cref="BfsMunicipalities"/> incl. the <see cref="BfsMunicipalities"/> themselves.</param>
/// <param name="BfsInclChildren">The set of accessible BFS numbers of the current user, including all children.</param>
/// <param name="BfsInclChildrenAndParents">The set of accessible BFS numbers of the current user, including children and all parents.</param>
public record AclBfsLists(
    IReadOnlySet<string> Bfs,
    IReadOnlySet<string> BfsMunicipalities,
    IReadOnlySet<string> BfsMunicipalitiesInclParents,
    IReadOnlySet<string> BfsInclChildren,
    IReadOnlySet<string> BfsInclChildrenAndParents)
{
    public static readonly AclBfsLists Empty = new(
        new HashSet<string>(),
        new HashSet<string>(),
        new HashSet<string>(),
        new HashSet<string>(),
        new HashSet<string>());
}
