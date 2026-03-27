using System;
using Alis.Reactive.Builders;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Extensions
{
    /// <summary>
    /// Razor view extensions for adding reactive behavior to a plan.
    /// </summary>
    public static class HtmlExtensions
    {
        /// <summary>
        /// Adds reactive behavior to <paramref name="plan"/> by configuring triggers and
        /// what happens when the producer dispatches an event in the browser.
        /// </summary>
        /// <remarks>
        /// Triggers execute the intent expressed in the body of <c>DomReady</c>,
        /// <c>CustomEvent</c>, <c>ServerPush</c> (SSE), and <c>SignalR</c> in the browser, in declaration order,
        /// when the producer dispatches the particular event via the <see cref="TriggerBuilder{TModel}"/> API.
        /// Avoid defining the same event twice in the same view — duplicate listeners are an antipattern
        /// unless there is a legitimate reason to split the reaction across multiple blocks.
        /// </remarks>
        /// <typeparam name="TModel">The view model type</typeparam>
        /// <param name="html">The Razor HTML helper.</param>
        /// <param name="plan">The plan to add reactive behavior to.</param>
        /// <param name="triggerBuilder">
        /// Configures one or more triggers via the fluent <see cref="TriggerBuilder{TModel}"/> API.
        /// Triggers can be chained: <c>t.DomReady(...).CustomEvent(...).SignalR(...).ServerPush(...)</c>.
        /// </param>
        public static void On<TModel>(this IHtmlHelper<TModel> html, IReactivePlan<TModel> plan,
            Action<TriggerBuilder<TModel>> triggerBuilder) where TModel : class
        {
            var trigger = new TriggerBuilder<TModel>(plan);
            triggerBuilder(trigger);
        }
    }
}