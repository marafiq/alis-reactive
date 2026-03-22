using System;
using Alis.Reactive.Builders;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        public static void On<TModel>(this IHtmlHelper<TModel> html, IReactivePlan<TModel> plan,
            Action<TriggerBuilder<TModel>> triggerBuilder) where TModel : class
        {
            var trigger = new TriggerBuilder<TModel>(plan);
            triggerBuilder(trigger);
        }
    }
}