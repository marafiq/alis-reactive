using System;
using System.Collections.Generic;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>A validator whose rules the browser receives: <see cref="ReactiveValidator{T}"/>.</summary>
    internal interface IClientValidationMetadataSource
    {
        /// <summary>The browser's rules; child validators are resolved from <paramref name="services"/>.</summary>
        IReadOnlyList<ClientValidationField> GetClientRules(IServiceProvider services);
    }
}
