using System;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Guards
{
    /// <summary>
    /// Logical NOT wrapper: inverts the result of the <see cref="Inner"/> guard.
    /// Serialized as <c>kind: "not"</c> with a single <c>inner</c> guard in the JSON plan.
    /// </summary>
    /// <remarks>
    /// Wraps any <see cref="Guard"/> subtree. The runtime evaluates the inner guard
    /// and negates its boolean result. Can wrap leaf guards (<see cref="ValueGuard"/>),
    /// combinators (<see cref="AllGuard"/>, <see cref="AnyGuard"/>), or other inversions.
    /// </remarks>
    public sealed class InvertGuard : Guard
    {
        /// <summary>Gets the type discriminator. Always <c>"not"</c>.</summary>
        [JsonPropertyOrder(-1)]
        public string Kind => "not";

        /// <summary>Gets the child guard whose result is negated.</summary>
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
