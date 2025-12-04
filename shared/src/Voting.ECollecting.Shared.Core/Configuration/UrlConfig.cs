// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Enums;
using Voting.Lib.Common;

namespace Voting.ECollecting.Shared.Core.Configuration;

public class UrlConfig
{
    /// <summary>
    /// Gets or sets the public admin url.
    /// </summary>
    public string Admin { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the public citizen url.
    /// </summary>
    public string Citizen { get; set; } = string.Empty;

    // use the fragment to not send the token to the server in the url,
    // only send it in the body which is less likely to get logged etc.
    public string BuildPermissionApprovalUrl(UrlToken? token)
        => BuildUrl(true, $"permission-approval#token={token}");

    // use the fragment to not send the token to the server in the url,
    // only send it in the body which is less likely to get logged etc.
    public string BuildInitiativeCommitteeMembershipApprovalUrl(UrlToken? token)
        => BuildUrl(true, $"initiative-committee-membership-approval#token={token}");

    public string BuildCollectionUrl(Guid id, CollectionType collectionType, bool isCitizen)
    {
        var segment = collectionType switch
        {
            CollectionType.Initiative => "initiatives",
            CollectionType.Referendum => "referendums",
            _ => throw new ArgumentOutOfRangeException(nameof(collectionType), collectionType, null),
        };

        return BuildUrl(isCitizen, segment, id.ToString());
    }

    private string BuildUrl(bool isCitizen, params string[] pathSegments)
    {
        var baseUrl = isCitizen ? Citizen : Admin;
        baseUrl = baseUrl.TrimEnd('/');
        return string.Join('/', [baseUrl, .. pathSegments]);
    }
}
