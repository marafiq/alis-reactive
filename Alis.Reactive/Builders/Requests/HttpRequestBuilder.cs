using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive.Builders.Requests
{
    public class HttpRequestBuilder<TModel> where TModel : class
    {
        private readonly PlanBuildContext _context;
        private string _verb = "GET";
        private string _url = "";
        private List<GatherField> _gather;
        private string _transport = "json";
        private List<Reaction> _whileLoading;
        private ResponseBuilder<TModel> _response;
        private string _container;
        private Type _validatorType;

        internal HttpRequestBuilder(PlanBuildContext context)
        {
            _context = context;
        }

        internal HttpRequestBuilder<TModel> SetVerb(string verb) { _verb = verb; return this; }
        internal HttpRequestBuilder<TModel> SetUrl(string url) { _url = url; return this; }

        public HttpRequestBuilder<TModel> Get(string url) { _verb = "GET"; _url = url; return this; }
        public HttpRequestBuilder<TModel> Post(string url) { _verb = "POST"; _url = url; return this; }
        public HttpRequestBuilder<TModel> Put(string url) { _verb = "PUT"; _url = url; return this; }
        public HttpRequestBuilder<TModel> Delete(string url) { _verb = "DELETE"; _url = url; return this; }

        public HttpRequestBuilder<TModel> Gather(Action<GatherBuilder<TModel>> gather)
        {
            var builder = new GatherBuilder<TModel>(_context);
            gather(builder);
            _gather = builder.Fields;
            return this;
        }

        public HttpRequestBuilder<TModel> AsJson() { _transport = "json"; return this; }
        public HttpRequestBuilder<TModel> AsFormData() { _transport = "form-data"; return this; }

        public HttpRequestBuilder<TModel> WhileLoading(Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            _whileLoading = pb.Steps;
            return this;
        }

        public HttpRequestBuilder<TModel> Validate<TValidator>(string formId)
            where TValidator : class
        {
            _validatorType = typeof(TValidator);
            _container = formId;
            return this;
        }

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

            if (_gather != null && _gather.Count > 0)
                request.Input = new GatherInput(_gather, _transport);

            if (_container != null)
                request.Container = _container;

            if (_whileLoading != null && _whileLoading.Count > 0)
                request.Before = _whileLoading;

            if (_response != null)
            {
                if (_response.SuccessHandlers.Count > 0)
                    request.Success = _response.SuccessHandlers;
                if (_response.ErrorHandlers.Count > 0)
                    request.Error = _response.ErrorHandlers;
                if (_response.ChainedRequest != null)
                    request.Next = _response.ChainedRequest;
            }

            // Validator type stored for resolution at Render() time
            if (_validatorType != null)
                request.ValidatorType = _validatorType;

            return request;
        }
    }
}
