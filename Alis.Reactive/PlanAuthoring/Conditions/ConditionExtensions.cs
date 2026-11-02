using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// The conditions a pipeline branches on. Each extends <see cref="PipelineBuilder{TModel}"/>, so the
    /// pipeline knows nothing of conditions and the condition builders depend on it.
    /// </summary>
    public static class ConditionExtensions
    {
        /// <summary>Starts a branch whose guard reads from a typed value source.</summary>
        /// <typeparam name="TModel">The view model.</typeparam>
        /// <typeparam name="TProp">Source value type.</typeparam>
        /// <param name="pipeline">The pipeline the branch is written in.</param>
        /// <param name="source">Typed condition value source.</param>
        public static ConditionSourceBuilder<TModel, TProp, GuardBuilder<TModel>> When<TModel, TProp>(
            this PipelineBuilder<TModel> pipeline, TypedSource<TProp> source) where TModel : class
        {
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
            var continuation = ConditionContinuation<TModel>.InCallback(pipeline.Context);
            return new ConditionSourceBuilder<TModel, TProp, GuardBuilder<TModel>>(
                source, graph => new GuardBuilder<TModel>(graph, continuation));
        }

        /// <summary>Adds a user-decision guard before the pipeline continues.</summary>
        /// <typeparam name="TModel">The view model.</typeparam>
        /// <param name="pipeline">The pipeline the guard is written in.</param>
        /// <param name="message">User-decision confirmation message.</param>
        public static GuardBuilder<TModel> Confirm<TModel>(this PipelineBuilder<TModel> pipeline, string message) where TModel : class
        {
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
            return new GuardBuilder<TModel>(Condition.Confirm(message), ConditionContinuation<TModel>.InCallback(pipeline.Context));
        }
    }
}
