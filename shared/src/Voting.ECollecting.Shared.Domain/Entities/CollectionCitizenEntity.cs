// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities.IntegritySignature;

namespace Voting.ECollecting.Shared.Domain.Entities;

public class CollectionCitizenEntity : IntegritySignatureEntity
{
    public Guid? CollectionMunicipalityId { get; set; }

    public CollectionMunicipalityEntity? CollectionMunicipality { get; set; }

    public bool Electronic => !SignatureSheetId.HasValue;

    public Guid? SignatureSheetId { get; set; }

    public CollectionSignatureSheetEntity? SignatureSheet { get; set; }

    public int Age { get; set; }

    public int Sex { get; set; }

    public DateTime CollectionDateTime { get; set; }

    public CollectionCitizenLogEntity? Log { get; set; }
}
