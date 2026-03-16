// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Core.Configuration;

public class DmDocConfig : Lib.DmDoc.Configuration.DmDocConfig
{
    public bool EnableMock { get; set; }

    public DmDocTemplateKeysConfig TemplateKeys { get; set; } = new();

    public string SignatureSheetTemplateFileName { get; set; } = "Unterschriftenliste_{0}.pdf";

    public string CommitteeListTemplateFileName { get; set; } = "Formular_Komiteemitglieder.pdf";

    public string SignatureSheetAttestationFileName { get; set; } = "Begleitschreiben_Unterschriftenlisten.pdf";

    public string OfficialJournalPublicationProtocolFileName { get; set; } = "Amtsblattpublikation.pdf";

    public string ElectronicSignaturesProtocolFileName { get; set; } = "Elektronische_Unterschriften.pdf";

    public string FileNameSuffixDateFormat { get; set; } = "_yyyyMMdd-HHmm";
}
