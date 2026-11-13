using Alis.Reactive.Declarations;
using Alis.Reactive.Native.NativeComponents;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.Native.NativeAppLevelObjects;

/// <summary>
/// The page's drawer: a panel the layout renders once, which a view opens, sizes and fills with a partial view.
/// </summary>
/// <remarks>
/// A view reaches it with no id, <c>pipeline.Component&lt;NativeDrawer&gt;()</c>. Its content is a slot: a view
/// loads a partial into <see cref="ContentId"/>, and unloads it in a handler of <see cref="Closed"/> or replaces it
/// with its next load. A handler of <see cref="BeforeClose"/> keeps the drawer open by writing the payload's
/// <c>Cancel</c> before its first awaited step.
/// </remarks>
public sealed class NativeDrawer : NativeComponent, IAppLevelComponent
{
    /// <summary>The id of the drawer's element on every page.</summary>
    public const string ElementId = "alis-drawer";

    /// <summary>The id of the drawer's heading, whose text a view sets.</summary>
    public const string TitleId = "alis-drawer-title";

    /// <summary>The id of the drawer's content, the slot a view loads a partial view into.</summary>
    public const string ContentId = "alis-drawer-content";

    /// <summary>Gets the id of the drawer's element, <see cref="ElementId"/>.</summary>
    public string DefaultId => ElementId;

    /// <summary>Gets the drawer's size, medium from the render and after each close.</summary>
    public Property<DrawerSize> Size => Property<DrawerSize>();

    /// <summary>Opens the drawer.</summary>
    public void Open() => Method();

    /// <summary>Closes the drawer, raising <see cref="BeforeClose"/> first.</summary>
    public void Close() => Method();

    /// <summary>Gets the event raised before the drawer closes, by Escape, its close button or <see cref="Close"/>.</summary>
    public Event<BeforeCloseArgs> BeforeClose => Event<BeforeCloseArgs>();

    /// <summary>Gets the event raised once a close no handler cancelled is done and the drawer is reset.</summary>
    public Event Closed => Event();

    /// <summary>
    /// What a listener of <see cref="BeforeClose"/> receives.
    /// </summary>
    public sealed class BeforeCloseArgs : JsObject
    {
        /// <summary>Gets what is closing the drawer.</summary>
        public ReadOnly<DrawerClosedBy> ClosedBy => ReadOnly<DrawerClosedBy>();

        /// <summary>Gets whether the close is cancelled; a write of true before the handler's first awaited step keeps the drawer open.</summary>
        public Property<bool> Cancel => Property<bool>();
    }
}
