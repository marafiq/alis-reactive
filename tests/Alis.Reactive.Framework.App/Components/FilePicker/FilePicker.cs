using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Framework.App.Components;

/// <summary>
/// A file input for a model member (file-picker.ts): the input element keeps the picker as <c>filePicker</c>,
/// takes listeners through <c>on</c> and <c>off</c>, and holds the files the person chose as the browser's own
/// <c>File</c> objects, the way an uploader hands them to a page. Choosing commits the input.
/// </summary>
public sealed class FilePicker : Component, IInputComponent
{
    public static ComponentVendor FilePickerVendor { get; } = ComponentVendor.Define(
        "file-picker",
        objectPath: "filePicker",
        subscribe: "on",
        unsubscribe: "off",
        payload: VendorEventPayload.Argument);

    public override ComponentVendor Vendor => FilePickerVendor;

    /// <summary>The files the person chose, in the order chosen; none until they choose.</summary>
    public ReadOnly<IReadOnlyList<BrowserFile>> Files => ReadOnly<IReadOnlyList<BrowserFile>>();

    /// <summary>Raised when the person chooses files.</summary>
    public Event<FilesChosen> Chosen => Event<FilesChosen>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Files, committedBy: Chosen);
}

/// <summary>Payload of <see cref="FilePicker.Chosen"/>: the choosing itself, with nothing to read.</summary>
public sealed class FilesChosen : JsObject;

public static class FilePickerHtml
{
    /// <summary>A picker of any number of files for the member the input field binds.</summary>
    public static RenderedComponent<FilePicker, TModel> FilePicker<TModel, TProp>(this InputBoundField<TModel, TProp> field)
        where TModel : class =>
        field.Render<FilePicker>(new HtmlString(
            $"<input class=\"file-picker\" id=\"{HtmlEncoder.Default.Encode(field.ElementId)}\" type=\"file\" multiple>"));
}
