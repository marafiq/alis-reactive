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
        private GatherBuilder<TModel> _gatherBuilder;
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
            _gatherBuilder = builder;
            return this;
        }

        public HttpRequestBuilder<TModel> AsJson() { _transport = "json"; return this; }
        public HttpRequestBuilder<TModel> AsFormData() { _transport = "form-data"; return this; }

        public HttpRequestBuilder<TModel> WhileLoading(Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            var reaction = pb.BuildReaction();
            if (!(reaction is SequenceReaction))
                throw new InvalidOperationException(
                    "WhileLoading only supports plain commands (sequential). " +
                    "Conditions, HTTP, and parallel pipelines are not valid here.");
            _whileLoading = new List<Reaction>(((SequenceReaction)reaction).Steps);
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

            if (_gatherBuilder != null)
            {
                // Expand IncludeAll: add a GatherField for every registered input component
                // with explicit bindingPath and shape — the plan carries all information.
                if (_gatherBuilder.IsIncludeAll)
                {
                    var registered = _context.GetRegisteredComponents();
                    foreach (var kvp in registered)
                    {
                        var reg = kvp.Value;
                        if (!_gatherBuilder.Fields.Exists(f => f.Component == reg.ComponentId))
                            _gatherBuilder.AddField(GatherField.Of(reg.ComponentId, kvp.Key, kvp.Key, reg.Shape));
                    }
                }

                // Build request input from gather fields + static fields + event fields.
                // When both component and static/event fields exist, merge statics into
                // a ValueInput object that the runtime emits alongside gathered components.
                if (_gatherBuilder.Fields.Count > 0)
                {
                    ValueProducer statics = null;
                    if (_gatherBuilder.StaticFields.Count > 0 || _gatherBuilder.EventFields.Count > 0)
                    {
                        var fields = new Dictionary<string, ValueProducer>();
                        foreach (var sf in _gatherBuilder.StaticFields)
                            fields[sf.Key] = ValueProducer.LiteralRaw(sf.Value, Shape.FromClrType(sf.Value?.GetType()));
                        foreach (var ef in _gatherBuilder.EventFields)
                            fields[ef.Key] = ValueProducer.Read(PayloadSource.Event(), ef.EventPath);
                        statics = ValueProducer.Object(fields);
                    }
                    var gatherInput = new GatherInput(_gatherBuilder.Fields, _transport, statics);
                    if (_gatherBuilder.IsIncludeAll) gatherInput.IncludeAll = true;
                    request.Input = gatherInput;
                }

                // Static and event fields become a ValueInput when no component fields
                if (request.Input == null && (_gatherBuilder.StaticFields.Count > 0 || _gatherBuilder.EventFields.Count > 0))
                {
                    var fields = new Dictionary<string, ValueProducer>();
                    foreach (var sf in _gatherBuilder.StaticFields)
                        fields[sf.Key] = ValueProducer.LiteralRaw(sf.Value, Shape.FromClrType(sf.Value?.GetType()));
                    foreach (var ef in _gatherBuilder.EventFields)
                        fields[ef.Key] = ValueProducer.Read(PayloadSource.Event(), ef.EventPath);
                    request.Input = new ValueInput(ValueProducer.Object(fields), _transport);
                }
            }

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

            if (_validatorType != null)
                request.ValidatorType = _validatorType;

            return request;
        }
    }
}
