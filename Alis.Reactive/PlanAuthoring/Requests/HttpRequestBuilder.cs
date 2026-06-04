using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    /// <summary>
    /// Configures one HTTP request reaction: endpoint, gathered input, body
    /// format, validation, loading/finally reactions, and response routes.
    /// </summary>
    /// <remarks>
    /// Created by HTTP entry points such as <c>p.Get("/url")</c>,
    /// <c>p.Post("/url")</c>, or branches inside <c>p.Parallel(...)</c>.
    /// </remarks>
    /// <typeparam name="TModel">The view model that owns model-bound component IDs.</typeparam>
    public class HttpRequestBuilder<TModel> where TModel : class
    {
        private readonly PlanBuildContext _context;
        private RequestEndpoint? _endpoint;
        private GatherInputDraft _requestInput = new GatherInputDraft();
        private RequestBodyFormat _bodyFormat = RequestBodyFormat.Json;
        private readonly List<ReactionGraph> _whileLoading = new List<ReactionGraph>();
        private readonly List<ReactionGraph> _finally = new List<ReactionGraph>();
        private ResponseBuilder<TModel> _response;
        private ClientValidationBeforeRequest? _validation;

        internal HttpRequestBuilder(PlanBuildContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _response = new ResponseBuilder<TModel>(_context);
        }

        /// <summary>Uses GET for this request builder's endpoint.</summary>
        /// <param name="url">The request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        public HttpRequestBuilder<TModel> Get(string url) { SelectEndpoint(HttpMethodName.Get, url); return this; }
        /// <summary>Uses POST for this request builder's endpoint.</summary>
        /// <param name="url">The request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        public HttpRequestBuilder<TModel> Post(string url) { SelectEndpoint(HttpMethodName.Post, url); return this; }
        /// <summary>Uses PUT for this request builder's endpoint.</summary>
        /// <param name="url">The request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        public HttpRequestBuilder<TModel> Put(string url) { SelectEndpoint(HttpMethodName.Put, url); return this; }
        /// <summary>Uses DELETE for this request builder's endpoint.</summary>
        /// <param name="url">The request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        public HttpRequestBuilder<TModel> Delete(string url) { SelectEndpoint(HttpMethodName.Delete, url); return this; }

        /// <summary>Collects runtime values for request body fields, headers, and route template parameters.</summary>
        /// <param name="gather">Builds the values to resolve immediately before the request is sent.</param>
        public HttpRequestBuilder<TModel> Gather(Action<GatherBuilder<TModel>> gather)
        {
            var draft = new GatherInputDraft();
            var builder = new GatherBuilder<TModel>(_context, draft);
            gather(builder);
            _requestInput = draft;
            return this;
        }

        /// <summary>Serializes gathered body fields as JSON. This is the default body format.</summary>
        public HttpRequestBuilder<TModel> AsJson() { _bodyFormat = RequestBodyFormat.Json; return this; }
        /// <summary>Serializes gathered body fields as <c>FormData</c>, including browser file values.</summary>
        public HttpRequestBuilder<TModel> AsFormData() { _bodyFormat = RequestBodyFormat.FormData; return this; }

        /// <summary>Runs a reaction before the HTTP request is sent.</summary>
        /// <param name="pipeline">Builds the pre-request reaction graph, such as showing a spinner.</param>
        public HttpRequestBuilder<TModel> WhileLoading(Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            var reaction = pb.BuildReaction();
            _whileLoading.Clear();
            _whileLoading.Add(reaction);
            return this;
        }

        /// <summary>Runs reactions after the HTTP request settles, regardless of success, error, or network failure.</summary>
        /// <remarks>
        /// <para>Supports element commands, component commands, and condition guards.
        /// Does not provide response body access because the response may not
        /// exist on network failure.</para>
        /// <para>Typical use: hide a loading spinner that <see cref="WhileLoading"/> showed.</para>
        /// </remarks>
        /// <param name="pipeline">Builds the cleanup reaction graph to run after the request settles.</param>
        public HttpRequestBuilder<TModel> Finally(Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            var reaction = pb.BuildReaction();
            _finally.Clear();
            _finally.Add(reaction);
            return this;
        }

        /// <summary>Runs client validation for the target container before sending the request.</summary>
        /// <typeparam name="TValidationSource">The source type whose metadata declares the client validation rules.</typeparam>
        /// <param name="formId">The DOM element ID of the form container for error display.</param>
        public HttpRequestBuilder<TModel> Validate<TValidationSource>(string formId)
            where TValidationSource : class
        {
            _validation = ClientValidationBeforeRequest.Using(
                typeof(TValidationSource),
                ComponentId.Of(formId));
            return this;
        }

        /// <summary>Configures success routes, error routes, and follow-up requests for the response.</summary>
        /// <param name="response">Builds the response routing graph for this request.</param>
        public HttpRequestBuilder<TModel> Response(Action<ResponseBuilder<TModel>> response)
        {
            var builder = new ResponseBuilder<TModel>(_context);
            response(builder);
            _response = builder;
            return this;
        }

        internal RequestPlan BuildRequest()
        {
            var endpoint = _endpoint ?? throw new InvalidOperationException(
                "HTTP request endpoint was not selected. Call Get, Post, Put, or Delete before the request is built.");
            var input = ResolveInput(endpoint.Url);
            var validation = _validation;

            var request = RequestPlan.Create(
                endpoint,
                input,
                RequestReactions.From(_whileLoading, _finally),
                _response.Draft.BuildRouting(),
                validation.HasValue
                    ? validation.Value.Target
                    : RequestValidationTarget.None);

            if (validation.HasValue)
                validation.Value.Register(_context, request);

            return request;
        }

        private void SelectEndpoint(HttpMethodName method, string url)
        {
            _endpoint = RequestEndpoint.To(method, RequestUrl.Of(url));
        }

        private RequestInput ResolveInput(RequestUrl url)
        {
            return _requestInput.BuildRequestInput(_bodyFormat, url);
        }

    }

    internal readonly struct ClientValidationBeforeRequest
    {
        private readonly Type _sourceType;
        private readonly ComponentId _container;

        private ClientValidationBeforeRequest(Type sourceType, ComponentId container)
        {
            _sourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        internal RequestValidationTarget Target =>
            RequestValidationTarget.DisplayIn(_container);

        internal static ClientValidationBeforeRequest Using(Type sourceType, ComponentId container) =>
            new ClientValidationBeforeRequest(sourceType, container);

        internal void Register(PlanBuildContext context, RequestPlan request)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            context.RegisterValidationJob(request, _container, _sourceType);
        }
    }

}
