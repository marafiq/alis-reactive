using System;
using System.Collections.Generic;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Client-side validation rule sets keyed by their validation source type.
    /// </summary>
    public sealed class ClientValidationRules : IClientValidationRuleSource
    {
        private readonly IReadOnlyDictionary<Type, IReadOnlyList<ClientValidationField>> _fieldsBySource;

        private ClientValidationRules(
            IReadOnlyDictionary<Type, IReadOnlyList<ClientValidationField>> fieldsBySource)
        {
            _fieldsBySource = fieldsBySource ?? throw new ArgumentNullException(nameof(fieldsBySource));
        }

        public static ClientValidationRules Create(
            params ClientValidationRuleSetDefinition[] ruleSets)
        {
            if (ruleSets == null) throw new ArgumentNullException(nameof(ruleSets));

            var fieldsBySource = new Dictionary<Type, IReadOnlyList<ClientValidationField>>();
            foreach (var ruleSet in ruleSets)
            {
                if (ruleSet == null)
                    throw new ArgumentException("Client validation rule set must not be null.", nameof(ruleSets));

                ruleSet.AddTo(fieldsBySource);
            }

            return new ClientValidationRules(fieldsBySource);
        }

        public static ClientValidationRuleSetDefinition For<TValidationSource, TModel>(
            Action<ClientValidationRulesBuilder<TModel>> define)
            where TModel : class =>
            ClientValidationRuleSetDefinition.For<TValidationSource, TModel>(define);

        public IReadOnlyList<ClientValidationField> GetClientRules(Type validationSourceType)
        {
            if (validationSourceType == null) throw new ArgumentNullException(nameof(validationSourceType));

            if (_fieldsBySource.TryGetValue(validationSourceType, out var fields))
                return fields;

            throw new InvalidOperationException(
                $"No client validation rules are defined for validation source '{validationSourceType.FullName}'. " +
                "Add them with ClientValidationRules.For<TValidationSource, TModel>(rules => ...).");
        }
    }

    public sealed class ClientValidationRuleSetDefinition
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
