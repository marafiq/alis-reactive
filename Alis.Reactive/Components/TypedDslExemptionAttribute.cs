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
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal sealed class TypedDslExemptionAttribute : Attribute
    {
        internal TypedDslExemptionAttribute(string reason)
        {
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        }

        /// <summary>Why this untyped public surface is a legit exception, and what refinement is owed.</summary>
        internal string Reason { get; }
    }
}
