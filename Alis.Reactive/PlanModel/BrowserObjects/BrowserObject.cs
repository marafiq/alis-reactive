using System.Collections.Generic;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Plan-registered object target: id, vendor, role, and, for inputs, a model binding. Members are not
    /// listed here: every read, write and call carries the path and shape its declaration names.
    /// </summary>
    internal sealed class BrowserObject
    {
        private readonly ComponentId _id;
        private readonly ComponentVendor _vendor;
        private readonly ComponentRole _role;
        private readonly InputBinding _binding;
        private readonly ValidationContainerBinding _container;

        public string Id => _id.Value;
        public string Vendor => _vendor.Name;
        public ComponentRole Role => _role;
        public InputBinding Binding => _binding;
        public ValidationContainerBinding Container => _container;

        internal ComponentVendor ComponentVendor => _vendor;

        private BrowserObject(
            ComponentId id,
            ComponentVendor vendor,
            ComponentRole role,
            InputBinding binding,
            ValidationContainerBinding container)
        {
            _id = id ?? throw new System.ArgumentNullException(nameof(id));
            _vendor = vendor ?? throw new System.ArgumentNullException(nameof(vendor));
            _role = role ?? throw new System.ArgumentNullException(nameof(role));
            _binding = binding ?? throw new System.ArgumentNullException(nameof(binding));
            _container = container ?? throw new System.ArgumentNullException(nameof(container));
        }

        internal static BrowserObject Element(ComponentId id, ComponentVendor vendor) =>
            new BrowserObject(
                id,
                vendor,
                ComponentRole.ObjectTarget,
                InputBinding.None,
                ValidationContainerBinding.None);

        internal static BrowserObject LayoutObject(ComponentId id, ComponentVendor vendor) =>
            new BrowserObject(
                id,
                vendor,
                ComponentRole.LayoutObject,
                InputBinding.None,
                ValidationContainerBinding.None);

        internal static BrowserObject PlanInput(
            ComponentId id,
            ComponentVendor vendor,
            InputBinding binding) =>
            new BrowserObject(
                id,
                vendor,
                ComponentRole.PlanInput,
                binding,
                ValidationContainerBinding.None);

        /// <summary>Copies the object with binding info filled where currently absent (first registration wins).</summary>
        internal BrowserObject WithBindingIfAbsent(InputBinding binding) =>
            new BrowserObject(_id, _vendor, ComponentRole.PlanInput, _binding.FillIfAbsent(binding), _container);

        internal BrowserObject WithContainer(ContainerScope container) =>
            new BrowserObject(_id, _vendor, ComponentRole.ValidationContainer, _binding, ValidationContainerBinding.Scoped(container));

        internal BrowserObject WithValidationRulesMerged(IReadOnlyList<ComponentValidation> validationRules) =>
            new BrowserObject(_id, _vendor, ComponentRole.ValidationContainer, _binding, _container.WithValidationRulesMerged(validationRules));
    }

    internal sealed class ComponentRole
    {
        private readonly string _kind;

        private ComponentRole(string kind)
        {
            _kind = kind ?? throw new System.ArgumentNullException(nameof(kind));
        }

        internal static ComponentRole ObjectTarget { get; } =
            new ComponentRole("object-target");

        internal static ComponentRole PlanInput { get; } =
            new ComponentRole("plan-input");

        internal static ComponentRole ValidationContainer { get; } =
            new ComponentRole("validation-container");

        internal static ComponentRole LayoutObject { get; } =
            new ComponentRole("layout-object");

        public string Kind => _kind;
    }

    [System.Text.Json.Serialization.JsonConverter(typeof(PlanNodeDiscriminator<InputBinding>))]
    internal abstract class InputBinding
    {
        private protected InputBinding() { }

        internal static InputBinding None { get; } =
            new NoInputBinding();

        public abstract string Kind { get; }

        internal static InputBinding RegisteredInput(BindingPath bindingPath, MemberName valueMember, Shape valueShape) =>
            new RegisteredInputBinding(
                bindingPath,
                valueMember,
                valueShape);

        internal abstract InputBinding FillIfAbsent(InputBinding incoming);
    }

    internal sealed class NoInputBinding : InputBinding
    {
        public override string Kind => "none";

        internal override InputBinding FillIfAbsent(InputBinding incoming)
        {
            if (incoming == null) throw new System.ArgumentNullException(nameof(incoming));
            return incoming;
        }
    }

    internal sealed class RegisteredInputBinding : InputBinding
    {
        private readonly BindingPath _bindingPath;
        private readonly MemberName _valueMember;

        internal RegisteredInputBinding(BindingPath bindingPath, MemberName valueMember, Shape valueShape)
        {
            _bindingPath = bindingPath ?? throw new System.ArgumentNullException(nameof(bindingPath));
            _valueMember = valueMember ?? throw new System.ArgumentNullException(nameof(valueMember));
            ValueShape = valueShape ?? throw new System.ArgumentNullException(nameof(valueShape));
        }

        public override string Kind => "registered-input";
        public string BindingPath => _bindingPath.Value;
        public Path Path => _bindingPath.Path;
        public string ValueMember => _valueMember.Value;

        /// <summary>The JavaScript path of the input's value on its object.</summary>
        public Path ValuePath => Path.Parse(_valueMember.Value);

        /// <summary>The shape the input's value is read as.</summary>
        public Shape ValueShape { get; }

        internal override InputBinding FillIfAbsent(InputBinding incoming)
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

        internal InputBinding BindingFor(BindingPath bindingPath) =>
            InputBinding.RegisteredInput(bindingPath, _valueMember, Shape);
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

        internal InputBinding InputBinding =>
            _valueContract.BindingFor(_bindingPath);

        internal BrowserObject CreateComponent() =>
            BrowserObject.PlanInput(
                _componentId,
                _vendor,
                InputBinding);

        internal static InputComponentPlanBinding For(
            ComponentId componentId,
            ComponentVendor vendor,
            BindingPath bindingPath,
            InputValueContract valueContract) =>
            new InputComponentPlanBinding(componentId, vendor, bindingPath, valueContract);
    }
}
