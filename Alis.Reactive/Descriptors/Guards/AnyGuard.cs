using System;
using System.Collections.Generic;

namespace Alis.Reactive.Descriptors.Guards
{
    /// <summary>
    /// Logical OR combinator: at least one child guard must pass for the condition to be true.
    /// Serialized as <c>kind: "any"</c> with a <c>guards</c> array in the JSON plan.
    /// </summary>
    /// <remarks>
    /// Requires at least two child guards. A single condition does not need a combinator;
    /// use <see cref="ValueGuard"/> directly. Combinators nest: an <see cref="AnyGuard"/>
    /// can contain <see cref="AllGuard"/> children and vice versa.
    /// </remarks>
    public sealed class AnyGuard : Guard
    {
        /// <summary>Gets the type discriminator. Always <c>"any"</c>.</summary>
        [System.Text.Json.Serialization.JsonPropertyOrder(-1)]
        public string Kind => "any";

        /// <summary>Gets the child guards where at least one must evaluate to <see langword="true"/>.</summary>
        public IReadOnlyList<Guard> Guards { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        /// <param name="guards">At least two child guards to combine with logical OR.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="guards"/> is <see langword="null"/> or contains fewer than two entries.</exception>
        internal AnyGuard(IReadOnlyList<Guard> guards)
        {
            if (guards == null || guards.Count < 2)
                throw new ArgumentException("AnyGuard requires at least two guards.", nameof(guards));
            Guards = guards;
        }
    }
}
