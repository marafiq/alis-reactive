namespace Alis.Reactive.Framework.App.Components;

/// <summary>
/// A board that holds a title, a note and rows, and shows them in its own markup (slate.ts). It keeps
/// itself on its element as <c>slate</c>, takes listeners through <c>on</c> and <c>off</c>, and hands a
/// listener the event's payload.
/// </summary>
public sealed class Slate : Component
{
    public static ComponentVendor SlateVendor { get; } = ComponentVendor.Define(
        "slate",
        objectPath: "slate",
        subscribe: "on",
        unsubscribe: "off",
        payload: VendorEventPayload.Argument);

    public override ComponentVendor Vendor => SlateVendor;

    /// <summary>How many rows the slate holds; the slate counts them, nothing writes it.</summary>
    public ReadOnly<int> Count => ReadOnly<int>();

    /// <summary>A note the slate shows under its title; it can be written, never read back.</summary>
    public WriteOnly<string> Note => WriteOnly<string>();

    /// <summary>The slate's title, shown as its heading.</summary>
    public Property<string> Title => Property<string>();
}
