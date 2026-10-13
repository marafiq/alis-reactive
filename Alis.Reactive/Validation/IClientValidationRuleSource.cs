using System;
using System.Collections.Generic;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// The browser rules of a validator, by its type: what a plan's forms check and what
    /// <c>plan.ClientRules&lt;TValidator&gt;()</c> returns. <c>AddReactiveFluentValidation</c> registers
    /// <c>ReactiveValidator</c>'s; another validation library registers its own, built with <see cref="ClientRuleSet"/>.
    /// </summary>
    public interface IClientValidationRuleSource
    {
        /// <summary>The browser rules the validator declares.</summary>
        /// <param name="validationSourceType">The validator's type.</param>
        /// <returns>Each member the validator names, with its rules.</returns>
        IReadOnlyList<ClientValidationField> GetClientRules(Type validationSourceType);
    }
}
