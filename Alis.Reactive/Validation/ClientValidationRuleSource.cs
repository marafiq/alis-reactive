using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Alis.Reactive.Validation
{
    internal interface IClientValidationMetadataProvider
    {
        bool TryGetClientRules(
            Type validationSourceType,
            [NotNullWhen(true)]
            out IReadOnlyList<ClientValidationField>? fields);
    }

    internal sealed class ClientValidationRuleSource : IClientValidationRuleSource
    {
        private readonly IReadOnlyList<IClientValidationMetadataProvider> _providers;

        public ClientValidationRuleSource(IEnumerable<IClientValidationMetadataProvider> providers)
        {
            if (providers == null) throw new ArgumentNullException(nameof(providers));
            _providers = providers.ToArray();
        }

        public IReadOnlyList<ClientValidationField> GetClientRules(Type validationSourceType)
        {
            if (validationSourceType == null) throw new ArgumentNullException(nameof(validationSourceType));

            foreach (var provider in _providers)
                if (provider.TryGetClientRules(validationSourceType, out var fields))
                    return fields;

            throw new InvalidOperationException(
                $"No Reactive Plan client validation metadata is registered for validation source '{validationSourceType.FullName}'. " +
                "Register its validator with services.AddReactiveFluentValidation(rules => rules.Add<TValidator>()).");
        }
    }
}
