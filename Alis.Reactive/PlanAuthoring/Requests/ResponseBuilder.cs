using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    /// <summary>
    /// Configures success routes, error routes, and the optional follow-up
    /// request for one HTTP request.
    /// </summary>
    /// <remarks>
    /// Created by <c>request.Response(r =&gt; ...)</c>.
    /// Exact error-status routes are matched before any-status error routes.
    /// </remarks>
    /// <typeparam name="TModel">View model that owns model-bound component IDs.</typeparam>
    public class ResponseBuilder<TModel> where TModel : class
    {
        private readonly PlanBuildContext _context;

        internal ResponseBuilder(PlanBuildContext context)
        {
            _context = context;
        }

        /// <summary>The routes this builder configures, written where the request is.</summary>
        internal ResponseRoutes Routes { get; } = new ResponseRoutes();

        /// <summary>Adds route for any successful 2xx HTTP response.</summary>
        /// <param name="pipeline">Builds the reaction graph to execute on success.</param>
        public ResponseBuilder<TModel> OnSuccess(Action<PipelineBuilder<TModel>> pipeline)
        {
            Routes.OnSuccess(PipelineBuilder<TModel>.Build(_context, pipeline));
            return this;
        }

        /// <summary>
        /// Adds route for any successful 2xx HTTP response with typed
        /// response-body access.
        /// </summary>
        /// <typeparam name="TResponse">Response body type exposed to downstream value sources.</typeparam>
        /// <param name="pipeline">
        /// Builds the reactions, given the response body and the pipeline: <c>json.Get(x =&gt; x.Name)</c> reads a
        /// member, and <c>json</c> itself is the whole body.
        /// </param>
        public ResponseBuilder<TModel> OnSuccess<TResponse>(
            Action<Data<TResponse>, PipelineBuilder<TModel>> pipeline)
            where TResponse : class
        {
            var body = new Data<TResponse>(Receiver.Response(Routes.Handle));
            Routes.OnSuccess(PipelineBuilder<TModel>.Build(_context, p => pipeline(body, p)));
            return this;
        }

        /// <summary>Adds route for any non-2xx response or response-unavailable failure.</summary>
        /// <param name="pipeline">Builds the reaction graph to execute for the error route.</param>
        public ResponseBuilder<TModel> OnError(Action<PipelineBuilder<TModel>> pipeline)
        {
            Routes.OnError(PipelineBuilder<TModel>.Build(_context, pipeline));
            return this;
        }

        /// <summary>Adds route for a non-2xx response with a specific status code.</summary>
        /// <param name="statusCode">HTTP status code to match.</param>
        /// <param name="pipeline">Builds the reaction graph to execute for the matching status code.</param>
        public ResponseBuilder<TModel> OnError(int statusCode, Action<PipelineBuilder<TModel>> pipeline)
        {
            Routes.OnError(statusCode, PipelineBuilder<TModel>.Build(_context, pipeline));
            return this;
        }

        /// <summary>
        /// Adds route for any error outcome with typed error-body access when
        /// a response body is available.
        /// </summary>
        /// <typeparam name="TError">Error body type exposed to downstream value sources.</typeparam>
        /// <param name="pipeline">
        /// Builds the reactions, given the error body and the pipeline: <c>error.Get(x =&gt; x.Message)</c> reads a
        /// member, and <c>error</c> itself is the whole body.
        /// </param>
        public ResponseBuilder<TModel> OnError<TError>(
            Action<Data<TError>, PipelineBuilder<TModel>> pipeline)
            where TError : class
        {
            var body = new Data<TError>(Receiver.Response(Routes.Handle));
            Routes.OnError(PipelineBuilder<TModel>.Build(_context, p => pipeline(body, p)));
            return this;
        }

        /// <summary>
        /// Adds route for a non-2xx response with a specific status code and
        /// typed error-body access.
        /// </summary>
        /// <typeparam name="TError">Error body type exposed to downstream value sources.</typeparam>
        /// <param name="statusCode">HTTP status code to match.</param>
        /// <param name="pipeline">
        /// Builds the reactions, given the error body and the pipeline: <c>error.Get(x =&gt; x.Message)</c> reads a
        /// member, and <c>error</c> itself is the whole body.
        /// </param>
        public ResponseBuilder<TModel> OnError<TError>(int statusCode,
            Action<Data<TError>, PipelineBuilder<TModel>> pipeline)
            where TError : class
        {
            var body = new Data<TError>(Receiver.Response(Routes.Handle));
            Routes.OnError(statusCode, PipelineBuilder<TModel>.Build(_context, p => pipeline(body, p)));
            return this;
        }

        /// <summary>
        /// Adds one follow-up HTTP request that runs only after the current
        /// request succeeds.
        /// </summary>
        /// <param name="request">Builds the follow-up request.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this response already has a follow-up request.
        /// </exception>
        public ResponseBuilder<TModel> Chained(Func<RequestStart<TModel>, HttpRequestBuilder<TModel>> request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            request(new RequestStart<TModel>(_context, Routes.ContinueWith));
            return this;
        }

        /// <summary>
        /// Adds one follow-up HTTP request that runs only after the current request succeeds, given the
        /// body it succeeded with, so the follow-up can send what the first answer said.
        /// </summary>
        /// <typeparam name="TResponse">The current request's success body type.</typeparam>
        /// <param name="request">
        /// Builds the follow-up request, given the success body and the request:
        /// <c>(week, holds) =&gt; holds.Get(url).Gather&lt;HoldsQuery&gt;(g =&gt; g.Set(x =&gt; x.Wing, week.Get(x =&gt; x.Wing)))</c>.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this response already has a follow-up request.
        /// </exception>
        public ResponseBuilder<TModel> Chained<TResponse>(Func<Data<TResponse>, RequestStart<TModel>, HttpRequestBuilder<TModel>> request)
            where TResponse : class
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var body = new Data<TResponse>(Receiver.Response(Routes.Handle));
            return Chained(start => request(body, start));
        }
    }
}
