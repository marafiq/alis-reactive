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
        /// Adds reactive behavior to <paramref name="plan"/> by configuring browser triggers
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
        /// A trigger is a browser event that starts a reaction: the page loading (<c>DomReady</c>),
        /// a custom event (<c>CustomEvent</c>), a server-sent event (<c>ServerPush</c>), or a
        /// SignalR message (<c>SignalR</c>). When the trigger fires, the commands declared in
        /// its callback execute in declaration order.
        /// </para>
        /// <para>
        /// Avoid defining the same event twice in the same view. Duplicate listeners are
        /// rarely needed and usually indicate the reaction should be combined into one block.
        /// </para>
        /// </remarks>
        /// <typeparam name="TModel">The view model type</typeparam>
        /// <param name="html">The Razor HTML helper.</param>
        /// <param name="plan">The plan to add reactive behavior to.</param>
        /// <param name="trigger">
        /// Configures one or more triggers via the fluent <see cref="TriggerBuilder{TModel}"/> API.
        /// Triggers can be chained: <c>t.DomReady(...).CustomEvent(...).SignalR(...).ServerPush(...)</c>.
        /// </param>
        public static void On<TModel>(this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan,
            Action<TriggerBuilder<TModel>> trigger) where TModel : class
        {
            var builder = new TriggerBuilder<TModel>(plan);
            trigger(builder);
        }
    }
}