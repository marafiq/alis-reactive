using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    public partial class PipelineBuilder<TModel> where TModel : class
    {
        /// <summary>Starts a branch whose guard reads from a typed value source.</summary>
        /// <typeparam name="TProp">Source value type.</typeparam>
        /// <param name="source">Typed condition value source.</param>
        public ConditionSourceBuilder<TModel, TProp, GuardBuilder<TModel>> When<TProp>(TypedSource<TProp> source)
        {
            var continuation = ConditionContinuation<TModel>.InCallback(Context);
            return new ConditionSourceBuilder<TModel, TProp, GuardBuilder<TModel>>(
                source, graph => new GuardBuilder<TModel>(graph, continuation));
        }

        /// <summary>Adds a user-decision guard before the pipeline continues.</summary>
        /// <param name="message">User-decision confirmation message.</param>
        public GuardBuilder<TModel> Confirm(string message) =>
            new GuardBuilder<TModel>(ConditionGraph.Confirm(message), ConditionContinuation<TModel>.InCallback(Context));
    }
}
