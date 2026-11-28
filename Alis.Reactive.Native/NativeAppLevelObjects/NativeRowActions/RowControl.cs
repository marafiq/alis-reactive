using System.Collections.Generic;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Native.NativeAppLevelObjects;

/// <summary>
/// One row's control of a row action: a button, or a link whose address is the action's request.
/// </summary>
/// <remarks>The control's id and, for a link, its address are the action's; a view sets its classes.</remarks>
public sealed partial class RowControl
{
    private readonly string _tag;
    private readonly string _id;
    private readonly string _text;
    private readonly IReadOnlyList<KeyValuePair<string, string>> _attributes;
    private bool _hasCssClass;
    private string _cssClass = "";

    private RowControl(string tag, string id, string text, IReadOnlyList<KeyValuePair<string, string>> attributes)
    {
        _tag = tag;
        _id = id;
        _text = text;
        _attributes = attributes;
    }

    /// <summary>Sets the CSS classes of the control's element.</summary>
    /// <param name="css">The class names, separated by spaces.</param>
    /// <returns>The control.</returns>
    public RowControl CssClass(string css)
    {
        _hasCssClass = true;
        _cssClass = css;
        return this;
    }

    internal static RowControl Button(string id, string text) => new("button", id, text, [new("type", "button")]);

    internal static RowControl Link(RowLink link, string text) => new("a", link.Id, text, [new("href", link.Address)]);
}
