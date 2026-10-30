using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Razor.Extensions;
#if NET48
using System.Web;
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive
{
    /// <summary>
    /// Razor view extensions for adding behavior to a Reactive Plan, and for markup of the view that raises a
    /// declared event when it is clicked.
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
        ///         p.Element("status").Text.Set("Ready");
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
        /// <param name="html">The view's HTML helper.</param>
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

        /// <summary>
        /// The attributes that make an element of the view raise a declared event with no payload when it is
        /// clicked, or when an element inside it is. Write them in the element's start tag:
        /// <c>&lt;button type="button" @Html.Raises(GateEvents.Opened)&gt;Open the gate&lt;/button&gt;</c>.
        /// </summary>
        /// <remarks>
        /// A click inside several such elements raises the event of the nearest. The listener is any plan's
        /// <c>t.CustomEvent(appEvent, ...)</c>; the element needs no plan of its own (grammar.md 6.1, D4).
        /// </remarks>
        /// <param name="html">The view's HTML helper.</param>
        /// <param name="appEvent">The event, declared once with <see cref="AppEvent.Declare(string, string)"/>.</param>
        /// <returns>The attributes, for the element's start tag.</returns>
#if NET48
        public static IHtmlString Raises(this HtmlHelper html, AppEvent appEvent) =>
#else
        public static IHtmlContent Raises(this IHtmlHelper html, AppEvent appEvent) =>
#endif
            Attributes(MarkupRaise.Of((appEvent ?? throw new ArgumentNullException(nameof(appEvent))).Name));

        /// <summary>
        /// The attributes that make an element of the view raise a declared event without its payload when it is
        /// clicked, or when an element inside it is: the listener reads every member as missing.
        /// </summary>
        /// <typeparam name="TPayload">The payload the event declares.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
        /// <param name="appEvent">The event, declared once with <see cref="AppEvent.Declare{TPayload}(string, string)"/>.</param>
        /// <returns>The attributes, for the element's start tag.</returns>
#if NET48
        public static IHtmlString Raises<TPayload>(this HtmlHelper html, AppEvent<TPayload> appEvent)
#else
        public static IHtmlContent Raises<TPayload>(this IHtmlHelper html, AppEvent<TPayload> appEvent)
#endif
            where TPayload : class, new() =>
            Attributes(MarkupRaise.Of((appEvent ?? throw new ArgumentNullException(nameof(appEvent))).Name));

        /// <summary>
        /// The attributes that make an element of the view raise a declared event with a payload fixed when the
        /// view renders, when it is clicked or when an element inside it is. Write them in the element's start tag:
        /// <c>&lt;li @Html.Raises(GateEvents.Announced, new Announcement { Type = "a visitor" })&gt;</c>.
        /// </summary>
        /// <remarks>
        /// Each member of the payload is one attribute holding its value as text, which the listener reads as the
        /// member's type, as it reads every value: text, a number, a bool, a date, a Guid or an enum. A member left
        /// null is read as missing. A member that is a record or a list is refused when the view renders; a payload
        /// that holds one is raised from a pipeline, with <c>p.Dispatch</c> (grammar.md 6.1, D4).
        /// </remarks>
        /// <typeparam name="TPayload">The payload the event declares.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
        /// <param name="appEvent">The event, declared once with <see cref="AppEvent.Declare{TPayload}(string, string)"/>.</param>
        /// <param name="payload">The payload, written into the attributes.</param>
        /// <returns>The attributes, for the element's start tag.</returns>
#if NET48
        public static IHtmlString Raises<TPayload>(this HtmlHelper html, AppEvent<TPayload> appEvent, TPayload payload)
#else
        public static IHtmlContent Raises<TPayload>(this IHtmlHelper html, AppEvent<TPayload> appEvent, TPayload payload)
#endif
            where TPayload : class, new() =>
            Attributes(MarkupRaise.Of(
                (appEvent ?? throw new ArgumentNullException(nameof(appEvent))).Name,
                payload ?? throw new ArgumentNullException(nameof(payload))));

#if NET48
        private static IHtmlString Attributes(string markup) => new MvcHtmlString(markup);
#else
        private static IHtmlContent Attributes(string markup) => new HtmlString(markup);
#endif
    }
}
