using System;
using Alis.Reactive.Declarations;

namespace Alis.Reactive.Vendors;

/// <summary>
/// How the components of one library are found on their elements and listened to.
/// </summary>
/// <remarks>
/// A vendor is defined once, as a static member, and every declaration of the library names it. The page's plans
/// share a vendor by its name, so one name states one way of finding and listening.
/// </remarks>
public sealed class ComponentVendor
{
    private const string NamesTheVendorsOwnApi = "The object's path and its methods are named by their JavaScript names: this door describes a library's own API to the framework.";

    private ComponentVendor()
    {
    }

    /// <summary>Gets the vendor of an element that is its own object, such as a custom element, listened to as the DOM listens.</summary>
    public static ComponentVendor Native => throw new NotImplementedException();

    /// <summary>Gets the vendor's name, by which every plan of a page names it.</summary>
    public string Name => throw new NotImplementedException();

    /// <summary>Defines a vendor whose object is at a path from its element, with one method that adds a listener and one that removes it.</summary>
    /// <param name="name">The vendor's name, one for each library.</param>
    /// <param name="objectPath">The path from the element to the component's object: empty for the element itself, a member such as <c>tomselect</c>, or an index such as <c>ej2_instances.0</c>.</param>
    /// <param name="subscribe">The JavaScript name of the method that adds a listener, called with the event's name and the listener.</param>
    /// <param name="unsubscribe">The JavaScript name of the method that removes one, called with the same two.</param>
    /// <param name="elementPath">The path from the object to the element that holds it, for a library whose events hand a component over; null for one whose events hand none.</param>
    /// <returns>The vendor.</returns>
    [TypedDslExemption(NamesTheVendorsOwnApi)]
    public static ComponentVendor Define(string name, string objectPath, string subscribe, string unsubscribe, string? elementPath = null)
        => throw new NotImplementedException();

    /// <summary>Defines a vendor whose object is at a path from its element and raises an event by calling the property the event names.</summary>
    /// <param name="name">The vendor's name, one for each library.</param>
    /// <param name="objectPath">The path from the element to the component's object: empty for the element itself, a member such as <c>tomselect</c>, or an index such as <c>ej2_instances.0</c>.</param>
    /// <param name="elementPath">The path from the object to the element that holds it, for a library whose events hand a component over; null for one whose events hand none.</param>
    /// <returns>The vendor.</returns>
    [TypedDslExemption(NamesTheVendorsOwnApi)]
    public static ComponentVendor DefineCallbackProperties(string name, string objectPath, string? elementPath = null)
        => throw new NotImplementedException();

    /// <summary>Defines a vendor whose object is at a path from its element, with one method that adds a listener and returns the function that removes it.</summary>
    /// <param name="name">The vendor's name, one for each library.</param>
    /// <param name="objectPath">The path from the element to the component's object: empty for the element itself, a member such as <c>tomselect</c>, or an index such as <c>ej2_instances.0</c>.</param>
    /// <param name="subscribe">The JavaScript name of the method that adds a listener and returns its remover.</param>
    /// <param name="elementPath">The path from the object to the element that holds it, for a library whose events hand a component over; null for one whose events hand none.</param>
    /// <returns>The vendor.</returns>
    [TypedDslExemption(NamesTheVendorsOwnApi)]
    public static ComponentVendor DefineReturnedUnsubscriber(string name, string objectPath, string subscribe, string? elementPath = null)
        => throw new NotImplementedException();

    /// <summary>Defines a vendor whose object a lookup finds, with one method that adds a listener and one that removes it.</summary>
    /// <param name="name">The vendor's name, one for each library.</param>
    /// <param name="lookup">The lookup that finds the component's object for its element.</param>
    /// <param name="subscribe">The JavaScript name of the method that adds a listener, called with the event's name and the listener.</param>
    /// <param name="unsubscribe">The JavaScript name of the method that removes one, called with the same two.</param>
    /// <param name="elementPath">The path from the object to the element that holds it, for a library whose events hand a component over; null for one whose events hand none.</param>
    /// <returns>The vendor.</returns>
    [TypedDslExemption(NamesTheVendorsOwnApi)]
    public static ComponentVendor Define(string name, VendorLookup lookup, string subscribe, string unsubscribe, string? elementPath = null)
        => throw new NotImplementedException();

    /// <summary>Defines a vendor whose object a lookup finds and which raises an event by calling the property the event names.</summary>
    /// <param name="name">The vendor's name, one for each library.</param>
    /// <param name="lookup">The lookup that finds the component's object for its element.</param>
    /// <param name="elementPath">The path from the object to the element that holds it, for a library whose events hand a component over; null for one whose events hand none.</param>
    /// <returns>The vendor.</returns>
    [TypedDslExemption(NamesTheVendorsOwnApi)]
    public static ComponentVendor DefineCallbackProperties(string name, VendorLookup lookup, string? elementPath = null)
        => throw new NotImplementedException();

    /// <summary>Defines a vendor whose object a lookup finds, with one method that adds a listener and returns the function that removes it.</summary>
    /// <param name="name">The vendor's name, one for each library.</param>
    /// <param name="lookup">The lookup that finds the component's object for its element.</param>
    /// <param name="subscribe">The JavaScript name of the method that adds a listener and returns its remover.</param>
    /// <param name="elementPath">The path from the object to the element that holds it, for a library whose events hand a component over; null for one whose events hand none.</param>
    /// <returns>The vendor.</returns>
    [TypedDslExemption(NamesTheVendorsOwnApi)]
    public static ComponentVendor DefineReturnedUnsubscriber(string name, VendorLookup lookup, string subscribe, string? elementPath = null)
        => throw new NotImplementedException();
}

/// <summary>
/// Where a library that keeps its objects off their elements finds one: a lookup its browser code registers as a plugin.
/// </summary>
/// <remarks>
/// The lookup is called with the element and returns the component's object. An element it finds nothing for
/// stops the pipeline that reaches the component.
/// </remarks>
public sealed class VendorLookup
{
    private VendorLookup()
    {
    }

    /// <summary>Names the plugin a library's browser code registers as its lookup.</summary>
    /// <param name="name">The name the lookup is registered under.</param>
    /// <returns>The lookup, given to a vendor in place of its object's path.</returns>
    public static VendorLookup ByPlugin(string name) => throw new NotImplementedException();
}
