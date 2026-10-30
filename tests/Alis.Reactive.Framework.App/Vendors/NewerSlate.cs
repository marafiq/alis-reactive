using System.Text;
using System.Text.Encodings.Web;
using Alis.Reactive.Framework.App.Components;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Framework.App.Vendors;

/// <summary>
/// The slate as a later version of it is declared: it keeps the slate's title, and names members the slate
/// the page runs does not have (pins, a pinned flag, a pin method), a last card it holds as an object where
/// the slate holds nothing until a card is filled, and a row its row-added event does not pass. A page
/// written against this declaration meets the slate at the checked paths of its members.
/// </summary>
public sealed class NewerSlate : Component
{
    public override ComponentVendor Vendor => Slate.SlateVendor;

    public Property<string> Title => Property<string>();

    /// <summary>How many times the slate was pinned: the slate the page runs has no pins.</summary>
    public ReadOnly<int> Pins => ReadOnly<int>();

    /// <summary>Whether the slate is pinned: the slate the page runs has no such member.</summary>
    public Property<bool> Pinned => Property<bool>();

    /// <summary>Pins the slate: the slate the page runs has no such method.</summary>
    public void Pin() => Method();

    /// <summary>The card filled last, as an object: the slate holds nothing there until a card is filled.</summary>
    public NewerCard LastCard => Object<NewerCard>();

    /// <summary>Raised by the slate's own Add row button; the slate passes no row with it.</summary>
    public Event<NewerRowAdded> RowAdded => Event<NewerRowAdded>();
}

public sealed class NewerCard : JsObject
{
    public ReadOnly<string> Name => ReadOnly<string>();
}

public sealed class NewerRowAdded : JsObject
{
    public ReadOnly<int> Count => ReadOnly<int>();

    /// <summary>The row added, as the later version passes it.</summary>
    public NewerRow Row => Object<NewerRow>();
}

public sealed class NewerRow : JsObject
{
    public ReadOnly<string> Label => ReadOnly<string>();
}

public static class NewerSlateHtml
{
    /// <summary>A slate with its title and first rows, written against the later declaration.</summary>
    public static RenderedComponent<NewerSlate, TModel> NewerSlate<TModel>(
        this ReactivePlan<TModel> plan,
        string id,
        string title,
        params string[] rows)
        where TModel : class
    {
        var markup = new StringBuilder()
            .Append("<div class=\"slate\" id=\"").Append(Encode(id))
            .Append("\" data-title=\"").Append(Encode(title))
            .Append("\" data-rows=\"").Append(Encode(string.Join("|", rows)))
            .Append("\"></div>");
        return plan.Render<NewerSlate>(id, new HtmlString(markup.ToString()));
    }

    private static string Encode(string value) => HtmlEncoder.Default.Encode(value);
}
