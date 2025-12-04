# Architecture Tests

This project validates the solution architecture as defined within the [solution-architecture.puml](./solution-architecture.puml).

## Prerequisites and Docs

* [PlantUML](https://marketplace.visualstudio.com/items?itemName=jebbs.plantuml) Visual Studio Code extension to export `puml` to `PNG`.
* [ArchUnitNET](https://github.com/TNG/ArchUnitNET) library as NuGet package reference.

## Validate Solution Architecture

The defined solutionarchitecture within the [solution-architecture.puml](./solution-architecture.puml) can be validated running the [PlantUmlTests](./PlantUmlTests.cs)

In order to load the solution architecture respectively the solution assemblies, marker classes named `ArchMarker.cs` are used to load assemblies by type.

## Export PUML to PNG

1. In VS Code select the [solution-architecture.puml](./solution-architecture.puml).
2. Right click the diagram and `Export Workspace Diagrams` or press `F1` and type `PlantUML: Export Current Diagram`.
3. Select the desired file path.
4. Viev Report popup and navigate to the displayed path.
