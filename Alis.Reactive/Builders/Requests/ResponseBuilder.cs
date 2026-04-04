using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    /// <summary>
    /// Configures success and error response handlers for an HTTP request, plus optional
    /// chained follow-up requests.
    /// </summary>
    /// <remarks>
    /// Accessed via <see cref="HttpRequestBuilder{TModel}.Response"/>:
    /// <code>
    /// p.Post("/api/save", gather: g =&gt; g.IncludeAll())
    ///  .Response(response: r =&gt;
    ///  {
    ///      r.OnSuccess(pipeline: s =&gt; s.Into("result"));
    ///      r.OnError(400, pipeline: s =&gt; s.ValidationErrors("myForm"));
    ///  });
    /// </code>
    /// </remarks>
    /// <typeparam name="TModel">The view model type.</typeparam>
    public class ResponseBuilder<TModel> where TModel : class
    {
        private readonly PlanAuthoringContext _authoring;
        private readonly WorkflowScope _scope;

        internal List<ResponseHandlerDefinition<TModel>> SuccessHandlers { get; } = new List<ResponseHandlerDefinition<TModel>>();
        internal List<ResponseHandlerDefinition<TModel>> ErrorHandlers { get; } = new List<ResponseHandlerDefinition<TModel>>();
        internal HttpRequestBuilder<TModel>? ChainedRequest { get; private set; }

        internal ResponseBuilder(PlanAuthoringContext authoring, WorkflowScope scope)
        {
            _authoring = authoring;
            _scope = scope;
        }

        /// <summary>
        /// Registers a success handler (status 2xx, no specific code filter).
        /// </summary>
        /// <param name="pipeline">Builds the workflow actions that run on success.</param>
        public ResponseBuilder<TModel> OnSuccess(Action<PipelineBuilder<TModel>> pipeline)
        {
            var builder = new PipelineBuilder<TModel>(_authoring, _scope);
            pipeline(builder);
            SuccessHandlers.Add(BuildHandler(null, builder));
            return this;
        }

        /// <summary>
        /// Registers a typed JSON success handler. The ResponseBody&lt;T&gt; surface enables
        /// compile-time selection of response members in the same way ReactiveEvent payloads
        /// enable compile-time selection of event members.
        ///
        /// Usage: .OnSuccess&lt;ApiResponse&gt;((json, s) =&gt; s.Element("x").SetText(json, r =&gt; r.Data.Name))
        /// </summary>
        public ResponseBuilder<TModel> OnSuccess<TResponse>(
            Action<ResponseBody<TResponse>, PipelineBuilder<TModel>> pipeline)
            where TResponse : class, new()
        {
            var builder = new PipelineBuilder<TModel>(_authoring, _scope);
            pipeline(new ResponseBody<TResponse>(new TResponse()), builder);
            SuccessHandlers.Add(BuildHandler(null, builder));
            return this;
        }

        /// <summary>
        /// Registers an error handler for a specific HTTP status code.
        /// </summary>
        /// <param name="statusCode">The HTTP status code to handle.</param>
        /// <param name="pipeline">Builds the workflow actions that run on this error status.</param>
        public ResponseBuilder<TModel> OnError(int statusCode, Action<PipelineBuilder<TModel>> pipeline)
        {
            var builder = new PipelineBuilder<TModel>(_authoring, _scope);
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
            var chainedBuilder = new HttpRequestBuilder<TModel>(_authoring, _scope);
            request(chainedBuilder);
            ChainedRequest = chainedBuilder;
            return this;
        }

        /// <summary>
        /// Applies the configured response pipelines to the authored request plan.
        /// </summary>
        internal void ApplyTo(RequestPlan request)
        {
            if (SuccessHandlers.Count > 0)
                request.OnSuccess = BuildHandlers(SuccessHandlers);

            if (ErrorHandlers.Count > 0)
                request.OnError = BuildHandlers(ErrorHandlers);

            if (ChainedRequest != null)
                request.Next = ChainedRequest.BuildRequestPlan();
        }

        private static ResponseHandlerDefinition<TModel> BuildHandler(int? statusCode, PipelineBuilder<TModel> builder)
        {
            return new ResponseHandlerDefinition<TModel>(statusCode, builder);
        }

        private static List<ResponseHandlerPlan> BuildHandlers(List<ResponseHandlerDefinition<TModel>> handlers)
        {
            var result = new List<ResponseHandlerPlan>();
            foreach (var handler in handlers)
            {
                var responseHandler = new ResponseHandlerPlan(handler.Pipeline.BuildAction())
                {
                    StatusCode = handler.StatusCode
                };

                result.Add(responseHandler);
            }

            return result;
        }
    }

    internal sealed class ResponseHandlerDefinition<TModel> where TModel : class
    {
        internal ResponseHandlerDefinition(int? statusCode, PipelineBuilder<TModel> pipeline)
        {
            StatusCode = statusCode;
            Pipeline = pipeline;
        }

        internal int? StatusCode { get; }
        internal PipelineBuilder<TModel> Pipeline { get; }
    }
}
