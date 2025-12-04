// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Voting.ECollecting.Shared.Core.Services;
using Xunit;

namespace Voting.ECollecting.Shared.Core.Unit.Tests;

public class CollectionCryptoIdSerializerTest
{
    [Fact]
    public void SerializeStimmregisterIdShouldReturnCorrectBigEndianByteArray()
    {
        var testGuid = new Guid("c4e67305-64de-4613-8178-f71694f4107f");
        byte[] expectedBytes = [0xC4, 0xE6, 0x73, 0x05, 0x64, 0xDE, 0x46, 0x13, 0x81, 0x78, 0xF7, 0x16, 0x94, 0xF4, 0x10, 0x7F];
        var serializedBytes = CollectionCryptoIdSerializer.SerializeStimmregisterId(testGuid);
        serializedBytes.Should().Equal(expectedBytes);
    }

    [Fact]
    public void DeserializeStimmregisterIdShouldReturnCorrectGuidFromBigEndianByteArray()
    {
        var expectedGuid = new Guid("c4e67305-64de-4613-8178-f71694f4107f");
        byte[] serializedBytes = [0xC4, 0xE6, 0x73, 0x05, 0x64, 0xDE, 0x46, 0x13, 0x81, 0x78, 0xF7, 0x16, 0x94, 0xF4, 0x10, 0x7F];
        var deserializedGuid = CollectionCryptoIdSerializer.DeserializeStimmregisterId(serializedBytes);
        deserializedGuid.Should().Be(expectedGuid);
    }

    [Fact]
    public void RoundtripSerializationShouldPreserveOriginalGuid()
    {
        var originalGuid = Guid.NewGuid();
        var serializedBytes = CollectionCryptoIdSerializer.SerializeStimmregisterId(originalGuid);
        var deserializedGuid = CollectionCryptoIdSerializer.DeserializeStimmregisterId(serializedBytes);
        deserializedGuid.Should().Be(originalGuid);
    }
}
