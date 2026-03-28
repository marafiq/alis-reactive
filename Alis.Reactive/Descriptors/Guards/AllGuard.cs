using System;
using System.Collections.Generic;

namespace Alis.Reactive.Descriptors.Guards
{
    /// <summary>
    /// Logical AND combinator: all child guards must pass for the condition to be true.
    /// Serialized as <c>kind: "all"</c> with a <c>guards</c> array in the JSON plan.
    /// </summary>
    /// <remarks>
    /// Requires at least two child guards. A single condition does not need a combinator;
    /// use <see cref="ValueGuard"/> directly. Combinators nest: an <see cref="AllGuard"/>
    /// can contain <see cref="AnyGuard"/> children and vice versa.
    /// </remarks>
    public sealed class AllGuard : Guard
    {
        /// <summary>Gets the type discriminator. Always <c>"all"</c>.</summary>
        [System.Text.Json.Serialization.JsonPropertyOrder(-1)]
        public string Kind => "all";

        /// <summary>Gets the child guards that must all evaluate to <see langword="true"/>.</summary>
        public IReadOnlyList<Guard> Guards { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        /// <param name="guards">At least two child guards to combine with logical AND.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="guards"/> is <see langword="null"/> or contains fewer than two entries.</exception>
        public AllGuard(IReadOnlyList<Guard> guards)
        {
            if (guards == null || guards.Count < 2)
                throw new ArgumentException("AllGuard requires at least two guards.", nameof(guards));
            Guards = guards;
        }
    }
}
