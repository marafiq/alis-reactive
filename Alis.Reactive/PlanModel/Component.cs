using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    internal sealed class Component
    {
        public string Id { get; }
        public string Vendor { get; }
        public string Type { get; }
        public ContainerScope Container { get; set; }

        internal Component(string id, string vendor, string type)
        {
            Id = id;
            Vendor = vendor;
            Type = type;
        }

        internal static Component Create(string id, string vendor, string type) =>
            new Component(id, vendor, type);
    }

    internal sealed class ContainerScope
    {
        public List<string> Components { get; }
        public List<ComponentValidation> ValidationRules { get; set; }

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

        internal ComponentValidation(string component, List<ValidationRule> rules)
        {
            Component = component;
            Rules = rules;
        }
    }

    internal sealed class ValidationRule
    {
        public string Name { get; }
        public string Message { get; }
        public ValueProducer Constraint { get; set; }
        public string OtherComponent { get; set; }
        public Condition When { get; set; }
        public Shape Shape { get; set; }

        internal ValidationRule(string name, string message)
        {
            Name = name;
            Message = message;
        }
    }
}
