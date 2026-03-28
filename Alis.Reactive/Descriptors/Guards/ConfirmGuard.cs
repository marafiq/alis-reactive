using System;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Guards
{
    /// <summary>
    /// A halting guard that pauses pipeline execution and prompts the user for confirmation.
    /// Serialized as <c>kind: "confirm"</c> with a <c>message</c> string in the JSON plan.
    /// </summary>
    /// <remarks>
    /// Unlike boolean guards, a confirm guard does not test a value. The runtime displays
    /// a confirmation dialog with <see cref="Message"/>. If the user confirms, the pipeline
    /// continues. If the user cancels, the pipeline halts. This guard cannot be composed
    /// inside <see cref="AllGuard"/> or <see cref="AnyGuard"/> because it has side effects.
    /// </remarks>
    public sealed class ConfirmGuard : Guard
    {
        /// <summary>Gets the type discriminator. Always <c>"confirm"</c>.</summary>
        [JsonPropertyOrder(-1)]
        public string Kind => "confirm";

        /// <summary>Gets the message displayed to the user in the confirmation dialog.</summary>
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
