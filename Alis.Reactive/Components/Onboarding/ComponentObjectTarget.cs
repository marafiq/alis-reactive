using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// Deterministic target for a component reference in the Reactive Plan.
    /// Target owns the component id and vendor before any property, method, or event
    /// contract is declared against the runtime object.
    /// </summary>
    internal sealed class ComponentObjectTarget
    {
        private ComponentObjectTarget(ComponentId componentId, ComponentVendor vendor)
        {
            ComponentId = componentId ?? throw new ArgumentNullException(nameof(componentId));
            Vendor = vendor ?? throw new ArgumentNullException(nameof(vendor));
        }

        internal ComponentId ComponentId { get; }

        internal ComponentVendor Vendor { get; }

        internal ComponentKey DeclareIn(PlanBuildContext context) =>
            context.DeclareObjectTarget(ComponentId.Value, Vendor);

        internal static ComponentObjectTarget For<TComponent>(string componentId)
            where TComponent : IComponent, new()
        {
            if (componentId == null) throw new ArgumentNullException(nameof(componentId));

            var component = new TComponent();
            return For(componentId, component.Vendor);
        }

        /// <summary>An app-level object at its fixed id.</summary>
        internal static ComponentObjectTarget ForLayout<TComponent>()
            where TComponent : IAppLevelComponent, new()
        {
            var component = new TComponent();
            return new ComponentObjectTarget(ComponentId.Of(component.DefaultId), component.Vendor);
        }

        internal static ComponentObjectTarget For(string componentId, ComponentVendor vendor)
        {
            if (componentId == null) throw new ArgumentNullException(nameof(componentId));
            if (vendor == null) throw new ArgumentNullException(nameof(vendor));

            return new ComponentObjectTarget(ComponentId.Of(componentId), vendor);
        }
    }
}
