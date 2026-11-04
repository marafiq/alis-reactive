using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Framework.App.Components;

public sealed class FileInput : Component, IInputComponent
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public ReadOnly<IReadOnlyList<BrowserFile>> Files => ReadOnly<IReadOnlyList<BrowserFile>>();

    public Event<FilesChanged> Change => Event<FilesChanged>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Files, committedBy: Change);
}

public sealed class FilesChanged : JsObject;

public static class FileInputHtml
{
    public static RenderedComponent<FileInput, TModel> FileInput<TModel, TProp>(this InputBoundField<TModel, TProp> field)
        where TModel : class
        => field.Render<FileInput>(new HtmlString(
            $"<input id=\"{HtmlEncoder.Default.Encode(field.ElementId)}\" name=\"{HtmlEncoder.Default.Encode(field.BindingPath)}\" type=\"file\" multiple>"));
}
