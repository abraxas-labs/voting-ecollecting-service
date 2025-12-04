// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Abstractions.Adapter.VotingStimmregister;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.Lib.Database.Models;
using IVotingStimmregisterPersonInfo = Voting.ECollecting.Admin.Domain.Models.IVotingStimmregisterPersonInfo;

namespace Voting.ECollecting.Admin.Adapter.VotingStimmregister;

// keep in sync with CollectionCitizens and InitiativeCommitteeMembers of the seeder data and the mock in Citizen.
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
            Guid.Parse("1482ac2e-8dc4-424f-ae47-6b722a359d0b"),
            true,
            true,
            true,
            "Trautmann",
            "Lars",
            new DateOnly(1963, 12, 14),
            61,
            1,
            "Vonwilstr.",
            "29",
            "St. Gallen",
            "9000",
            3203,
            "Stadt St.Gallen");

    public static readonly PersonInfo VotingRightPerson2 =
        new PersonInfo(
            Guid.Parse("5898d5f7-8659-400d-bf38-92cfe52d98eb"),
            true,
            true,
            true,
            "Abramovic",
            "Anthony",
            new DateOnly(1988, 3, 11),
            55,
            1,
            "Vonwilstr.",
            "29",
            "St. Gallen",
            "9000",
            3203,
            "Stadt St.Gallen");

    public static readonly PersonInfo VotingRightPerson3 =
        new PersonInfo(
            Guid.Parse("c17581ba-d985-44a8-8be0-4b3d6f321860"),
            true,
            true,
            true,
            "Golling",
            "Peter",
            new DateOnly(2006, 12, 02),
            18,
            1,
            "Tannenstr.",
            "42",
            "St. Gallen",
            "9010",
            3203,
            "Stadt St.Gallen");

    public static readonly PersonInfo VotingRightPerson4 =
        new PersonInfo(
            Guid.Parse("92a99a29-e9e2-4a33-962e-7f45b9fbb6d9"),
            true,
            true,
            true,
            "Ziegler",
            "Juliana",
            new DateOnly(1944, 12, 11),
            53,
            2,
            "Kreuzbühlstr.",
            "35",
            "St. Gallen",
            "9015",
            3203,
            "Stadt St.Gallen");

    public static readonly PersonInfo VotingRightPerson5 =
        new PersonInfo(
            Guid.Parse("f4a01bcf-08e9-4d90-91d5-c00fbecf1180"),
            true,
            true,
            true,
            "Achilles",
            "Linnea",
            new DateOnly(1940, 6, 28),
            91,
            1,
            "Müller-Friedberg-Str.",
            "24",
            "St. Gallen",
            "9008",
            3203,
            "Stadt St.Gallen");

    public static readonly PersonInfo VotingRightPerson6 =
        new PersonInfo(
            Guid.Parse("8f1e02cc-6d35-469e-9e81-ce66e12abe94"),
            true,
            true,
            true,
            "Hügel",
            "Hans",
            new DateOnly(1948, 01, 26),
            77,
            1,
            "Hölderlinstr.",
            "17",
            "St. Gallen",
            "9008",
            3213,
            "Goldach");

    public static readonly PersonInfo VotingRightPerson7 =
        new PersonInfo(
            Guid.Parse("510ecfff-62f5-41ce-91cc-9b734d5d3a8a"),
            true,
            true,
            true,
            "Abel",
            "Angela",
            new DateOnly(1988, 07, 25),
            37,
            2,
            "Oststr.",
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
