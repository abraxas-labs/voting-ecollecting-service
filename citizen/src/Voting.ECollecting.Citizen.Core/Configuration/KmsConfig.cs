// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Citizen.Core.Configuration;

public class KmsConfig : Lib.Cryptography.Kms.Configuration.KmsConfig
{
    public bool EnableMock { get; set; }
}
