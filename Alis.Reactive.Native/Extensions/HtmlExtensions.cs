using System;
using Alis.Reactive.Builders;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Native.Extensions
{
    public static class HtmlExtensions
    {
#if NET48
        public static void On<TModel>(this HtmlHelper<TModel> html, IReactivePlan<TModel> plan,
            Action<TriggerBuilder<TModel>> triggerBuilder) where TModel : class
#else
        public static void On<TModel>(this IHtmlHelper<TModel> html, IReactivePlan<TModel> plan,
            Action<TriggerBuilder<TModel>> triggerBuilder) where TModel : class
#endif
        {
            var trigger = new TriggerBuilder<TModel>(plan);
            triggerBuilder(trigger);
        }
    }
}