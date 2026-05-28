using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// Deterministic browser object target for a component reference.
    /// The target owns the component id and vendor before any property, method, or event
    /// contract is declared against the runtime object.
    /// </summary>
    internal abstract class ComponentObjectTarget
    {
        private protected ComponentObjectTarget(
            ComponentId componentId,
            ComponentVendor vendor)
        {
            ComponentId = componentId ?? throw new ArgumentNullException(nameof(componentId));
            Vendor = vendor ?? throw new ArgumentNullException(nameof(vendor));
        }

        internal ComponentId ComponentId { get; }

        internal ComponentVendor Vendor { get; }

        internal string IdForJson => ComponentId.Value;

        internal abstract ComponentKey EnsureIn(PlanBuildContext context);

        internal static ComponentObjectTarget For<TComponent>(string componentId)
            where TComponent : IComponent, new()
        {
            if (componentId == null) throw new ArgumentNullException(nameof(componentId));

            var component = new TComponent();
            return For(componentId, component.Vendor);
        }

        internal static ComponentObjectTarget ForLayout<TComponent>()
            where TComponent : IAppLevelComponent, new()
        {
            var component = new TComponent();
            return new LayoutObjectTarget(
                ComponentId.Of(component.DefaultId),
                ComponentVendor.From(component.Vendor));
        }

        internal static ComponentObjectTarget For(string componentId, string vendor)
        {
            if (componentId == null) throw new ArgumentNullException(nameof(componentId));
            if (vendor == null) throw new ArgumentNullException(nameof(vendor));

            return new ObjectTarget(
                ComponentId.Of(componentId),
                ComponentVendor.From(vendor));
        }

        private sealed class ObjectTarget : ComponentObjectTarget
        {
            internal ObjectTarget(ComponentId componentId, ComponentVendor vendor)
                : base(componentId, vendor)
            {
            }

            internal override ComponentKey EnsureIn(PlanBuildContext context)
            {
                return context.EnsureObjectTarget(ComponentId.Value, Vendor.Value);
            }
        }

        private sealed class LayoutObjectTarget : ComponentObjectTarget
        {
            internal LayoutObjectTarget(ComponentId componentId, ComponentVendor vendor)
                : base(componentId, vendor)
            {
            }

            internal override ComponentKey EnsureIn(PlanBuildContext context)
            {
                return context.EnsureLayoutObject(ComponentId.Value, Vendor.Value);
            }
        }
    }
}
