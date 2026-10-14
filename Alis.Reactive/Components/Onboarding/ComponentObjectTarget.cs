using System;

namespace Alis.Reactive
{
    /// <summary>A component the page reaches by its element id, from the vendor its declaration names.</summary>
    internal sealed class ComponentObjectTarget
    {
        private ComponentObjectTarget(string componentId, ComponentVendor vendor)
        {
            ComponentId = componentId ?? throw new ArgumentNullException(nameof(componentId));
            Vendor = vendor ?? throw new ArgumentNullException(nameof(vendor));
        }

        internal string ComponentId { get; }

        internal ComponentVendor Vendor { get; }

        internal static ComponentObjectTarget For<TComponent>(string componentId)
            where TComponent : IComponent, new() =>
            new ComponentObjectTarget(componentId, new TComponent().Vendor);

        /// <summary>An app-level object at its fixed id.</summary>
        internal static ComponentObjectTarget ForLayout<TComponent>()
            where TComponent : IAppLevelComponent, new()
        {
            var component = new TComponent();
            return new ComponentObjectTarget(component.DefaultId, component.Vendor);
        }
    }
}
