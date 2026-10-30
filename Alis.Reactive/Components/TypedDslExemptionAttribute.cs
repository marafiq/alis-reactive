using System;

namespace Alis.Reactive.Components
{
    /// <summary>
    /// Marks a component-slice public API that the typed-DSL analyzer (ALIS009) must not flag,
    /// for a legitimate, documented exception. The canonical case is a bridge to a Syncfusion
    /// MVC builder slot whose vendor type is <see cref="object"/> (render-time configuration the
    /// framework does not own). Every use carries a concrete reason and is greppable, so the set
    /// of sanctioned untyped escapes stays small, visible, and auditable — never a silent loophole.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class TypedDslExemptionAttribute : Attribute
    {
        /// <summary>Exempts the method, for the given reason.</summary>
        /// <param name="reason">Why the untyped surface is a legitimate exception, and what refinement is owed.</param>
        public TypedDslExemptionAttribute(string reason)
        {
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        }

        /// <summary>Why this untyped public surface is a legit exception, and what refinement is owed.</summary>
        public string Reason { get; }
    }
}
