using System;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>A condition built so far: what a group callback returns.</summary>
    /// <typeparam name="TModel">View model that owns the condition.</typeparam>
    public abstract class Condition<TModel> where TModel : class
    {
        private protected Condition(ConditionGraph graph)
        {
            Graph = graph ?? throw new ArgumentNullException(nameof(graph));
        }

        internal ConditionGraph Graph { get; }
    }

    /// <summary>
    /// One comparison in a pipeline: continue with <c>And</c> or <c>Or</c>, negate it with <c>Not</c>,
    /// or run reactions with <c>Then</c>.
    /// </summary>
    /// <remarks>
    /// <c>And</c> and <c>Or</c> do not mix in one chain: after <c>And</c> only <c>And</c> follows, and
    /// after <c>Or</c> only <c>Or</c>. A mixed condition takes a group, so its precedence is written:
    /// <c>p.When(a).Eq(1).And(g =&gt; g.When(b).Gt(2).Or(c).Truthy()).Then(..)</c>.
    /// </remarks>
    /// <typeparam name="TModel">View model that owns the guarded pipeline.</typeparam>
    public sealed class GuardBuilder<TModel> : Condition<TModel> where TModel : class
    {
        private readonly ConditionContinuation<TModel> _continuation;

        internal GuardBuilder(ConditionGraph graph, ConditionContinuation<TModel> continuation) : base(graph)
        {
            _continuation = continuation ?? throw new ArgumentNullException(nameof(continuation));
        }

        /// <summary>Also requires a comparison of the triggering event's payload.</summary>
        public ConditionSourceBuilder<TModel, TProp, AllGuard<TModel>> And<TPayload, TProp>(
            TPayload payload, Expression<Func<TPayload, TProp>> path) =>
            And(PayloadTypedSource<TPayload, TProp>.FromEvent(path));

        /// <summary>Also requires a comparison of the response body.</summary>
        public ConditionSourceBuilder<TModel, TProp, AllGuard<TModel>> And<TResponse, TProp>(
            ResponseBody<TResponse> responseBody, Expression<Func<TResponse, TProp>> path)
            where TResponse : class =>
            And(responseBody.Read(path));

        /// <summary>Also requires a comparison of a value read when the condition runs.</summary>
        public ConditionSourceBuilder<TModel, TProp, AllGuard<TModel>> And<TProp>(TypedSource<TProp> source) =>
            new ConditionSourceBuilder<TModel, TProp, AllGuard<TModel>>(
                source, next => new AllGuard<TModel>(ConditionComposition.All(Graph, next), _continuation));

        /// <summary>Also requires a group, whose own <c>And</c> and <c>Or</c> are evaluated first.</summary>
        public AllGuard<TModel> And(Func<ConditionStart<TModel>, Condition<TModel>> group) =>
            new AllGuard<TModel>(ConditionComposition.All(Graph, ConditionComposition.Group(group)), _continuation);

        /// <summary>Alternatively accepts a comparison of the triggering event's payload.</summary>
        public ConditionSourceBuilder<TModel, TProp, AnyGuard<TModel>> Or<TPayload, TProp>(
            TPayload payload, Expression<Func<TPayload, TProp>> path) =>
            Or(PayloadTypedSource<TPayload, TProp>.FromEvent(path));

        /// <summary>Alternatively accepts a comparison of the response body.</summary>
        public ConditionSourceBuilder<TModel, TProp, AnyGuard<TModel>> Or<TResponse, TProp>(
            ResponseBody<TResponse> responseBody, Expression<Func<TResponse, TProp>> path)
            where TResponse : class =>
            Or(responseBody.Read(path));

        /// <summary>Alternatively accepts a comparison of a value read when the condition runs.</summary>
        public ConditionSourceBuilder<TModel, TProp, AnyGuard<TModel>> Or<TProp>(TypedSource<TProp> source) =>
            new ConditionSourceBuilder<TModel, TProp, AnyGuard<TModel>>(
                source, next => new AnyGuard<TModel>(ConditionComposition.Any(Graph, next), _continuation));

        /// <summary>Alternatively accepts a group, whose own <c>And</c> and <c>Or</c> are evaluated first.</summary>
        public AnyGuard<TModel> Or(Func<ConditionStart<TModel>, Condition<TModel>> group) =>
            new AnyGuard<TModel>(ConditionComposition.Any(Graph, ConditionComposition.Group(group)), _continuation);

        /// <summary>Negates this one comparison.</summary>
        public GuardBuilder<TModel> Not() => new GuardBuilder<TModel>(ConditionGraph.Not(Graph), _continuation);

        /// <summary>Runs <paramref name="pipeline"/> when the condition holds.</summary>
        /// <param name="pipeline">The reactions of the branch.</param>
        public BranchBuilder<TModel> Then(Action<PipelineBuilder<TModel>> pipeline)
        {
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
            return _continuation.Then(Graph, pipeline);
        }
    }

    /// <summary>Comparisons that must all hold: continue with <c>And</c>, or run reactions with <c>Then</c>.</summary>
    /// <typeparam name="TModel">View model that owns the guarded pipeline.</typeparam>
    public sealed class AllGuard<TModel> : Condition<TModel> where TModel : class
    {
        private readonly ConditionContinuation<TModel> _continuation;

        internal AllGuard(ConditionGraph graph, ConditionContinuation<TModel> continuation) : base(graph)
        {
            _continuation = continuation;
        }

        /// <summary>Also requires a comparison of the triggering event's payload.</summary>
        public ConditionSourceBuilder<TModel, TProp, AllGuard<TModel>> And<TPayload, TProp>(
            TPayload payload, Expression<Func<TPayload, TProp>> path) =>
            And(PayloadTypedSource<TPayload, TProp>.FromEvent(path));

        /// <summary>Also requires a comparison of the response body.</summary>
        public ConditionSourceBuilder<TModel, TProp, AllGuard<TModel>> And<TResponse, TProp>(
            ResponseBody<TResponse> responseBody, Expression<Func<TResponse, TProp>> path)
            where TResponse : class =>
            And(responseBody.Read(path));

        /// <summary>Also requires a comparison of a value read when the condition runs.</summary>
        public ConditionSourceBuilder<TModel, TProp, AllGuard<TModel>> And<TProp>(TypedSource<TProp> source) =>
            new ConditionSourceBuilder<TModel, TProp, AllGuard<TModel>>(
                source, next => new AllGuard<TModel>(ConditionComposition.All(Graph, next), _continuation));

        /// <summary>Also requires a group, whose own <c>And</c> and <c>Or</c> are evaluated first.</summary>
        public AllGuard<TModel> And(Func<ConditionStart<TModel>, Condition<TModel>> group) =>
            new AllGuard<TModel>(ConditionComposition.All(Graph, ConditionComposition.Group(group)), _continuation);

        /// <summary>Runs <paramref name="pipeline"/> when every comparison holds.</summary>
        /// <param name="pipeline">The reactions of the branch.</param>
        public BranchBuilder<TModel> Then(Action<PipelineBuilder<TModel>> pipeline)
        {
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
            return _continuation.Then(Graph, pipeline);
        }
    }

    /// <summary>Comparisons of which one must hold: continue with <c>Or</c>, or run reactions with <c>Then</c>.</summary>
    /// <typeparam name="TModel">View model that owns the guarded pipeline.</typeparam>
    public sealed class AnyGuard<TModel> : Condition<TModel> where TModel : class
    {
        private readonly ConditionContinuation<TModel> _continuation;

        internal AnyGuard(ConditionGraph graph, ConditionContinuation<TModel> continuation) : base(graph)
        {
            _continuation = continuation;
        }

        /// <summary>Alternatively accepts a comparison of the triggering event's payload.</summary>
        public ConditionSourceBuilder<TModel, TProp, AnyGuard<TModel>> Or<TPayload, TProp>(
            TPayload payload, Expression<Func<TPayload, TProp>> path) =>
            Or(PayloadTypedSource<TPayload, TProp>.FromEvent(path));

        /// <summary>Alternatively accepts a comparison of the response body.</summary>
        public ConditionSourceBuilder<TModel, TProp, AnyGuard<TModel>> Or<TResponse, TProp>(
            ResponseBody<TResponse> responseBody, Expression<Func<TResponse, TProp>> path)
            where TResponse : class =>
            Or(responseBody.Read(path));

        /// <summary>Alternatively accepts a comparison of a value read when the condition runs.</summary>
        public ConditionSourceBuilder<TModel, TProp, AnyGuard<TModel>> Or<TProp>(TypedSource<TProp> source) =>
            new ConditionSourceBuilder<TModel, TProp, AnyGuard<TModel>>(
                source, next => new AnyGuard<TModel>(ConditionComposition.Any(Graph, next), _continuation));

        /// <summary>Alternatively accepts a group, whose own <c>And</c> and <c>Or</c> are evaluated first.</summary>
        public AnyGuard<TModel> Or(Func<ConditionStart<TModel>, Condition<TModel>> group) =>
            new AnyGuard<TModel>(ConditionComposition.Any(Graph, ConditionComposition.Group(group)), _continuation);

        /// <summary>Runs <paramref name="pipeline"/> when any comparison holds.</summary>
        /// <param name="pipeline">The reactions of the branch.</param>
        public BranchBuilder<TModel> Then(Action<PipelineBuilder<TModel>> pipeline)
        {
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
            return _continuation.Then(Graph, pipeline);
        }
    }

    /// <summary>One comparison inside a group: continue with <c>And</c> or <c>Or</c>, or negate it with <c>Not</c>. A group has no <c>Then</c>.</summary>
    /// <typeparam name="TModel">View model that owns the condition.</typeparam>
    public sealed class GroupCondition<TModel> : Condition<TModel> where TModel : class
    {
        internal GroupCondition(ConditionGraph graph) : base(graph) { }

        /// <summary>Also requires a comparison of the triggering event's payload.</summary>
        public ConditionSourceBuilder<TModel, TProp, GroupAll<TModel>> And<TPayload, TProp>(
            TPayload payload, Expression<Func<TPayload, TProp>> path) =>
            And(PayloadTypedSource<TPayload, TProp>.FromEvent(path));

        /// <summary>Also requires a comparison of the response body.</summary>
        public ConditionSourceBuilder<TModel, TProp, GroupAll<TModel>> And<TResponse, TProp>(
            ResponseBody<TResponse> responseBody, Expression<Func<TResponse, TProp>> path)
            where TResponse : class =>
            And(responseBody.Read(path));

        /// <summary>Also requires a comparison of a value read when the condition runs.</summary>
        public ConditionSourceBuilder<TModel, TProp, GroupAll<TModel>> And<TProp>(TypedSource<TProp> source) =>
            new ConditionSourceBuilder<TModel, TProp, GroupAll<TModel>>(
                source, next => new GroupAll<TModel>(ConditionComposition.All(Graph, next)));

        /// <summary>Also requires a nested group.</summary>
        public GroupAll<TModel> And(Func<ConditionStart<TModel>, Condition<TModel>> group) =>
            new GroupAll<TModel>(ConditionComposition.All(Graph, ConditionComposition.Group(group)));

        /// <summary>Alternatively accepts a comparison of the triggering event's payload.</summary>
        public ConditionSourceBuilder<TModel, TProp, GroupAny<TModel>> Or<TPayload, TProp>(
            TPayload payload, Expression<Func<TPayload, TProp>> path) =>
            Or(PayloadTypedSource<TPayload, TProp>.FromEvent(path));

        /// <summary>Alternatively accepts a comparison of the response body.</summary>
        public ConditionSourceBuilder<TModel, TProp, GroupAny<TModel>> Or<TResponse, TProp>(
            ResponseBody<TResponse> responseBody, Expression<Func<TResponse, TProp>> path)
            where TResponse : class =>
            Or(responseBody.Read(path));

        /// <summary>Alternatively accepts a comparison of a value read when the condition runs.</summary>
        public ConditionSourceBuilder<TModel, TProp, GroupAny<TModel>> Or<TProp>(TypedSource<TProp> source) =>
            new ConditionSourceBuilder<TModel, TProp, GroupAny<TModel>>(
                source, next => new GroupAny<TModel>(ConditionComposition.Any(Graph, next)));

        /// <summary>Alternatively accepts a nested group.</summary>
        public GroupAny<TModel> Or(Func<ConditionStart<TModel>, Condition<TModel>> group) =>
            new GroupAny<TModel>(ConditionComposition.Any(Graph, ConditionComposition.Group(group)));

        /// <summary>Negates this one comparison.</summary>
        public GroupCondition<TModel> Not() => new GroupCondition<TModel>(ConditionGraph.Not(Graph));
    }

    /// <summary>Comparisons inside a group that must all hold: continue with <c>And</c>.</summary>
    /// <typeparam name="TModel">View model that owns the condition.</typeparam>
    public sealed class GroupAll<TModel> : Condition<TModel> where TModel : class
    {
        internal GroupAll(ConditionGraph graph) : base(graph) { }

        /// <summary>Also requires a comparison of the triggering event's payload.</summary>
        public ConditionSourceBuilder<TModel, TProp, GroupAll<TModel>> And<TPayload, TProp>(
            TPayload payload, Expression<Func<TPayload, TProp>> path) =>
            And(PayloadTypedSource<TPayload, TProp>.FromEvent(path));

        /// <summary>Also requires a comparison of the response body.</summary>
        public ConditionSourceBuilder<TModel, TProp, GroupAll<TModel>> And<TResponse, TProp>(
            ResponseBody<TResponse> responseBody, Expression<Func<TResponse, TProp>> path)
            where TResponse : class =>
            And(responseBody.Read(path));

        /// <summary>Also requires a comparison of a value read when the condition runs.</summary>
        public ConditionSourceBuilder<TModel, TProp, GroupAll<TModel>> And<TProp>(TypedSource<TProp> source) =>
            new ConditionSourceBuilder<TModel, TProp, GroupAll<TModel>>(
                source, next => new GroupAll<TModel>(ConditionComposition.All(Graph, next)));

        /// <summary>Also requires a nested group.</summary>
        public GroupAll<TModel> And(Func<ConditionStart<TModel>, Condition<TModel>> group) =>
            new GroupAll<TModel>(ConditionComposition.All(Graph, ConditionComposition.Group(group)));
    }

    /// <summary>Comparisons inside a group of which one must hold: continue with <c>Or</c>.</summary>
    /// <typeparam name="TModel">View model that owns the condition.</typeparam>
    public sealed class GroupAny<TModel> : Condition<TModel> where TModel : class
    {
        internal GroupAny(ConditionGraph graph) : base(graph) { }

        /// <summary>Alternatively accepts a comparison of the triggering event's payload.</summary>
        public ConditionSourceBuilder<TModel, TProp, GroupAny<TModel>> Or<TPayload, TProp>(
            TPayload payload, Expression<Func<TPayload, TProp>> path) =>
            Or(PayloadTypedSource<TPayload, TProp>.FromEvent(path));

        /// <summary>Alternatively accepts a comparison of the response body.</summary>
        public ConditionSourceBuilder<TModel, TProp, GroupAny<TModel>> Or<TResponse, TProp>(
            ResponseBody<TResponse> responseBody, Expression<Func<TResponse, TProp>> path)
            where TResponse : class =>
            Or(responseBody.Read(path));

        /// <summary>Alternatively accepts a comparison of a value read when the condition runs.</summary>
        public ConditionSourceBuilder<TModel, TProp, GroupAny<TModel>> Or<TProp>(TypedSource<TProp> source) =>
            new ConditionSourceBuilder<TModel, TProp, GroupAny<TModel>>(
                source, next => new GroupAny<TModel>(ConditionComposition.Any(Graph, next)));

        /// <summary>Alternatively accepts a nested group.</summary>
        public GroupAny<TModel> Or(Func<ConditionStart<TModel>, Condition<TModel>> group) =>
            new GroupAny<TModel>(ConditionComposition.Any(Graph, ConditionComposition.Group(group)));
    }
}
