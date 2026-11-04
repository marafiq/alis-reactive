using System.Collections.Generic;

namespace Alis.Reactive
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

        /// <summary>The browser rules the validator declares in one set, as <c>Validate&lt;TValidator&gt;(formId, ruleSet)</c> reads them.</summary>
        /// <typeparam name="TValidator">The validator's type.</typeparam>
        /// <param name="ruleSet">The set, as the validator's <c>RuleSet</c> named it; a set it does not declare is refused.</param>
        /// <returns>Each member the set names, with its rules.</returns>
        IReadOnlyList<BrowserField> GetBrowserRules<TValidator>(string ruleSet) where TValidator : class;
    }
}
