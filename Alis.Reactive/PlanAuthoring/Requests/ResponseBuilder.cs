using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    /// <summary>
    /// Configures the reactions that run for HTTP success routes, error routes,
    /// and successful follow-up requests.
    /// </summary>
    /// <remarks>
    /// Created by <c>request.Response(r =&gt; ...)</c>.
    /// </remarks>
    /// <typeparam name="TModel">The view model that owns model-bound component IDs.</typeparam>
    public class ResponseBuilder<TModel> where TModel : class
    {
        private readonly PlanBuildContext _context;
        private readonly ResponseRoutingDraft _draft = new ResponseRoutingDraft();

        internal ResponseBuilder(PlanBuildContext context)
        {
            _context = context;
        }

        internal ResponseRoutingDraft Draft => _draft;

        /// <summary>Adds a route for any successful HTTP response.</summary>
        /// <param name="pipeline">Builds the reaction graph to execute on success.</param>
        public ResponseBuilder<TModel> OnSuccess(Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            _draft.AddSuccessRoute(pb.BuildReaction());
            return this;
        }

        /// <summary>Adds a route for any successful HTTP response with typed response-body access.</summary>
        /// <typeparam name="TResponse">The response body type exposed to downstream value sources.</typeparam>
        /// <param name="pipeline">Builds the reaction graph using the success body scope.</param>
        public ResponseBuilder<TModel> OnSuccess<TResponse>(
            Action<ResponseBody<TResponse>, PipelineBuilder<TModel>> pipeline)
            where TResponse : class
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(
                new ResponseBody<TResponse>(PayloadSource.Success(PayloadContract.ForPayload(typeof(TResponse)))),
                pb);
            _draft.AddSuccessRoute(pb.BuildReaction());
            return this;
        }

        /// <summary>Adds a route for any HTTP error response.</summary>
        /// <param name="pipeline">Builds the reaction graph to execute on error.</param>
        public ResponseBuilder<TModel> OnError(Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            _draft.AddErrorRoute(pb.BuildReaction());
            return this;
        }

        /// <summary>Adds a route for an HTTP error response with a specific status code.</summary>
        /// <param name="statusCode">The HTTP status code to match.</param>
        /// <param name="pipeline">Builds the reaction graph to execute for this status code.</param>
        public ResponseBuilder<TModel> OnError(int statusCode, Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            _draft.AddErrorRoute(statusCode, pb.BuildReaction());
            return this;
        }

        /// <summary>Adds a route for any HTTP error response with typed error-body access.</summary>
        /// <typeparam name="TError">The error body type exposed to downstream value sources.</typeparam>
        /// <param name="pipeline">Builds the reaction graph using the error body scope.</param>
        public ResponseBuilder<TModel> OnError<TError>(
            Action<ResponseBody<TError>, PipelineBuilder<TModel>> pipeline)
            where TError : class
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(
                new ResponseBody<TError>(PayloadSource.Error(PayloadContract.ForPayload(typeof(TError)))),
                pb);
            _draft.AddErrorRoute(pb.BuildReaction());
            return this;
        }

        /// <summary>Adds a route for a specific HTTP error status code with typed error-body access.</summary>
        /// <typeparam name="TError">The error body type exposed to downstream value sources.</typeparam>
        /// <param name="statusCode">The HTTP status code to match.</param>
        /// <param name="pipeline">Builds the reaction graph using the error body scope.</param>
        public ResponseBuilder<TModel> OnError<TError>(int statusCode,
            Action<ResponseBody<TError>, PipelineBuilder<TModel>> pipeline)
            where TError : class
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(
                new ResponseBody<TError>(PayloadSource.Error(PayloadContract.ForPayload(typeof(TError)))),
                pb);
            _draft.AddErrorRoute(statusCode, pb.BuildReaction());
            return this;
        }

        /// <summary>Adds a follow-up HTTP request that runs after the current request succeeds.</summary>
        /// <param name="request">Builds the follow-up request.</param>
        public ResponseBuilder<TModel> Chained(Action<HttpRequestBuilder<TModel>> request)
        {
            var chainedBuilder = new HttpRequestBuilder<TModel>(_context);
            request(chainedBuilder);
            _draft.ContinueWith(chainedBuilder.BuildRequest());
            return this;
        }
    }
}
