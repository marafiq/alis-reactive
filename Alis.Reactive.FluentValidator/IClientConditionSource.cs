using System.Collections.Generic;
using FluentValidation;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>
    /// Implemented by ReactiveValidator to expose client-projectable conditions
    /// registered via WhenField(). The adapter reads this during projection.
    /// </summary>
    internal interface IClientConditionSource
    {
        IReadOnlyDictionary<IValidationRule, ClientConditionProjection> ClientConditions { get; }
    }
}
