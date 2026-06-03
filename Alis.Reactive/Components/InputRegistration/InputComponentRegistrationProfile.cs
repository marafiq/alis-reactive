using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    internal sealed class InputComponentRegistrationProfile
    {
        private readonly IInputComponent _component;
        private readonly ComponentKind _componentType;

        private InputComponentRegistrationProfile(IInputComponent component, ComponentKind componentType)
        {
            _component = component;
            _componentType = componentType;
        }

        internal string Vendor => _component.Vendor;

        internal string ValueMember => _component.ValueMember;

        internal ComponentRegistration Register(ModelBoundInputComponentSlot slot)
        {
            var identity = RegisteredComponentIdentity.For(slot.ComponentId, ComponentVendor.From(_component.Vendor));
            var binding = RegisteredInputBinding.For(slot.BindingPath, MemberName.Of(_component.ValueMember));
            return ComponentRegistration.RegisteredInput(identity, binding, _componentType, slot.ValueShape);
        }

        internal static InputComponentRegistrationProfile For(
            IInputComponent component,
            string componentType) =>
            new InputComponentRegistrationProfile(component, ComponentKind.Of(componentType));
    }
}
