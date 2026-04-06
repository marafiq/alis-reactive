using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    internal sealed class Component
    {
        public string Id { get; }
        public string Vendor { get; }
        public string Type { get; }
        public ContainerScope Container { get; internal set; }

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
        public List<ComponentValidation> ValidationRules { get; internal set; }

        internal ContainerScope(List<string> components)
        {
            Components = components;
        }

        internal static ContainerScope Of(params string[] components) =>
            new ContainerScope(new List<string>(components));
    }

    internal sealed class ComponentValidation
    {
        public string Component { get; }
        public List<ValidationRule> Rules { get; }

        [System.Text.Json.Serialization.JsonInclude]
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        internal string ServerFieldName { get; }

        internal ComponentValidation(string component, List<ValidationRule> rules, string serverFieldName = null)
        {
            Component = component;
            Rules = rules;
            ServerFieldName = serverFieldName;
        }
    }

    internal sealed class ValidationRule
    {
        public string Name { get; }
        public string Message { get; }
        public ValueProducer Constraint { get; internal set; }
        public string OtherComponent { get; internal set; }
        public Condition When { get; internal set; }
        public Shape Shape { get; internal set; }

        internal ValidationRule(string name, string message)
        {
            Name = name;
            Message = message;
        }
    }
}
