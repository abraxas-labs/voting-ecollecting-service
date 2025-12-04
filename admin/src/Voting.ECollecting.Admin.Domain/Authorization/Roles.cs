// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Admin.Domain.Authorization;

public static class Roles
{
    public const string ApiNotify = "ApiNotify";
    public const string Stammdatenverwalter = "Stammdatenverwalter";
    public const string Kontrollzeichenerfasser = "Kontrollzeichenerfasser";
    public const string Kontrollzeichenloescher = "Kontrollzeichenlöscher";
    public const string Zertifikatsverwalter = "Zertifikatsverwalter";
    public const string Stichprobenverwalter = "Stichprobenverwalter";

    public static IEnumerable<string> All()
    {
        yield return ApiNotify;
        yield return Stammdatenverwalter;
        yield return Kontrollzeichenerfasser;
        yield return Kontrollzeichenloescher;
        yield return Zertifikatsverwalter;
        yield return Stichprobenverwalter;
    }

    public static IEnumerable<string> AllHumanUserRoles()
    {
        yield return Stammdatenverwalter;
        yield return Kontrollzeichenerfasser;
        yield return Kontrollzeichenloescher;
        yield return Zertifikatsverwalter;
        yield return Stichprobenverwalter;
    }
}
