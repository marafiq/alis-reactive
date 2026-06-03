namespace Alis.Reactive
{
    /// <summary>
    /// Marker interface for a component object that can be referenced in a Reactive Plan.
    /// </summary>
    public interface IComponent
    {
        /// <summary>Gets the vendor identifier for component resolution.</summary>
        string Vendor { get; }
    }

    /// <summary>
    /// Marker interface for model-bound input components that expose a readable value member.
    /// </summary>
    public interface IInputComponent : IComponent
    {
        /// <summary>
        /// Gets the member name on the JavaScript component object that gather and validation read.
        /// </summary>
        string ValueMember { get; }
    }

    /// <summary>
    /// Marker interface for layout-owned app components with a well-known component id.
    /// </summary>
    public interface IAppLevelComponent : IComponent
    {
        /// <summary>Gets the default DOM element ID for this layout-owned component.</summary>
        string DefaultId { get; }
    }
}
