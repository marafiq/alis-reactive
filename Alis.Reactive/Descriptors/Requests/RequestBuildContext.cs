using System;
using System.Collections.Generic;

namespace Alis.Reactive.Descriptors.Requests
{
    /// <summary>
    /// Builder-phase metadata for a RequestDescriptor.
    /// Consumed at render time by ValidationResolver — never serialized to JSON.
    /// </summary>
    internal sealed class RequestBuildContext
    {
        internal Type? ValidatorType { get; }
        internal Dictionary<string, string>? ReadExprOverrides { get; }

        internal RequestBuildContext(Type? validatorType, Dictionary<string, string>? readExprOverrides)
        {
            ValidatorType = validatorType;
            ReadExprOverrides = readExprOverrides;
        }
    }
}
