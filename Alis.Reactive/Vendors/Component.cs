using System;
using Alis.Reactive.Declarations;

namespace Alis.Reactive.Vendors;

/// <summary>
/// A JavaScript object an element holds: the component a library makes on it, declared member for member.
/// </summary>
/// <remarks>
/// A component declares its members with the doors of <see cref="JsObject"/> and names its vendor, which says
/// where the object is found on its element and how its events are listened to. An element the page lacks, or
/// one that holds no such object, stops the pipeline that reaches the component.
/// </remarks>
public abstract class Component : EventedObject
{
    /// <summary>Gets the vendor: where the component's object is found on its element and how its events are listened to.</summary>
    public abstract ComponentVendor Vendor { get; }

    /// <inheritdoc />
    protected internal sealed override EventListening Listening => throw new NotImplementedException();
}
