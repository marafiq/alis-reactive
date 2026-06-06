using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// Immutable registration of a component in the Reactive Plan.
    /// Populated at view render time by each vertical slice's HtmlExtensions.
    /// Consumed by gather (HTTP serialization) and validation field binding.
    /// </summary>
    internal sealed class ComponentRegistration
    {
        private readonly RegisteredComponentIdentity _identity;
        private readonly RegisteredInputBinding _binding;
        private readonly ComponentKind _componentType;

        /// <summary>Controlled DOM element ID used as the component key in the Reactive Plan.</summary>
        public string ComponentId => _identity.ComponentId.Value;

        /// <summary>Vendor token, such as <c>native</c> or <c>fusion</c>, written for runtime component resolution.</summary>
        public string Vendor => _identity.Vendor.Value;

        /// <summary>Model binding path used as the join key for gather, validation, and payload serialization.</summary>
        public string BindingPath => _binding.BindingPath.Value;

        /// <summary>Component-object member read for the registered input value, such as <c>value</c> or <c>checked</c>.</summary>
        public string ValueMember => _binding.ValueMember.Value;

        /// <summary>Component kind recorded for duplicate-registration diagnostics and component-specific metadata.</summary>
        public string ComponentType => _componentType.Value;

        /// <summary>
        /// Shape inferred from <c>typeof(TProp)</c> at registration time.
        /// Flows to plan JSON and is consumed by gather serialization and validation binding.
        /// </summary>
        public Shape Shape { get; }

        internal InputValueContract ValueContract =>
            InputValueContract.For(_binding.ValueMember, Shape);

        internal BindingPath RegisteredBindingPath => _binding.BindingPath;

        internal InputComponentPlanBinding PlanBinding =>
            InputComponentPlanBinding.For(
                _identity.ComponentId,
                _identity.Vendor,
                _binding.BindingPath,
                ValueContract);

        internal InputValueContract RequireValueContract(MemberName expectedValueMember)
        {
            if (expectedValueMember == null) throw new System.ArgumentNullException(nameof(expectedValueMember));
            if (!_binding.ValueMember.Equals(expectedValueMember))
                throw new System.InvalidOperationException(
                    $"Component '{ComponentId}' is registered for binding path '{BindingPath}' " +
                    $"with value member '{_binding.ValueMember.Value}', but this usage requested " +
                    $"'{expectedValueMember.Value}'. Use the component type that matches the rendered input.");

            return ValueContract;
        }

        internal void DeclareValueContract(BrowserObjectContracts contracts, BrowserObjectId typeKey)
        {
            if (contracts == null) throw new System.ArgumentNullException(nameof(contracts));
            if (typeKey == null) throw new System.ArgumentNullException(nameof(typeKey));

            contracts.DeclareInputValueContract(typeKey, ValueContract);
        }

        internal BrowserObject CreateComponent(ComponentId componentId, ComponentVendor vendor, BrowserObjectId typeKey)
        {
            if (componentId == null) throw new System.ArgumentNullException(nameof(componentId));
            if (vendor == null) throw new System.ArgumentNullException(nameof(vendor));
            if (typeKey == null) throw new System.ArgumentNullException(nameof(typeKey));

            return BrowserObject.PlanInput(
                componentId,
                vendor,
                typeKey,
                ValueContract.BindingFor(RegisteredBindingPath));
        }

        internal BrowserObject AddBindingTo(BrowserObject component, BrowserObjectContract objectContract)
        {
            if (component == null) throw new System.ArgumentNullException(nameof(component));
            if (objectContract == null) throw new System.ArgumentNullException(nameof(objectContract));

            ValueContract.Enrich(objectContract);
            return component.WithBindingIfAbsent(ValueContract.BindingFor(RegisteredBindingPath));
        }

        internal string DescribeContract() =>
            $"[{_identity.ComponentId.Value}, {_identity.Vendor.Value}, {_binding.ValueMember.Value}, {_componentType.Value}, {Shape.DescribeContract()}]";

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
