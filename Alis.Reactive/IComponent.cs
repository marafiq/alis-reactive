namespace Alis.Reactive
{
    /// <summary>
    /// Base interface for all reactive components (Fusion SF, Native DOM).
    /// Every component declares its vendor ("native" or "fusion") as an instance property.
    /// Constrains p.Component&lt;T&gt;() to only accept component types.
    /// </summary>
    public interface IComponent
    {
        /// <summary>
        /// The vendor identifier for this component ("native" or "fusion").
        /// Determines how the runtime resolves the component root.
        /// </summary>
        string Vendor { get; }
    }

    /// <summary>
    /// Marker for components that expose a bindable browser value.
    /// The actual browser member is declared once on the component type via
    /// the slice-owned component metadata.
    /// </summary>
    public interface IBindableComponent : IComponent
    {
    }

    /// <summary>
    /// Marker for app-level components that have a well-known element ID.
    /// Enables the parameterless overload: p.Component&lt;FusionConfirm&gt;()
    /// </summary>
    public interface IAppLevelComponent : IComponent
    {
        /// <summary>Gets the well-known element ID for this app-level component.</summary>
        string DefaultId { get; }
    }
}
