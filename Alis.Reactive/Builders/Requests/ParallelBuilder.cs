using System;
using System.Collections.Generic;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Descriptors.Reactions;
using Alis.Reactive.Descriptors.Requests;

namespace Alis.Reactive.Builders.Requests
{
    /// <summary>
    /// Builds a parallel HTTP reaction — multiple requests fire concurrently.
    /// Each branch owns its own response chain. OnAllSettled fires after all branches complete.
    /// </summary>
    public class ParallelBuilder<TModel> where TModel : class
    {
        private readonly List<RequestDescriptor> _branches = new List<RequestDescriptor>();
        private PipelineBuilder<TModel>? _onAllSettled;

        internal void AddBranch(Action<HttpRequestBuilder<TModel>> configure)
        {
            var builder = new HttpRequestBuilder<TModel>();
            configure(builder);
            _branches.Add(builder.BuildRequestDescriptor());
        }

        /// <summary>
        /// Commands to execute after all parallel requests complete, regardless of individual success or failure.
        /// </summary>
        public ParallelBuilder<TModel> OnAllSettled(Action<PipelineBuilder<TModel>> configure)
        {
            var pb = new PipelineBuilder<TModel>();
            configure(pb);
            var reaction = pb.BuildReaction();
            if (!(reaction is SequentialReaction))
                throw new InvalidOperationException(
                    "OnAllSettled only supports plain commands (sequential). " +
                    "Conditions, HTTP, and parallel pipelines are not valid here.");
            _onAllSettled = pb;
            return this;
        }

        internal ParallelHttpReaction BuildReaction(List<Command>? preFetch)
        {
            return new ParallelHttpReaction(
                preFetch,
                _branches,
                _onAllSettled?.Commands.Count > 0 ? _onAllSettled.Commands : null);
        }
    }
}
