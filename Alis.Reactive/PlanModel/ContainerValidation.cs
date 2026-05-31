using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;
using Alis.Reactive.Validation;

namespace Alis.Reactive.PlanModel
{
    [System.Text.Json.Serialization.JsonConverter(typeof(PlanNodeDiscriminator<ValidationContainerBinding>))]
    public abstract class ValidationContainerBinding
    {
        private protected ValidationContainerBinding() { }

        internal static ValidationContainerBinding None { get; } =
            new NoValidationContainer();

        internal static ValidationContainerBinding Scoped(ContainerScope scope) =>
            new ScopedValidationContainer(scope);

        public abstract string Kind { get; }

        internal abstract ValidationContainerBinding WithValidationRulesMerged(IReadOnlyList<ComponentValidation> incoming);
    }

    internal sealed class NoValidationContainer : ValidationContainerBinding
    {
        public override string Kind => "none";

        internal override ValidationContainerBinding WithValidationRulesMerged(IReadOnlyList<ComponentValidation> incoming) =>
            Scoped(ContainerScope.Empty.WithValidationRulesMerged(incoming));
    }

    internal sealed class ScopedValidationContainer : ValidationContainerBinding
    {
        private readonly ContainerScope _scope;

        internal ScopedValidationContainer(ContainerScope scope)
        {
            _scope = scope ?? throw new System.ArgumentNullException(nameof(scope));
        }

        public override string Kind => "validation-container";
        public IReadOnlyList<ComponentValidation> ValidationRules => _scope.ValidationRules;

        internal override ValidationContainerBinding WithValidationRulesMerged(IReadOnlyList<ComponentValidation> incoming) =>
            Scoped(_scope.WithValidationRulesMerged(incoming));
    }

    internal sealed class ContainerScope
    {
        private readonly ContainerValidations _validations;

        private ContainerScope(ContainerValidations validations)
        {
            _validations = validations ?? throw new System.ArgumentNullException(nameof(validations));
        }

        public IReadOnlyList<ComponentValidation> ValidationRules => _validations.ForJson;

        internal static ContainerScope Empty { get; } =
            new ContainerScope(ContainerValidations.Empty);

        /// <summary>
        /// Returns a copy whose validation rules are merged with <paramref name="incoming"/>.
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

        internal IReadOnlyList<ComponentValidation> ForJson => _validations;

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

    /// <summary>Validation rules for a single component within a container scope.
    /// The Value expression reads the component's current value via the shared evaluateValue path.</summary>
    internal sealed class ComponentValidation
    {
        private readonly ComponentId _component;
        private readonly ValidationFieldPath _serverFieldName;
        private readonly ComponentValidationRules _rules;

        /// <summary>Component ID — used for DOM error display and serverFieldName mapping.</summary>
        public string Component => _component.Value;
        /// <summary>How to read this component's value for validation. Evaluated via evaluateValue().</summary>
        public ValueExpression Value { get; }
        public IReadOnlyList<ValidationRule> Rules => _rules.ForJson;

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
            IReadOnlyList<ValidationRule> rules,
            string serverFieldName) =>
            new ComponentValidation(
                ComponentId.Of(component),
                value,
                ComponentValidationRules.From(rules),
                ValidationFieldPath.Of(serverFieldName));
    }

    internal sealed class ComponentValidationRules
    {
        private readonly IReadOnlyList<ValidationRule> _rules;

        private ComponentValidationRules(IReadOnlyList<ValidationRule> rules)
        {
            _rules = rules;
        }

        internal IReadOnlyList<ValidationRule> ForJson => _rules;

        internal static ComponentValidationRules From(IEnumerable<ValidationRule> rules)
        {
            if (rules == null) throw new System.ArgumentNullException(nameof(rules));

            var snapshot = new List<ValidationRule>();
            foreach (var rule in rules)
            {
                if (rule == null)
                    throw new System.ArgumentException("Validation rule must not be null.", nameof(rules));

                snapshot.Add(rule);
            }

            return new ComponentValidationRules(snapshot);
        }
    }

    [JsonConverter(typeof(ValidationRuleJsonConverter))]
    internal sealed class ValidationRule
    {
        private readonly ValidationRuleName _name;
        private readonly ValidationMessage _message;
        private readonly ValidationRuleExecution _execution;

        public string Name => _name.Value;
        public string Message => _message.Value;
        internal ValidationRuleExecution Execution => _execution;

        internal ValidationRule(
            ValidationRuleName name,
            ValidationMessage message,
            ValidationRuleExecution execution)
        {
            _name = name ?? throw new System.ArgumentNullException(nameof(name));
            _message = message ?? throw new System.ArgumentNullException(nameof(message));
            _execution = execution ?? throw new System.ArgumentNullException(nameof(execution));
        }
    }

    internal sealed class ValidationRuleJsonConverter : JsonConverter<ValidationRule>
    {
        public override void Write(Utf8JsonWriter writer, ValidationRule value, JsonSerializerOptions options)
        {
            if (value == null) throw new System.ArgumentNullException(nameof(value));

            writer.WriteStartObject();
            writer.WriteString("name", value.Name);
            writer.WriteString("message", value.Message);
            WriteExecution(writer, options, value.Execution);
            writer.WriteEndObject();
        }

        public override ValidationRule Read(
            ref Utf8JsonReader reader,
            System.Type typeToConvert,
            JsonSerializerOptions options) =>
            throw new System.NotSupportedException("Plan types are write-only.");

        private static void WriteExecution(
            Utf8JsonWriter writer,
            JsonSerializerOptions options,
            ValidationRuleExecution execution)
        {
            writer.WritePropertyName("execution");
            execution.WriteTo(writer, options);
        }
    }

    internal abstract class ValidationRuleExecution
    {
        private readonly ValidationRuleActivation _activation;

        private protected ValidationRuleExecution(
            ValidationRuleActivation activation,
            Shape comparisonShape)
        {
            _activation = activation ?? throw new System.ArgumentNullException(nameof(activation));
            ComparisonShape = comparisonShape ?? throw new System.ArgumentNullException(nameof(comparisonShape));
        }

        public ValidationRuleActivation Activation => _activation;
        public Shape ComparisonShape { get; }
        public abstract string Kind { get; }

        internal void WriteTo(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("kind", Kind);
            WriteOperand(writer, options);
            PlanJsonWriter.WriteProperty(writer, options, "activation", Activation);
            PlanJsonWriter.WriteProperty(writer, options, "comparisonShape", ComparisonShape);
            writer.WriteEndObject();
        }

        internal abstract void WriteOperand(Utf8JsonWriter writer, JsonSerializerOptions options);

        internal static ValidationRuleExecution WithoutTarget(
            ValidationRuleActivation activation,
            Shape comparisonShape) =>
            new NoOperandValidationRuleExecution(
                activation,
                comparisonShape);

        internal static ValidationRuleExecution WithConstraint(
            ValueExpression constraint,
            ValidationRuleActivation activation,
            Shape comparisonShape)
        {
            if (constraint is not LiteralExpression literal)
                throw new System.ArgumentException("Validation rule constraints must be literal values.", nameof(constraint));

            return new ConstraintValidationRuleExecution(
                literal,
                activation,
                comparisonShape);
        }

        internal static ValidationRuleExecution WithPeer(
            ValueExpression peer,
            ValidationRuleActivation activation,
            Shape comparisonShape)
        {
            if (peer is not ReadExpression read)
                throw new System.ArgumentException("Validation rule peer values must read another field value.", nameof(peer));

            return new PeerValidationRuleExecution(
                read,
                activation,
                comparisonShape);
        }

        private sealed class NoOperandValidationRuleExecution : ValidationRuleExecution
        {
            public override string Kind => "none";

            internal NoOperandValidationRuleExecution(
                ValidationRuleActivation activation,
                Shape comparisonShape)
                : base(activation, comparisonShape)
            {
            }

            internal override void WriteOperand(Utf8JsonWriter writer, JsonSerializerOptions options)
            {
            }
        }

        private sealed class ConstraintValidationRuleExecution : ValidationRuleExecution
        {
            private readonly LiteralExpression _value;

            internal ConstraintValidationRuleExecution(
                LiteralExpression value,
                ValidationRuleActivation activation,
                Shape comparisonShape)
                : base(activation, comparisonShape)
            {
                _value = value ?? throw new System.ArgumentNullException(nameof(value));
            }

            public override string Kind => "constraint";
            public LiteralExpression Value => _value;

            internal override void WriteOperand(Utf8JsonWriter writer, JsonSerializerOptions options) =>
                PlanJsonWriter.WriteProperty(writer, options, "value", _value);
        }

        private sealed class PeerValidationRuleExecution : ValidationRuleExecution
        {
            private readonly ReadExpression _value;

            internal PeerValidationRuleExecution(
                ReadExpression value,
                ValidationRuleActivation activation,
                Shape comparisonShape)
                : base(activation, comparisonShape)
            {
                _value = value ?? throw new System.ArgumentNullException(nameof(value));
            }

            public override string Kind => "peer";
            public ReadExpression Value => _value;

            internal override void WriteOperand(Utf8JsonWriter writer, JsonSerializerOptions options) =>
                PlanJsonWriter.WriteProperty(writer, options, "value", _value);
        }
    }

    [JsonConverter(typeof(ValidationRuleActivationJsonConverter))]
    internal abstract class ValidationRuleActivation
    {
        private protected ValidationRuleActivation() { }

        internal static ValidationRuleActivation Always { get; } =
            new AlwaysActiveValidationRule();

        public abstract string Kind { get; }
        internal abstract void WritePayload(Utf8JsonWriter writer, JsonSerializerOptions options);

        internal static ValidationRuleActivation When(ConditionGraph condition) =>
            new ConditionallyActiveValidationRule(condition);

        private sealed class AlwaysActiveValidationRule : ValidationRuleActivation
        {
            public override string Kind => "always";

            internal override void WritePayload(Utf8JsonWriter writer, JsonSerializerOptions options)
            {
            }
        }

        private sealed class ConditionallyActiveValidationRule : ValidationRuleActivation
        {
            private readonly ConditionGraph _condition;

            internal ConditionallyActiveValidationRule(ConditionGraph condition)
            {
                _condition = condition ?? throw new System.ArgumentNullException(nameof(condition));
            }

            public override string Kind => "when";
            internal override void WritePayload(Utf8JsonWriter writer, JsonSerializerOptions options) =>
                PlanJsonWriter.WriteProperty(writer, options, "condition", _condition);
        }
    }

    internal sealed class ValidationRuleActivationJsonConverter : JsonConverter<ValidationRuleActivation>
    {
        public override void Write(Utf8JsonWriter writer, ValidationRuleActivation value, JsonSerializerOptions options)
        {
            if (value == null) throw new System.ArgumentNullException(nameof(value));

            writer.WriteStartObject();
            writer.WriteString("kind", value.Kind);
            value.WritePayload(writer, options);
            writer.WriteEndObject();
        }

        public override ValidationRuleActivation Read(
            ref Utf8JsonReader reader,
            System.Type typeToConvert,
            JsonSerializerOptions options) =>
            throw new System.NotSupportedException("Plan types are write-only.");
    }
}
