using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// Deterministic browser object target for a component reference.
    /// The target owns the component id and vendor before any property, method, or event
    /// contract is declared against the runtime object.
    /// </summary>
    internal sealed class ComponentObjectTarget
    {
        private ComponentObjectTarget(
            ComponentId componentId,
            ComponentVendor vendor,
            ComponentRole role)
        {
            ComponentId = componentId ?? throw new ArgumentNullException(nameof(componentId));
            Vendor = vendor ?? throw new ArgumentNullException(nameof(vendor));
            Role = role ?? throw new ArgumentNullException(nameof(role));
        }

        internal ComponentId ComponentId { get; }

        internal ComponentVendor Vendor { get; }

        internal ComponentRole Role { get; }

        internal string IdForJson => ComponentId.Value;

        internal ComponentKey EnsureIn(PlanBuildContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return context.EnsureComponent(ComponentId.Value, Vendor.Value, Role);
        }

        internal static ComponentObjectTarget For<TComponent>(string componentId)
            where TComponent : IComponent, new()
        {
            if (componentId == null) throw new ArgumentNullException(nameof(componentId));

            var component = new TComponent();
            return For(componentId, component.Vendor, ComponentRole.ObjectTarget);
        }

        internal static ComponentObjectTarget ForLayout<TComponent>()
            where TComponent : IAppLevelComponent, new()
        {
            var component = new TComponent();
            return For(component.DefaultId, component.Vendor, ComponentRole.LayoutObject);
        }

        internal static ComponentObjectTarget For(string componentId, string vendor)
            => For(componentId, vendor, ComponentRole.ObjectTarget);

        private static ComponentObjectTarget For(
            string componentId,
            string vendor,
            ComponentRole role)
        {
            if (componentId == null) throw new ArgumentNullException(nameof(componentId));
            if (vendor == null) throw new ArgumentNullException(nameof(vendor));
            if (role == null) throw new ArgumentNullException(nameof(role));

            return new ComponentObjectTarget(
                ComponentId.Of(componentId),
                ComponentVendor.From(vendor),
                role);
        }
    }
}
