using System;
using System.Collections.Generic;
using Alis.Reactive.Descriptors.Commands;
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
            SuccessHandlers.Add(new StatusHandler(null, builder.Commands));
            return this;
        }

        /// <summary>
        /// Registers an error handler for a specific HTTP status code.
        /// </summary>
        public ResponseBuilder<TModel> OnError(int statusCode, Action<PipelineBuilder<TModel>> configure)
        {
            var builder = new PipelineBuilder<TModel>();
            configure(builder);
            ErrorHandlers.Add(new StatusHandler(statusCode, builder.Commands));
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
    }
}
