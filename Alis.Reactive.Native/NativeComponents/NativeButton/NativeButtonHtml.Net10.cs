#if NET10_0_OR_GREATER
using System;
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.NativeComponents;

/// <summary>
/// The door of a view to a button.
/// </summary>
public static class NativeButtonHtml
{
    /// <summary>Renders a button with a text, under the id the view gives it.</summary>
    /// <remarks>A button is no input: no field renders it and it binds no member.</remarks>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="id">The id the view gives the button's element.</param>
    /// <param name="text">The button's text.</param>
    /// <returns>The button's builder, which writes its markup where the view puts it.</returns>
    public static NativeButtonBuilder<TModel> NativeButton<TModel>(this IHtmlHelper<TModel> html, string id, string text)
        => throw new NotImplementedException();
}

public sealed partial class NativeButtonBuilder<TModel> : IHtmlContent
{
    /// <summary>Writes the button's markup.</summary>
    /// <param name="writer">The writer of the view.</param>
    /// <param name="encoder">The encoder of the view.</param>
    public void WriteTo(TextWriter writer, HtmlEncoder encoder) => throw new NotImplementedException();
}
#endif
