using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// An input a view rendered into its field: the element that holds it, the vendor whose object it is,
    /// the model member it binds and the value it holds. Gather, validation and the model read it.
    /// </summary>
    internal sealed class ComponentRegistration
    {
        private readonly RegisteredComponentIdentity _identity;
        private readonly RegisteredInputBinding _binding;
        private readonly ComponentKind _componentType;

        /// <summary>The element ID, the component's key in the plan.</summary>
        internal string ComponentId => _identity.ComponentId.Value;

        internal ComponentVendor ComponentVendor => _identity.Vendor;

        /// <summary>The MVC binding name: the join key for gather, validation and the request body.</summary>
        internal string BindingPath => _binding.BindingPath.Value;

        /// <summary>The value property and the event that commits it, as the component declares them.</summary>
        internal ComponentInput Input => _binding.Input;

        /// <summary>The model member's type: the model, gather and validation read the value in it.</summary>
        internal Shape Shape { get; }

        internal BindingPath RegisteredBindingPath => _binding.BindingPath;

        internal string DescribeContract() =>
            $"[{_identity.ComponentId.Value}, {_identity.Vendor.Name}, {_binding.Input.Describe()}, {_componentType.Value}, {Shape.DescribeContract()}]";

        internal bool HasSameContractAs(ComponentRegistration other)
        {
            if (other == null) throw new System.ArgumentNullException(nameof(other));
            return _identity.Matches(other._identity)
                && _binding.Matches(other._binding)
                && _componentType.Equals(other._componentType)
                && Shape == other.Shape;
        }

        private ComponentRegistration(
            RegisteredComponentIdentity identity,
            RegisteredInputBinding binding,
            ComponentKind componentType,
            Shape shape)
        {
            _identity = identity ?? throw new System.ArgumentNullException(nameof(identity));
            _binding = binding ?? throw new System.ArgumentNullException(nameof(binding));
            _componentType = componentType ?? throw new System.ArgumentNullException(nameof(componentType));
            Shape = shape ?? throw new System.ArgumentNullException(nameof(shape));
        }

        internal static ComponentRegistration RegisteredInput(
            RegisteredComponentIdentity identity,
            RegisteredInputBinding binding,
            ComponentKind componentType,
            Shape shape) =>
            new ComponentRegistration(identity, binding, componentType, shape);
    }
}
