using System.Collections.Generic;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    internal interface IClientValidationMetadataSource
    {
        IReadOnlyList<ClientValidationField> GetClientRules();
    }
}
