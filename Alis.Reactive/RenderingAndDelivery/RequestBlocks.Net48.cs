#if NET48
using System;
using System.Web.Mvc;

namespace Alis.Reactive.RenderingAndDelivery;

/// <summary>
/// The doors of a view to the request containers it renders.
/// </summary>
public static class RequestBlocks
{
    /// <summary>Opens a native form as a request container of the view: a block, ended by <c>using</c>, that holds the form's markup.</summary>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="containerId">The id of the form's element.</param>
    /// <returns>The block, whose <c>Form</c> a request sends.</returns>
    public static RequestFormBlock<TModel> BeginRequestForm<TModel>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, string containerId)
        => throw new NotImplementedException();

    /// <summary>Opens an element as a request container of the view: a block, ended by <c>using</c>, that holds the container's markup.</summary>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="containerId">The id of the container's element.</param>
    /// <returns>The block, whose <c>Container</c> a request sends.</returns>
    public static RequestContainerBlock<TModel> BeginRequestContainer<TModel>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, string containerId)
        => throw new NotImplementedException();
}
#endif
