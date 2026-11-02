using System.Collections.Generic;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// The browser rules of a validator, by its type: what a plan's forms check and what
    /// <c>plan.BrowserRules&lt;TValidator&gt;()</c> returns. <c>AddReactiveFluentValidation</c> registers
    /// <c>ReactiveValidator</c>'s; another validation library registers its own, built with <see cref="BrowserRuleSet"/>.
    /// </summary>
    public interface IBrowserRuleSource
    {
        /// <summary>The browser rules the validator declares.</summary>
        /// <typeparam name="TValidator">The validator's type.</typeparam>
        /// <returns>Each member the validator names, with its rules.</returns>
        IReadOnlyList<BrowserField> GetBrowserRules<TValidator>() where TValidator : class;
    }
}
