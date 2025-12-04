// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Runtime.CompilerServices;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        // contains large blob of binary data.
        VerifierSettings.IgnoreMember<FileContentEntity>(x => x.Data);

        // not deterministic
        VerifierSettings.IgnoreMember<CollectionCitizenLogEntity>(x => x.VotingStimmregisterIdEncrypted);

        // verify the record json objects of the audit trail (required for dynamic json objects).
        VerifySystemJson.Initialize();

#if UPDATE_SNAPSHOTS
        VerifierSettings.AutoVerify(false);
#endif
    }
}
