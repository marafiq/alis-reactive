using System.Text;
using System.Text.Encodings.Web;
using Alis.Reactive.Framework.App.Components;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Framework.App.Vendors;

public sealed class NewerSlate : Component
{
    public override ComponentVendor Vendor => Slate.SlateVendor;

    public Property<string> Title => Property<string>();

    public ReadOnly<int> Pins => ReadOnly<int>();

    public Property<bool> Pinned => Property<bool>();

    public void Pin() => Method();

    public void Clear() => Method();

    public NewerRow RowAt(Arg<int> index) => Object<NewerRow>(index);

    public NewerRow FindRow(Arg<string> label) => Object<NewerRow>(label);

    public TypedSource<int> RowsSaved() => Method<int>(name: "saveRows");

    public NewerCard LastCard => Object<NewerCard>();

    public Event<NewerRowAdded> RowAdded => Event<NewerRowAdded>();

    public Event<NewerCleared> Cleared => Event<NewerCleared>();
}

public sealed class NewerCard : JsObject
{
    public ReadOnly<string> Name => ReadOnly<string>();
}

public sealed class NewerRowAdded : JsObject
{
    public ReadOnly<int> Count => ReadOnly<int>();

    public NewerRow Row => Object<NewerRow>();

    public void Highlight() => Method();
}

public sealed class NewerCleared : JsObject
{
    public ReadOnly<string> By => ReadOnly<string>();

    public Property<string> Note => Property<string>();

    public void Acknowledge() => Method();
}

public sealed class NewerRow : JsObject
{
    public ReadOnly<string> Label => ReadOnly<string>();

    public Property<string> Note => Property<string>();

    public void Mark() => Method();

    public void Pin() => Method();
}

public static class NewerSlateHtml
{
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
