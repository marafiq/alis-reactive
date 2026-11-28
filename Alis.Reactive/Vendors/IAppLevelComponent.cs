namespace Alis.Reactive.Vendors;

/// <summary>
/// An app-level object: a component the page holds once, on an element whose id its declaration fixes, such as a
/// drawer or a confirm.
/// </summary>
/// <remarks>
/// A view reaches it with no id, <c>pipeline.Component&lt;TComponent&gt;()</c>, so the id is written once, in the
/// declaration. The object is the page's: no partial that leaves removes it.
/// </remarks>
public interface IAppLevelComponent
{
    /// <summary>Gets the id of the element that holds the object on every page.</summary>
    string DefaultId { get; }
}
