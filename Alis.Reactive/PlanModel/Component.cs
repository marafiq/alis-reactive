using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;
using Alis.Reactive.Validation;

namespace Alis.Reactive.PlanModel
{
    internal sealed class Component
    {
        private readonly ComponentId _id;
        private readonly ComponentVendor _vendor;
        private readonly TypeKey _type;
        private readonly ComponentContributionIntent _contribution;
        private readonly ComponentBinding _binding;
        private readonly ComponentContainer _container;

        public string Id => _id.Value;
        public string Vendor => _vendor.Value;
        public string Type => _type.Value;
        public ComponentContributionIntent Contribution => _contribution;
        public ComponentBinding Binding => _binding;
        public ComponentContainer Container => _container;

        private Component(
            ComponentId id,
            ComponentVendor vendor,
            TypeKey type,
            ComponentContributionIntent contribution,
            ComponentBinding binding,
            ComponentContainer container)
        {
            _id = id ?? throw new System.ArgumentNullException(nameof(id));
            _vendor = vendor ?? throw new System.ArgumentNullException(nameof(vendor));
            _type = type ?? throw new System.ArgumentNullException(nameof(type));
            _contribution = contribution ?? throw new System.ArgumentNullException(nameof(contribution));
            _binding = binding ?? throw new System.ArgumentNullException(nameof(binding));
            _container = container ?? throw new System.ArgumentNullException(nameof(container));
        }

        internal static Component Element(string id, string vendor, string type) =>
            new Component(
                ComponentId.Of(id),
                ComponentVendor.From(vendor),
                TypeKey.Of(type),
                ComponentContributionIntent.ObjectTarget,
                ComponentBinding.None,
                ComponentContainer.None);

        internal static Component LayoutObject(string id, string vendor, string type) =>
            new Component(
                ComponentId.Of(id),
                ComponentVendor.From(vendor),
                TypeKey.Of(type),
                ComponentContributionIntent.LayoutObject,
                ComponentBinding.None,
                ComponentContainer.None);

        internal static Component Input(
            string id,
            string vendor,
            string type,
            ComponentBinding binding) =>
            new Component(
                ComponentId.Of(id),
                ComponentVendor.From(vendor),
                TypeKey.Of(type),
                ComponentContributionIntent.OwnedDefinition,
                binding,
                ComponentContainer.None);

        /// <summary>Returns a copy with binding info filled where currently absent (first registration wins).</summary>
        internal Component WithBindingIfAbsent(ComponentBinding binding) =>
            new Component(_id, _vendor, _type, ComponentContributionIntent.OwnedDefinition, _binding.FillIfAbsent(binding), _container);

        /// <summary>Returns a copy carrying the given container scope.</summary>
        internal Component WithContainer(ContainerScope container) =>
            new Component(_id, _vendor, _type, ComponentContributionIntent.ValidationContainer, _binding, ComponentContainer.Scoped(container));

        internal Component WithValidationRulesMerged(IReadOnlyList<ComponentValidation> validationRules) =>
            new Component(_id, _vendor, _type, ComponentContributionIntent.ValidationContainer, _binding, _container.WithValidationRulesMerged(validationRules));
    }

    [System.Text.Json.Serialization.JsonConverter(typeof(WriteOnlyPolymorphicConverter<ComponentContributionIntent>))]
    public abstract class ComponentContributionIntent
    {
        private protected ComponentContributionIntent() { }

        internal static ComponentContributionIntent ObjectTarget { get; } =
            new ObjectTargetComponentContribution();

        internal static ComponentContributionIntent OwnedDefinition { get; } =
            new OwnedDefinitionComponentContribution();

        internal static ComponentContributionIntent ValidationContainer { get; } =
            new ValidationContainerComponentContribution();

        internal static ComponentContributionIntent LayoutObject { get; } =
            new LayoutObjectComponentContribution();

        public abstract string Kind { get; }
    }

    internal sealed class ObjectTargetComponentContribution : ComponentContributionIntent
    {
        public override string Kind => "object-target";
    }

    internal sealed class OwnedDefinitionComponentContribution : ComponentContributionIntent
    {
        public override string Kind => "owned-definition";
    }

    internal sealed class ValidationContainerComponentContribution : ComponentContributionIntent
    {
        public override string Kind => "validation-container";
    }

    internal sealed class LayoutObjectComponentContribution : ComponentContributionIntent
    {
        public override string Kind => "layout-object";
    }

    [System.Text.Json.Serialization.JsonConverter(typeof(WriteOnlyPolymorphicConverter<ComponentBinding>))]
    public abstract class ComponentBinding
    {
        private protected ComponentBinding() { }

        internal static ComponentBinding None { get; } =
            new UnboundComponentBinding();

        public abstract string Kind { get; }

        internal static ComponentBinding RegisteredInput(string bindingPath, string valueMember) =>
            RegisteredInput(BindingPath.Of(bindingPath), MemberName.Of(valueMember));

        internal static ComponentBinding RegisteredInput(BindingPath bindingPath, MemberName valueMember) =>
            new RegisteredInputBinding(
                bindingPath,
                valueMember);

        internal abstract ComponentBinding FillIfAbsent(ComponentBinding incoming);
    }

    internal sealed class UnboundComponentBinding : ComponentBinding
    {
        public override string Kind => "none";

        internal override ComponentBinding FillIfAbsent(ComponentBinding incoming)
        {
            if (incoming == null) throw new System.ArgumentNullException(nameof(incoming));
            return incoming;
        }
    }

    internal sealed class RegisteredInputBinding : ComponentBinding
    {
        private readonly BindingPath _bindingPath;
        private readonly MemberName _valueMember;

        internal RegisteredInputBinding(BindingPath bindingPath, MemberName valueMember)
        {
            _bindingPath = bindingPath ?? throw new System.ArgumentNullException(nameof(bindingPath));
            _valueMember = valueMember ?? throw new System.ArgumentNullException(nameof(valueMember));
        }

        public override string Kind => "registered-input";
        public string BindingPath => _bindingPath.Value;
        public string ValueMember => _valueMember.Value;

        internal override ComponentBinding FillIfAbsent(ComponentBinding incoming)
        {
            if (incoming == null) throw new System.ArgumentNullException(nameof(incoming));
            return this;
        }
    }

    internal sealed class InputValueContract
    {
        private static readonly MemberName CanonicalValueMember = MemberName.Of("value");
        private readonly MemberName _valueMember;

        private InputValueContract(MemberName valueMember, Shape shape)
        {
            _valueMember = valueMember ?? throw new System.ArgumentNullException(nameof(valueMember));
            Shape = shape ?? throw new System.ArgumentNullException(nameof(shape));
        }

        internal string ValueMember => _valueMember.Value;

        internal Shape Shape { get; }

        internal static InputValueContract For(string valueMember, Shape shape) =>
            For(MemberName.Of(valueMember), shape);

        internal static InputValueContract For(MemberName valueMember, Shape shape) =>
            new InputValueContract(valueMember, shape);

        internal static InputValueContract ForCanonicalValue(Shape shape) =>
            For(CanonicalValueMember, shape);

        internal ComponentBinding BindingFor(string bindingPath) =>
            BindingFor(BindingPath.Of(bindingPath));

        internal ComponentBinding BindingFor(BindingPath bindingPath) =>
            ComponentBinding.RegisteredInput(bindingPath, _valueMember);

        internal void Enrich(JsType jsType)
        {
            if (jsType == null) throw new System.ArgumentNullException(nameof(jsType));

            var valuePath = Path.Parse(ValueMember);
            jsType.Declare(JsPropertyContract.Create(
                _valueMember,
                valuePath,
                Shape,
                MemberAccess.Read));

            var valueMemberNeedsCanonicalAlias = !_valueMember.Equals(CanonicalValueMember);
            if (valueMemberNeedsCanonicalAlias)
                jsType.Declare(JsPropertyContract.Create(
                    CanonicalValueMember,
                    valuePath,
                    Shape,
                    MemberAccess.Read));
        }
    }

    internal sealed class InputComponentPlanBinding
    {
        private readonly ComponentId _componentId;
        private readonly ComponentVendor _vendor;
        private readonly BindingPath _bindingPath;
        private readonly InputValueContract _valueContract;

        private InputComponentPlanBinding(
            ComponentId componentId,
            ComponentVendor vendor,
            BindingPath bindingPath,
            InputValueContract valueContract)
        {
            _componentId = componentId ?? throw new System.ArgumentNullException(nameof(componentId));
            _vendor = vendor ?? throw new System.ArgumentNullException(nameof(vendor));
            _bindingPath = bindingPath ?? throw new System.ArgumentNullException(nameof(bindingPath));
            _valueContract = valueContract ?? throw new System.ArgumentNullException(nameof(valueContract));
        }

        internal ComponentId ComponentId => _componentId;
        internal ComponentVendor Vendor => _vendor;
        internal InputValueContract ValueContract => _valueContract;
        internal TypeKey TypeKey => TypeKey.Component(_vendor, _componentId);

        internal ComponentBinding ComponentBinding =>
            _valueContract.BindingFor(_bindingPath);

        internal Component CreateComponent() =>
            Component.Input(
                _componentId.Value,
                _vendor.Value,
                TypeKey.Value,
                ComponentBinding);

        internal static InputComponentPlanBinding For(
            ComponentId componentId,
            ComponentVendor vendor,
            BindingPath bindingPath,
            InputValueContract valueContract) =>
            new InputComponentPlanBinding(componentId, vendor, bindingPath, valueContract);
    }

    [System.Text.Json.Serialization.JsonConverter(typeof(WriteOnlyPolymorphicConverter<ComponentContainer>))]
    public abstract class ComponentContainer
    {
        private protected ComponentContainer() { }

        internal static ComponentContainer None { get; } =
            new UnscopedComponentContainer();

        internal static ComponentContainer Scoped(ContainerScope scope) =>
            new ScopedComponentContainer(scope);

        public abstract string Kind { get; }

        internal abstract ComponentContainer WithValidationRulesMerged(IReadOnlyList<ComponentValidation> incoming);
    }

    internal sealed class UnscopedComponentContainer : ComponentContainer
    {
        public override string Kind => "none";

        internal override ComponentContainer WithValidationRulesMerged(IReadOnlyList<ComponentValidation> incoming) =>
            Scoped(ContainerScope.Empty.WithValidationRulesMerged(incoming));
    }

    internal sealed class ScopedComponentContainer : ComponentContainer
    {
        private readonly ContainerScope _scope;

        internal ScopedComponentContainer(ContainerScope scope)
        {
            _scope = scope ?? throw new System.ArgumentNullException(nameof(scope));
        }

        public override string Kind => "validation-container";
        public IReadOnlyList<string> Components => _scope.Components;
        public IReadOnlyList<ComponentValidation> ValidationRules => _scope.ValidationRules;

        internal override ComponentContainer WithValidationRulesMerged(IReadOnlyList<ComponentValidation> incoming) =>
            Scoped(_scope.WithValidationRulesMerged(incoming));
    }

    internal sealed class ContainerScope
    {
        private readonly ContainerComponentIds _components;
        private readonly ContainerValidations _validations;

        private ContainerScope(
            ContainerComponentIds components,
            ContainerValidations validations)
        {
            _components = components ?? throw new System.ArgumentNullException(nameof(components));
            _validations = validations ?? throw new System.ArgumentNullException(nameof(validations));
        }

        public IReadOnlyList<string> Components => _components.ForJson;
        public IReadOnlyList<ComponentValidation> ValidationRules => _validations.ForJson;

        internal static ContainerScope Empty { get; } =
            new ContainerScope(ContainerComponentIds.Empty, ContainerValidations.Empty);

        internal static ContainerScope Of(params string[] components) =>
            new ContainerScope(ContainerComponentIds.From(components), ContainerValidations.Empty);

        /// <summary>
        /// Returns a copy whose validation rules are merged with <paramref name="incoming"/>.
        /// Rules for an already-present component are replaced; rules for components not yet
        /// present are appended. Used when more than one request validates the same container.
        /// </summary>
        internal ContainerScope WithValidationRulesMerged(IReadOnlyList<ComponentValidation> incoming)
        {
            var incomingValidations = ContainerValidations.From(incoming);
            return new ContainerScope(
                _components,
                _validations.MergeReplacingComponentRules(incomingValidations));
        }
    }

    internal sealed class ContainerComponentIds
    {
        private readonly IReadOnlyList<string> _components;

        private ContainerComponentIds(IReadOnlyList<string> components)
        {
            _components = components;
        }

        internal IReadOnlyList<string> ForJson => _components;

        internal static ContainerComponentIds Empty { get; } =
            new ContainerComponentIds(new List<string>());

        internal static ContainerComponentIds From(IEnumerable<string> components)
        {
            if (components == null) throw new System.ArgumentNullException(nameof(components));

            var snapshot = new List<string>();
            foreach (var component in components)
                snapshot.Add(ComponentId.Of(component).Value);

            return new ContainerComponentIds(snapshot);
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
    /// The Value producer reads the component's current value via the shared evaluateValue path.</summary>
    internal sealed class ComponentValidation
    {
        private readonly ComponentId _component;
        private readonly ValidationFieldPath _serverFieldName;
        private readonly ComponentValidationRules _rules;

        /// <summary>Component ID — used for DOM error display and serverFieldName mapping.</summary>
        public string Component => _component.Value;
        /// <summary>How to read this component's value for validation. Evaluated via evaluateValue().</summary>
        public ValueProducer Value { get; }
        public IReadOnlyList<ValidationRule> Rules => _rules.ForJson;

        [System.Text.Json.Serialization.JsonInclude]
        internal string ServerFieldName => _serverFieldName.Value;

        private ComponentValidation(
            ComponentId component,
            ValueProducer value,
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
            ValueProducer value,
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

        private static void WriteProperty<T>(
            Utf8JsonWriter writer,
            JsonSerializerOptions options,
            string name,
            T value)
        {
            writer.WritePropertyName(name);
            JsonSerializer.Serialize(writer, value, options);
        }

        private static void WriteExecution(
            Utf8JsonWriter writer,
            JsonSerializerOptions options,
            ValidationRuleExecution execution)
        {
            writer.WritePropertyName("execution");
            writer.WriteStartObject();
            WriteProperty(writer, options, "constraint", execution.Constraint);
            WriteProperty(writer, options, "otherValue", execution.OtherValue);
            WriteProperty(writer, options, "activation", execution.Activation);
            WriteProperty(writer, options, "comparisonShape", execution.ComparisonShape);
            writer.WriteEndObject();
        }
    }

    internal sealed class ValidationRuleExecution
    {
        private readonly ValidationRuleOperand _constraint;
        private readonly ValidationRuleOperand _otherValue;
        private readonly ValidationRuleActivation _activation;

        private ValidationRuleExecution(
            ValidationRuleOperand constraint,
            ValidationRuleOperand otherValue,
            ValidationRuleActivation activation,
            Shape comparisonShape)
        {
            _constraint = constraint ?? throw new System.ArgumentNullException(nameof(constraint));
            _otherValue = otherValue ?? throw new System.ArgumentNullException(nameof(otherValue));
            _activation = activation ?? throw new System.ArgumentNullException(nameof(activation));
            ComparisonShape = comparisonShape ?? throw new System.ArgumentNullException(nameof(comparisonShape));
        }

        public ValidationRuleOperand Constraint => _constraint;
        public ValidationRuleOperand OtherValue => _otherValue;
        public ValidationRuleActivation Activation => _activation;
        public Shape ComparisonShape { get; }

        internal static ValidationRuleExecution Execute(
            ValidationRuleOperand constraint,
            ValidationRuleOperand otherValue,
            ValidationRuleActivation activation,
            Shape comparisonShape) =>
            new ValidationRuleExecution(
                constraint,
                otherValue,
                activation,
                comparisonShape);
    }

    [JsonConverter(typeof(ValidationRuleOperandJsonConverter))]
    internal abstract class ValidationRuleOperand
    {
        private protected ValidationRuleOperand() { }

        internal static ValidationRuleOperand None { get; } =
            new MissingValidationRuleOperand();

        public abstract string Kind { get; }
        internal abstract void WritePayload(Utf8JsonWriter writer, JsonSerializerOptions options);

        internal static ValidationRuleOperand From(ValueProducer value) =>
            new PresentValidationRuleOperand(value);

        private sealed class MissingValidationRuleOperand : ValidationRuleOperand
        {
            public override string Kind => "none";

            internal override void WritePayload(Utf8JsonWriter writer, JsonSerializerOptions options)
            {
            }
        }

        private sealed class PresentValidationRuleOperand : ValidationRuleOperand
        {
            private readonly ValueProducer _value;

            internal PresentValidationRuleOperand(ValueProducer value)
            {
                _value = value ?? throw new System.ArgumentNullException(nameof(value));
            }

            public override string Kind => "value";
            public ValueProducer Value => _value;
            internal override void WritePayload(Utf8JsonWriter writer, JsonSerializerOptions options) =>
                ValidationRuleOperandJsonConverter.WriteProperty(writer, options, "value", _value);
        }
    }

    internal sealed class ValidationRuleOperandJsonConverter : JsonConverter<ValidationRuleOperand>
    {
        public override void Write(Utf8JsonWriter writer, ValidationRuleOperand value, JsonSerializerOptions options)
        {
            if (value == null) throw new System.ArgumentNullException(nameof(value));

            writer.WriteStartObject();
            writer.WriteString("kind", value.Kind);
            value.WritePayload(writer, options);
            writer.WriteEndObject();
        }

        public override ValidationRuleOperand Read(
            ref Utf8JsonReader reader,
            System.Type typeToConvert,
            JsonSerializerOptions options) =>
            throw new System.NotSupportedException("Plan types are write-only.");

        internal static void WriteProperty<T>(
            Utf8JsonWriter writer,
            JsonSerializerOptions options,
            string name,
            T value)
        {
            writer.WritePropertyName(name);
            JsonSerializer.Serialize(writer, value, options);
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

        internal static ValidationRuleActivation When(ValidationCondition condition) =>
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
            private readonly ValidationCondition _condition;

            internal ConditionallyActiveValidationRule(ValidationCondition condition)
            {
                _condition = condition ?? throw new System.ArgumentNullException(nameof(condition));
            }

            public override string Kind => "when";
            public ValidationCondition Condition => _condition;
            internal override void WritePayload(Utf8JsonWriter writer, JsonSerializerOptions options) =>
                ValidationRuleActivationJsonConverter.WriteProperty(writer, options, "condition", _condition);
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

        internal static void WriteProperty<T>(
            Utf8JsonWriter writer,
            JsonSerializerOptions options,
            string name,
            T value)
        {
            writer.WritePropertyName(name);
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
