// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Reflection;
using Xunit.Sdk;

namespace Voting.ECollecting.Shared.Test.Utils;

public class EnumDataAttribute<T>(bool includeDefaultValue = false) : DataAttribute
    where T : struct, Enum
{
    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        foreach (var value in Enum.GetValues<T>())
        {
            if (!includeDefaultValue && default(T).Equals(value))
            {
                continue;
            }

            yield return [value];
        }
    }
}
