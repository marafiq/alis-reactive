using System.Collections.Generic;
using Alis.Reactive.Validation;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// The validation rules of one form, by component; a second request that validates the same form
    /// replaces a component's rules and adds the components it newly names.
    /// </summary>
    internal sealed class ContainerScope
    {
        private readonly ContainerValidations _validations;

        private ContainerScope(ContainerValidations validations)
        {
            _validations = validations ?? throw new System.ArgumentNullException(nameof(validations));
        }

        public IReadOnlyList<ComponentValidation> ValidationRules => _validations.Items;

        internal static ContainerScope Empty { get; } =
            new ContainerScope(ContainerValidations.Empty);

        /// <summary>
        /// Copies the container with validation rules merged from <paramref name="incoming"/>.
        /// Rules for an already-present component are replaced; rules for components not yet
        /// present are appended. Used when more than one request validates the same container.
        /// </summary>
        internal ContainerScope WithValidationRulesMerged(IReadOnlyList<ComponentValidation> incoming)
        {
            var incomingValidations = ContainerValidations.From(incoming);
            return new ContainerScope(_validations.MergeReplacingComponentRules(incomingValidations));
        }
    }

    internal sealed class ContainerValidations
    {
        private readonly IReadOnlyList<ComponentValidation> _validations;

        private ContainerValidations(IReadOnlyList<ComponentValidation> validations)
        {
            _validations = validations;
        }

        internal IReadOnlyList<ComponentValidation> Items => _validations;

        private bool IsEmpty => _validations.Count == 0;

        internal static ContainerValidations Empty { get; } =
            new ContainerValidations(new List<ComponentValidation>());

        internal static ContainerValidations From(IEnumerable<ComponentValidation> validations)
        {
            if (validations == null) throw new System.ArgumentNullException(nameof(validations));

            var snapshot = new List<ComponentValidation>();
            foreach (var validation in validations)
            {
                if (validation == null)
                    throw new System.ArgumentException("Component validation must not be null.", nameof(validations));

                snapshot.Add(validation);
            }

            return new ContainerValidations(snapshot);
        }

        internal ContainerValidations MergeReplacingComponentRules(ContainerValidations incoming)
        {
            if (incoming == null) throw new System.ArgumentNullException(nameof(incoming));
            if (incoming.IsEmpty) return this;
            if (IsEmpty) return incoming;

            var byComponent = new Dictionary<string, ComponentValidation>(System.StringComparer.Ordinal);
            var componentOrder = new List<string>();
            foreach (var validation in _validations)
                AddOrReplace(validation, byComponent, componentOrder);
            foreach (var validation in incoming._validations)
                AddOrReplace(validation, byComponent, componentOrder);

            var merged = new List<ComponentValidation>();
            foreach (var component in componentOrder)
                merged.Add(byComponent[component]);

            return new ContainerValidations(merged);
        }

        private static void AddOrReplace(
            ComponentValidation validation,
            Dictionary<string, ComponentValidation> byComponent,
            List<string> componentOrder)
        {
            var componentIsNewToContainer = !byComponent.ContainsKey(validation.Component);
            if (componentIsNewToContainer)
                componentOrder.Add(validation.Component);

            byComponent[validation.Component] = validation;
        }
    }

    /// <summary>
    /// Validation rules and value source for one component within a validation container.
    /// </summary>
    internal sealed class ComponentValidation
    {
        private readonly ComponentId _component;
        private readonly ValidationFieldPath _serverFieldName;
        private readonly ComponentValidationRules _rules;

        /// <summary>Component ID used for DOM error display and server field matching.</summary>
        public string Component => _component.Value;
        /// <summary>Value expression evaluated before client validation runs.</summary>
        public ValueExpression Value { get; }
        public IReadOnlyList<ValidationRuleNode> Rules => _rules.Items;

        [System.Text.Json.Serialization.JsonInclude]
        internal string ServerFieldName => _serverFieldName.Value;

        private ComponentValidation(
            ComponentId component,
            ValueExpression value,
            ComponentValidationRules rules,
            ValidationFieldPath serverFieldName)
        {
            _component = component ?? throw new System.ArgumentNullException(nameof(component));
            Value = value ?? throw new System.ArgumentNullException(nameof(value));
            _rules = rules ?? throw new System.ArgumentNullException(nameof(rules));
            _serverFieldName = serverFieldName ?? throw new System.ArgumentNullException(nameof(serverFieldName));
        }

        internal static ComponentValidation ForServerField(
            string component,
            ValueExpression value,
            IReadOnlyList<ValidationRuleNode> rules,
            string serverFieldName) =>
            new ComponentValidation(
                ComponentId.Of(component),
                value,
                ComponentValidationRules.From(rules),
                ValidationFieldPath.Of(serverFieldName));
    }

    internal sealed class ComponentValidationRules
    {
        private readonly IReadOnlyList<ValidationRuleNode> _rules;

        private ComponentValidationRules(IReadOnlyList<ValidationRuleNode> rules)
        {
            _rules = rules;
        }

        internal IReadOnlyList<ValidationRuleNode> Items => _rules;

        internal static ComponentValidationRules From(IEnumerable<ValidationRuleNode> rules)
        {
            if (rules == null) throw new System.ArgumentNullException(nameof(rules));

            var snapshot = new List<ValidationRuleNode>();
            foreach (var rule in rules)
            {
                if (rule == null)
                    throw new System.ArgumentException("Validation rule must not be null.", nameof(rules));

                snapshot.Add(rule);
            }

            return new ComponentValidationRules(snapshot);
        }
    }
}
