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
        /// <summary>
        /// Syncfusion EJ2: the component object is <c>ej2_instances[0]</c> on the rendered element,
        /// events are added with <c>addEventListener</c>, and a listener receives the event arguments.
        /// </summary>
        public static ComponentVendor FusionVendor { get; } = ComponentVendor.Define(
            "fusion",
            objectPath: "ej2_instances.0",
            subscribe: "addEventListener",
            unsubscribe: "removeEventListener",
            payload: VendorEventPayload.Argument);

        /// <inheritdoc />
        public sealed override ComponentVendor Vendor => FusionVendor;
    }
}
