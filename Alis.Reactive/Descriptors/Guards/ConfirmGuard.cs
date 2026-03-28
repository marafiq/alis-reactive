using System;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Guards
{
    public sealed class ConfirmGuard : Guard
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "confirm";

        public string Message { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal ConfirmGuard(string message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }
    }
}
