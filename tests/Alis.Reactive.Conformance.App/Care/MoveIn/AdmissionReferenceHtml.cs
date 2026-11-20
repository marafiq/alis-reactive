using System.Text.Encodings.Web;
using System.Text.Json;
using Alis.Reactive.Native.NativeComponents;
using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Conformance.App.Care;

public static class AdmissionReferenceHtml
{
    public static void WriteAdmissionReference(this InputField<AdmissionPrintPacket, string> field, int revision)
    {
        var draft = JsonSerializer.Deserialize<string>(field.ValueJson) ?? throw new InvalidOperationException("A printable admission needs its draft reference.");
        var input = new TagBuilder("input") { TagRenderMode = TagRenderMode.SelfClosing };
        input.Attributes["name"] = field.Name;
        input.Attributes["id"] = field.Id;
        input.Attributes["type"] = "text";
        input.Attributes["readonly"] = "readonly";
        input.Attributes["value"] = draft;
        input.Attributes["data-print-reference"] = field.Json(new { DraftId = draft, Revision = revision });
        var writer = field.Html.ViewContext.Writer;
        field.Html.LabelFor(field.Member, "Draft reference", new { @for = field.Id }).WriteTo(writer, HtmlEncoder.Default);
        field.Render<NativeTextBox>(input).WriteTo(writer, HtmlEncoder.Default);
    }
}
