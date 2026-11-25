#if NET48
using System.Web.Mvc;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.RenderingAndDelivery;

/// <summary>
/// The doors of a view to the request containers it renders.
/// </summary>
public static class RequestBlocks
{
    /// <summary>Opens a fieldset as a request container of the view: a block, ended by <c>using</c>, that holds the container's markup.</summary>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="containerId">The id of the container's element.</param>
    /// <returns>The block, whose <c>Container</c> a request sends.</returns>
    public static RequestContainerBlock<TModel> BeginRequestContainer<TModel>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, string containerId)
        => new(html.ViewContext.Writer, plan, containerId,
            () => FieldPrefixes.RefuseAMoveInsideThePlan(plan.StartPrefix, html.ViewData.TemplateInfo.HtmlFieldPrefix, () => ViewPaths.Of(html)));
}
#endif
