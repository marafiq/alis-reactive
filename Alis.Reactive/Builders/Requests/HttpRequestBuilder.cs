using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    /// <summary>
    /// Builds an HTTP request with optional gather, validation, response handling, and chaining.
    /// </summary>
    /// <remarks>
    /// Obtained via <c>p.Get("/url")</c>, <c>p.Post("/url")</c>, etc.
    /// </remarks>
    /// <typeparam name="TModel">The view model type.</typeparam>
    public class HttpRequestBuilder<TModel> where TModel : class
    {
        private readonly PlanBuildContext _context;
        private RequestEndpointDraft _endpoint = RequestEndpointDraft.Unselected;
        private GatherBuilder<TModel>? _gather;
        private RequestTransport _transport = RequestTransport.Json;
        private readonly List<Reaction> _whileLoading = new List<Reaction>();
        private readonly List<Reaction> _finally = new List<Reaction>();
        private ResponseBuilder<TModel> _response;
        private RequestValidation _validation = RequestValidation.None;

        internal HttpRequestBuilder(PlanBuildContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _response = new ResponseBuilder<TModel>(_context);
        }

        /// <summary>Sets the request to HTTP GET.</summary>
        /// <param name="url">The request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> Get(string url) { _endpoint = RequestEndpointDraft.Select(HttpMethodName.Get, RequestUrl.Of(url)); return this; }
        /// <summary>Sets the request to HTTP POST.</summary>
        /// <param name="url">The request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> Post(string url) { _endpoint = RequestEndpointDraft.Select(HttpMethodName.Post, RequestUrl.Of(url)); return this; }
        /// <summary>Sets the request to HTTP PUT.</summary>
        /// <param name="url">The request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> Put(string url) { _endpoint = RequestEndpointDraft.Select(HttpMethodName.Put, RequestUrl.Of(url)); return this; }
        /// <summary>Sets the request to HTTP DELETE.</summary>
        /// <param name="url">The request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> Delete(string url) { _endpoint = RequestEndpointDraft.Select(HttpMethodName.Delete, RequestUrl.Of(url)); return this; }

        /// <summary>Configures the request body by gathering values from components, events, plugins, and static data.</summary>
        /// <param name="gather">Builds the gather fields: <c>g =&gt; g.Include(m =&gt; m.Name).Header("X-Key", source)</c>.</param>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> Gather(Action<GatherBuilder<TModel>> gather)
        {
            var builder = new GatherBuilder<TModel>(_context);
            gather(builder);
            _gather = builder;
            return this;
        }

        /// <summary>Sends the request body as JSON (default).</summary>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> AsJson() { _transport = RequestTransport.Json; return this; }
        /// <summary>Sends the request body as form-data.</summary>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> AsFormData() { _transport = RequestTransport.FormData; return this; }

        /// <summary>Executes a reaction graph before the HTTP request is sent (e.g. show a spinner or run a guarded prerequisite).</summary>
        /// <param name="pipeline">Builds the reaction graph to execute before the request.</param>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> WhileLoading(Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            var reaction = pb.BuildReaction();
            _whileLoading.Clear();
            AddReactionGraph(_whileLoading, reaction);
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
            AddReactionGraph(_finally, reaction);
            return this;
        }

        /// <summary>Validates the form before sending the request using the specified validation source.</summary>
        /// <typeparam name="TValidationSource">The validation source type used by the configured client projection source.</typeparam>
        /// <param name="formId">The DOM element ID of the form container for error display.</param>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> Validate<TValidationSource>(string formId)
            where TValidationSource : class
        {
            _validation = RequestValidation.For(typeof(TValidationSource), formId);
            return this;
        }

        /// <summary>Configures response handlers for success and error outcomes.</summary>
        /// <param name="response">Builds the response handlers: <c>r =&gt; r.OnSuccess(...).OnError(...)</c>.</param>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> Response(Action<ResponseBuilder<TModel>> response)
        {
            var builder = new ResponseBuilder<TModel>(_context);
            response(builder);
            _response = builder;
            return this;
        }

        internal Request BuildRequest()
        {
            var endpoint = _endpoint.Build();
            var input = ResolveInput();

            var request = Request.Create(
                endpoint,
                input,
                ResolveRequestLifecycle(),
                ResolveParameters(endpoint.Url),
                _validation.Target);

            _validation.Register(_context, request);

            return request;
        }

        private RequestInput ResolveInput()
        {
            if (_gather == null)
                return RequestInput.None;

            var declaredFields = new List<RequestPayloadAssignment>(_gather.Draft.DeclaredFields);
            var registeredInputFields = new List<RequestPayloadAssignment>();
            var selection = _gather.Draft.Selection;
            var supplementalFields = _gather.Draft.SupplementalFields;

            selection.AddBuildTimeRegisteredInputFields(registeredInputFields, _context);

            var gatherDslAddedInput =
                declaredFields.Count > 0
                || registeredInputFields.Count > 0
                || supplementalFields.Count > 0
                || selection.MayExpandRegisteredInputsAtRuntime;
            if (!gatherDslAddedInput)
                return RequestInput.None;

            return GatherInput.From(
                declaredFields,
                registeredInputFields,
                _transport,
                supplementalFields,
                selection);
        }

        private RequestParameters ResolveParameters(RequestUrl url)
        {
            if (_gather == null)
            {
                var routeParameters = RequestRouteTemplate
                    .For(url)
                    .Bind(new Dictionary<string, ValueProducer>());

                return RequestParameters.From(
                    new Dictionary<string, ValueProducer>(),
                    routeParameters);
            }

            return RequestParameters.From(
                _gather.Draft.HeadersForRequest(),
                _gather.Draft.RouteParametersFor(url));
        }

        private RequestLifecycle ResolveRequestLifecycle()
        {
            return RequestLifecycle.Create(
                RequestReactionStages.From(
                    Snapshot(_whileLoading),
                    Snapshot(_response.Draft.SuccessHandlers),
                    Snapshot(_response.Draft.ErrorHandlers),
                    Snapshot(_finally)),
                _response.Draft.Chain);
        }

        private static IReadOnlyList<T> Snapshot<T>(IReadOnlyList<T> items)
        {
            var hasNoItems = items.Count == 0;
            if (hasNoItems) return Array.Empty<T>();
            return new List<T>(items);
        }

        private static void AddReactionGraph(List<Reaction> target, Reaction reaction)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (reaction == null) throw new ArgumentNullException(nameof(reaction));

            if (reaction is SequenceReaction sequence)
            {
                target.AddRange(sequence.Steps);
                return;
            }

            target.Add(reaction);
        }

    }

    internal abstract class RequestEndpointDraft
    {
        internal static RequestEndpointDraft Unselected { get; } =
            new UnselectedRequestEndpointDraft();

        internal static RequestEndpointDraft Select(HttpMethodName method, RequestUrl url) =>
            new SelectedRequestEndpointDraft(method, url);

        internal abstract RequestEndpoint Build();
    }

    internal sealed class UnselectedRequestEndpointDraft : RequestEndpointDraft
    {
        internal override RequestEndpoint Build()
        {
            throw new InvalidOperationException(
                "HTTP request endpoint was not selected. Call Get, Post, Put, or Delete before the request is built.");
        }
    }

    internal sealed class SelectedRequestEndpointDraft : RequestEndpointDraft
    {
        private readonly RequestEndpoint _endpoint;

        internal SelectedRequestEndpointDraft(HttpMethodName method, RequestUrl url)
        {
            _endpoint = RequestEndpoint.To(method, url);
        }

        internal override RequestEndpoint Build() => _endpoint;
    }

    internal sealed class RequestRouteTemplate
    {
        private readonly RequestUrl _url;
        private readonly RequestRouteTemplatePlaceholders _placeholders;

        private RequestRouteTemplate(RequestUrl url)
        {
            _url = url ?? throw new ArgumentNullException(nameof(url));
            _placeholders = RequestRouteTemplatePlaceholders.In(url);
        }

        internal static RequestRouteTemplate For(RequestUrl url) =>
            new RequestRouteTemplate(url);

        internal IReadOnlyDictionary<string, ValueProducer> Bind(
            IReadOnlyDictionary<string, ValueProducer> routeParams)
        {
            if (routeParams == null) throw new ArgumentNullException(nameof(routeParams));

            EnsureEveryRouteParameterHasPlaceholder(routeParams);
            EnsureEveryPlaceholderHasRouteParameter(routeParams);
            return Copy(routeParams);
        }

        private void EnsureEveryRouteParameterHasPlaceholder(
            IReadOnlyDictionary<string, ValueProducer> routeParams)
        {
            foreach (var paramName in routeParams.Keys)
            {
                var routeParameterIsUsedByTemplate = _placeholders.Contains(paramName);
                if (routeParameterIsUsedByTemplate)
                    continue;

                throw new InvalidOperationException(
                    $"Route param '{paramName}' does not match any placeholder in URL '{_url.Value}'. " +
                    $"Expected '{{{paramName}}}' in the URL template.");
            }
        }

        private void EnsureEveryPlaceholderHasRouteParameter(
            IReadOnlyDictionary<string, ValueProducer> routeParams)
        {
            foreach (var placeholder in _placeholders.Names)
            {
                var placeholderHasRouteParameter = routeParams.ContainsKey(placeholder);
                if (placeholderHasRouteParameter)
                    continue;

                throw new InvalidOperationException(
                    $"URL template '{_url.Value}' has placeholder '{{{placeholder}}}' " +
                    $"but no matching .RouteParam(\"{placeholder}\", ...) was provided.");
            }
        }

        private static Dictionary<string, ValueProducer> Copy(
            IReadOnlyDictionary<string, ValueProducer> routeParams)
        {
            var copy = new Dictionary<string, ValueProducer>();
            foreach (var routeParam in routeParams)
                copy[routeParam.Key] = routeParam.Value;
            return copy;
        }
    }

    internal sealed class RequestRouteTemplatePlaceholders
    {
        private readonly IReadOnlyList<string> _names;
        private readonly HashSet<string> _lookup;

        internal RequestRouteTemplatePlaceholders(IReadOnlyList<string> names)
        {
            _names = names ?? throw new ArgumentNullException(nameof(names));
            _lookup = new HashSet<string>(_names, StringComparer.Ordinal);
        }

        internal IReadOnlyList<string> Names => _names;

        internal static RequestRouteTemplatePlaceholders In(RequestUrl url)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            return new RequestRouteTemplateParser(url).Parse();
        }

        internal bool Contains(string routeParameterName)
        {
            if (routeParameterName == null) throw new ArgumentNullException(nameof(routeParameterName));
            return _lookup.Contains(routeParameterName);
        }
    }

    internal sealed class RequestRouteTemplateParser
    {
        private readonly RequestUrl _url;

        internal RequestRouteTemplateParser(RequestUrl url)
        {
            _url = url ?? throw new ArgumentNullException(nameof(url));
        }

        internal RequestRouteTemplatePlaceholders Parse()
        {
            var names = new List<string>();
            var text = _url.Value;
            for (var index = 0; index < text.Length; index++)
            {
                var current = text[index];
                if (current == '{')
                {
                    index = ReadPlaceholderAt(index, names);
                    continue;
                }

                if (current == '}')
                    throw InvalidTemplate("unexpected closing brace '}'");
            }

            return new RequestRouteTemplatePlaceholders(names);
        }

        private int ReadPlaceholderAt(int startIndex, List<string> names)
        {
            var text = _url.Value;
            var endIndex = text.IndexOf('}', startIndex + 1);
            if (endIndex < 0)
                throw InvalidTemplate("missing closing brace '}'");

            var name = text.Substring(startIndex + 1, endIndex - startIndex - 1);
            try
            {
                names.Add(RouteParameterName.Of(name).Value);
            }
            catch (ArgumentException ex)
            {
                throw InvalidTemplate(
                    $"invalid placeholder '{{{name}}}'. Names must match [a-zA-Z0-9_] (ASCII only)",
                    ex);
            }

            return endIndex;
        }

        private InvalidOperationException InvalidTemplate(string reason) =>
            InvalidTemplate(reason, null);

        private InvalidOperationException InvalidTemplate(string reason, Exception? inner) =>
            new InvalidOperationException(
                $"URL template '{_url.Value}' is invalid: {reason}.",
                inner);
    }

    internal abstract class RequestValidation
    {
        internal static RequestValidation None { get; } = new NoRequestValidation();

        internal static RequestValidation For(Type validationSourceType, string containerId) =>
            new ConfiguredRequestValidation(validationSourceType, containerId);

        internal abstract RequestValidationTarget Target { get; }

        internal abstract void Register(PlanBuildContext context, Request request);
    }

    internal sealed class NoRequestValidation : RequestValidation
    {
        internal override RequestValidationTarget Target => RequestValidationTarget.None;

        internal override void Register(PlanBuildContext context, Request request)
        {
        }
    }

    internal sealed class ConfiguredRequestValidation : RequestValidation
    {
        private readonly Type _validationSourceType;
        private readonly ComponentId _container;

        internal ConfiguredRequestValidation(Type validationSourceType, string containerId)
        {
            _validationSourceType = validationSourceType ?? throw new ArgumentNullException(nameof(validationSourceType));
            _container = ComponentId.Of(containerId);
        }

        internal override RequestValidationTarget Target =>
            RequestValidationTarget.DisplayIn(_container);

        internal override void Register(PlanBuildContext context, Request request)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            context.RegisterValidationJob(request, _container, _validationSourceType);
        }
    }

}
