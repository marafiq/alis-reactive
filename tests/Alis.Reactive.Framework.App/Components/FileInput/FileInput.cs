using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Framework.App.Components;

/// <summary>
/// A plain <c>&lt;input type="file"&gt;</c> for a model member: the element is the object, reached through the
/// core's native vendor. Its files are the browser's own <c>FileList</c>, not an array, and choosing raises
/// its <c>change</c>, which commits the input. It needs no TypeScript.
/// </summary>
public sealed class FileInput : Component, IInputComponent
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    /// <summary>The files the person chose, as the browser lists them: the input's own <c>FileList</c>.</summary>
    public ReadOnly<IReadOnlyList<BrowserFile>> Files => ReadOnly<IReadOnlyList<BrowserFile>>();

    /// <summary>Raised when the person chooses files.</summary>
    public Event<FilesChanged> Change => Event<FilesChanged>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Files, committedBy: Change);
}

/// <summary>Payload of <see cref="FileInput.Change"/>: the DOM event, with nothing to read.</summary>
public sealed class FilesChanged : JsObject;

public static class FileInputHtml
{
    /// <summary>A file input of any number of files for the member the input field binds.</summary>
    public static RenderedComponent<FileInput, TModel> FileInput<TModel, TProp>(this InputBoundField<TModel, TProp> field)
        where TModel : class =>
        field.Render<FileInput>(new HtmlString(
            $"<input id=\"{HtmlEncoder.Default.Encode(field.ElementId)}\" name=\"{HtmlEncoder.Default.Encode(field.BindingPath)}\" type=\"file\" multiple>"));
}
