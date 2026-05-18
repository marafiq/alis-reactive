using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    internal sealed class Component
    {
        public string Id { get; }
        public string Vendor { get; }
        public string Type { get; }

        [System.Text.Json.Serialization.JsonInclude]
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        internal string? BindingPath { get; }

        /// <summary>Which member (property or method) on this component reads its form value.
        /// For input components: "value", "checked", etc. from IInputComponent.ValueMember.
        /// Used by IncludeAll to know which property to gather at runtime.</summary>
        [System.Text.Json.Serialization.JsonInclude]
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        internal string? ValueMember { get; }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public ContainerScope? Container { get; }

        private Component(string id, string vendor, string type,
            string? bindingPath, string? valueMember, ContainerScope? container)
        {
            Id = id ?? throw new System.ArgumentNullException(nameof(id));
            Vendor = vendor ?? throw new System.ArgumentNullException(nameof(vendor));
            Type = type ?? throw new System.ArgumentNullException(nameof(type));
            BindingPath = bindingPath;
            ValueMember = valueMember;
            Container = container;
        }

        internal static Component Create(string id, string vendor, string type,
            string? bindingPath = null, string? valueMember = null) =>
            new Component(id, vendor, type, bindingPath, valueMember, null);

        /// <summary>Returns a copy with binding info filled where currently absent (first registration wins).</summary>
        internal Component WithBindingIfAbsent(string? bindingPath, string? valueMember) =>
            new Component(Id, Vendor, Type,
                BindingPath ?? bindingPath, ValueMember ?? valueMember, Container);

        /// <summary>Returns a copy carrying the given container scope.</summary>
        internal Component WithContainer(ContainerScope container) =>
            new Component(Id, Vendor, Type, BindingPath, ValueMember, container);
    }

    internal sealed class ContainerScope
    {
        public IReadOnlyList<string> Components { get; }
        public IReadOnlyList<ComponentValidation> ValidationRules { get; }

        internal ContainerScope(
            IReadOnlyList<string> components,
            IReadOnlyList<ComponentValidation> validationRules)
        {
            Components = components;
            ValidationRules = validationRules;
        }

        internal static ContainerScope Of(params string[] components) =>
            new ContainerScope(new List<string>(components), new List<ComponentValidation>());

        /// <summary>
        /// Returns a copy whose validation rules are merged with <paramref name="incoming"/>.
        /// Rules for an already-present component are replaced; rules for components not yet
        /// present are appended. Used when more than one request validates the same container.
        /// </summary>
        internal ContainerScope WithValidationRulesMerged(IReadOnlyList<ComponentValidation> incoming)
        {
            if (ValidationRules.Count == 0)
                return new ContainerScope(Components, incoming);

            var byComponent = new Dictionary<string, ComponentValidation>();
            foreach (var rule in ValidationRules)
                byComponent[rule.Component] = rule;
            foreach (var rule in incoming)
                byComponent[rule.Component] = rule;
            return new ContainerScope(Components, new List<ComponentValidation>(byComponent.Values));
        }
    }

    /// <summary>Validation rules for a single component within a container scope.
    /// The Value producer reads the component's current value via the shared evaluateValue path.</summary>
    internal sealed class ComponentValidation
    {
        /// <summary>Component ID — used for DOM error display and serverFieldName mapping.</summary>
        public string Component { get; }
        /// <summary>How to read this component's value for validation. Evaluated via evaluateValue().</summary>
        public ValueProducer Value { get; }
        public IReadOnlyList<ValidationRule> Rules { get; }

        [System.Text.Json.Serialization.JsonInclude]
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        internal string? ServerFieldName { get; }

        internal ComponentValidation(string component, ValueProducer value, IReadOnlyList<ValidationRule> rules, string? serverFieldName = null)
        {
            Component = component;
            Value = value;
            Rules = rules;
            ServerFieldName = serverFieldName;
        }
    }

    internal sealed class ValidationRule
    {
        public string Name { get; }
        public string Message { get; }
        public ValueProducer Constraint { get; }

        /// <summary>For peer-comparison rules (equalTo, notEqualTo, min/max with cross-field).
        /// Pre-resolved by the orchestrator before passing to the pure rule engine.</summary>
        public ValueProducer OtherValue { get; }

        public Condition When { get; }

        public Shape Shape { get; }

        internal ValidationRule(string name, string message,
            ValueProducer? constraint = null, ValueProducer? otherValue = null,
            Condition? when = null, Shape? shape = null)
        {
            Name = name;
            Message = message;
            Constraint = constraint ?? ValueProducer.None;
            OtherValue = otherValue ?? ValueProducer.None;
            When = when ?? Condition.None;
            Shape = shape ?? Shape.None;
        }
    }
}
