using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Builders.Requests;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// What a <see cref="NativeActionLink"/>'s click does: one request to the link's own <c>href</c>,
    /// asked for first with <see cref="Confirm"/> when it needs the user's word.
    /// </summary>
    /// <remarks>
    /// The request takes the link's URL, so the URL is written once and a modified click (a new tab)
    /// opens the address a plain click requests. The type offers one request: calling a second throws
    /// when the view renders, and so does a link that sends none.
    /// </remarks>
    /// <typeparam name="TModel">The view model.</typeparam>
    public sealed class NativeActionLinkRequest<TModel> where TModel : class
    {
        private readonly PipelineBuilder<TModel> _pipeline;
        private readonly string _href;
        private readonly LinkRequestCount _sent;

        internal NativeActionLinkRequest(PipelineBuilder<TModel> pipeline, string href, LinkRequestCount sent)
        {
            _pipeline = pipeline;
            _href = href;
            _sent = sent;
        }

        /// <summary>Sends a GET to the link's <c>href</c>.</summary>
        public HttpRequestBuilder<TModel> Get() { _sent.One(_href); return _pipeline.Get(_href); }

        /// <summary>Sends a POST to the link's <c>href</c>.</summary>
        public HttpRequestBuilder<TModel> Post() { _sent.One(_href); return _pipeline.Post(_href); }

        /// <summary>Sends a PUT to the link's <c>href</c>.</summary>
        public HttpRequestBuilder<TModel> Put() { _sent.One(_href); return _pipeline.Put(_href); }

        /// <summary>Sends a DELETE to the link's <c>href</c>.</summary>
        public HttpRequestBuilder<TModel> Delete() { _sent.One(_href); return _pipeline.Delete(_href); }

        /// <summary>A component on the page, for a value the request sends (<c>Set(x =&gt; x.Ids, grid.Get(..))</c>).</summary>
        /// <typeparam name="TComponent">The component's declaration.</typeparam>
        /// <param name="refId">The component's id.</param>
        public ComponentRef<TComponent, TModel> Component<TComponent>(string refId)
            where TComponent : IComponent, new() =>
            _pipeline.Component<TComponent>(refId);

        /// <summary>Asks the user first; the request goes in <c>Then</c>, and <c>Else</c> runs when they decline.</summary>
        /// <param name="message">The question.</param>
        public NativeActionLinkConfirm<TModel> Confirm(string message) =>
            new NativeActionLinkConfirm<TModel>(_pipeline.Confirm(message), _href, _sent);
    }

    /// <summary>A link's question to the user, before its request.</summary>
    /// <typeparam name="TModel">The view model.</typeparam>
    public sealed class NativeActionLinkConfirm<TModel> where TModel : class
    {
        private readonly GuardBuilder<TModel> _question;
        private readonly string _href;
        private readonly LinkRequestCount _sent;

        internal NativeActionLinkConfirm(GuardBuilder<TModel> question, string href, LinkRequestCount sent)
        {
            _question = question;
            _href = href;
            _sent = sent;
        }

        /// <summary>Sends the link's request when the user confirms.</summary>
        /// <param name="confirmed">Sends the one request.</param>
        /// <returns>The branch, whose <c>Else</c> runs when the user declines.</returns>
        public BranchBuilder<TModel> Then(Action<NativeActionLinkRequest<TModel>> confirmed)
        {
            if (confirmed == null) throw new ArgumentNullException(nameof(confirmed));
            return _question.Then(yes => confirmed(new NativeActionLinkRequest<TModel>(yes, _href, _sent)));
        }
    }

    internal sealed class LinkRequestCount
    {
        private int _sent;

        internal void One(string href)
        {
            _sent++;
            if (_sent > 1)
                throw new InvalidOperationException(
                    $"A NativeActionLink sends one request, to its href '{href}'; this one sends a second.");
        }

        internal void RequireOne(string href)
        {
            if (_sent == 0)
                throw new InvalidOperationException(
                    $"The NativeActionLink to '{href}' sends no request; call Get(), Post(), Put() or Delete().");
        }
    }
}
