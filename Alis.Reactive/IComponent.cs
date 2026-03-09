namespace Alis.Reactive
{
    /// <summary>
    /// Marker interface for all reactive components (Fusion SF, Native DOM).
    /// Constrains p.Component&lt;T&gt;() to only accept component types.
    /// </summary>
    public interface IComponent { }

    /// <summary>
    /// Marker for app-level components that have a well-known element ID.
    /// Enables the parameterless overload: p.Component&lt;FusionConfirm&gt;()
    /// </summary>
    public interface IAppLevelComponent : IComponent
    {
        string DefaultId { get; }
    }
}
