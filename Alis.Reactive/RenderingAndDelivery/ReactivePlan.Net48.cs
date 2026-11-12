#if NET48
using System;
using System.Web;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.RenderingAndDelivery;

public sealed partial class ReactivePlan<TModel>
{
    /// <summary>Renders a component's element and declares the component on it.</summary>
    /// <remarks>An id the plan renders twice is refused where the view renders.</remarks>
    /// <param name="id">The id the view gives the component's element.</param>
    /// <param name="html">The element's markup.</param>
    /// <typeparam name="TComponent">The component's declaration.</typeparam>
    /// <returns>The rendered component, which writes its markup where the view puts it.</returns>
    public Rendered<TComponent, TModel> Render<TComponent>(string id, IHtmlString html)
        where TComponent : Component, new()
        => throw new NotImplementedException();
}

public sealed partial class Rendered<TComponent, TModel> : IHtmlString
{
    /// <summary>Returns the component's markup.</summary>
    /// <returns>The markup, encoded as HTML.</returns>
    public string ToHtmlString() => throw new NotImplementedException();
}
#endif
