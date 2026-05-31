using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Buttons;

namespace Alis.Reactive.Fusion.Components
{
    public static class FusionChipListHtmlExtensions
    {
        public static FusionChipListBuilder<TModel> FusionChipList<TModel>(
            this IHtmlHelper<TModel> html,
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
