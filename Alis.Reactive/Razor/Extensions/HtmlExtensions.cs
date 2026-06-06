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
        /// and the reactions that run when each trigger fires.
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
        /// Trigger is an event source that starts a reaction, such as page loading
        /// (<c>DomReady</c>) or a DOM custom event (<c>CustomEvent</c>). See
        /// <see cref="TriggerBuilder{TModel}"/> for the full trigger surface. When the
        /// trigger fires, the reactions declared in its callback execute in declaration order.
        /// </para>
        /// <para>
        /// Each call appends behavior declarations to the same plan. Render the plan once,
        /// after all triggers for the view have been declared.
        /// </para>
        /// </remarks>
        /// <typeparam name="TModel">View model used to author typed expression paths.</typeparam>
        /// <param name="plan">Reactive Plan that receives the trigger declarations.</param>
        /// <param name="trigger">
        /// Configures one or more triggers via the fluent <see cref="TriggerBuilder{TModel}"/> API.
        /// Triggers can be chained, for example <c>t.DomReady(...).CustomEvent(...)</c>.
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
