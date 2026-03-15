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
        /// <inheritdoc />
        public string Vendor => "fusion";
    }
}
