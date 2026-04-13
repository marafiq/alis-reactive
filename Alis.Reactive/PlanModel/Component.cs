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
        internal string? BindingPath { get; set; }

        /// <summary>Which member (property or method) on this component reads its form value.
        /// For input components: "value", "checked", etc. from IInputComponent.ValueMember.
        /// Used by IncludeAll to know which property to gather at runtime.</summary>
        [System.Text.Json.Serialization.JsonInclude]
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        internal string? ValueMember { get; set; }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public ContainerScope? Container { get; internal set; }

        internal Component(string id, string vendor, string type)
        {
            Id = id ?? throw new System.ArgumentNullException(nameof(id));
            Vendor = vendor ?? throw new System.ArgumentNullException(nameof(vendor));
            Type = type ?? throw new System.ArgumentNullException(nameof(type));
        }

        internal static Component Create(string id, string vendor, string type) =>
            new Component(id, vendor, type);
    }

    internal sealed class ContainerScope
    {
        public List<string> Components { get; }
        public List<ComponentValidation> ValidationRules { get; internal set; } = new List<ComponentValidation>();

        internal ContainerScope(List<string> components)
        {
            Components = components;
        }

        internal static ContainerScope Of(params string[] components) =>
            new ContainerScope(new List<string>(components));
    }

    /// <summary>Validation rules for a single component within a container scope.
    /// The Value producer reads the component's current value via the shared evaluateValue path.</summary>
    internal sealed class ComponentValidation
    {
        /// <summary>Component ID — used for DOM error display and serverFieldName mapping.</summary>
        public string Component { get; }
        /// <summary>How to read this component's value for validation. Evaluated via evaluateValue().</summary>
        public ValueProducer Value { get; }
        public List<ValidationRule> Rules { get; }

        [System.Text.Json.Serialization.JsonInclude]
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        internal string? ServerFieldName { get; }

        internal ComponentValidation(string component, ValueProducer value, List<ValidationRule> rules, string? serverFieldName = null)
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
        public ValueProducer Constraint { get; internal set; } = ValueProducer.None;

        /// <summary>For peer-comparison rules (equalTo, notEqualTo, min/max with cross-field).
        /// Pre-resolved by the orchestrator before passing to the pure rule engine.</summary>
        public ValueProducer OtherValue { get; internal set; } = ValueProducer.None;

        public Condition When { get; internal set; } = Condition.None;

        public Shape Shape { get; internal set; } = Shape.None;

        internal ValidationRule(string name, string message)
        {
            Name = name;
            Message = message;
        }
    }
}
