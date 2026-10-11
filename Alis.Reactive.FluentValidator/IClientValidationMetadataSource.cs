using System.Collections.Generic;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>A validator whose rules the browser receives: <see cref="ReactiveValidator{T}"/>.</summary>
    internal interface IClientValidationMetadataSource
    {
        /// <summary>The browser's rules, with those of the child validators the rules hold.</summary>
        IReadOnlyList<ClientValidationField> GetClientRules();
    }
}
