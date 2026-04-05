using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    public class ResponseBuilder<TModel> where TModel : class
    {
        private readonly PlanBuildContext _context;
        internal List<ResponseHandler> SuccessHandlers { get; } = new List<ResponseHandler>();
        internal List<ResponseHandler> ErrorHandlers { get; } = new List<ResponseHandler>();
        internal Request ChainedRequest { get; private set; }

        internal ResponseBuilder(PlanBuildContext context)
        {
            _context = context;
        }

        public ResponseBuilder<TModel> OnSuccess(Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            SuccessHandlers.Add(new ResponseHandler(pb.BuildReaction()));
            return this;
        }

        public ResponseBuilder<TModel> OnSuccess<TResponse>(
            Action<ResponseBody<TResponse>, PipelineBuilder<TModel>> pipeline)
            where TResponse : class, new()
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(new ResponseBody<TResponse>(new TResponse()), pb);
            SuccessHandlers.Add(new ResponseHandler(pb.BuildReaction()));
            return this;
        }

        public ResponseBuilder<TModel> OnError(int statusCode, Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            var handler = new ResponseHandler(pb.BuildReaction()) { Status = statusCode };
            ErrorHandlers.Add(handler);
            return this;
        }

        public ResponseBuilder<TModel> Chained(Action<HttpRequestBuilder<TModel>> request)
        {
            var chainedBuilder = new HttpRequestBuilder<TModel>(_context);
            request(chainedBuilder);
            ChainedRequest = chainedBuilder.BuildRequest();
            return this;
        }
    }
}
