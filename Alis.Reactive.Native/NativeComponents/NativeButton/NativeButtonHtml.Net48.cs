#if NET48
using System;
using System.Web;
using System.Web.Mvc;

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
    public static NativeButtonBuilder<TModel> NativeButton<TModel>(this HtmlHelper<TModel> html, string id, string text)
        => throw new NotImplementedException();
}

public sealed partial class NativeButtonBuilder<TModel> : IHtmlString
{
    /// <summary>Returns the button's markup.</summary>
    /// <returns>The markup, encoded as HTML.</returns>
    public string ToHtmlString() => throw new NotImplementedException();
}
#endif
