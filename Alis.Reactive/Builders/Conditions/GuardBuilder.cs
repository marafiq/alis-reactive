using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Descriptors.Guards;
using Alis.Reactive.Descriptors.Reactions;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// Composes guard conditions with <c>And</c>, <c>Or</c>, and <c>Not</c>, then terminates
    /// with <c>Then</c> to define the commands that execute when the guard passes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Reached after calling an operator on <see cref="ConditionSourceBuilder{TModel, TProp}"/>.
    /// From here you can:
    /// </para>
    /// <para>
    /// <b>Compose:</b> chain additional conditions with <c>And</c> or <c>Or</c> (both direct-source
    /// and lambda forms), or invert with <c>Not</c>.
    /// </para>
    /// <para>
    /// <b>Terminate:</b> call <c>Then</c> to define the branch body, which returns a
    /// <see cref="BranchBuilder{TModel}"/> for <c>ElseIf</c>/<c>Else</c> chaining.
    /// </para>
    /// <code>
    /// // Compound guard with And (both conditions must pass):
    /// p.When(args, x =&gt; x.Score).Gt(0)
    ///  .And(args, x =&gt; x.Score).Lt(100)
    ///  .Then(t =&gt; t.Element("valid-range").Show());
    ///
    /// // Lambda And for mixed boolean logic (And + Or):
    /// p.When(args, x =&gt; x.Score).Gte(90)
    ///  .And(cs =&gt; cs.When(args, x =&gt; x.Role).Eq("admin")
    ///               .Or(args, x =&gt; x.Role).Eq("nurse"))
    ///  .Then(t =&gt; t.Element("badge").Show());
    ///
    /// // Not (invert any guard):
    /// p.When(args, x =&gt; x.Status).Eq("archived").Not()
    ///  .Then(t =&gt; t.Element("edit-btn").Show());
    /// </code>
    /// </remarks>
    /// <typeparam name="TModel">The view model type.</typeparam>
    public sealed class GuardBuilder<TModel> where TModel : class
    {
        /// <summary>
        /// Gets the guard descriptor built by the operator chain.
        /// </summary>
        internal Guard Guard { get; }

        // Back-references — only one is set depending on the entry path.
        private readonly PipelineBuilder<TModel>? _pipeline;
        private readonly BranchBuilder<TModel>? _branchBuilder;

        /// <summary>
        /// NEVER make public. Constructed by <see cref="ConditionSourceBuilder{TModel, TProp}"/>
        /// operator methods when a pipeline context is available.
        /// </summary>
        internal GuardBuilder(Guard guard, PipelineBuilder<TModel> pipeline)
        {
            Guard = guard;
            _pipeline = pipeline;
        }

        /// <summary>
        /// NEVER make public. Constructed by <see cref="ConditionSourceBuilder{TModel, TProp}"/>
        /// operator methods when continuing an <c>ElseIf</c> chain.
        /// </summary>
        internal GuardBuilder(Guard guard, BranchBuilder<TModel> branchBuilder)
        {
            Guard = guard;
            _branchBuilder = branchBuilder;
        }

        /// <summary>
        /// NEVER make public. Constructed by <see cref="ConditionSourceBuilder{TModel, TProp}"/>
        /// operator methods inside <c>And</c>/<c>Or</c> lambdas (no pipeline context).
        /// </summary>
        internal GuardBuilder(Guard guard)
        {
            Guard = guard;
        }

        // ── Direct And/Or (flat composition from event args) ──

        /// <summary>
        /// Adds an AND condition from an event payload property. Both this guard and the
        /// new operator must pass for the branch to execute.
        /// </summary>
        /// <typeparam name="TPayload">The event args type.</typeparam>
        /// <typeparam name="TProp">The property type, inferred from the expression.</typeparam>
        /// <param name="payload">The event args instance (type inference only).</param>
        /// <param name="path">Expression selecting the property to test.</param>
        /// <returns>A <see cref="ConditionSourceBuilder{TModel, TProp}"/> for applying the next operator.</returns>
        public ConditionSourceBuilder<TModel, TProp> And<TPayload, TProp>(
            TPayload payload, Expression<Func<TPayload, TProp>> path)
        {
            var source = new EventArgSource<TPayload, TProp>(path);
            return new ConditionSourceBuilder<TModel, TProp>(
                source, CompositionMode.All, Guard, _pipeline, _branchBuilder);
        }

        /// <summary>
        /// Adds an OR condition from an event payload property. Either this guard or the
        /// new operator must pass for the branch to execute.
        /// </summary>
        /// <typeparam name="TPayload">The event args type.</typeparam>
        /// <typeparam name="TProp">The property type, inferred from the expression.</typeparam>
        /// <param name="payload">The event args instance (type inference only).</param>
        /// <param name="path">Expression selecting the property to test.</param>
        /// <returns>A <see cref="ConditionSourceBuilder{TModel, TProp}"/> for applying the next operator.</returns>
        public ConditionSourceBuilder<TModel, TProp> Or<TPayload, TProp>(
            TPayload payload, Expression<Func<TPayload, TProp>> path)
        {
            var source = new EventArgSource<TPayload, TProp>(path);
            return new ConditionSourceBuilder<TModel, TProp>(
                source, CompositionMode.Any, Guard, _pipeline, _branchBuilder);
        }

        // ── Direct And/Or with TypedSource (component value) ──

        /// <summary>
        /// Adds an AND condition from a component's current value. Both this guard and the
        /// new operator must pass for the branch to execute.
        /// </summary>
        /// <typeparam name="TProp">The component's value type.</typeparam>
        /// <param name="source">A typed source from a component's <c>Value()</c> extension.</param>
        /// <returns>A <see cref="ConditionSourceBuilder{TModel, TProp}"/> for applying the next operator.</returns>
        public ConditionSourceBuilder<TModel, TProp> And<TProp>(TypedSource<TProp> source)
        {
            return new ConditionSourceBuilder<TModel, TProp>(
                source, CompositionMode.All, Guard, _pipeline, _branchBuilder);
        }

        /// <summary>
        /// Adds an OR condition from a component's current value. Either this guard or the
        /// new operator must pass for the branch to execute.
        /// </summary>
        /// <typeparam name="TProp">The component's value type.</typeparam>
        /// <param name="source">A typed source from a component's <c>Value()</c> extension.</param>
        /// <returns>A <see cref="ConditionSourceBuilder{TModel, TProp}"/> for applying the next operator.</returns>
        public ConditionSourceBuilder<TModel, TProp> Or<TProp>(TypedSource<TProp> source)
        {
            return new ConditionSourceBuilder<TModel, TProp>(
                source, CompositionMode.Any, Guard, _pipeline, _branchBuilder);
        }

        // ── Lambda And/Or (for complex nesting) ──

        /// <summary>
        /// Combines this guard with an inner guard group using AND. All guards must pass.
        /// </summary>
        /// <remarks>
        /// Use the lambda form when mixing <c>And</c> and <c>Or</c> in the same expression.
        /// The inner lambda receives a <see cref="ConditionStart{TModel}"/> to build a
        /// nested guard tree:
        /// <code>
        /// p.When(args, x =&gt; x.Score).Gte(90)
        ///  .And(cs =&gt; cs.When(args, x =&gt; x.Role).Eq("admin")
        ///               .Or(args, x =&gt; x.Role).Eq("nurse"))
        ///  .Then(t =&gt; t.Element("badge").Show());
        /// </code>
        /// Chained <c>.And().And()</c> calls flatten into a single guard group rather
        /// than nesting.
        /// </remarks>
        /// <param name="inner">
        /// A lambda that builds the inner guard via <see cref="ConditionStart{TModel}.When{TPayload, TProp}"/>.
        /// </param>
        /// <returns>This builder with the combined guard for further composition or <c>Then</c>.</returns>
        public GuardBuilder<TModel> And(
            Func<ConditionStart<TModel>, GuardBuilder<TModel>> inner)
        {
            var innerResult = inner(new ConditionStart<TModel>());
            var guards = new List<Guard>();
            FlattenAllStatic(Guard, guards);
            FlattenAllStatic(innerResult.Guard, guards);
            return WrapGuard(new AllGuard(guards));
        }

        /// <summary>
        /// Combines this guard with an inner guard group using OR. Any guard may pass.
        /// </summary>
        /// <remarks>
        /// Use the lambda form when mixing <c>And</c> and <c>Or</c> in the same expression.
        /// Chained <c>.Or().Or()</c> calls flatten into a single guard group rather
        /// than nesting.
        /// </remarks>
        /// <param name="inner">
        /// A lambda that builds the inner guard via <see cref="ConditionStart{TModel}.When{TPayload, TProp}"/>.
        /// </param>
        /// <returns>This builder with the combined guard for further composition or <c>Then</c>.</returns>
        public GuardBuilder<TModel> Or(
            Func<ConditionStart<TModel>, GuardBuilder<TModel>> inner)
        {
            var innerResult = inner(new ConditionStart<TModel>());
            var guards = new List<Guard>();
            FlattenAnyStatic(Guard, guards);
            FlattenAnyStatic(innerResult.Guard, guards);
            return WrapGuard(new AnyGuard(guards));
        }

        // ── Not ──

        /// <summary>
        /// Inverts the current guard (logical NOT). The branch executes only when
        /// the guard does not pass.
        /// </summary>
        /// <returns>This builder with the inverted guard for further composition or <c>Then</c>.</returns>
        public GuardBuilder<TModel> Not()
        {
            return WrapGuard(new InvertGuard(Guard));
        }

        // ── Then ──

        /// <summary>
        /// Terminates the guard chain and defines the commands that execute when the
        /// guard passes.
        /// </summary>
        /// <remarks>
        /// The returned <see cref="BranchBuilder{TModel}"/> supports <c>ElseIf</c> and <c>Else</c>
        /// to add additional branches:
        /// <code>
        /// p.When(args, x =&gt; x.Value).Eq("A")
        ///  .Then(t =&gt; t.Element("label").SetText("Alpha"))
        ///  .ElseIf(args, x =&gt; x.Value).Eq("B")
        ///  .Then(t =&gt; t.Element("label").SetText("Bravo"))
        ///  .Else(e =&gt; e.Element("label").SetText("Unknown"));
        /// </code>
        /// </remarks>
        /// <param name="pipeline">
        /// Builds the pipeline commands for this branch. The callback receives a fresh
        /// <see cref="PipelineBuilder{TModel}"/> scoped to this branch.
        /// </param>
        /// <returns>
        /// A <see cref="BranchBuilder{TModel}"/> for chaining <c>ElseIf</c> or <c>Else</c> branches.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <c>Then</c> is called without a pipeline context (e.g. inside a
        /// standalone lambda not connected to a <c>When</c> call).
        /// </exception>
        public BranchBuilder<TModel> Then(Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>();
            pipeline(pb);
            var reaction = pb.BuildReaction();
            var branch = new Branch(Guard, reaction);

            if (_branchBuilder != null)
            {
                _branchBuilder.AddBranch(branch);
                return _branchBuilder;
            }

            if (_pipeline == null)
                throw new InvalidOperationException(
                    "Then() requires a pipeline context. Use PipelineBuilder.When() or BranchBuilder.ElseIf() to start a condition.");

            var branches = new List<Branch> { branch };
            _pipeline.SetConditionalBranches(branches);
            return new BranchBuilder<TModel>(_pipeline, branches);
        }

        internal GuardBuilder<TModel> WrapGuard(Guard combined)
        {
            if (_pipeline != null)
                return new GuardBuilder<TModel>(combined, _pipeline);
            if (_branchBuilder != null)
                return new GuardBuilder<TModel>(combined, _branchBuilder);
            return new GuardBuilder<TModel>(combined);
        }

        internal static void FlattenAllStatic(Guard guard, List<Guard> target)
        {
            if (guard is AllGuard all) target.AddRange(all.Guards);
            else target.Add(guard);
        }

        internal static void FlattenAnyStatic(Guard guard, List<Guard> target)
        {
            if (guard is AnyGuard any) target.AddRange(any.Guards);
            else target.Add(guard);
        }
    }
}
