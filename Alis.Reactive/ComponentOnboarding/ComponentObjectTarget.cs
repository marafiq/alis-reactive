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
            ComponentContributionIntent contribution)
        {
            ComponentId = componentId ?? throw new ArgumentNullException(nameof(componentId));
            Vendor = vendor ?? throw new ArgumentNullException(nameof(vendor));
            Contribution = contribution ?? throw new ArgumentNullException(nameof(contribution));
        }

        internal ComponentId ComponentId { get; }

        internal ComponentVendor Vendor { get; }

        internal ComponentContributionIntent Contribution { get; }

        internal string IdForJson => ComponentId.Value;

        internal ComponentKey EnsureIn(PlanBuildContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return context.EnsureComponent(ComponentId.Value, Vendor.Value, Contribution);
        }

        internal static ComponentObjectTarget For<TComponent>(string componentId)
            where TComponent : IComponent, new()
        {
            if (componentId == null) throw new ArgumentNullException(nameof(componentId));

            var component = new TComponent();
            return For(componentId, component.Vendor, ComponentContributionIntent.ObjectTarget);
        }

        internal static ComponentObjectTarget ForLayout<TComponent>(string componentId)
            where TComponent : IAppLevelComponent, new()
        {
            if (componentId == null) throw new ArgumentNullException(nameof(componentId));

            var component = new TComponent();
            return For(componentId, component.Vendor, ComponentContributionIntent.LayoutObject);
        }

        internal static ComponentObjectTarget For(string componentId, string vendor)
            => For(componentId, vendor, ComponentContributionIntent.ObjectTarget);

        private static ComponentObjectTarget For(
            string componentId,
            string vendor,
            ComponentContributionIntent contribution)
        {
            if (componentId == null) throw new ArgumentNullException(nameof(componentId));
            if (vendor == null) throw new ArgumentNullException(nameof(vendor));
            if (contribution == null) throw new ArgumentNullException(nameof(contribution));

            return new ComponentObjectTarget(
                ComponentId.Of(componentId),
                ComponentVendor.From(vendor),
                contribution);
        }
    }
}
