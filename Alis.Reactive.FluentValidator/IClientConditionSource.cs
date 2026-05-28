using System.Collections.Generic;
using FluentValidation;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>
    /// Implemented by ReactiveValidator to expose browser-safe conditions
    /// registered via WhenField(). The adapter reads this while extracting client rules.
    /// </summary>
    internal interface IClientConditionSource
    {
        IReadOnlyDictionary<IValidationRule, ClientRuleCondition> ClientConditions { get; }
    }
}
