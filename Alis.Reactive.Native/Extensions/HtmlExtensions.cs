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
        /// <summary>
        /// Entry point for wiring reactive triggers to a plan.
        /// <para>
        /// Supports: <c>DomReady</c>, <c>CustomEvent</c>, <c>ServerPush</c> (SSE),
        /// and <c>SignalR</c> (hub method invocation). Each trigger pairs with a pipeline
        /// that defines what happens when the trigger fires.
        /// </para>
        /// <para>
        /// Can be called anywhere in the <c>.cshtml</c> view — order and placement do not matter.
        /// Calls build descriptors only; no connections are opened until the browser boots the plan.
        /// </para>
        /// </summary>
        /// <param name="html">The Razor HTML helper (implicit via extension method).</param>
        /// <param name="plan">The reactive plan that collects trigger–reaction entries.</param>
        /// <param name="triggerBuilder">
        /// Lambda that configures one or more triggers via the fluent <see cref="TriggerBuilder{TModel}"/> API.
        /// Triggers can be chained: <c>t.DomReady(...).CustomEvent(...).SignalR(...)</c>.
        /// </param>
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