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
        /// <param name="pipeline">Builds the reaction commands that run on success.</param>
        public ResponseBuilder<TModel> OnSuccess(Action<PipelineBuilder<TModel>> pipeline)
        {
            var builder = new PipelineBuilder<TModel>();
            pipeline(builder);
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
            Action<ResponseBody<TResponse>, PipelineBuilder<TModel>> pipeline)
            where TResponse : class, new()
        {
            var builder = new PipelineBuilder<TModel>();
            pipeline(new ResponseBody<TResponse>(new TResponse()), builder);
            SuccessHandlers.Add(BuildHandler(null, builder));
            return this;
        }

        /// <summary>
        /// Registers an error handler for a specific HTTP status code.
        /// </summary>
        /// <param name="statusCode">The HTTP status code to handle.</param>
        /// <param name="pipeline">Builds the reaction commands that run on this error status.</param>
        public ResponseBuilder<TModel> OnError(int statusCode, Action<PipelineBuilder<TModel>> pipeline)
        {
            var builder = new PipelineBuilder<TModel>();
            pipeline(builder);
            ErrorHandlers.Add(BuildHandler(statusCode, builder));
            return this;
        }

        /// <summary>
        /// Chains a sequential HTTP request that fires after the current request succeeds.
        /// </summary>
        /// <param name="request">Configures the chained HTTP request.</param>
        public ResponseBuilder<TModel> Chained(Action<HttpRequestBuilder<TModel>> request)
        {
            var chainedBuilder = new HttpRequestBuilder<TModel>();
            request(chainedBuilder);
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

