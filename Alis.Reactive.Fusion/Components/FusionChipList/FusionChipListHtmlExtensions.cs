using System;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using Syncfusion.EJ2;
using Syncfusion.EJ2.Buttons;

namespace Alis.Reactive.Fusion.Components
{
    public static class FusionChipListHtmlExtensions
    {
        public static FusionChipListBuilder<TModel> FusionChipList<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<ChipListBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().ChipList(elementId);
            build(builder);

            return new FusionChipListBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
