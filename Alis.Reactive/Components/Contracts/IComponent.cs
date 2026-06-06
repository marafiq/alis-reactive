namespace Alis.Reactive
{
    /// <summary>
    /// Represents a component contract that can be registered in a Reactive Plan.
    /// </summary>
    public interface IComponent
    {
        /// <summary>Vendor key used to resolve the component implementation at runtime.</summary>
        string Vendor { get; }
    }

    /// <summary>
    /// Represents a model-bound input component whose current value can be gathered or validated.
    /// </summary>
    public interface IInputComponent : IComponent
    {
        /// <summary>
        /// Component member read by gather and validation for model-bound inputs.
        /// </summary>
        string ValueMember { get; }
    }

    /// <summary>
    /// Represents a layout-owned component that can be referenced without an explicit element ID.
    /// </summary>
    public interface IAppLevelComponent : IComponent
    {
        /// <summary>Element ID used when the DSL references the app-level component without an explicit ID.</summary>
        string DefaultId { get; }
    }
}
