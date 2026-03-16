// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Basis.Services.V1.Models;
using Abraxas.Voting.Basis.Shared.V1;
using Voting.ECollecting.Shared.Test.MockedData;
using DomainOfInfluenceType = Abraxas.Voting.Basis.Shared.V1.DomainOfInfluenceType;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.DoiTests;

public static class DoiVotingBasisMockedData
{
    public static PoliticalDomainOfInfluence SG_Kanton_StGallen_L1_CH
        => new()
        {
            Id = "24b12f20-a4a9-4f62-a805-60e7c61359e2",
            Name = "Kanton St.Gallen (CH)",
            ParentId = string.Empty,
            Bfs = "1",
            TenantName = "Dienst für Politische Rechte",
            TenantId = MockedTenantIds.KTSG,
            Type = DomainOfInfluenceType.Ch,
            Canton = DomainOfInfluenceCanton.Sg,
            Children = { SG_Auslandschweizer_L2_MU, SG_Kanton_StGallen_L2_CT },
            ReturnAddress = new DomainOfInfluenceVotingCardReturnAddress
            {
                AddressLine1 = "Staatskanzlei St. Gallen",
            },
            ECollectingEnabled = true,
        };

    public static PoliticalDomainOfInfluence SG_Auslandschweizer_L2_MU
        => new()
        {
            Id = "e4f482e6-20d8-4c8f-9e31-fecf092bf06b",
            Name = "Auslandschweizer SG (MU)",
            ParentId = "24b12f20-a4a9-4f62-a805-60e7c61359e2",
            Bfs = "9170",
            TenantName = "Dienst für Politische Rechte",
            TenantId = MockedTenantIds.KTSG,
            Type = DomainOfInfluenceType.Mu,
            Canton = DomainOfInfluenceCanton.Sg,
        };

    public static PoliticalDomainOfInfluence SG_Kanton_StGallen_L2_CT
        => new()
        {
            Id = "5a9772c4-a504-40b3-88b2-620af634d9b5",
            Name = "Kanton St. Gallen SG (CT)",
            ParentId = "24b12f20-a4a9-4f62-a805-60e7c61359e2",
            Bfs = "17",
            TenantName = "Dienst für Politische Rechte",
            TenantId = MockedTenantIds.KTSG,
            Type = DomainOfInfluenceType.Ct,
            Canton = DomainOfInfluenceCanton.Sg,
            Children = { SG_Gerichtskreis_StGallen_L3_BZ },
        };

    public static PoliticalDomainOfInfluence SG_Gerichtskreis_StGallen_L3_BZ
        => new()
        {
            Id = "27e721c1-0590-40f7-94da-1526ab47c5d4",
            Name = "Gerichtskreis St.Gallen (BZ)",
            ParentId = "5a9772c4-a504-40b3-88b2-620af634d9b5",
            Bfs = string.Empty,
            TenantName = "Dienst für Politische Rechte",
            TenantId = MockedTenantIds.KTSG,
            Type = DomainOfInfluenceType.Bz,
            Canton = DomainOfInfluenceCanton.Sg,
            Children = { SG_Wahlkreis_StGallen_L4_BZ },
        };

    public static PoliticalDomainOfInfluence SG_Wahlkreis_StGallen_L4_BZ
        => new()
        {
            Id = "03fd4057-9da9-4fd5-8310-d4d420762aef",
            Name = "Wahlkreis St.Gallen (BZ)",
            ParentId = "27e721c1-0590-40f7-94da-1526ab47c5d4",
            Bfs = string.Empty,
            TenantName = "Dienst für Politische Rechte",
            TenantId = MockedTenantIds.KTSG,
            Type = DomainOfInfluenceType.Bz,
            Canton = DomainOfInfluenceCanton.Sg,
            Children = { SG_StGallen_L5_MU, SG_Goldach_L5_MU, SG_Moerschwil_L5_MU, SG_Rorschacherberg_L5_MU },
            SortNumber = 1,
        };

    public static PoliticalDomainOfInfluence SG_StGallen_L5_MU
        => new()
        {
            Id = "a500760a-3e0f-4c98-a13a-9a614b74127a",
            Name = "St.Gallen (MU)",
            ParentId = "03fd4057-9da9-4fd5-8310-d4d420762aef",
            Bfs = "3203",
            TenantName = "Stadt St.Gallen",
            TenantId = MockedTenantIds.MUSG,
            Type = DomainOfInfluenceType.Mu,
            Canton = DomainOfInfluenceCanton.Sg,
            ECollectingEnabled = true,
            SortNumber = 1,
            NameForProtocol = "Stadt St. Gallen",
        };

    public static PoliticalDomainOfInfluence SG_Goldach_L5_MU
        => new()
        {
            Id = "a8d6078f-5ace-4284-a99f-d33a5df47f23",
            Name = "Goldach (MU)",
            ParentId = "03fd4057-9da9-4fd5-8310-d4d420762aef",
            Bfs = "3213",
            TenantName = "Gemeinde Goldach",
            TenantId = MockedTenantIds.MUGOLDACH,
            Type = DomainOfInfluenceType.Mu,
            Canton = DomainOfInfluenceCanton.Sg,
            ReturnAddress = new DomainOfInfluenceVotingCardReturnAddress
            {
                AddressLine1 = "Gemeindekanzlei Goldach",
                City = "Goldach",
                Country = "SWITZERLAND",
                Street = "Postfach",
                ZipCode = "9403",
            },
            SortNumber = 11,
            NameForProtocol = "Gemeinde Goldach",
        };

    public static PoliticalDomainOfInfluence SG_Rorschacherberg_L5_MU
        => new()
        {
            Id = "b0570f77-a2a8-4fb9-8092-d6916776624f",
            Name = "Rorschacherberg (MU)",
            ParentId = "03fd4057-9da9-4fd5-8310-d4d420762aef",
            TenantName = "Gemeinde Rorschacherberg",
            TenantId = MockedTenantIds.KTSG,
            Type = DomainOfInfluenceType.Mu,
            Canton = DomainOfInfluenceCanton.Unspecified,
        };

    public static PoliticalDomainOfInfluence SG_Moerschwil_L5_MU
        => new()
        {
            Id = "f53c1865-30e3-4c1e-88ae-7ea471daa4b4",
            Name = "Mörschwil (MU)",
            ParentId = "03fd4057-9da9-4fd5-8310-d4d420762aef",
            Bfs = "3214",
            TenantName = "Gemeinde Mörschwil",
            TenantId = MockedTenantIds.KTSG,
            Type = DomainOfInfluenceType.Mu,
            Canton = DomainOfInfluenceCanton.Sg,
            ECollectingEnabled = true,
            SortNumber = 10,
            NameForProtocol = "Gemeinde Mörschwil",
        };

    public static PoliticalDomainOfInfluence TG_Kanton_Thurgau_L1_CH
        => new()
        {
            Id = "cf0fb17f-8e71-4f9e-ab80-6412d42d00aa",
            Name = "Kanton Thurgau (CH)",
            ParentId = string.Empty,
            Bfs = string.Empty,
            TenantName = "Staatskanzlei des Kantons Thurgau",
            TenantId = MockedTenantIds.KTTG,
            Type = DomainOfInfluenceType.Ch,
            Canton = DomainOfInfluenceCanton.Tg,
            Children = { TG_Auslandschweizer_L2_MU },
            ECollectingEnabled = true,
        };

    public static PoliticalDomainOfInfluence TG_Auslandschweizer_L2_MU
        => new()
        {
            Id = "3e2ff4ab-4147-4b29-bac7-1651e7d8422d",
            Name = "Auslandschweizer (MU)",
            ParentId = "cf0fb17f-8e71-4f9e-ab80-6412d42d00aa",
            Bfs = "4000",
            TenantName = "Staatskanzlei des Kantons Thurgau",
            TenantId = MockedTenantIds.KTTG,
            Type = DomainOfInfluenceType.Mu,
            Canton = DomainOfInfluenceCanton.Tg,
        };
}
