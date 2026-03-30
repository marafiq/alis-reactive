namespace Alis.Reactive.Fusion
{
    /// <summary>
    /// Base type for all Syncfusion-backed components.
    /// </summary>
    /// <remarks>
    /// Sealed subclasses (e.g. <see cref="Components.FusionDropDownList"/>,
    /// <see cref="Components.FusionDatePicker"/>) serve as type parameters in
    /// <c>p.Component&lt;T&gt;()</c> to scope which extension methods are available.
    /// </remarks>
    public abstract class FusionComponent : IComponent
    {
        /// <inheritdoc />
        public string Vendor => "fusion";
    }
}
