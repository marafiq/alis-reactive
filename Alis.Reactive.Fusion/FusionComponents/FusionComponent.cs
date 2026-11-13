using Alis.Reactive.Vendors;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A Syncfusion EJ2 component: the object EJ2 creates on the element its render helper rendered is the object a view
/// reads, writes and listens to.
/// </summary>
/// <remarks>
/// A listener is added by EJ2's <c>addEventListener</c> and removed by its <c>removeEventListener</c>, and it receives
/// the event's arguments object, which each event declares.
/// </remarks>
public abstract class FusionComponent : Component
{
    /// <summary>Gets the vendor of every Fusion component: EJ2's object on the element, listened to as EJ2 listens.</summary>
    public static ComponentVendor FusionVendor => ComponentVendor.Define("fusion", objectPath: "ej2_instances.0", subscribe: "addEventListener", unsubscribe: "removeEventListener");

    /// <summary>Gets the vendor of the component, <see cref="FusionVendor"/>.</summary>
    public sealed override ComponentVendor Vendor => FusionVendor;
}
