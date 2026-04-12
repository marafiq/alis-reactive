using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

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
        private string _verb = "GET";
        private string _url = "";
        private GatherBuilder<TModel> _gatherBuilder;
        private string _transport = "json";
        private List<Reaction> _whileLoading;
        private List<Reaction> _finally;
        private ResponseBuilder<TModel> _response;
        private string _container;
        private Type _validatorType;

        internal HttpRequestBuilder(PlanBuildContext context)
        {
            _context = context;
        }

        internal HttpRequestBuilder<TModel> SetVerb(string verb) { _verb = verb; return this; }
        internal HttpRequestBuilder<TModel> SetUrl(string url) { _url = url; return this; }

        /// <summary>Sets the request to HTTP GET.</summary>
        /// <param name="url">The request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> Get(string url) { _verb = "GET"; _url = url; return this; }
        /// <summary>Sets the request to HTTP POST.</summary>
        /// <param name="url">The request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> Post(string url) { _verb = "POST"; _url = url; return this; }
        /// <summary>Sets the request to HTTP PUT.</summary>
        /// <param name="url">The request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> Put(string url) { _verb = "PUT"; _url = url; return this; }
        /// <summary>Sets the request to HTTP DELETE.</summary>
        /// <param name="url">The request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> Delete(string url) { _verb = "DELETE"; _url = url; return this; }

        /// <summary>Configures the request body by gathering values from components, events, plugins, and static data.</summary>
        /// <param name="gather">Builds the gather fields: <c>g =&gt; g.Include(m =&gt; m.Name).Header("X-Key", source)</c>.</param>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> Gather(Action<GatherBuilder<TModel>> gather)
        {
            var builder = new GatherBuilder<TModel>(_context);
            gather(builder);
            _gatherBuilder = builder;
            return this;
        }

        /// <summary>Sends the request body as JSON (default).</summary>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> AsJson() { _transport = "json"; return this; }
        /// <summary>Sends the request body as form-data.</summary>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> AsFormData() { _transport = "form-data"; return this; }

        /// <summary>Executes commands before the HTTP request is sent (e.g. show a spinner).</summary>
        /// <param name="pipeline">Builds the commands to execute before the request.</param>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> WhileLoading(Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            var reaction = pb.BuildReaction();
            var isPlainSequence = reaction is SequenceReaction;
            if (!isPlainSequence)
                throw new InvalidOperationException(
                    "WhileLoading only supports plain commands (sequential). " +
                    "Conditions, HTTP, and parallel pipelines are not valid here.");
            _whileLoading = new List<Reaction>(((SequenceReaction)reaction).Steps);
            return this;
        }

        /// <summary>Executes commands after the HTTP request completes, regardless of success, error, or network failure (e.g. hide a spinner).</summary>
        /// <remarks>
        /// Supports element commands, component commands, and condition guards.
        /// Does not provide response body access because the response may not
        /// exist on network failure.
        /// </remarks>
        /// <param name="pipeline">Builds the commands to execute after the request completes.</param>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> Finally(Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            var reaction = pb.BuildReaction();
            _finally = reaction is SequenceReaction seq
                ? new List<Reaction>(seq.Steps)
                : new List<Reaction> { reaction };
            return this;
        }

        /// <summary>Validates the form before sending the request using the specified validator.</summary>
        /// <typeparam name="TValidator">The validator type.</typeparam>
        /// <param name="formId">The DOM element ID of the form container for error display.</param>
        /// <returns>This builder for chaining.</returns>
        public HttpRequestBuilder<TModel> Validate<TValidator>(string formId)
            where TValidator : class
        {
            _validatorType = typeof(TValidator);
            _container = formId;
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
            var request = new Request(_verb, _url);

            if (_gatherBuilder != null)
                ResolveRequestPayload(request);

            if (_container != null)
                request.Container = _container;

            var hasWhileLoadingCommands = _whileLoading != null && _whileLoading.Count > 0;
            if (hasWhileLoadingCommands)
                request.Before = _whileLoading;

            var hasFinallyCommands = _finally != null && _finally.Count > 0;
            if (hasFinallyCommands)
                request.Complete = _finally;

            if (_response != null)
                AttachResponseLifecycle(request);

            if (_validatorType != null)
                request.ValidatorType = _validatorType;

            var hasHeaders = _gatherBuilder != null && _gatherBuilder.HeaderFields.Count > 0;
            if (hasHeaders)
                request.Headers = new Dictionary<string, ValueProducer>(_gatherBuilder.HeaderFields);

            var hasRouteParams = _gatherBuilder != null && _gatherBuilder.RouteParamFields.Count > 0;
            if (hasRouteParams)
                ValidateRouteParamAlignment(request);

            return request;
        }

        /// <summary>
        /// Resolves the request payload from gathered component fields, static values,
        /// and event-sourced values. IncludeAll expands to every registered input component.
        /// </summary>
        private void ResolveRequestPayload(Request request)
        {
            if (_gatherBuilder.IsIncludeAll)
                ExpandIncludeAllComponents();

            var hasComponentFields = _gatherBuilder.Fields.Count > 0;
            if (hasComponentFields)
            {
                var statics = BuildStaticAndEventFields();
                var gatherInput = new GatherInput(_gatherBuilder.Fields, _transport, statics);
                if (_gatherBuilder.IsIncludeAll) gatherInput.IncludeAll = true;
                request.Input = gatherInput;
                return;
            }

            var hasStaticOrEventFieldsOnly = _gatherBuilder.StaticFields.Count > 0 || _gatherBuilder.EventFields.Count > 0;
            if (hasStaticOrEventFieldsOnly)
                request.Input = new ValueInput(BuildStaticAndEventFields(), _transport);
        }

        /// <summary>
        /// Adds a GatherField for every registered input component that isn't already
        /// explicitly included. Each field carries its binding path and shape from the
        /// component registration — the plan carries all information.
        /// </summary>
        private void ExpandIncludeAllComponents()
        {
            var registered = _context.GetRegisteredComponents();
            foreach (var kvp in registered)
            {
                var alreadyIncluded = _gatherBuilder.Fields.Exists(f => f.Key == kvp.Key);
                if (alreadyIncluded)
                    continue;

                var reg = kvp.Value;
                var componentValue = ValueProducer.Read(
                    ComponentSource.Of(reg.ComponentId), reg.ValueMember, shape: reg.Shape);
                _gatherBuilder.AddField(GatherField.Of(kvp.Key, componentValue));
            }
        }

        /// <summary>
        /// Builds a single ValueProducer.Object from static literal values and event-sourced
        /// values. Static fields use LiteralRaw with shape inference; event fields read from
        /// the trigger payload. Returns null when neither source has fields.
        /// </summary>
        private ValueProducer BuildStaticAndEventFields()
        {
            var hasNoFields = _gatherBuilder.StaticFields.Count == 0 && _gatherBuilder.EventFields.Count == 0;
            if (hasNoFields) return null;

            var fields = new Dictionary<string, ValueProducer>();
            foreach (var sf in _gatherBuilder.StaticFields)
                fields[sf.Key] = ValueProducer.LiteralRaw(sf.Value, Shape.FromClrType(sf.Value?.GetType()));
            foreach (var ef in _gatherBuilder.EventFields)
                fields[ef.Key] = ValueProducer.Read(PayloadSource.Event(), ef.EventPath);
            return ValueProducer.Object(fields);
        }

        /// <summary>
        /// Wires success handlers, error handlers, and chained request onto the request.
        /// Each is independently optional — a request may have success without error, or
        /// a chained next without any handlers.
        /// </summary>
        private void AttachResponseLifecycle(Request request)
        {
            if (_response.SuccessHandlers.Count > 0)
                request.Success = _response.SuccessHandlers;
            if (_response.ErrorHandlers.Count > 0)
                request.Error = _response.ErrorHandlers;
            if (_response.ChainedRequest != null)
                request.Next = _response.ChainedRequest;
        }

        /// <summary>
        /// Validates bidirectional alignment between URL template placeholders and RouteParam
        /// declarations. Every RouteParam must match a {placeholder} in the URL, and every
        /// {placeholder} in the URL must have a corresponding RouteParam.
        /// </summary>
        private void ValidateRouteParamAlignment(Request request)
        {
            var placeholderRe = new System.Text.RegularExpressions.Regex(@"\{(\w+)\}");

            foreach (var paramName in _gatherBuilder.RouteParamFields.Keys)
            {
                if (!_url.Contains("{" + paramName + "}"))
                    throw new InvalidOperationException(
                        $"Route param '{paramName}' does not match any placeholder in URL '{_url}'. " +
                        $"Expected '{{{paramName}}}' in the URL template.");
            }

            foreach (System.Text.RegularExpressions.Match match in placeholderRe.Matches(_url))
            {
                var placeholder = match.Groups[1].Value;
                if (!_gatherBuilder.RouteParamFields.ContainsKey(placeholder))
                    throw new InvalidOperationException(
                        $"URL template '{_url}' has placeholder '{{{placeholder}}}' " +
                        $"but no matching .RouteParam(\"{placeholder}\", ...) was provided.");
            }

            request.RouteParams = new Dictionary<string, ValueProducer>(_gatherBuilder.RouteParamFields);
        }
    }
}
