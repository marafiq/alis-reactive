using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Descriptors.Reactions;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// Chains additional branches after an initial <c>When().Then()</c> block using
    /// <c>ElseIf</c> and <c>Else</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returned by <see cref="GuardBuilder{TModel}.Then"/>. Branches evaluate in declaration
    /// order: the first branch whose guard passes wins. <c>Else</c> is the fallback when no
    /// guard passes and must be the last branch.
    /// </para>
    /// <code>
    /// p.When(args, x =&gt; x.Score).Gte(90)
    ///  .Then(t =&gt; t.Element("grade").SetText("A"))
    ///  .ElseIf(args, x =&gt; x.Score).Gte(80)
    ///  .Then(t =&gt; t.Element("grade").SetText("B"))
    ///  .ElseIf(args, x =&gt; x.Score).Gte(70)
    ///  .Then(t =&gt; t.Element("grade").SetText("C"))
    ///  .Else(e =&gt; e.Element("grade").SetText("F"));
    /// </code>
    /// </remarks>
    /// <typeparam name="TModel">The view model type.</typeparam>
    public sealed class BranchBuilder<TModel> where TModel : class
    {
        private readonly List<Branch> _branches;
        private bool _elseCalled;

        /// <summary>
        /// Gets the pipeline this condition block belongs to.
        /// </summary>
        internal PipelineBuilder<TModel> Pipeline { get; }

        /// <summary>
        /// NEVER make public. Constructed by <see cref="GuardBuilder{TModel}.Then"/> when
        /// the first branch is created.
        /// </summary>
        internal BranchBuilder(PipelineBuilder<TModel> pipeline, List<Branch> branches)
        {
            Pipeline = pipeline;
            _branches = branches;
        }

        /// <summary>
        /// Adds a conditional branch that tests a property from the event payload.
        /// </summary>
        /// <typeparam name="TPayload">The event args type.</typeparam>
        /// <typeparam name="TProp">The property type, inferred from the expression.</typeparam>
        /// <param name="payload">The event args instance (type inference only).</param>
        /// <param name="path">Expression selecting the property to test.</param>
        /// <returns>A <see cref="ConditionSourceBuilder{TModel, TProp}"/> for applying an operator, then <c>Then</c>.</returns>
        /// <exception cref="InvalidOperationException">Thrown when called after <see cref="Else"/>.</exception>
        public ConditionSourceBuilder<TModel, TProp> ElseIf<TPayload, TProp>(
            TPayload payload,
            Expression<Func<TPayload, TProp>> path)
        {
            if (_elseCalled)
                throw new InvalidOperationException(
                    "Cannot add ElseIf after Else. Else must be the last branch.");

            var source = new EventArgSource<TPayload, TProp>(path);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>
        /// Adds a conditional branch that tests a component's current value.
        /// </summary>
        /// <typeparam name="TProp">The component's value type.</typeparam>
        /// <param name="source">A typed source from a component's <c>Value()</c> extension.</param>
        /// <returns>A <see cref="ConditionSourceBuilder{TModel, TProp}"/> for applying an operator, then <c>Then</c>.</returns>
        /// <exception cref="InvalidOperationException">Thrown when called after <see cref="Else"/>.</exception>
        public ConditionSourceBuilder<TModel, TProp> ElseIf<TProp>(TypedSource<TProp> source)
        {
            if (_elseCalled)
                throw new InvalidOperationException(
                    "Cannot add ElseIf after Else. Else must be the last branch.");

            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>
        /// Adds the fallback branch that executes when no previous guard passes. Must be the
        /// last branch. Only one <c>Else</c> is allowed per condition block.
        /// </summary>
        /// <param name="configure">
        /// Configures the pipeline commands for the fallback branch. The callback receives
        /// a fresh <see cref="PipelineBuilder{TModel}"/> scoped to this branch.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <c>Else</c> has already been called on this condition block.
        /// </exception>
        public void Else(Action<PipelineBuilder<TModel>> configure)
        {
            if (_elseCalled)
                throw new InvalidOperationException(
                    "Else has already been called. Only one Else branch is allowed and it must be last.");

            var pb = new PipelineBuilder<TModel>();
            configure(pb);
            var reaction = pb.BuildReaction();
            _branches.Add(new Branch(null, reaction));
            _elseCalled = true;
        }

        /// <summary>
        /// Adds a branch to the shared list. Called by <see cref="GuardBuilder{TModel}.Then"/>
        /// when continuing an existing branch chain.
        /// </summary>
        internal void AddBranch(Branch branch)
        {
            if (_elseCalled)
                throw new InvalidOperationException(
                    "Cannot add branches after Else. Else must be the last branch.");

            _branches.Add(branch);
        }
    }
}
