// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Admin.Core.Configuration;

public class KmsConfig : Lib.Cryptography.Kms.Configuration.KmsConfig
{
    public string KeyEnvironmentPrefix { get; set; } = string.Empty;

    public bool EnableMock { get; set; }
}
