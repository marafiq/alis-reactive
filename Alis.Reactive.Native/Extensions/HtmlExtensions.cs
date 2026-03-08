using System;
using Alis.Reactive.Builders;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Extensions
{
    public static class HtmlExtensions
    {
        public static void On<TModel>(this IHtmlHelper<TModel> html, IReactivePlan<TModel> plan,
            Action<TriggerBuilder<TModel>> triggerBuilder) where TModel : class
        {
            var trigger = new TriggerBuilder<TModel>(plan);
            triggerBuilder(trigger);
        }
    }
}