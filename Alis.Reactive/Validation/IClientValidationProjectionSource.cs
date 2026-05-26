using System;
using System.Collections.Generic;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Projects the deterministic browser validation rules for a validation source type.
    /// </summary>
    public interface IClientValidationProjectionSource
    {
        IReadOnlyList<ClientValidationField> ProjectClientRules(Type validationSourceType);
    }
}
