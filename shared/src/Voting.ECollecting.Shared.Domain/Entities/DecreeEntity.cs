// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities.IntegritySignature;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Shared.Domain.Entities;

public class DecreeEntity : IntegritySignatureEntity, IHasBfs
{
    private CollectionPeriodState? _periodState;

    public DomainOfInfluenceType DomainOfInfluenceType { get; set; }

    public string Bfs { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime CollectionStartDate { get; set; }

    public DateTime CollectionEndDate { get; set; }

    public int MinSignatureCount { get; set; }

    public int MaxElectronicSignatureCount { get; set; }

    public string Link { get; set; } = string.Empty;

    public List<ReferendumEntity> Collections { get; set; } = [];

    public List<CollectionCitizenLogEntity>? CollectionCitizenLogs { get; set; }

    public CollectionPeriodState PeriodState => _periodState ?? throw new InvalidOperationException("State not initialized.");

    public DecreeState State { get; set; }

    public CollectionCameNotAboutReason? CameNotAboutReason { get; set; }

    public DateOnly? SensitiveDataExpiryDate { get; set; }

    public void SetPeriodState(DateTime utcNow)
    {
        if (CollectionStartDate > utcNow)
        {
            _periodState = CollectionPeriodState.Published;
        }
        else if (CollectionStartDate <= utcNow && CollectionEndDate >= utcNow)
        {
            _periodState = CollectionPeriodState.InCollection;
        }
        else
        {
            _periodState = CollectionPeriodState.Expired;
        }
    }
}
