using System;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// Starts a group inside <c>And(g =&gt; ..)</c> or <c>Or(g =&gt; ..)</c>. A group is evaluated as one
    /// term and has no <c>Then</c>.
    /// </summary>
    /// <typeparam name="TModel">View model that owns the condition being authored.</typeparam>
    public sealed class ConditionStart<TModel> where TModel : class
    {
        internal ConditionStart() { }

        /// <summary>Starts the group with a comparison of the triggering event's payload.</summary>
        public ConditionSourceBuilder<TModel, TProp, GroupCondition<TModel>> When<TPayload, TProp>(
            TPayload payload,
            Expression<Func<TPayload, TProp>> path) =>
            When(PayloadTypedSource<TPayload, TProp>.FromEvent(path));

        /// <summary>Starts the group with a comparison of the response body.</summary>
        public ConditionSourceBuilder<TModel, TProp, GroupCondition<TModel>> When<TResponse, TProp>(
            ResponseBody<TResponse> responseBody,
            Expression<Func<TResponse, TProp>> path)
            where TResponse : class =>
            When(responseBody.Read(path));

        /// <summary>Starts the group with a comparison of a value read when the condition runs.</summary>
        public ConditionSourceBuilder<TModel, TProp, GroupCondition<TModel>> When<TProp>(TypedSource<TProp> source) =>
            new ConditionSourceBuilder<TModel, TProp, GroupCondition<TModel>>(source, graph => new GroupCondition<TModel>(graph));

        /// <summary>Asks the user, and holds when they confirm.</summary>
        /// <param name="message">The question.</param>
        public GroupCondition<TModel> Confirm(string message) =>
            new GroupCondition<TModel>(ConditionGraph.Confirm(message));

        /// <summary>Holds when the nested group does not: <c>g.Not(n =&gt; n.When(a).Eq(1).And(b).Truthy())</c>.</summary>
        /// <param name="group">The group to negate.</param>
        public GroupCondition<TModel> Not(Func<ConditionStart<TModel>, Condition<TModel>> group) =>
            new GroupCondition<TModel>(ConditionGraph.Not(ConditionComposition.Group(group)));
    }
}
