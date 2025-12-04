// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Runtime.CompilerServices;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifierSettings.IgnoreMember<FileContentEntity>(x => x.Data); // large binary data
        VerifierSettings.IgnoreMember<CollectionCitizenLogEntity>(x => x.VotingStimmregisterIdEncrypted); // not deterministic

        VerifySystemJson.Initialize(); // verify the record json objects of the audit trail (required for dynamic json objects).

#if UPDATE_SNAPSHOTS
        VerifierSettings.AutoVerify(false);
#endif
    }
}
