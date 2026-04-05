using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<Reaction>))]
    public abstract class Reaction
    {
        private protected Reaction() { }

        internal static Reaction Sequence(params Reaction[] steps) =>
            new SequenceReaction(new List<Reaction>(steps));

        internal static Reaction Sequence(List<Reaction> steps) =>
            new SequenceReaction(steps);

        internal static Reaction Parallel(List<Reaction> steps, Reaction onSettled = null) =>
            new ParallelReaction(steps, onSettled);

        internal static Reaction Branch(params BranchCase[] cases) =>
            new BranchReaction(new List<BranchCase>(cases));

        internal static Reaction Branch(List<BranchCase> cases) =>
            new BranchReaction(cases);

        internal static Reaction Set(Source on, string property, ValueProducer value) =>
            new SetReaction(on, property, value);

        internal static Reaction Call(Source on, string method, List<ValueProducer> args = null) =>
            new CallReaction(on, method, args);

        internal static Reaction Request(Request request) =>
            new RequestReaction(request);

        internal static Reaction Dispatch(string eventName, ValueProducer data = null, string payloadType = null) =>
            new DispatchReaction(eventName, data, payloadType);

        internal static Reaction Inject(string component, ValueProducer value) =>
            new InjectReaction(component, value);

        internal static Reaction ShowValidationErrors(string container) =>
            new ShowValidationErrorsReaction(container);

        internal static Reaction SequenceOrSingle(List<Reaction> reactions)
        {
            if (reactions.Count == 1) return reactions[0];
            return Sequence(reactions);
        }
    }

    public sealed class SequenceReaction : Reaction
    {
        public string Kind => "sequence";
        public List<Reaction> Steps { get; }

        internal SequenceReaction(List<Reaction> steps) { Steps = steps; }
    }

    public sealed class ParallelReaction : Reaction
    {
        public string Kind => "parallel";
        public List<Reaction> Steps { get; }
        public Reaction OnSettled { get; }

        internal ParallelReaction(List<Reaction> steps, Reaction onSettled)
        {
            Steps = steps;
            OnSettled = onSettled;
        }
    }

    public sealed class BranchReaction : Reaction
    {
        public string Kind => "branch";
        public List<BranchCase> Cases { get; }

        internal BranchReaction(List<BranchCase> cases) { Cases = cases; }
    }

    public sealed class BranchCase
    {
        public Condition When { get; }
        public Reaction Reaction { get; }

        internal BranchCase(Condition when, Reaction reaction)
        {
            When = when;
            Reaction = reaction ?? throw new ArgumentNullException(nameof(reaction));
        }

        internal static BranchCase Of(Condition when, Reaction reaction) => new BranchCase(when, reaction);
        internal static BranchCase Default(Reaction reaction) => new BranchCase(null, reaction);
    }

    public sealed class SetReaction : Reaction
    {
        public string Kind => "set";
        public Source On { get; }
        public string Property { get; }
        public ValueProducer Value { get; }

        internal SetReaction(Source on, string property, ValueProducer value)
        {
            On = on ?? throw new ArgumentNullException(nameof(on));
            Property = property ?? throw new ArgumentNullException(nameof(property));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
    }

    public sealed class CallReaction : Reaction
    {
        public string Kind => "call";
        public Source On { get; }
        public string Method { get; }
        public List<ValueProducer> Args { get; }

        internal CallReaction(Source on, string method, List<ValueProducer> args)
        {
            On = on ?? throw new ArgumentNullException(nameof(on));
            Method = method ?? throw new ArgumentNullException(nameof(method));
            Args = args != null && args.Count > 0 ? args : null;
        }
    }

    public sealed class RequestReaction : Reaction
    {
        public string Kind => "request";
        public Request Request { get; }

        internal RequestReaction(Request request) { Request = request; }
    }

    public sealed class DispatchReaction : Reaction
    {
        public string Kind => "dispatch";
        public string Event { get; }
        public ValueProducer Data { get; }
        public string PayloadType { get; }

        internal DispatchReaction(string eventName, ValueProducer data, string payloadType)
        {
            Event = eventName ?? throw new ArgumentNullException(nameof(eventName));
            Data = data;
            PayloadType = payloadType;
        }
    }

    public sealed class InjectReaction : Reaction
    {
        public string Kind => "inject";
        public string Component { get; }
        public ValueProducer Value { get; }

        internal InjectReaction(string component, ValueProducer value)
        {
            Component = component ?? throw new ArgumentNullException(nameof(component));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
    }

    public sealed class ShowValidationErrorsReaction : Reaction
    {
        public string Kind => "show-validation-errors";
        public string Container { get; }

        internal ShowValidationErrorsReaction(string container)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
        }
    }
}
