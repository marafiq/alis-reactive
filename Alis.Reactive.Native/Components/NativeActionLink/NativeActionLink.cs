using System;
using Alis.Reactive.Builders;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// A native <c>&lt;a&gt;</c> whose unmodified left click runs a request pipeline instead of
    /// navigating, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.NativeActionLink("Mark Read", url, p =&gt; p.Post(url)...)</c>. The anchor
    /// keeps the URL in <c>href</c>, so a modified click (a new tab, a download) stays the browser's,
    /// and it carries a plan of its own, so a link in a loaded partial or a grid row works without a
    /// plan on the page.
    /// </remarks>
    public sealed class NativeActionLink : Component
    {
        /// <summary>
        /// The anchor itself is the object, and a click handler receives the click's own DOM event,
        /// which the link reads for the button and modifier keys and stops from navigating.
        /// </summary>
        public static ComponentVendor LinkVendor { get; } = ComponentVendor.Define(
            "native-link",
            objectPath: "",
            subscribe: "addEventListener",
            unsubscribe: "removeEventListener",
            payload: VendorEventPayload.Argument);

        /// <inheritdoc />
        public override ComponentVendor Vendor => LinkVendor;

        /// <summary>Raised when the user clicks the link.</summary>
        public TypedEvent<NativeActionLinkClickArgs> Click => Event<NativeActionLinkClickArgs>("click");
    }

    /// <summary>The DOM click a <see cref="NativeActionLink"/> receives.</summary>
    public sealed class NativeActionLinkClickArgs : JsObject
    {
        /// <summary>The mouse button: 0 for the main one.</summary>
        public int Button { get; set; }

        /// <summary>Whether a handler earlier in the page already handled the click.</summary>
        public bool DefaultPrevented { get; set; }

        /// <summary>Whether the Meta (Command) key was down.</summary>
        public bool MetaKey { get; set; }

        /// <summary>Whether the Ctrl key was down.</summary>
        public bool CtrlKey { get; set; }

        /// <summary>Whether the Shift key was down.</summary>
        public bool ShiftKey { get; set; }

        /// <summary>Whether the Alt key was down.</summary>
        public bool AltKey { get; set; }

        /// <summary>Stops the browser from following the link.</summary>
        public ComponentMethod PreventDefault => Method("preventDefault");
    }

    /// <summary>
    /// HTML helper extensions for links that execute reactive request pipelines.
    /// </summary>
    public static class NativeActionLinkHtmlExtensions
    {
        /// <summary>
        /// Renders an anchor whose unmodified left click executes a reactive request pipeline.
        /// </summary>
        /// <remarks>
        /// The click pipeline holds one HTTP request to <paramref name="url"/>; requests inside its
        /// response routes, WhileLoading or Finally are kept as written. Analyzer ALIS002 refuses, in
        /// the view, a pipeline with no request or more than one, a request to another URL, parallel
        /// or chained requests, route parameters, IncludeAll and validation: the link's href must be
        /// a URL a modified click can open, and its plan sees no page inputs.
        /// <code>
        /// @Html.NativeActionLink("Delete", "/residents/42/delete", p =&gt; p.Post("/residents/42/delete"))
        /// </code>
        /// </remarks>
        /// <param name="linkText">Anchor text.</param>
        /// <param name="url">Anchor href, and the URL of the pipeline's request.</param>
        /// <param name="pipeline">Click pipeline with one HTTP request.</param>
        /// <returns>Anchor builder for CSS classes or custom attributes.</returns>
        public static NativeActionLinkBuilder<TModel> NativeActionLink<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            string linkText,
            string url,
            Action<PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));

            var elementId = NativeActionLinkIdGenerator.Next<TModel>(html.ViewContext);
            var plan = html.ElementPlan(elementId);
            var link = new NativeActionLinkBuilder<TModel>(html, plan, elementId, linkText, url);

            // A modified or non-main click, or one the page already handled, stays the browser's.
            plan.Render<NativeActionLink>(elementId, link).Reactive(evt => evt.Click, (click, p) =>
                p.When(click, x => x.Button).Eq(0)
                    .And(click, x => x.DefaultPrevented).Falsy()
                    .And(click, x => x.MetaKey).Falsy()
                    .And(click, x => x.CtrlKey).Falsy()
                    .And(click, x => x.ShiftKey).Falsy()
                    .And(click, x => x.AltKey).Falsy()
                    .Then(follow =>
                    {
                        follow.Payload(click).Call(x => x.PreventDefault);
                        pipeline(follow);
                    }));
            return link;
        }
    }
}
