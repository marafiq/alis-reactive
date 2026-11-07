using Alis.Reactive.Components;
using Alis.Reactive.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Poc.App.Components;

public sealed class NativeTextBox : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public Property<string> Value => Property<string>();

    public Event Input => Event();
}

public static class NativeTextBoxHtml
{
    public static Rendered<NativeTextBox, TModel> TextBox<TModel>(this ReactivePlan<TModel> plan, string id, string label, string value)
    {
        var caption = new TagBuilder("label");
        caption.Attributes["for"] = id;
        caption.InnerHtml.Append(label);
        var input = new TagBuilder("input") { TagRenderMode = TagRenderMode.SelfClosing };
        input.Attributes["id"] = id;
        input.Attributes["type"] = "text";
        input.Attributes["value"] = value;
        var html = new HtmlContentBuilder().AppendHtml(caption).AppendHtml(input);
        return plan.Render<NativeTextBox>(id, html);
    }

    public static Rendered<NativeTextBox, TModel> TextBox<TModel, TValue>(this InputField<TModel, TValue> field, string label)
    {
        var caption = new TagBuilder("label");
        caption.Attributes["for"] = field.Id;
        caption.InnerHtml.Append(label);
        var input = field.Html.TextBoxFor(field.Member, new Dictionary<string, object> { ["id"] = field.Id });
        return field.Render<NativeTextBox>(new HtmlContentBuilder().AppendHtml(caption).AppendHtml(input));
    }
}
