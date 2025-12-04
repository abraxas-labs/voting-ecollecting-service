// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Runtime.CompilerServices;

namespace Voting.ECollecting.Admin.Core.Unit.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
#if UPDATE_SNAPSHOTS
        VerifierSettings.AutoVerify(false);
#endif
    }
}
