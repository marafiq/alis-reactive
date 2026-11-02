namespace Alis.Reactive
{
    /// <summary>
    /// Represents a component contract that can be registered in a Reactive Plan.
    /// </summary>
    public interface IComponent
    {
        /// <summary>Where the component's JavaScript object lives on its element and how it raises events.</summary>
        ComponentVendor Vendor { get; }
    }

    /// <summary>
    /// Represents a model-bound input component whose current value can be gathered or validated.
    /// </summary>
    public interface IInputComponent : IComponent
    {
        /// <summary>
        /// The property that holds the input's value and the event that commits it. Implement it
        /// explicitly, so it stays off the members a page reads and calls:
        /// <c>ComponentInput IInputComponent.Input =&gt; ComponentInput.Of(Value, committedBy: Change);</c>
        /// </summary>
        ComponentInput Input { get; }
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
