using System;
using Alis.Reactive.Builders;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Native.Extensions
{
    /// <summary>
    /// Razor view extensions for adding behavior to a Reactive Plan.
    /// </summary>
    public static class HtmlExtensions
    {
        /// <summary>
        /// Adds behavior to <paramref name="plan"/> by configuring triggers
        /// and the commands that run when each trigger fires.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Minimal reactive view:
        /// </para>
        /// <code>
        /// @{
        ///     var plan = Html.ReactivePlan&lt;MyModel&gt;();
        ///
        ///     Html.On(plan, trigger: t =&gt; t.DomReady(pipeline: p =&gt;
        ///     {
        ///         p.Element("status").SetText("Ready");
        ///     }));
        /// }
        /// @Html.RenderPlan(plan)
        /// </code>
        /// <para>
        /// A trigger is an event source that starts a reaction: the page loading (<c>DomReady</c>),
        /// a DOM custom event (<c>CustomEvent</c>), a server-sent event (<c>ServerPush</c>), or a
        /// SignalR message (<c>SignalR</c>). When the trigger fires, the commands declared in
        /// its callback execute in declaration order.
        /// </para>
        /// <para>
        /// Avoid defining the same event twice in the same view. Duplicate listeners are
        /// rarely needed and usually indicate the reaction should be combined into one block.
        /// </para>
        /// </remarks>
        /// <typeparam name="TModel">The view model used to author typed expression paths.</typeparam>
        /// <param name="html">The Razor HTML helper.</param>
        /// <param name="plan">The Reactive Plan that receives the trigger declarations.</param>
        /// <param name="trigger">
        /// Configures one or more triggers via the fluent <see cref="TriggerBuilder{TModel}"/> API.
        /// Triggers can be chained: <c>t.DomReady(...).CustomEvent(...).SignalR(...).ServerPush(...)</c>.
        /// </param>
        public static void On<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            Action<TriggerBuilder<TModel>> trigger) where TModel : class
        {
            var builder = new TriggerBuilder<TModel>(plan, plan.Context);
            trigger(builder);
        }
    }
}
