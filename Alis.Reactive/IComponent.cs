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
    /// Interface for components that can be read — provides ReadExpr as an instance property.
    /// Used by gather and validation extensions as a generic constraint (with new()).
    /// </summary>
    public interface IInputComponent : IComponent
    {
        /// <summary>
        /// The property path from the vendor-determined root for reading.
        /// Examples: "value", "checked"
        /// </summary>
        string ReadExpr { get; }
    }

    /// <summary>
    /// Marker for app-level components that have a well-known element ID.
    /// Enables the parameterless overload: p.Component&lt;FusionConfirm&gt;()
    /// </summary>
    public interface IAppLevelComponent : IComponent
    {
        string DefaultId { get; }
    }
}
