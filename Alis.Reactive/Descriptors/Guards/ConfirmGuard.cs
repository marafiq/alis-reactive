using System;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Guards
{
    public sealed class ConfirmGuard : Guard
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "confirm";

        public string Message { get; }

        public ConfirmGuard(string message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }
    }
}
