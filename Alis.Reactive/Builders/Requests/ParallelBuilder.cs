using System;
using System.Collections.Generic;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Descriptors.Reactions;
using Alis.Reactive.Descriptors.Requests;

namespace Alis.Reactive.Builders.Requests
{
    public class ParallelBuilder<TModel> where TModel : class
    {
        private readonly List<RequestDescriptor> _branches = new List<RequestDescriptor>();
        private ResponseBuilder<TModel>? _response;

        internal void AddBranch(Action<HttpRequestBuilder<TModel>> configure)
        {
            var builder = new HttpRequestBuilder<TModel>();
            configure(builder);
            _branches.Add(builder.BuildRequestDescriptor());
        }

        /// <summary>
        /// Configures handlers that fire after ALL parallel requests complete successfully.
        /// </summary>
        public ParallelBuilder<TModel> Response(Action<ResponseBuilder<TModel>> configure)
        {
            var builder = new ResponseBuilder<TModel>();
            configure(builder);
            _response = builder;
            return this;
        }

        internal ParallelHttpReaction BuildReaction(List<Command>? preFetch)
        {
            return new ParallelHttpReaction(
                preFetch,
                _branches,
                _response?.SuccessHandlers.Count > 0 ? _response.SuccessHandlers : null);
        }
    }
}
