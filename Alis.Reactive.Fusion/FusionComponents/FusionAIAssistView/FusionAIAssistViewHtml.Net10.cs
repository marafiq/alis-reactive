#if NET10_0_OR_GREATER
using System;
using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2.InteractiveChat;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The door of a view to an AI assist view.
/// </summary>
public static class FusionAIAssistViewHtml
{
    /// <summary>Renders an AI assist view under the id the view gives it, with what the view sets on EJ2's builder.</summary>
    /// <remarks>The view shows each response whole: it streams none.</remarks>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="elementId">The id the view gives the assist view's element.</param>
    /// <param name="build">What the view sets on the assist view.</param>
    /// <returns>The rendered assist view, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionAIAssistView, TModel> FusionAIAssistView<TModel>(this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, Action<AIAssistViewBuilder> build)
        => throw new NotImplementedException();
}
#endif
