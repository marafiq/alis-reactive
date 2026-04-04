using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    internal sealed class ComponentMetadata
    {
        internal ComponentMetadata(string vendor, string? kind, CapabilityProperty? binding, ValueShape? bindingShape)
        {
            Vendor = vendor;
            Kind = kind;
            Binding = binding;
            BindingShape = bindingShape;
        }

        internal string Vendor { get; }
        internal string? Kind { get; }
        internal CapabilityProperty? Binding { get; }
        internal ValueShape? BindingShape { get; }

        internal static ComponentMetadata Create(
            string vendor,
            string kind,
            CapabilityProperty? binding,
            ValueShape? bindingShape = null)
        {
            return new ComponentMetadata(
                vendor,
                kind,
                binding,
                bindingShape);
        }
    }

    internal interface IReactiveComponentDescriptor
    {
        ComponentMetadata Metadata { get; }
    }

    internal static class ReactiveComponentMetadata
    {
        internal static ComponentMetadata For(IComponent component)
        {
            return component is IReactiveComponentDescriptor descriptor
                ? descriptor.Metadata
                : throw new InvalidOperationException(
                    $"{component.GetType().FullName} must explicitly declare component metadata.");
        }

        internal static ComponentMetadata For<TComponent>()
            where TComponent : IComponent, new()
        {
            return For(new TComponent());
        }
    }
}
