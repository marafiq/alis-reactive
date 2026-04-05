using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    public sealed class BranchBuilder<TModel> where TModel : class
    {
        private readonly List<BranchCase> _cases;
        private bool _elseCalled;

        internal PipelineBuilder<TModel> Pipeline { get; }

        internal BranchBuilder(PipelineBuilder<TModel> pipeline, List<BranchCase> cases)
        {
            Pipeline = pipeline;
            _cases = cases;
        }

        public ConditionSourceBuilder<TModel, TProp> ElseIf<TPayload, TProp>(
            TPayload payload,
            Expression<Func<TPayload, TProp>> path)
        {
            if (_elseCalled)
                throw new InvalidOperationException("Cannot add ElseIf after Else.");

            var source = new EventArgSource<TPayload, TProp>(path);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        public ConditionSourceBuilder<TModel, TProp> ElseIf<TProp>(TypedSource<TProp> source)
        {
            if (_elseCalled)
                throw new InvalidOperationException("Cannot add ElseIf after Else.");

            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        public void Else(Action<PipelineBuilder<TModel>> pipeline)
        {
            if (_elseCalled)
                throw new InvalidOperationException("Else already called.");

            var pb = new PipelineBuilder<TModel>(Pipeline.Context);
            pipeline(pb);
            _cases.Add(BranchCase.Default(pb.BuildReaction()));
            _elseCalled = true;
        }

        internal void AddBranch(BranchCase branchCase)
        {
            if (_elseCalled)
                throw new InvalidOperationException("Cannot add branches after Else.");
            _cases.Add(branchCase);
        }
    }
}
