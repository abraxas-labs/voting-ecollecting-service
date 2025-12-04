// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Voting.ECollecting.Citizen.Architecture.Unit.Tests;

public class PlantUmlTests
{
    private static readonly ArchUnitNET.Domain.Architecture _architecture =
        new ArchLoader().LoadAssemblies(
                typeof(Abstractions.Adapter.Data.Markers.IArchMarker).Assembly,
                typeof(Abstractions.Adapter.ELogin.Markers.IArchMarker).Assembly,
                typeof(Abstractions.Adapter.VotingStimmregister.Markers.IArchMarker).Assembly,
                typeof(Abstractions.Adapter.Admin.Markers.IArchMarker).Assembly,
                typeof(Abstractions.Api.Markers.IArchMarker).Assembly,
                typeof(Abstractions.Core.Markers.IArchMarker).Assembly,
                typeof(Adapter.Data.Markers.IArchMarker).Assembly,
                typeof(Adapter.ELogin.Markers.IArchMarker).Assembly,
                typeof(Adapter.VotingStimmregister.Markers.IArchMarker).Assembly,
                typeof(Adapter.Admin.Markers.IArchMarker).Assembly,
                typeof(Api.Markers.IArchMarker).Assembly,
                typeof(Core.Markers.IArchMarker).Assembly,
                typeof(Domain.Markers.IArchMarker).Assembly,
                typeof(Shared.Abstractions.Adapter.Data.Markers.IArchMarker).Assembly,
                typeof(Shared.Abstractions.Core.Markers.IArchMarker).Assembly,
                typeof(Shared.Domain.Markers.IArchMarker).Assembly,
                typeof(Shared.Core.Markers.IArchMarker).Assembly,
                typeof(WebService.Markers.IArchMarker).Assembly).Build();

    [Fact]
    public void SolutionArchitectureShouldMatchPlantUml()
    {
        const string solutionArchitectureDiagram = "./solution-architecture.puml";
        var solutionArchitectureRule = Types().That().ResideInNamespace("Voting.ECollecting.Citizen.*", true).Should().AdhereToPlantUmlDiagram(solutionArchitectureDiagram);
        solutionArchitectureRule.Check(_architecture);
    }
}
