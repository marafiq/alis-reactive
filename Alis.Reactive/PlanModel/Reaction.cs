using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Base class for all executable actions in a reactive plan. Not constructed in application code.
    /// </summary>
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<Reaction>))]
    public abstract class Reaction
    {
        private protected Reaction() { }

        internal static Reaction Sequence(params Reaction[] steps) =>
            new SequenceReaction(new List<Reaction>(steps));

        internal static Reaction Sequence(List<Reaction> steps) =>
            new SequenceReaction(steps);

        internal static Reaction Parallel(List<Reaction> steps, Reaction? onSettled = null) =>
            new ParallelReaction(steps, onSettled);

        internal static Reaction Branch(params BranchCase[] cases) =>
            new BranchReaction(new List<BranchCase>(cases));

        internal static Reaction Branch(List<BranchCase> cases) =>
            new BranchReaction(cases);

        internal static Reaction Set(Source on, string property, ValueProducer value) =>
            new SetReaction(on, property, value);

        internal static Reaction Call(Source on, string method, List<ValueProducer>? args = null) =>
            new CallReaction(on, method, args);

        internal static Reaction Request(Request request) =>
            new RequestReaction(request);

        internal static Reaction Dispatch(string eventName, ValueProducer? data = null, string? payloadType = null) =>
            new DispatchReaction(eventName, data, payloadType);

        internal static Reaction Inject(string component, ValueProducer value) =>
            new InjectReaction(component, value);

        internal static Reaction ShowValidationErrors(string container) =>
            new ShowValidationErrorsReaction(container);

        internal static Reaction SequenceOrSingle(List<Reaction> reactions)
        {
            return Sequence(reactions);
        }
    }

    /// <summary>Executes a list of reactions in declaration order.</summary>
    public sealed class SequenceReaction : Reaction
    {
        /// <summary>Gets the kind. Always <c>"sequence"</c>.</summary>
        public string Kind => "sequence";
        /// <summary>Gets the ordered reactions to execute.</summary>
        public IReadOnlyList<Reaction> Steps { get; }

        internal SequenceReaction(List<Reaction> steps) { Steps = steps ?? throw new ArgumentNullException(nameof(steps)); }
    }

    /// <summary>Executes a list of reactions concurrently.</summary>
    public sealed class ParallelReaction : Reaction
    {
        /// <summary>Gets the kind. Always <c>"parallel"</c>.</summary>
        public string Kind => "parallel";
        /// <summary>Gets the reactions to execute concurrently.</summary>
        public IReadOnlyList<Reaction> Steps { get; }
        /// <summary>Gets the reaction to execute after all steps settle, or <see langword="null"/> if none.</summary>
        public Reaction? OnSettled { get; }

        internal ParallelReaction(List<Reaction> steps, Reaction? onSettled)
        {
            Steps = steps ?? throw new ArgumentNullException(nameof(steps));
            OnSettled = onSettled;
        }
    }

    /// <summary>Evaluates conditions and executes the first matching case.</summary>
    public sealed class BranchReaction : Reaction
    {
        /// <summary>Gets the kind. Always <c>"branch"</c>.</summary>
        public string Kind => "branch";
        /// <summary>Gets the ordered cases to evaluate.</summary>
        public IReadOnlyList<BranchCase> Cases { get; }

        internal BranchReaction(List<BranchCase> cases) { Cases = cases ?? throw new ArgumentNullException(nameof(cases)); }
    }

    /// <summary>Pairs an optional guard condition with a reaction to execute.</summary>
    public sealed class BranchCase
    {
        /// <summary>Gets the guard condition, or <see langword="null"/> for the default (else) case.</summary>
        public Condition? When { get; }
        /// <summary>Gets the reaction to execute when the condition is met.</summary>
        public Reaction Reaction { get; }

        internal BranchCase(Condition? when, Reaction reaction)
        {
            When = when;
            Reaction = reaction ?? throw new ArgumentNullException(nameof(reaction));
        }

        internal static BranchCase Of(Condition when, Reaction reaction) => new BranchCase(when, reaction);
        internal static BranchCase Default(Reaction reaction) => new BranchCase(null, reaction);
    }

    /// <summary>Sets a property value on a component or DOM element.</summary>
    public sealed class SetReaction : Reaction
    {
        /// <summary>Gets the kind. Always <c>"set"</c>.</summary>
        public string Kind => "set";
        /// <summary>Gets the target source to set the property on.</summary>
        public Source On { get; }
        /// <summary>Gets the property name to set.</summary>
        public string Property { get; }
        /// <summary>Gets the value to assign.</summary>
        public ValueProducer Value { get; }

        internal SetReaction(Source on, string property, ValueProducer value)
        {
            On = on ?? throw new ArgumentNullException(nameof(on));
            Property = property ?? throw new ArgumentNullException(nameof(property));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
    }

    /// <summary>Calls a method on a component or DOM element.</summary>
    public sealed class CallReaction : Reaction
    {
        /// <summary>Gets the kind. Always <c>"call"</c>.</summary>
        public string Kind => "call";
        /// <summary>Gets the target source to call the method on.</summary>
        public Source On { get; }
        /// <summary>Gets the method name to call.</summary>
        public string Method { get; }
        /// <summary>Gets the method arguments, or <see langword="null"/> when the method takes no arguments.</summary>
        public IReadOnlyList<ValueProducer>? Args { get; }

        internal CallReaction(Source on, string method, List<ValueProducer>? args)
        {
            On = on ?? throw new ArgumentNullException(nameof(on));
            Method = method ?? throw new ArgumentNullException(nameof(method));
            Args = args != null && args.Count > 0 ? args : null;
        }
    }

    /// <summary>Sends an HTTP request as defined by the enclosed <see cref="PlanModel.Request"/>.</summary>
    public sealed class RequestReaction : Reaction
    {
        /// <summary>Gets the kind. Always <c>"request"</c>.</summary>
        public string Kind => "request";
        /// <summary>Gets the HTTP request definition.</summary>
        public new Request Request { get; }

        internal RequestReaction(Request request) { Request = request ?? throw new ArgumentNullException(nameof(request)); }
    }

    /// <summary>Dispatches a custom browser event.</summary>
    public sealed class DispatchReaction : Reaction
    {
        /// <summary>Gets the kind. Always <c>"dispatch"</c>.</summary>
        public string Kind => "dispatch";
        /// <summary>Gets the event name to dispatch.</summary>
        public string Event { get; }
        /// <summary>Gets the optional event payload, or <see langword="null"/> for no data.</summary>
        public ValueProducer? Data { get; }
        /// <summary>Gets the optional payload type tag, or <see langword="null"/> when untyped.</summary>
        public string? PayloadType { get; }

        internal DispatchReaction(string eventName, ValueProducer? data, string? payloadType)
        {
            Event = eventName ?? throw new ArgumentNullException(nameof(eventName));
            Data = data;
            PayloadType = payloadType;
        }
    }

    /// <summary>Injects a value into a named component.</summary>
    public sealed class InjectReaction : Reaction
    {
        /// <summary>Gets the kind. Always <c>"inject"</c>.</summary>
        public string Kind => "inject";
        /// <summary>Gets the target component name.</summary>
        public string Component { get; }
        /// <summary>Gets the value to inject.</summary>
        public ValueProducer Value { get; }

        internal InjectReaction(string component, ValueProducer value)
        {
            Component = component ?? throw new ArgumentNullException(nameof(component));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
    }

    /// <summary>Displays accumulated validation errors in the target container.</summary>
    public sealed class ShowValidationErrorsReaction : Reaction
    {
        /// <summary>Gets the kind. Always <c>"show-validation-errors"</c>.</summary>
        public string Kind => "show-validation-errors";
        /// <summary>Gets the element ID of the validation error container.</summary>
        public string Container { get; }

        internal ShowValidationErrorsReaction(string container)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
        }
    }
}
