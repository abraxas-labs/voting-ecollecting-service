// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Abstractions.Adapter.VotingStimmregister;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Citizen.Adapter.VotingStimmregister;

// keep in sync with CollectionCitizens of the seeder data and the mock in Citizen.
public class VotingStimmregisterAdapterMock : IVotingStimmregisterAdapter
{
    public const string NoVotingRightPerson1Ssn = "756.7499.5587.93";
    public const string VotingRightPerson1Ssn = "756.2147.7416.95";
    public const string VotingRightPerson2Ssn = "756.6819.4412.96";
    public const string VotingRightPerson3Ssn = "756.7799.2101.55";
    public const string VotingRightPerson4Ssn = "756.0270.4688.85";
    public const string VotingRightPerson5Ssn = "756.5215.1085.67";
    public const string VotingRightPerson6Ssn = "756.3211.0746.19";
    public const string VotingRightPerson7Ssn = "756.4120.6846.47";
    public const string VotingRightPerson8Ssn = "756.3476.4914.59";
    public const string VotingRightPerson9Ssn = "756.2142.4065.99";
    public const string VotingRightPerson10Ssn = "756.9474.5303.74";
    public const string VotingRightPerson11Ssn = "756.2318.8662.02";
    public const string VotingRightPerson12Ssn = "756.0051.1442.43";

    public static readonly PersonInfo VotingRightPerson1 =
        new PersonInfo(
            Guid.Parse("1482ac2e-8dc4-424f-ae47-6b722a359d0b"),
            61,
            1,
            3203,
            "Stadt St.Gallen");

    public static readonly PersonInfo VotingRightPerson2 =
        new PersonInfo(
            Guid.Parse("5898d5f7-8659-400d-bf38-92cfe52d98eb"),
            55,
            1,
            3203,
            "Stadt St.Gallen");

    public static readonly PersonInfo VotingRightPerson3 =
        new PersonInfo(
            Guid.Parse("c17581ba-d985-44a8-8be0-4b3d6f321860"),
            18,
            1,
            3203,
            "Stadt St.Gallen");

    public static readonly PersonInfo VotingRightPerson4 =
        new PersonInfo(
            Guid.Parse("92a99a29-e9e2-4a33-962e-7f45b9fbb6d9"),
            53,
            2,
            3203,
            "Stadt St.Gallen");

    public static readonly PersonInfo VotingRightPerson5 =
        new PersonInfo(
            Guid.Parse("f4a01bcf-08e9-4d90-91d5-c00fbecf1180"),
            91,
            1,
            3203,
            "Stadt St.Gallen");

    public static readonly PersonInfo VotingRightPerson6 =
        new PersonInfo(
            Guid.Parse("8f1e02cc-6d35-469e-9e81-ce66e12abe94"),
            77,
            1,
            3213,
            "Goldach");

    public static readonly PersonInfo VotingRightPerson7 =
        new PersonInfo(
            Guid.Parse("510ecfff-62f5-41ce-91cc-9b734d5d3a8a"),
            37,
            2,
            3213,
            "Goldach");

    public static readonly PersonInfo VotingRightPerson8 =
        new PersonInfo(
            Guid.Parse("0730dd3d-7e0d-41fc-b5c4-31baa485d7a2"),
            82,
            1,
            3203,
            "St.Gallen");

    public static readonly PersonInfo VotingRightPerson9 =
        new PersonInfo(
            Guid.Parse("5f6a1b12-3e64-4d4c-9f31-d9c8f0dcd001"),
            46,
            2,
            3203,
            "St.Gallen");

    public static readonly PersonInfo VotingRightPerson10 =
        new PersonInfo(
            Guid.Parse("2a1dfae7-41c0-4853-8c66-3c889e45c9e9"),
            70,
            1,
            3203,
            "St.Gallen");

    public static readonly PersonInfo VotingRightPerson11 =
        new PersonInfo(
            Guid.Parse("b6c0f991-8df5-44b0-8e2a-5a2161bbf452"),
            35,
            3,
            3203,
            "St.Gallen");

    public static readonly PersonInfo VotingRightPerson12 =
        new PersonInfo(
            Guid.Parse("de7e1a43-2e2c-42b1-b1a8-f7a89b23f781"),
            58,
            2,
            3203,
            "St.Gallen");

    private static readonly IEnumerable<(string Ssn, PersonInfo Person)> _all = [
        (VotingRightPerson1Ssn, VotingRightPerson1),
        (VotingRightPerson2Ssn, VotingRightPerson2),
        (VotingRightPerson3Ssn, VotingRightPerson3),
        (VotingRightPerson4Ssn, VotingRightPerson4),
        (VotingRightPerson5Ssn, VotingRightPerson5),
        (VotingRightPerson6Ssn, VotingRightPerson6),
        (VotingRightPerson7Ssn, VotingRightPerson7),
        (VotingRightPerson8Ssn, VotingRightPerson8),
        (VotingRightPerson9Ssn, VotingRightPerson9),
        (VotingRightPerson10Ssn, VotingRightPerson10),
        (VotingRightPerson11Ssn, VotingRightPerson11),
        (VotingRightPerson12Ssn, VotingRightPerson12),
    ];

    private static readonly IReadOnlyDictionary<(string Ssn, DomainOfInfluenceType DoiType, string Bfs), PersonInfo> _votingRightOk =
            _all.SelectMany(e => new[]
                {
                    new { e.Ssn, DoiType = DomainOfInfluenceType.Ch, Bfs = "1", e.Person }, // Bund
                    new { e.Ssn, DoiType = DomainOfInfluenceType.Ct, Bfs = "17", e.Person }, // canton st. gallen
                    new { e.Ssn, DoiType = DomainOfInfluenceType.Mu, Bfs = e.Person.MunicipalityId.ToString(), e.Person, },
                })
                .ToDictionary(x => (x.Ssn, x.DoiType, x.Bfs), x => x.Person);

    public Task<bool> HasVotingRight(string socialSecurityNumber, DomainOfInfluenceType doiType, string bfs)
        => Task.FromResult(_votingRightOk.ContainsKey((socialSecurityNumber, doiType, bfs)));

    public Task<IVotingStimmregisterPersonInfo> GetPersonInfo(string socialSecurityNumber, DomainOfInfluenceType doiType, string bfs)
    {
        if (!_votingRightOk.TryGetValue((socialSecurityNumber, doiType, bfs), out var personInfo))
        {
            throw new PersonOrVotingRightNotFoundException();
        }

        return Task.FromResult<IVotingStimmregisterPersonInfo>(personInfo);
    }
}
