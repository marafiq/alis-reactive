using System;
using Alis.Reactive.Builders;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Native.Components
{
#nullable disable annotations
    /// <summary>
    /// A native <c>&lt;a&gt;</c> whose unmodified left click runs a request pipeline instead of
    /// navigating, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.NativeActionLink(id, "Mark Read", url, link =&gt; link.Post()...)</c>. The anchor
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
        public ReadOnly<int> Button => ReadOnly<int>();

        /// <summary>Whether a handler earlier in the page already handled the click.</summary>
        public ReadOnly<bool> DefaultPrevented => ReadOnly<bool>();

        /// <summary>Whether the Meta (Command) key was down.</summary>
        public ReadOnly<bool> MetaKey => ReadOnly<bool>();

        /// <summary>Whether the Ctrl key was down.</summary>
        public ReadOnly<bool> CtrlKey => ReadOnly<bool>();

        /// <summary>Whether the Shift key was down.</summary>
        public ReadOnly<bool> ShiftKey => ReadOnly<bool>();

        /// <summary>Whether the Alt key was down.</summary>
        public ReadOnly<bool> AltKey => ReadOnly<bool>();

        /// <summary>Stops the browser from following the link.</summary>
        public void PreventDefault() => Call();
    }
#nullable restore annotations

    /// <summary>
    /// HTML helper extensions for links that execute reactive request pipelines.
    /// </summary>
    public static class NativeActionLinkHtmlExtensions
    {
        /// <summary>
        /// Renders an anchor whose unmodified left click sends one request to its own <c>href</c>.
        /// </summary>
        /// <remarks>
        /// The id is the developer's: a link in a loop takes its row's key. The link carries a plan of
        /// its own, so it sees no page inputs: its request does not validate a form or send
        /// <c>Form()</c> (analyzer ALIS002).
        /// <code>
        /// @Html.NativeActionLink($"mark-read-{note.Key}", "Mark Read", markReadUrl, link =&gt; link
        ///     .Post()
        ///     .Response(r =&gt; r.OnSuccess&lt;HandoverNoteRead&gt;((json, s) =&gt; ..)))
        /// </code>
        /// </remarks>
        /// <typeparam name="TModel">The view's model.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
        /// <param name="id">The anchor's id, unique on the page.</param>
        /// <param name="linkText">Anchor text.</param>
        /// <param name="url">The anchor's href, and the URL of its request.</param>
        /// <param name="request">Sends the link's one request, after <c>Confirm</c> when it asks first.</param>
        /// <returns>Anchor builder for CSS classes or custom attributes.</returns>
        public static NativeActionLinkBuilder<TModel> NativeActionLink<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            string id,
            string linkText,
            string url,
            Action<NativeActionLinkRequest<TModel>> request)
            where TModel : class
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A link needs an id; a link in a loop takes its row's key.", nameof(id));
            if (url == null) throw new ArgumentNullException(nameof(url));
            if (request == null) throw new ArgumentNullException(nameof(request));

            var plan = html.ElementPlan(id);
            var link = new NativeActionLinkBuilder<TModel>(html, plan, id, linkText, url);
            var sent = new LinkRequestCount();

            // A modified or non-main click, or one the page already handled, stays the browser's.
            plan.Render<NativeActionLink>(id, link).Reactive(evt => evt.Click, (click, p) =>
                p.When(click.Button).Eq(0)
                    .And(click.DefaultPrevented).Falsy()
                    .And(click.MetaKey).Falsy()
                    .And(click.CtrlKey).Falsy()
                    .And(click.ShiftKey).Falsy()
                    .And(click.AltKey).Falsy()
                    .Then(follow =>
                    {
                        click.PreventDefault();
                        request(new NativeActionLinkRequest<TModel>(follow, url, sent));
                    }));
            sent.RequireOne(url);
            return link;
        }
    }
}
