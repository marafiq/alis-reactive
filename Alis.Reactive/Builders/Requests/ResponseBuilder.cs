using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    /// <summary>
    /// Builds response handlers for HTTP success, error, and chained requests.
    /// </summary>
    /// <remarks>
    /// Obtained via <c>.Response(r =&gt; r.OnSuccess(...).OnError(...))</c>.
    /// </remarks>
    /// <typeparam name="TModel">The view model type.</typeparam>
    public class ResponseBuilder<TModel> where TModel : class
    {
        private readonly PlanBuildContext _context;
        private readonly ResponseDraft _draft = new ResponseDraft();

        internal ResponseBuilder(PlanBuildContext context)
        {
            _context = context;
        }

        internal ResponseDraft Draft => _draft;

        /// <summary>Handles a successful HTTP response.</summary>
        /// <param name="pipeline">Builds the commands to execute on success.</param>
        /// <returns>This builder for chaining.</returns>
        public ResponseBuilder<TModel> OnSuccess(Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            _draft.HandleSuccess(pb.BuildReaction());
            return this;
        }

        /// <summary>Handles a successful HTTP response with a typed response body.</summary>
        /// <typeparam name="TResponse">The response body type.</typeparam>
        /// <param name="pipeline">Builds the commands. The response body provides typed access to response properties.</param>
        /// <returns>This builder for chaining.</returns>
        public ResponseBuilder<TModel> OnSuccess<TResponse>(
            Action<ResponseBody<TResponse>, PipelineBuilder<TModel>> pipeline)
            where TResponse : class
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(
                new ResponseBody<TResponse>(PayloadSource.Success(PayloadContract.ForPayload(typeof(TResponse)))),
                pb);
            _draft.HandleSuccess(pb.BuildReaction());
            return this;
        }

        /// <summary>Handles any HTTP error response.</summary>
        /// <param name="pipeline">Builds the commands to execute on error.</param>
        /// <returns>This builder for chaining.</returns>
        public ResponseBuilder<TModel> OnError(Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            _draft.HandleError(pb.BuildReaction());
            return this;
        }

        /// <summary>Handles an HTTP error response with a specific status code.</summary>
        /// <param name="statusCode">The HTTP status code to match.</param>
        /// <param name="pipeline">Builds the commands to execute for this status code.</param>
        /// <returns>This builder for chaining.</returns>
        public ResponseBuilder<TModel> OnError(int statusCode, Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            _draft.HandleError(statusCode, pb.BuildReaction());
            return this;
        }

        /// <summary>Handles any HTTP error response with a typed error body.</summary>
        /// <typeparam name="TError">The error response body type.</typeparam>
        /// <param name="pipeline">Builds the commands. The error body provides typed access to error properties.</param>
        /// <returns>This builder for chaining.</returns>
        public ResponseBuilder<TModel> OnError<TError>(
            Action<ResponseBody<TError>, PipelineBuilder<TModel>> pipeline)
            where TError : class
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(
                new ResponseBody<TError>(PayloadSource.Error(PayloadContract.ForPayload(typeof(TError)))),
                pb);
            _draft.HandleError(pb.BuildReaction());
            return this;
        }

        /// <summary>Handles a specific HTTP error status code with a typed error body.</summary>
        /// <typeparam name="TError">The error response body type.</typeparam>
        /// <param name="statusCode">The HTTP status code to match.</param>
        /// <param name="pipeline">Builds the commands. The error body provides typed access to error properties.</param>
        /// <returns>This builder for chaining.</returns>
        public ResponseBuilder<TModel> OnError<TError>(int statusCode,
            Action<ResponseBody<TError>, PipelineBuilder<TModel>> pipeline)
            where TError : class
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(
                new ResponseBody<TError>(PayloadSource.Error(PayloadContract.ForPayload(typeof(TError)))),
                pb);
            _draft.HandleError(statusCode, pb.BuildReaction());
            return this;
        }

        /// <summary>Chains a follow-up HTTP request that fires after the current response.</summary>
        /// <param name="request">Builds the chained request.</param>
        /// <returns>This builder for chaining.</returns>
        public ResponseBuilder<TModel> Chained(Action<HttpRequestBuilder<TModel>> request)
        {
            var chainedBuilder = new HttpRequestBuilder<TModel>(_context);
            request(chainedBuilder);
            _draft.ContinueWith(chainedBuilder.BuildRequest());
            return this;
        }
    }
}
