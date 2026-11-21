using Alis.Reactive.Components;
using Alis.Reactive.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Poc.App.Components;

[Mirrors("Components/ResidentFinder/resident-finder", "ResidentFinder")]
public sealed class ResidentFinder : Component
{
    public static ComponentVendor FinderVendor { get; } = ComponentVendor.Define("resident-finder", objectPath: "finder", subscribe: "on", unsubscribe: "off");

    public override ComponentVendor Vendor => FinderVendor;

    public Event<Lookup> Looked => Event<Lookup>();

    public sealed class Lookup : JsObject
    {
        public ReadOnly<string> Term => ReadOnly<string>();

        public MethodResult<ResidentMatch?> Match() => MethodAsync<ResidentMatch?>();

        public MethodResult<int> Count() => MethodAsync<int>();
    }

    public sealed record ResidentMatch(string Name, string Room, WingOnRecord? Wing);

    public sealed record WingOnRecord(string Name, int Floor);
}

public static class ResidentFinderHtml
{
    public static Rendered<ResidentFinder, TModel> ResidentFinder<TModel, TValue>(this InputField<TModel, TValue> field, string label)
    {
        var caption = new TagBuilder("label");
        caption.Attributes["for"] = field.Id;
        caption.InnerHtml.Append(label);
        var input = field.Html.TextBoxFor(field.Member, new Dictionary<string, object> { ["id"] = field.Id, ["data-resident-finder"] = "" });
        return field.Render<ResidentFinder>(new HtmlContentBuilder().AppendHtml(caption).AppendHtml(input));
    }
}
