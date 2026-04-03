using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive.Builders.Requests
{
    /// <summary>
    /// Configures an HTTP request: URL, verb, request values, loading state,
    /// client-side validation, and response handlers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Accessed via the pipeline's HTTP methods:
    /// <c>p.Post("/api/save", gather: g =&gt; g.IncludeAll()).Response(response: r =&gt; r.OnSuccess(...))</c>.
    /// </para>
    /// <para>
    /// Typical call order: verb (set by <see cref="PipelineBuilder{TModel}"/>)
    /// → <see cref="Gather"/> → <see cref="WhileLoading"/>
    /// → <see cref="Validate{TValidator}"/> → <see cref="Response"/>.
    /// All steps are optional except the verb and URL.
    /// </para>
    /// </remarks>
    /// <typeparam name="TModel">The view model type.</typeparam>
    public class HttpRequestBuilder<TModel> where TModel : class
    {
        private readonly PlanAuthoringContext _authoring;
        private readonly WorkflowScope _scope;
        private string _verb = "GET";
        private string _url = "";
        private List<RequestValuePart>? _requestValues;
        private string? _contentType;
        private List<PlanAction>? _whileLoading;
        private ResponseBuilder<TModel>? _response;
        private RequestValidation? _validation;
        private Type? _validatorType;

        internal HttpRequestBuilder(PlanAuthoringContext authoring, WorkflowScope scope)
        {
            _authoring = authoring;
            _scope = scope;
        }

        internal HttpRequestBuilder<TModel> SetVerb(string verb)
        {
            _verb = verb;
            return this;
        }

        internal HttpRequestBuilder<TModel> SetUrl(string url)
        {
            _url = url;
            return this;
        }

        // ── Public convenience verbs (used in Chained / Parallel lambdas) ──

        /// <summary>Sets the request verb to GET. Used inside <see cref="ResponseBuilder{TModel}.Chained"/> or <see cref="PipelineBuilder{TModel}.Parallel"/> lambdas.</summary>
        /// <param name="url">The request URL.</param>
        public HttpRequestBuilder<TModel> Get(string url) { _verb = "GET"; _url = url; return this; }

        /// <summary>Sets the request verb to POST. Used inside <see cref="ResponseBuilder{TModel}.Chained"/> or <see cref="PipelineBuilder{TModel}.Parallel"/> lambdas.</summary>
        /// <param name="url">The request URL.</param>
        public HttpRequestBuilder<TModel> Post(string url) { _verb = "POST"; _url = url; return this; }

        /// <summary>Sets the request verb to PUT. Used inside <see cref="ResponseBuilder{TModel}.Chained"/> or <see cref="PipelineBuilder{TModel}.Parallel"/> lambdas.</summary>
        /// <param name="url">The request URL.</param>
        public HttpRequestBuilder<TModel> Put(string url) { _verb = "PUT"; _url = url; return this; }

        /// <summary>Sets the request verb to DELETE. Used inside <see cref="ResponseBuilder{TModel}.Chained"/> or <see cref="PipelineBuilder{TModel}.Parallel"/> lambdas.</summary>
        /// <param name="url">The request URL.</param>
        public HttpRequestBuilder<TModel> Delete(string url) { _verb = "DELETE"; _url = url; return this; }

        /// <summary>
        /// Configures gather items for the request body/URL params.
        /// </summary>
        /// <param name="gather">Adds gather items for the request body or URL params.</param>
        public HttpRequestBuilder<TModel> Gather(Action<GatherBuilder<TModel>> gather)
        {
            var builder = new GatherBuilder<TModel>();
            gather(builder);
            _requestValues = builder.RequestValues;
            return this;
        }

        /// <summary>
        /// Sends the request body as application/json (default).
        /// </summary>
        public HttpRequestBuilder<TModel> AsJson() { _contentType = null; return this; }

        /// <summary>
        /// Sends the request body as multipart/form-data. Required for file uploads.
        /// </summary>
        public HttpRequestBuilder<TModel> AsFormData() { _contentType = "form-data"; return this; }

        /// <summary>
        /// Configures commands to execute while the request is in-flight.
        /// These commands are reverted after the response arrives.
        /// </summary>
        /// <param name="pipeline">Builds the loading-state commands (reverted after the response arrives).</param>
        public HttpRequestBuilder<TModel> WhileLoading(Action<PipelineBuilder<TModel>> pipeline)
        {
            var builder = new PipelineBuilder<TModel>(_authoring, _scope);
            pipeline(builder);
            var actions = builder.BuildActions();
            if (actions.Count != 1 || ContainsStructuredAction(actions[0]))
                throw new InvalidOperationException(
                    "WhileLoading only supports plain commands (sequential). " +
                    "Conditions, HTTP, and parallel pipelines are not valid here.");

            _whileLoading = FlattenSequential(actions[0]);
            return this;
        }

        /// <summary>
        /// Registers client-side validation from a pre-built form validation contract.
        /// When present, the runtime validates the form before sending the request.
        /// If validation fails, the request is aborted.
        /// </summary>
        public HttpRequestBuilder<TModel> Validate(FormValidation validation)
        {
            _validation = _authoring.ConvertValidation(validation);
            return this;
        }

        /// <summary>
        /// Registers client-side validation by validator type.
        /// Rules are extracted automatically at Render() time via <see cref="IFormValidationExtractor"/>.
        /// Field IDs use standard convention (property name = element ID).
        /// </summary>
        public HttpRequestBuilder<TModel> Validate<TValidator>(string formId)
            where TValidator : class
        {
            _validatorType = typeof(TValidator);
            _validation = new RequestValidation(formId, new List<RequestValidationField>());
            return this;
        }

        /// <summary>
        /// Configures success/error response handlers.
        /// </summary>
        /// <param name="response">Defines the success and error handlers for the response.</param>
        public HttpRequestBuilder<TModel> Response(Action<ResponseBuilder<TModel>> response)
        {
            var builder = new ResponseBuilder<TModel>(_authoring, _scope);
            response(builder);
            _response = builder;
            return this;
        }

        internal RequestPlan BuildRequestPlan()
        {
            var request = new RequestPlan(_verb, _url);

            if (_requestValues != null && _requestValues.Count > 0)
                request.Input = new RequestInput(ResolveTransport(), _authoring.BuildRequestValue(_requestValues));

            if (_whileLoading != null && _whileLoading.Count > 0)
                request.Before = _whileLoading;

            if (_validation != null)
                request.Validation = _validation;

            _response?.ApplyTo(request);

            if (_validatorType != null)
                _authoring.TrackPendingValidator(request, _validatorType);

            return request;
        }

        private string ResolveTransport()
        {
            if (_verb == "GET")
                return "query";

            return _contentType == "form-data" ? "form-data" : "json";
        }

        private static bool ContainsStructuredAction(PlanAction action)
        {
            if (action is SequenceAction sequence)
            {
                foreach (var step in sequence.Steps)
                    if (ContainsStructuredAction(step))
                        return true;

                return false;
            }

            return action is BranchAction || action is RequestAction || action is ParallelAction;
        }

        private static List<PlanAction> FlattenSequential(PlanAction action)
        {
            if (action is SequenceAction sequence)
                return sequence.Steps;

            return new List<PlanAction> { action };
        }
    }
}
