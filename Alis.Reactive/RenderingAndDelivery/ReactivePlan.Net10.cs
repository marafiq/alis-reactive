#if NET10_0_OR_GREATER
using System;
using System.IO;
using System.Text.Encodings.Web;
using Alis.Reactive.Vendors;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.RenderingAndDelivery;

public sealed partial class ReactivePlan<TModel>
{
    /// <summary>Renders a component's element and declares the component on it.</summary>
    /// <remarks>An id the plan renders twice is refused where the view renders.</remarks>
    /// <param name="id">The id the view gives the component's element.</param>
    /// <param name="html">The element's markup.</param>
    /// <typeparam name="TComponent">The component's declaration.</typeparam>
    /// <returns>The rendered component, which writes its markup where the view puts it.</returns>
    public Rendered<TComponent, TModel> Render<TComponent>(string id, IHtmlContent html)
        where TComponent : Component, new()
        => throw new NotImplementedException();
}

public sealed partial class Rendered<TComponent, TModel> : IHtmlContent
{
    /// <summary>Writes the component's markup.</summary>
    /// <param name="writer">The writer of the view.</param>
    /// <param name="encoder">The encoder of the view.</param>
    public void WriteTo(TextWriter writer, HtmlEncoder encoder) => throw new NotImplementedException();
}
#endif
