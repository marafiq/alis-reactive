namespace Alis.Reactive
{
    /// <summary>
    /// Marker interface for a component object that can be referenced in a Reactive Plan.
    /// </summary>
    public interface IComponent
    {
        /// <summary>Vendor token written into the Reactive Plan for runtime component resolution.</summary>
        string Vendor { get; }
    }

    /// <summary>
    /// Marker interface for model-bound input components that expose a readable value member.
    /// </summary>
    public interface IInputComponent : IComponent
    {
        /// <summary>
        /// JavaScript component-object member read by gather and validation for model-bound inputs.
        /// </summary>
        string ValueMember { get; }
    }

    /// <summary>
    /// Marker interface for layout-owned app components with a well-known component id.
    /// </summary>
    public interface IAppLevelComponent : IComponent
    {
        /// <summary>Well-known layout element ID used when the DSL references the app-level component without an explicit ID.</summary>
        string DefaultId { get; }
    }
}
