using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    /// <summary>
    /// Builds an HTTP request with optional gather, validation, response routing, and chaining.
    /// </summary>
    /// <remarks>
    /// Obtained via <c>p.Get("/url")</c>, <c>p.Post("/url")</c>, etc.
    /// </remarks>
    /// <typeparam name="TModel">The view model type.</typeparam>
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

        /// <summary>Sets the request to HTTP GET.</summary>
        /// <param name="url">The request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> Get(string url) { SelectEndpoint(HttpMethodName.Get, url); return this; }
        /// <summary>Sets the request to HTTP POST.</summary>
        /// <param name="url">The request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> Post(string url) { SelectEndpoint(HttpMethodName.Post, url); return this; }
        /// <summary>Sets the request to HTTP PUT.</summary>
        /// <param name="url">The request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> Put(string url) { SelectEndpoint(HttpMethodName.Put, url); return this; }
        /// <summary>Sets the request to HTTP DELETE.</summary>
        /// <param name="url">The request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> Delete(string url) { SelectEndpoint(HttpMethodName.Delete, url); return this; }

        /// <summary>Configures the request body by gathering values from components, events, plugins, and static data.</summary>
        /// <param name="gather">Builds the gather payload assignments: <c>g =&gt; g.Include(m =&gt; m.Name).Header("X-Key", source)</c>.</param>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> Gather(Action<GatherBuilder<TModel>> gather)
        {
            var draft = new GatherInputDraft();
            var builder = new GatherBuilder<TModel>(_context, draft);
            gather(builder);
            _requestInput = draft;
            return this;
        }

        /// <summary>Sends the request body as JSON (default).</summary>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> AsJson() { _bodyFormat = RequestBodyFormat.Json; return this; }
        /// <summary>Sends the request body as form-data.</summary>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> AsFormData() { _bodyFormat = RequestBodyFormat.FormData; return this; }

        /// <summary>Executes a reaction graph before the HTTP request is sent (e.g. show a spinner or run a guarded prerequisite).</summary>
        /// <param name="pipeline">Builds the reaction graph to execute before the request.</param>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> WhileLoading(Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            var reaction = pb.BuildReaction();
            _whileLoading.Clear();
            _whileLoading.Add(reaction);
            return this;
        }

        /// <summary>Executes commands after the HTTP request completes, regardless of success, error, or network failure.</summary>
        /// <remarks>
        /// <para>Supports element commands, component commands, and condition guards.
        /// Does not provide response body access because the response may not
        /// exist on network failure.</para>
        /// <para>Typical use: hide a loading spinner that <see cref="WhileLoading"/> showed.</para>
        /// </remarks>
        /// <param name="pipeline">Configures the cleanup commands to run after the request settles.</param>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> Finally(Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            var reaction = pb.BuildReaction();
            _finally.Clear();
            _finally.Add(reaction);
            return this;
        }

        /// <summary>Validates the form before sending the request using the specified validation source.</summary>
        /// <typeparam name="TValidationSource">The validation source type used by the configured client rule source.</typeparam>
        /// <param name="formId">The DOM element ID of the form container for error display.</param>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> Validate<TValidationSource>(string formId)
            where TValidationSource : class
        {
            _validation = ClientValidationBeforeRequest.Using(
                typeof(TValidationSource),
                ComponentId.Of(formId));
            return this;
        }

        /// <summary>Configures response routes for success and error outcomes.</summary>
        /// <param name="response">Builds the response routes: <c>r =&gt; r.OnSuccess(...).OnError(...)</c>.</param>
        /// <returns>This builder for chaining.</returns>
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
                Snapshot(_whileLoading),
                Snapshot(_response.Draft.SuccessRoutes),
                Snapshot(_response.Draft.ErrorRoutes),
                Snapshot(_finally),
                _response.Draft.Chain,
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

        private static IReadOnlyList<T> Snapshot<T>(IReadOnlyList<T> items)
        {
            var hasNoItems = items.Count == 0;
            if (hasNoItems) return Array.Empty<T>();
            return new List<T>(items);
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
