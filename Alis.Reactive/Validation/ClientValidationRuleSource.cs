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
                $"No browser validation metadata is registered for validation source '{validationSourceType.FullName}'. " +
                "Register FluentValidation metadata with services.AddReactiveFluentValidation(...), " +
                "or app-level rules with services.AddReactiveClientValidation(...).");
        }
    }

    internal sealed class ConfiguredClientValidationMetadataProvider : IClientValidationMetadataProvider
    {
        private readonly IReadOnlyDictionary<Type, IReadOnlyList<ClientValidationField>> _fieldsBySource;

        internal ConfiguredClientValidationMetadataProvider(
            IReadOnlyDictionary<Type, IReadOnlyList<ClientValidationField>> fieldsBySource)
        {
            if (fieldsBySource == null) throw new ArgumentNullException(nameof(fieldsBySource));

            // net48 has no Dictionary(IEnumerable<KeyValuePair>) ctor; project explicitly.
            _fieldsBySource = fieldsBySource.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public bool TryGetClientRules(
            Type validationSourceType,
            [NotNullWhen(true)]
            out IReadOnlyList<ClientValidationField>? fields)
        {
            if (validationSourceType == null) throw new ArgumentNullException(nameof(validationSourceType));
            return _fieldsBySource.TryGetValue(validationSourceType, out fields);
        }
    }
}
