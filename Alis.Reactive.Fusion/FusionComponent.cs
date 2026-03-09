namespace Alis.Reactive.Fusion
{
    /// <summary>
    /// Base marker for all Syncfusion-backed components.
    /// Phantom type — sealed subclasses carry zero state.
    /// Used as generic type parameter in p.Component&lt;T&gt;() to constrain
    /// which extension methods (vertical slice) are available.
    /// </summary>
    public abstract class FusionComponent : IComponent
    {
    }

    /// <summary>
    /// Marker interface for SF input components that bind to a model property.
    /// Components implementing this support SetValue, Enable, Disable, etc.
    /// </summary>
    public interface IFusionInputComponent { }
}
