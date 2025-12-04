// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Core.Configuration;

public class DmDocTemplateKeysConfig
{
    public string InitiativeSignatureSheetTemplate { get; set; } = "initiative_signature_list";

    public string ReferendumSignatureSheet { get; set; } = "referendum_signature_list";

    public string CommitteeList { get; set; } = "initiative_committee_list";

    public string SignatureSheetAttestation { get; set; } = "signature_list_certificate";

    public string OfficialJournalPublicationProtocol { get; set; } = "official_journal_publication_protocol";

    public string ElectronicSignaturesProtocol { get; set; } = "electronic_signatures_protocol";
}
