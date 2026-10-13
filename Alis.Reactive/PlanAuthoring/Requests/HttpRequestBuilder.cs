using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    /// <summary>
    /// Configures one HTTP request reaction: endpoint, request input,
    /// lifecycle reactions, validation, and response routes.
    /// </summary>
    /// <remarks>
    /// Created by HTTP entry points such as <c>p.Get("/url")</c> and
    /// <c>p.Post("/url")</c>. Branches inside <c>p.Parallel(...)</c> use the same builder.
    /// </remarks>
    /// <typeparam name="TModel">View model that owns model-bound component IDs.</typeparam>
    public class HttpRequestBuilder<TModel> where TModel : class
    {
        private readonly PlanBuildContext _context;

        internal HttpRequestBuilder(PlanBuildContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>The request this builder configures, written where the request was started.</summary>
        internal RequestStep Step { get; } = new RequestStep();

        /// <summary>Selects GET for this request endpoint.</summary>
        /// <param name="url">Request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        public HttpRequestBuilder<TModel> Get(string url) { Step.To("GET", url); return this; }
        /// <summary>Selects POST for this request endpoint.</summary>
        /// <param name="url">Request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        public HttpRequestBuilder<TModel> Post(string url) { Step.To("POST", url); return this; }
        /// <summary>Selects PUT for this request endpoint.</summary>
        /// <param name="url">Request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        public HttpRequestBuilder<TModel> Put(string url) { Step.To("PUT", url); return this; }
        /// <summary>Selects DELETE for this request endpoint.</summary>
        /// <param name="url">Request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        public HttpRequestBuilder<TModel> Delete(string url) { Step.To("DELETE", url); return this; }

        /// <summary>Builds the request from the view model's own members, for an endpoint that binds the view model.</summary>
        /// <param name="gather">Sets the members, the form's inputs and the headers sent.</param>
        public HttpRequestBuilder<TModel> Gather(Action<GatherBuilder<TModel, TModel>> gather) =>
            Gather<TModel>(gather);

        /// <summary>
        /// Builds the request the endpoint binds: a member named like a URL <c>{placeholder}</c> fills the path,
        /// and the others go to the query string for GET and to the body otherwise.
        /// </summary>
        /// <typeparam name="TRequest">The class the endpoint binds.</typeparam>
        /// <param name="gather">Sets the members, the form's inputs and the headers sent.</param>
        public HttpRequestBuilder<TModel> Gather<TRequest>(Action<GatherBuilder<TRequest, TModel>> gather)
            where TRequest : class
        {
            if (gather == null) throw new ArgumentNullException(nameof(gather));
            var draft = new GatherInputDraft();
            gather(new GatherBuilder<TRequest, TModel>(draft));
            Step.Gather = draft;
            return this;
        }

        /// <summary>Serializes gathered body fields as JSON. This is the default body format.</summary>
        public HttpRequestBuilder<TModel> AsJson() { Step.Format("json"); return this; }
        /// <summary>Serializes gathered body fields as <c>FormData</c>, including <c>File</c> and <c>FileList</c> values.</summary>
        public HttpRequestBuilder<TModel> AsFormData() { Step.Format("form-data"); return this; }

        /// <summary>Runs a reaction before the HTTP request is sent.</summary>
        /// <param name="pipeline">Builds the pre-request reaction graph, such as showing a spinner.</param>
        public HttpRequestBuilder<TModel> WhileLoading(Action<PipelineBuilder<TModel>> pipeline)
        {
            Step.WhileLoading = PipelineBuilder<TModel>.Build(_context, pipeline);
            return this;
        }

        /// <summary>Runs reactions after the HTTP request settles, regardless of success, error, or network failure.</summary>
        /// <remarks>
        /// <para>Supports element reactions, component reactions, and condition guards.
        /// Does not provide response body access because the response may not
        /// exist on network failure.</para>
        /// <para>Typical use: hide a loading spinner that <see cref="WhileLoading"/> showed.</para>
        /// <para>Finally also runs when the request cannot be built, for example when a route parameter's
        /// source has no value; the error that stopped the request is still reported, and no response route
        /// runs. The request is built after client validation passes and any <see cref="WhileLoading"/>
        /// reactions have run. If client validation blocks the request, or a <see cref="WhileLoading"/>
        /// reaction fails, the request does not start and Finally does not run.</para>
        /// </remarks>
        /// <param name="pipeline">Builds the cleanup reaction graph to run after the request settles.</param>
        public HttpRequestBuilder<TModel> Finally(Action<PipelineBuilder<TModel>> pipeline)
        {
            Step.Finally = PipelineBuilder<TModel>.Build(_context, pipeline);
            return this;
        }

        /// <summary>Runs client validation for the target container before sending the request.</summary>
        /// <typeparam name="TValidationSource">Source type whose metadata declares the client validation rules.</typeparam>
        /// <param name="formId">DOM element ID of the form container for error display.</param>
        public HttpRequestBuilder<TModel> Validate<TValidationSource>(string formId)
            where TValidationSource : class
        {
            if (string.IsNullOrWhiteSpace(formId)) throw new ArgumentException("Form ID required.", nameof(formId));
            var firstValidation = Step.Validation == null;
            Step.Validation = (typeof(TValidationSource), formId);
            if (firstValidation) _context.ValidateBefore(Step);
            return this;
        }

        /// <summary>Configures response success routes, error routes, and follow-up requests.</summary>
        /// <param name="response">Builds the response routing graph for this request.</param>
        public HttpRequestBuilder<TModel> Response(Action<ResponseBuilder<TModel>> response)
        {
            var builder = new ResponseBuilder<TModel>(_context);
            response(builder);
            Step.Routes = builder.Routes;
            return this;
        }
    }
}
