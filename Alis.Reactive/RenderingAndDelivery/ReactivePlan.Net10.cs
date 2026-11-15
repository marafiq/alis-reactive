#if NET10_0_OR_GREATER
using System.IO;
using System.Text.Encodings.Web;
using Alis.Reactive.Declarations;
using Alis.Reactive.Execution;
using Alis.Reactive.Schema;
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
        => new(id, Writer, html);
}

public sealed partial class Rendered<TComponent, TModel> : IHtmlContent
{
    private readonly IHtmlContent _html;

    internal Rendered(string id, BlockWriter writer, IHtmlContent html)
    {
        _writer = writer;
        _html = html;
        _component = JsObject.Reaching<TComponent>(new Reached(new ObjExpression(id), writer));
        var vendor = _component.Vendor;
        writer.Content.Uses(vendor.Name, vendor.Grammar);
        writer.Content.Declares(id, new ElementLocator(id, vendor.Name), rendered: true);
    }

    /// <summary>Writes the component's markup.</summary>
    /// <param name="writer">The writer of the view.</param>
    /// <param name="encoder">The encoder of the view.</param>
    public void WriteTo(TextWriter writer, HtmlEncoder encoder) => _html.WriteTo(writer, encoder);
}
#endif
