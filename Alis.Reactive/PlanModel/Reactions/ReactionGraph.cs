using System;
using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Wire base for Reactive Plan reactions authored through pipeline builders.
    /// </summary>
    internal abstract class ReactionGraph
    {
        private protected ReactionGraph() { }

        internal static ReactionGraph Sequence(List<ReactionGraph> steps) =>
            new SequenceReaction(steps);

        internal static ReactionGraph Parallel(List<ReactionGraph> steps, ParallelCompletion completion) =>
            new ParallelReaction(steps, completion);

        internal static ReactionGraph Branch(List<BranchCase> cases) =>
            new BranchReaction(cases);

        internal static ReactionGraph Set(Source on, string property, ValueExpression value) =>
            new SetReaction(on, property, value);

        internal static ReactionGraph Call(Source on, string method, IReadOnlyList<ValueExpression> args) =>
            new CallReaction(on, method, args);

        internal static ReactionGraph Request(RequestPlan request) =>
            new RequestReaction(request);

        internal static ReactionGraph Dispatch(string eventName) =>
            new DispatchReaction(eventName, DispatchPayload.None);

        internal static ReactionGraph Dispatch(string eventName, ValueExpression data) =>
            new DispatchReaction(eventName, DispatchPayload.Of(data));

        internal static ReactionGraph Inject(string slot, ValueExpression value) =>
            new InjectReaction(slot, value);

        internal static ReactionGraph ShowValidationErrors(string container) =>
            new ShowValidationErrorsReaction(container);

        internal static IReadOnlyList<ReactionGraph> OrderedSteps(IEnumerable<ReactionGraph> steps)
        {
            var snapshot = new List<ReactionGraph>(steps);
            return snapshot.Count == 0
                ? Array.Empty<ReactionGraph>()
                : snapshot;
        }
    }

    /// <summary>Executes a list of reactions in declaration order.</summary>
    internal sealed class SequenceReaction : ReactionGraph
    {
        private readonly IReadOnlyList<ReactionGraph> _steps;

        /// <summary>Reactions executed in declaration order.</summary>
        public IReadOnlyList<ReactionGraph> Steps => _steps;

        internal SequenceReaction(IEnumerable<ReactionGraph> steps)
        {
            _steps = ReactionGraph.OrderedSteps(steps);
        }
    }

    /// <summary>Executes a list of reactions concurrently.</summary>
    internal sealed class ParallelReaction : ReactionGraph
    {
        private readonly IReadOnlyList<ReactionGraph> _steps;
        private readonly ParallelCompletion _completion;

        /// <summary>Reactions started concurrently.</summary>
        public IReadOnlyList<ReactionGraph> Steps => _steps;
        /// <summary>Completion behavior to run after all steps settle.</summary>
        public ParallelCompletion Completion => _completion;

        internal ParallelReaction(IEnumerable<ReactionGraph> steps, ParallelCompletion completion)
        {
            _steps = ReactionGraph.OrderedSteps(steps);
            _completion = completion;
        }
    }

    /// <summary>Wire base for parallel completion behavior authored by request/parallel builders.</summary>
    internal abstract class ParallelCompletion
    {
        private protected ParallelCompletion() { }

        internal static ParallelCompletion None { get; } = new NoParallelCompletion();


        internal static ParallelCompletion OnSettled(ReactionGraph reaction)
        {
            return new SettledParallelCompletion(reaction);
        }
    }

    /// <summary>Represents a parallel reaction with no completion reaction.</summary>
    internal sealed class NoParallelCompletion : ParallelCompletion
    {
    }

    /// <summary>Runs a reaction after every parallel branch has settled.</summary>
    internal sealed class SettledParallelCompletion : ParallelCompletion
    {
        private readonly ReactionGraph _reaction;

        internal SettledParallelCompletion(ReactionGraph reaction)
        {
            _reaction = reaction;
        }


        /// <summary>Reaction to run after all parallel branches settle.</summary>
        public ReactionGraph Reaction => _reaction;
    }

    /// <summary>Evaluates conditions and executes the first matching case.</summary>
    internal sealed class BranchReaction : ReactionGraph
    {
        private readonly IReadOnlyList<BranchCase> _cases;

        /// <summary>Branch cases evaluated in declaration order.</summary>
        public IReadOnlyList<BranchCase> Cases => _cases;

        internal BranchReaction(IEnumerable<BranchCase> cases)
        {
            _cases = OrderedCases(cases);
        }

        private static IReadOnlyList<BranchCase> OrderedCases(IEnumerable<BranchCase> cases)
        {
            return new List<BranchCase>(cases);
        }
    }

    /// <summary>Pairs an explicit guard with a reaction to execute. A default guard is the else case.</summary>
    internal sealed class BranchCase
    {
        /// <summary>Guard that decides whether this case runs.</summary>
        public BranchGuard Guard { get; }
        /// <summary>Reaction to execute when the guard matches.</summary>
        public ReactionGraph Reaction { get; }

        private BranchCase(BranchGuard guard, ReactionGraph reaction)
        {
            Guard = guard;
            Reaction = reaction;
        }

        internal static BranchCase Of(ConditionGraph when, ReactionGraph reaction) =>
            new BranchCase(BranchGuard.When(when), reaction);

        internal static BranchCase Default(ReactionGraph reaction) =>
            new BranchCase(BranchGuard.Else, reaction);

    }


    internal abstract class BranchGuard
    {
        private protected BranchGuard() { }

        internal static BranchGuard Else { get; } =
            new DefaultBranchGuard();


        /// <summary>The guard's condition; the default guard has none and always matches.</summary>
        internal abstract bool TryGetCondition(out ConditionGraph condition);

        internal static BranchGuard When(ConditionGraph condition) =>
            new ConditionalBranchGuard(condition);

        private sealed class DefaultBranchGuard : BranchGuard
        {


            internal override bool TryGetCondition(out ConditionGraph condition)
            {
                condition = null!;
                return false;
            }
        }

        private sealed class ConditionalBranchGuard : BranchGuard
        {
            private readonly ConditionGraph _condition;

            internal ConditionalBranchGuard(ConditionGraph condition)
            {
                _condition = condition;
            }



            internal override bool TryGetCondition(out ConditionGraph condition)
            {
                condition = _condition;
                return true;
            }
        }
    }


    /// <summary>Sets a property value on a component or DOM element.</summary>
    internal sealed class SetReaction : ReactionGraph
    {
        private readonly MemberName _property;

        /// <summary>Runtime source whose property is set.</summary>
        public Source On { get; }
        /// <summary>Property name to set on the target source.</summary>
        public string Property => _property.Value;
        /// <summary>Value expression assigned to the target property.</summary>
        public ValueExpression Value { get; }

        internal SetReaction(Source on, string property, ValueExpression value)
        {
            On = on;
            _property = MemberName.Of(property);
            Value = value;
        }
    }

    /// <summary>Calls a method on a component or DOM element.</summary>
    internal sealed class CallReaction : ReactionGraph
    {
        private readonly MemberName _method;
        private readonly IReadOnlyList<ValueExpression> _args;

        /// <summary>Runtime source whose method is called.</summary>
        public Source On { get; }
        /// <summary>Method name to call on the target source.</summary>
        public string Method => _method.Value;
        /// <summary>Method argument expressions in call order; empty when the method takes no arguments.</summary>
        public IReadOnlyList<ValueExpression> Args => _args;

        internal CallReaction(Source on, string method, IReadOnlyList<ValueExpression> args)
        {
            On = on;
            _method = MemberName.Of(method);
            _args = OrderedArguments(args);
        }

        private static IReadOnlyList<ValueExpression> OrderedArguments(IReadOnlyList<ValueExpression> items)
        {
            if (items.Count == 0)
                return Array.Empty<ValueExpression>();

            return new List<ValueExpression>(items);
        }
    }

    /// <summary>Sends an HTTP request as defined by the enclosed <see cref="RequestPlan"/>.</summary>
    internal sealed class RequestReaction : ReactionGraph
    {
        /// <summary>HTTP request definition to execute in the async lane.</summary>
        public new RequestPlan Request { get; }

        internal RequestReaction(RequestPlan request) { Request = request; }
    }

    /// <summary>Dispatches a <c>CustomEvent</c>.</summary>
    internal sealed class DispatchReaction : ReactionGraph
    {
        private readonly EventName _event;
        private readonly DispatchPayload _payload;

        /// <summary>Custom event name to dispatch.</summary>
        public string Event => _event.Value;

        internal DispatchPayload Payload => _payload;

        internal DispatchReaction(string eventName, DispatchPayload payload)
        {
            _event = EventName.Of(eventName);
            _payload = payload;
        }
    }


    internal abstract class DispatchPayload
    {
        private protected DispatchPayload() { }

        internal static DispatchPayload None { get; } =
            new NoDispatchPayload();



        internal static DispatchPayload Of(ValueExpression data) =>
            new PresentDispatchPayload(data);
    }

    internal sealed class NoDispatchPayload : DispatchPayload
    {

    }

    internal sealed class PresentDispatchPayload : DispatchPayload
    {
        private readonly ValueExpression _data;

        internal PresentDispatchPayload(ValueExpression data)
        {
            _data = data;
        }

        internal ValueExpression Data => _data;


    }


    /// <summary>Injects HTML into a partial slot.</summary>
    internal sealed class InjectReaction : ReactionGraph
    {
        private readonly ComponentKey _slot;

        /// <summary>Partial slot that receives injected HTML.</summary>
        public string Slot => _slot.Value;
        /// <summary>Value expression that resolves to the HTML to inject.</summary>
        public ValueExpression Value { get; }

        internal InjectReaction(string slot, ValueExpression value)
        {
            _slot = ComponentKey.Of(slot);
            Value = value;
        }
    }

    /// <summary>Displays accumulated validation errors in the target container.</summary>
    internal sealed class ShowValidationErrorsReaction : ReactionGraph
    {
        private readonly ComponentId _container;

        /// <summary>Element ID of the validation error container.</summary>
        public string Container => _container.Value;

        internal ShowValidationErrorsReaction(string container)
        {
            _container = ComponentId.Of(container);
        }
    }
}
