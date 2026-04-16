// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Abstractions.Adapter.VotingStimmregister;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.Lib.Database.Models;
using IVotingStimmregisterPersonInfo = Voting.ECollecting.Admin.Domain.Models.IVotingStimmregisterPersonInfo;

namespace Voting.ECollecting.Admin.Adapter.VotingStimmregister;

// keep in sync with CollectionCitizens of the seeder data and the mock in Citizen.
public class VotingStimmregisterAdapterMock : IVotingStimmregisterAdapter
{
    public static readonly PersonInfo NoVotingRightPerson1 =
        new PersonInfo(
            Guid.Parse("12e1033d-f3d2-4edb-bd35-647da5defbd8"),
            false,
            true,
            false,
            "Muster",
            "Petra",
            new DateOnly(2002, 05, 05),
            23,
            2,
            "Kirchenstrasse",
            "2",
            "St.Gallen",
            "9001",
            3203,
            "Stadt St.Gallen");

    public static readonly PersonInfo NoVotingRightPerson2 =
        new PersonInfo(
            Guid.Parse("2fa9436a-0f02-41c1-8538-b10183064d97"),
            false,
            true,
            true,
            "Abraham",
            "Cheyenne",
            new DateOnly(1942, 01, 2),
            89,
            2,
            "Eschenstr.",
            "11",
            "St. Gallen",
            "9000",
            3213,
            "Goldach");

    public static readonly PersonInfo NoVotingRightPerson3 =
        new PersonInfo(
            Guid.Parse("0c4a0e32-e772-43b0-a38e-80cbbff2a7fb"),
            false,
            true,
            true,
            "Abicht",
            "Alicia",
            new DateOnly(1975, 05, 20),
            25,
            2,
            "Schwalbenstr.",
            "2",
            "St. Gallen",
            "9000",
            3203,
            "St.Gallen");

    public static readonly PersonInfo VotingRightPerson1 =
        new PersonInfo(
            Guid.Parse("daa64beb-14ad-42d0-94f8-8396aab60393"),
            true,
            true,
            true,
            "BolligerT",
            "LarsT",
            new DateOnly(1952, 04, 06),
            61,
            1,
            "Rorschacher Strasse",
            "75",
            "St. Gallen",
            "9000",
            3203,
            "Stadt St.Gallen");

    public static readonly PersonInfo VotingRightPerson2 =
        new PersonInfo(
            Guid.Parse("0694f1fd-86c9-40bc-9521-4d2879325c67"),
            true,
            true,
            true,
            "AbramovicT",
            "AdelinaT",
            new DateOnly(2004, 11, 25),
            55,
            2,
            "Zwinglistrasse",
            "23",
            "St. Gallen",
            "9000",
            3203,
            "Stadt St.Gallen");

    public static readonly PersonInfo VotingRightPerson3 =
        new PersonInfo(
            Guid.Parse("1b92b658-e4f9-4a22-897f-3c5117371290"),
            true,
            true,
            true,
            "GeigerT",
            "PeterT",
            new DateOnly(1999, 05, 18),
            18,
            1,
            "Höhenweg",
            "60",
            "St. Gallen",
            "9000",
            3203,
            "Stadt St.Gallen");

    public static readonly PersonInfo VotingRightPerson4 =
        new PersonInfo(
            Guid.Parse("bd151bbe-5120-4ec3-a430-d3771b858745"),
            true,
            true,
            true,
            "ZieglerT",
            "JulienneT",
            new DateOnly(2000, 06, 03),
            53,
            2,
            "Flaschnerweg",
            "5",
            "St. Gallen",
            "9008",
            3203,
            "Stadt St.Gallen");

    public static readonly PersonInfo VotingRightPerson5 =
        new PersonInfo(
            Guid.Parse("47da7287-31ef-4e39-9903-f40676c0213c"),
            true,
            true,
            true,
            "AchillesT",
            "CassandraT",
            new DateOnly(1945, 11, 11),
            91,
            2,
            "Ringelbergstrasse",
            "7",
            "St. Gallen",
            "9000",
            3203,
            "Stadt St.Gallen");

    public static readonly PersonInfo VotingRightPerson6 =
        new PersonInfo(
            Guid.Parse("fabc0bf4-4f4e-4eaf-a4d2-af539eaf7da3"),
            true,
            true,
            true,
            "BaderT",
            "HansT",
            new DateOnly(1917, 03, 19),
            77,
            1,
            "Hölderlinstrasse",
            "17",
            "St. Gallen",
            "9008",
            3213,
            "Goldach");

    public static readonly PersonInfo VotingRightPerson7 =
        new PersonInfo(
            Guid.Parse("ec45d7fc-b5e8-4b5a-b4c4-8f03865864da"),
            true,
            true,
            true,
            "SchaubT",
            "AngelaT",
            new DateOnly(1946, 07, 05),
            37,
            2,
            "Oststrasse",
            "19",
            "St. Gallen",
            "9000",
            3213,
            "Goldach");

    public static readonly PersonInfo VotingRightPerson8 =
        new PersonInfo(
            Guid.Parse("9f30ff5d-2b3f-4dd0-8c83-e5aad05d2470"),
            true,
            true,
            true,
            "Zimmer",
            "Hugo",
            new DateOnly(1943, 05, 08),
            82,
            1,
            "Zürcherstr.",
            "93",
            "St. Gallen",
            "9000",
            3203,
            "St.Gallen");

    public static readonly PersonInfo VotingRightPerson9 =
        new PersonInfo(
            Guid.Parse("5f6a1b12-3e64-4d4c-9f31-d9c8f0dcd001"),
            true,
            true,
            true,
            "Meier",
            "Claudia",
            new DateOnly(1978, 11, 22),
            46,
            2,
            "Bahnhofstr.",
            "15",
            "St. Gallen",
            "9000",
            3203,
            "St.Gallen");

    public static readonly PersonInfo VotingRightPerson10 =
        new PersonInfo(
            Guid.Parse("2a1dfae7-41c0-4853-8c66-3c889e45c9e9"),
            true,
            true,
            true,
            "Keller",
            "Thomas",
            new DateOnly(1955, 03, 14),
            70,
            1,
            "Schulhausgasse",
            "7",
            "St. Gallen",
            "9000",
            3203,
            "St.Gallen");

    public static readonly PersonInfo VotingRightPerson11 =
        new PersonInfo(
            Guid.Parse("b6c0f991-8df5-44b0-8e2a-5a2161bbf452"),
            true,
            true,
            true,
            "Marti",
            "Sandra",
            new DateOnly(1990, 09, 03),
            35,
            3,
            "Seestrasse",
            "128",
            "St. Gallen",
            "9000",
            3203,
            "St.Gallen");

    public static readonly PersonInfo VotingRightPerson12 =
        new PersonInfo(
            Guid.Parse("de7e1a43-2e2c-42b1-b1a8-f7a89b23f781"),
            true,
            true,
            true,
            "Schneider",
            "Markus",
            new DateOnly(1967, 01, 30),
            58,
            2,
            "Dorfstr.",
            "45",
            "St. Gallen",
            "9000",
            3203,
            "St.Gallen");

    private static IEnumerable<IVotingStimmregisterPersonInfo> All { get; } = [
        NoVotingRightPerson1,
        NoVotingRightPerson2,
        NoVotingRightPerson3,
        VotingRightPerson1,
        VotingRightPerson2,
        VotingRightPerson3,
        VotingRightPerson4,
        VotingRightPerson5,
        VotingRightPerson6,
        VotingRightPerson7,
        VotingRightPerson8,
        VotingRightPerson9,
        VotingRightPerson10,
        VotingRightPerson11,
        VotingRightPerson12,
    ];

    public Task<Page<IVotingStimmregisterPersonInfo>> ListPersonInfos(
        VotingStimmregisterPersonFilterData filterData,
        Pageable? pageable = null,
        CancellationToken cancellationToken = default)
    {
        var result = new List<IVotingStimmregisterPersonInfo>();

        foreach (var personInfo in All)
        {
            if (
                (string.IsNullOrEmpty(filterData.OfficialName) || personInfo.OfficialName.StartsWith(filterData.OfficialName, StringComparison.OrdinalIgnoreCase))
                && (string.IsNullOrEmpty(filterData.FirstName) || personInfo.FirstName.StartsWith(filterData.FirstName, StringComparison.OrdinalIgnoreCase))
                && (string.IsNullOrEmpty(filterData.ResidenceAddressStreet) || personInfo.ResidenceAddressStreet.StartsWith(filterData.ResidenceAddressStreet, StringComparison.OrdinalIgnoreCase))
                && (string.IsNullOrEmpty(filterData.ResidenceAddressHouseNumber) || personInfo.ResidenceAddressHouseNumber.StartsWith(filterData.ResidenceAddressHouseNumber, StringComparison.OrdinalIgnoreCase))
                && (!filterData.DateOfBirth.HasValue || filterData.DateOfBirth.Value == personInfo.DateOfBirth))
            {
                result.Add(personInfo);
            }
        }

        return Task.FromResult(new Page<IVotingStimmregisterPersonInfo>(result, result.Count, 1, Math.Max(1, result.Count)));
    }

    public async Task<IVotingStimmregisterPersonInfo> GetPersonInfo(
        VotingStimmregisterPersonFilterData filterData,
        CancellationToken cancellationToken = default)
    {
        var people = await ListPersonInfos(filterData, null, cancellationToken);
        return people.TotalItemsCount == 1
            ? people.Items[0]
            : throw new PersonNotFoundException();
    }

    public Task<IVotingStimmregisterPersonInfo> GetPersonInfo(
        string bfs,
        Guid registerId,
        DateTime actualityDate,
        CancellationToken cancellationToken = default)
    {
        var person = All.FirstOrDefault(x => x.RegisterId == registerId && x.MunicipalityId.ToString() == bfs);
        return person != null
            ? Task.FromResult(person)
            : throw new PersonNotFoundException();
    }

    public Task<IReadOnlyList<IVotingStimmregisterPersonInfo>> GetPersonInfos(
        string bfs,
        IReadOnlySet<Guid> registerIds,
        DateTime actualityDate,
        CancellationToken cancellationToken = default)
    {
        var people = All.Where(p => registerIds.Contains(p.RegisterId) && bfs == p.MunicipalityId.ToString()).ToList();
        if (people.Count != registerIds.Count)
        {
            throw new PersonNotFoundException();
        }

        return Task.FromResult<IReadOnlyList<IVotingStimmregisterPersonInfo>>(people);
    }
}
