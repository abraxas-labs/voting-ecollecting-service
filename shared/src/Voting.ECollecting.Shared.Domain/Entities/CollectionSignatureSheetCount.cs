// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Domain.Entities;

public class CollectionSignatureSheetCount
{
    public int Invalid { get; set; }

    public int Valid { get; set; }

    public int Total => Valid + Invalid;

    public bool CanAdd => Valid < Total;
}
