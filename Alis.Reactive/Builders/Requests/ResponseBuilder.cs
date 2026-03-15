using System;
using System.Collections.Generic;
using Alis.Reactive.Descriptors.Reactions;
using Alis.Reactive.Descriptors.Requests;

namespace Alis.Reactive.Builders.Requests
{
    public class ResponseBuilder<TModel> where TModel : class
    {
        internal List<StatusHandler> SuccessHandlers { get; } = new List<StatusHandler>();
        internal List<StatusHandler> ErrorHandlers { get; } = new List<StatusHandler>();
        internal RequestDescriptor? ChainedRequest { get; private set; }

        /// <summary>
        /// Registers a success handler (status 2xx, no specific code filter).
        /// </summary>
        public ResponseBuilder<TModel> OnSuccess(Action<PipelineBuilder<TModel>> configure)
        {
            var builder = new PipelineBuilder<TModel>();
            configure(builder);
            SuccessHandlers.Add(BuildHandler(null, builder));
            return this;
        }

        /// <summary>
        /// Registers a typed JSON success handler. The ResponseBody&lt;T&gt; phantom enables
        /// compile-time path walking into the response body — same pattern as
        /// CustomEvent&lt;T&gt; provides typed access to event payloads.
        ///
        /// Usage: .OnSuccess&lt;ApiResponse&gt;((json, s) =&gt; s.Element("x").SetText(json, r =&gt; r.Data.Name))
        /// Generates: source = "responseBody.data.name" — resolved at runtime via walk(ctx, path).
        /// </summary>
        public ResponseBuilder<TModel> OnSuccess<TResponse>(
            Action<ResponseBody<TResponse>, PipelineBuilder<TModel>> configure)
            where TResponse : class, new()
        {
            var builder = new PipelineBuilder<TModel>();
            configure(new ResponseBody<TResponse>(new TResponse()), builder);
            SuccessHandlers.Add(BuildHandler(null, builder));
            return this;
        }

        /// <summary>
        /// Registers an error handler for a specific HTTP status code.
        /// </summary>
        public ResponseBuilder<TModel> OnError(int statusCode, Action<PipelineBuilder<TModel>> configure)
        {
            var builder = new PipelineBuilder<TModel>();
            configure(builder);
            ErrorHandlers.Add(BuildHandler(statusCode, builder));
            return this;
        }

        /// <summary>
        /// Chains a sequential HTTP request that fires after the current request succeeds.
        /// </summary>
        public ResponseBuilder<TModel> Chained(Action<HttpRequestBuilder<TModel>> configure)
        {
            var chainedBuilder = new HttpRequestBuilder<TModel>();
            configure(chainedBuilder);
            ChainedRequest = chainedBuilder.BuildRequestDescriptor();
            return this;
        }

        /// <summary>
        /// Builds a StatusHandler from a PipelineBuilder. Sequential reactions use
        /// commands (backward compatible). Non-sequential (conditional, http) use
        /// the full reaction — conditions inside response handlers are preserved.
        /// </summary>
        private static StatusHandler BuildHandler(int? statusCode, PipelineBuilder<TModel> builder)
        {
            var reaction = builder.BuildReaction();
            if (reaction is SequentialReaction sr)
            {
                return statusCode.HasValue
                    ? new StatusHandler(statusCode.Value, sr.Commands)
                    : new StatusHandler(sr.Commands);
            }
            return statusCode.HasValue
                ? new StatusHandler(statusCode.Value, reaction)
                : new StatusHandler(reaction);
        }
    }
}

