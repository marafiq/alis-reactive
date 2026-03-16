using System;
using System.Collections.Generic;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Descriptors.Reactions;
using Alis.Reactive.Descriptors.Requests;
using Alis.Reactive.Validation;

namespace Alis.Reactive.Builders.Requests
{
    public class HttpRequestBuilder<TModel> where TModel : class
    {
        private string _verb = "GET";
        private string _url = "";
        private List<GatherItem>? _gather;
        private string? _contentType;
        private List<Command>? _whileLoading;
        private ResponseBuilder<TModel>? _response;
        private ValidationDescriptor? _validation;
        private Type? _validatorType;

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

        public HttpRequestBuilder<TModel> Get(string url) { _verb = "GET"; _url = url; return this; }
        public HttpRequestBuilder<TModel> Post(string url) { _verb = "POST"; _url = url; return this; }
        public HttpRequestBuilder<TModel> Put(string url) { _verb = "PUT"; _url = url; return this; }
        public HttpRequestBuilder<TModel> Delete(string url) { _verb = "DELETE"; _url = url; return this; }

        /// <summary>
        /// Configures gather items for the request body/URL params.
        /// </summary>
        public HttpRequestBuilder<TModel> Gather(Action<GatherBuilder<TModel>> configure)
        {
            var builder = new GatherBuilder<TModel>();
            configure(builder);
            _gather = builder.Items;
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
        public HttpRequestBuilder<TModel> WhileLoading(Action<PipelineBuilder<TModel>> configure)
        {
            var builder = new PipelineBuilder<TModel>();
            configure(builder);
            var reaction = builder.BuildReaction();
            if (!(reaction is SequentialReaction sr))
                throw new InvalidOperationException(
                    "WhileLoading only supports plain commands (sequential). " +
                    "Conditions, HTTP, and parallel pipelines are not valid here.");
            _whileLoading = sr.Commands;
            return this;
        }

        /// <summary>
        /// Registers client-side validation from a pre-built descriptor.
        /// When present, the runtime validates the form before sending the request.
        /// If validation fails, the request is aborted.
        /// </summary>
        public HttpRequestBuilder<TModel> Validate(ValidationDescriptor validation)
        {
            _validation = validation;
            return this;
        }

        /// <summary>
        /// Registers client-side validation by validator type.
        /// Rules are extracted automatically at Render() time via IValidationExtractor.
        /// Field IDs use standard convention (property name = element ID).
        /// </summary>
        public HttpRequestBuilder<TModel> Validate<TValidator>(string formId)
            where TValidator : class
        {
            _validatorType = typeof(TValidator);
            _validation = new ValidationDescriptor(formId, new List<ValidationField>());
            return this;
        }

        /// <summary>
        /// Configures success/error response handlers.
        /// </summary>
        public HttpRequestBuilder<TModel> Response(Action<ResponseBuilder<TModel>> configure)
        {
            var builder = new ResponseBuilder<TModel>();
            configure(builder);
            _response = builder;
            return this;
        }

        internal RequestDescriptor BuildRequestDescriptor()
        {
            var desc = new RequestDescriptor(
                _verb,
                _url,
                _gather,
                _contentType,
                _whileLoading,
                _response?.SuccessHandlers.Count > 0 ? _response.SuccessHandlers : null,
                _response?.ErrorHandlers.Count > 0 ? _response.ErrorHandlers : null,
                _response?.ChainedRequest,
                _validation);

            if (_validatorType != null)
                desc.AttachValidator(_validatorType);

            return desc;
        }
    }
}
