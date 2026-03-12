using System;

namespace Alis.Reactive.Descriptors.Requests
{
    /// <summary>
    /// Builder-phase metadata for a RequestDescriptor.
    /// Consumed at render time by ValidationResolver — never serialized to JSON.
    /// </summary>
    internal sealed class RequestBuildContext
    {
        internal Type? ValidatorType { get; }

        internal RequestBuildContext(Type? validatorType)
        {
            ValidatorType = validatorType;
        }
    }
}
