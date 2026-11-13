#if NET10_0_OR_GREATER
using System;
using System.Collections.Generic;
using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionMentionHtml
{
    /// <summary>Renders a mention under the id the view gives it, with what the view sets on EJ2's builder.</summary>
    /// <remarks>Name every type argument: the model and the type of the items' value member.</remarks>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="elementId">The id the view gives the mention.</param>
    /// <param name="build">What the view sets on the mention, the element it suggests in among them.</param>
    /// <returns>The rendered mention, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionMention<TValue>, TModel> FusionMention<TModel, TValue>(this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, Action<MentionBuilder> build)
        => throw new NotImplementedException();

    /// <summary>Renders a mention under the id the view gives it over the items the view hands it.</summary>
    /// <remarks>
    /// The items reach the mention named as the app's JSON names them, the names
    /// <see cref="Fields{TItem, TText, TItemValue}"/> reads. Name every type argument: the model, the type of the items'
    /// value member and the items' class.
    /// </remarks>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="elementId">The id the view gives the mention.</param>
    /// <param name="items">The items the mention suggests.</param>
    /// <param name="build">What the view sets on the mention, the element it suggests in among them.</param>
    /// <returns>The rendered mention, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionMention<TValue>, TModel> FusionMention<TModel, TValue, TItem>(this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, IReadOnlyList<TItem> items, Action<MentionBuilder> build)
        => throw new NotImplementedException();
}
#endif
