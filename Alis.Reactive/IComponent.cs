namespace Alis.Reactive
{
    /// <summary>
    /// Base interface for all reactive components (Fusion SF, Native DOM).
    /// Every component declares its vendor ("native" or "fusion") as an instance property.
    /// </summary>
    /// <summary>Marker interface for reactive components that can be referenced in the pipeline.</summary>
    public interface IComponent
    {
        /// <summary>Gets the vendor identifier for component resolution.</summary>
        /// <summary>Gets the vendor identifier for component resolution.</summary>
        string Vendor { get; }
    }

    /// <summary>
    /// Interface for input components — provides the member name for reading the component's value.
    /// Used by gather and validation extensions.
    /// </summary>
    public interface IInputComponent : IComponent
    {
        /// <summary>
        /// The member name on the JS object for reading the component's value.
        /// Examples: "value", "checked"
        /// </summary>
        string ValueMember { get; }
    }

    /// <summary>
    /// Marker for app-level components with a well-known element ID.
    /// </summary>
    /// <summary>Marker interface for app-level singleton components (Toast, Confirm) with a default DOM ID.</summary>
    public interface IAppLevelComponent : IComponent
    {
        /// <summary>Gets the default DOM element ID for this app-level component.</summary>
        /// <summary>Gets the default DOM element ID for this app-level component.</summary>
        string DefaultId { get; }
    }
}
