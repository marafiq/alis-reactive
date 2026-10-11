namespace Alis.Reactive.Fusion
{
    /// <summary>
    /// Base type for all Syncfusion-backed components.
    /// </summary>
    /// <remarks>
    /// Sealed subclasses (e.g. <see cref="Components.FusionDropDownList"/>,
    /// <see cref="Components.FusionDatePicker"/>) serve as type parameters in
    /// <c>p.Component&lt;T&gt;()</c> and declare the Syncfusion object's members with the
    /// names its JavaScript API uses.
    /// </remarks>
    public abstract class FusionComponent : Component
    {
        /// <inheritdoc />
        public sealed override string Vendor => "fusion";
    }
}
