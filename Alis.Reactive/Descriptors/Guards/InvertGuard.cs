using System;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Guards
{
    public sealed class InvertGuard : Guard
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "not";

        public Guard Inner { get; }

        public InvertGuard(Guard inner)
        {
            Inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }
    }
}
