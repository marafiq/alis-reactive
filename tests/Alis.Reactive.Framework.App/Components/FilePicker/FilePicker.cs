using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Framework.App.Components;

public sealed class FilePicker : Component, IInputComponent
{
    public static ComponentVendor FilePickerVendor { get; } = ComponentVendor.Define(
        "file-picker",
        objectPath: "filePicker",
        subscribe: "on",
        unsubscribe: "off",
        payload: VendorEventPayload.Argument);

    public override ComponentVendor Vendor => FilePickerVendor;

    public ReadOnly<IReadOnlyList<BrowserFile>> Files => ReadOnly<IReadOnlyList<BrowserFile>>();

    public ReadOnly<IReadOnlyList<BrowserFile>> Listed => ReadOnly<IReadOnlyList<BrowserFile>>();

    public Event<FilesChosen> Chosen => Event<FilesChosen>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Files, committedBy: Chosen);
}

public sealed class FilesChosen : JsObject;

public static class FilePickerHtml
{
    public static RenderedComponent<FilePicker, TModel> FilePicker<TModel, TProp>(this InputBoundField<TModel, TProp> field)
        where TModel : class =>
        field.Render<FilePicker>(new HtmlString(
            $"<input class=\"file-picker\" id=\"{HtmlEncoder.Default.Encode(field.ElementId)}\" type=\"file\" multiple>"));
}
