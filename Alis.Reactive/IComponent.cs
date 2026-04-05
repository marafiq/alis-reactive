namespace Alis.Reactive
{
    /// <summary>
    /// Base interface for all reactive components (Fusion SF, Native DOM).
    /// Every component declares its vendor ("native" or "fusion") as an instance property.
    /// </summary>
    public interface IComponent
    {
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
    public interface IAppLevelComponent : IComponent
    {
        string DefaultId { get; }
    }
}
