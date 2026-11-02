using System;
using System.Linq.Expressions;
using Alis.Reactive.PlanAuthoring.Conditions;

namespace Alis.Reactive
{
    /// <summary>
    /// Starts a group inside <c>And(g =&gt; ..)</c> or <c>Or(g =&gt; ..)</c>. A group is evaluated as one
    /// term and has no <c>Then</c>.
    /// </summary>
    /// <typeparam name="TModel">View model that owns the condition being authored.</typeparam>
    public sealed class ConditionStart<TModel> where TModel : class
    {
        internal ConditionStart() { }

        /// <summary>Starts the group with a comparison of a value read when the condition runs.</summary>
        public ConditionSourceBuilder<TModel, TProp, GroupGuard<TModel>> When<TProp>(TypedSource<TProp> source) =>
            new ConditionSourceBuilder<TModel, TProp, GroupGuard<TModel>>(source, graph => new GroupGuard<TModel>(graph));

        /// <summary>Asks the user, and holds when they confirm.</summary>
        /// <param name="message">The question.</param>
        public GroupGuard<TModel> Confirm(string message) =>
            new GroupGuard<TModel>(Condition.Confirm(message));

        /// <summary>Holds when the nested group does not: <c>g.Not(n =&gt; n.When(a).Eq(1).And(b).Truthy())</c>.</summary>
        /// <param name="group">The group to negate.</param>
        public GroupGuard<TModel> Not(Func<ConditionStart<TModel>, Guard<TModel>> group) =>
            new GroupGuard<TModel>(Condition.Not(ConditionComposition.Group(group)));
    }
}
