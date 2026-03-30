// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Domain.Entities;

public class CollectionAddress
{
    public string CommitteeOrPerson { get; set; } = string.Empty;

    public string StreetOrPostOfficeBox { get; set; } = string.Empty;

    public string ZipCode { get; set; } = string.Empty;

    public string Locality { get; set; } = string.Empty;

    public bool IsComplete
        => !string.IsNullOrWhiteSpace(CommitteeOrPerson)
           && !string.IsNullOrWhiteSpace(StreetOrPostOfficeBox)
           && !string.IsNullOrWhiteSpace(ZipCode)
           && !string.IsNullOrWhiteSpace(Locality);

    public bool Equals(CollectionAddress other)
    {
        return CommitteeOrPerson == other.CommitteeOrPerson
               && StreetOrPostOfficeBox == other.StreetOrPostOfficeBox
               && ZipCode == other.ZipCode
               && Locality == other.Locality;
    }
}
