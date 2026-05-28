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
            IEnumerable<ClientValidationRuleSetDefinition> ruleSets)
        {
            if (ruleSets == null) throw new ArgumentNullException(nameof(ruleSets));

            var fieldsBySource = new Dictionary<Type, IReadOnlyList<ClientValidationField>>();
            foreach (var ruleSet in ruleSets)
                ruleSet.AddTo(fieldsBySource);

            _fieldsBySource = fieldsBySource;
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

    internal sealed class ClientValidationRuleSetDefinition
    {
        private readonly Type _sourceType;
        private readonly IReadOnlyList<ClientValidationField> _fields;

        private ClientValidationRuleSetDefinition(
            Type sourceType,
            IReadOnlyList<ClientValidationField> fields)
        {
            _sourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
            _fields = fields ?? throw new ArgumentNullException(nameof(fields));
        }

        internal static ClientValidationRuleSetDefinition For<TValidationSource, TModel>(
            Action<ClientValidationRulesBuilder<TModel>> define)
            where TModel : class
        {
            if (define == null) throw new ArgumentNullException(nameof(define));

            var rules = new ClientValidationRulesBuilder<TModel>();
            define(rules);
            return new ClientValidationRuleSetDefinition(
                typeof(TValidationSource),
                rules.ToFields());
        }

        internal void AddTo(Dictionary<Type, IReadOnlyList<ClientValidationField>> fieldsBySource)
        {
            if (fieldsBySource == null) throw new ArgumentNullException(nameof(fieldsBySource));

            if (fieldsBySource.ContainsKey(_sourceType))
            {
                throw new InvalidOperationException(
                    $"Client validation rules for '{_sourceType.FullName}' are already defined.");
            }

            fieldsBySource.Add(_sourceType, _fields);
        }
    }
}
