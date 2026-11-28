using Alis.Reactive.Vendors;

namespace Alis.Reactive.Native.NativeComponents;

/// <summary>
/// A component that is a plain HTML element: the element its render helper rendered is the object a view reads,
/// writes and listens to.
/// </summary>
/// <remarks>
/// A listener is added by <c>addEventListener</c> and removed by <c>removeEventListener</c>, and it receives the
/// event object the browser hands it.
/// </remarks>
public abstract class NativeComponent : Component
{
    /// <summary>Gets the vendor of every native component: the element is its own object, listened to as the DOM listens.</summary>
    public sealed override ComponentVendor Vendor => ComponentVendor.Native;
}
