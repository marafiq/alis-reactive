using System;
using System.Collections.Generic;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Provides deterministic client validation rules for a validation source type.
    /// </summary>
    public interface IClientValidationRuleSource
    {
        IReadOnlyList<ClientValidationField> GetClientRules(Type validationSourceType);
    }
}
