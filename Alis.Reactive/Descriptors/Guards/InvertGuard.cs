using System;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Guards
{
    public sealed class InvertGuard : Guard
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "not";

        public Guard Inner { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal InvertGuard(Guard inner)
        {
            Inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }
    }
}
