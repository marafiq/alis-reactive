using Alis.Reactive.Components;
using Alis.Reactive.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Poc.App.Components;

public sealed class WardPicker : Component
{
    public static ComponentVendor PickerVendor { get; } = ComponentVendor.Define("ward", objectPath: "picker", subscribe: "addEventListener", unsubscribe: "removeEventListener");

    public override ComponentVendor Vendor => PickerVendor;

    public Property<string?> Selected => Property<string?>();

    public Event Change => Event();
}

public static class WardPickerHtml
{
    public static Rendered<WardPicker, TModel> WardPicker<TModel>(this InputField<TModel, string?> field, string label, IReadOnlyList<string> wards)
    {
        var picker = new TagBuilder("fieldset");
        picker.Attributes["id"] = field.Id;
        picker.Attributes["data-ward-picker"] = string.Join("|", wards);
        var legend = new TagBuilder("legend");
        legend.InnerHtml.Append(label);
        var posted = new TagBuilder("input") { TagRenderMode = TagRenderMode.SelfClosing };
        posted.Attributes["type"] = "hidden";
        posted.Attributes["name"] = field.Name;
        posted.Attributes["value"] = field.Html.ValueFor(field.Member);
        picker.InnerHtml.AppendHtml(legend).AppendHtml(posted);
        return field.Render<WardPicker>(picker);
    }
}
