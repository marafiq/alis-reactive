using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Framework.App.Components;

public sealed class TagPicker : Component, IInputComponent
{
    public static ComponentVendor TagPickerVendor { get; } = ComponentVendor.Define(
        "tag-picker",
        objectPath: "tagPicker",
        subscribe: "on",
        unsubscribe: "off",
        payload: VendorEventPayload.Argument);

    public override ComponentVendor Vendor => TagPickerVendor;

    public ReadOnly<IReadOnlyList<PickedTag>> Picked => ReadOnly<IReadOnlyList<PickedTag>>();

    public ReadOnly<IReadOnlyList<string>> Codes => Picked.Each(tag => tag.Code);

    public Event<TagsPicked> Changed => Event<TagsPicked>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Codes, committedBy: Changed);
}

public sealed class PickedTag
{
    public string Code { get; set; } = "";

    public string Label { get; set; } = "";
}

public sealed class TagsPicked : JsObject;

public static class TagPickerHtml
{
    public static RenderedComponent<TagPicker, TModel> TagPicker<TModel, TProp>(this InputBoundField<TModel, TProp> field)
        where TModel : class
        => field.Render<TagPicker>(new HtmlString(
            $"<div class=\"tag-picker\" id=\"{HtmlEncoder.Default.Encode(field.ElementId)}\"></div>"));
}
