using System.Collections.Generic;
using FluentValidation;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>
    /// Implemented by ReactiveValidator to expose client-extractable conditions
    /// registered via WhenField(). The adapter reads this during extraction.
    /// </summary>
    internal interface IClientConditionSource
    {
        IReadOnlyDictionary<IValidationRule, ValidationCondition> ClientConditions { get; }
    }
}
