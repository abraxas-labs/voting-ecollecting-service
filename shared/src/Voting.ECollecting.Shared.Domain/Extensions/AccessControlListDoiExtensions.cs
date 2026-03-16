// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Domain.Extensions;

public static class AccessControlListDoiExtensions
{
    public static IEnumerable<DomainOfInfluenceEntity> GetFlattenParentsInclSelf(this DomainOfInfluenceEntity acl)
    {
        yield return acl;

        while (true)
        {
            if (acl.Parent == null)
            {
                yield break;
            }

            yield return acl.Parent;
            acl = acl.Parent;
        }
    }

    public static IEnumerable<DomainOfInfluenceEntity> GetFlattenChildrenInclSelf(this DomainOfInfluenceEntity acl)
    {
        yield return acl;
        foreach (var childDoi in acl.Children.SelectMany(GetFlattenChildrenInclSelf))
        {
            yield return childDoi;
        }
    }

    public static IEnumerable<DomainOfInfluenceEntity> GetFlattenChildren(this DomainOfInfluenceEntity acl)
    {
        foreach (var child in acl.Children)
        {
            yield return child;
            foreach (var descendant in GetFlattenChildren(child))
            {
                yield return descendant;
            }
        }
    }

    public static IEnumerable<DomainOfInfluenceEntity> GetFlattenParents(this DomainOfInfluenceEntity acl)
    {
        while (true)
        {
            if (acl.Parent == null)
            {
                yield break;
            }

            yield return acl.Parent;
            acl = acl.Parent;
        }
    }
}
