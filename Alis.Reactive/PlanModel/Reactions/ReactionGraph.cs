using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Base class for all executable actions in a Reactive Plan. Not constructed in application code.
    /// </summary>
    [JsonConverter(typeof(PlanNodeDiscriminator<ReactionGraph>))]
    public abstract class ReactionGraph
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
            new DispatchReaction(eventName, DispatchPayload.Untyped(data));

        internal static ReactionGraph Dispatch(string eventName, ValueExpression data, PayloadContract payloadType) =>
            new DispatchReaction(eventName, DispatchPayload.Typed(data, payloadType));

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
    public sealed class SequenceReaction : ReactionGraph
    {
        private readonly IReadOnlyList<ReactionGraph> _steps;

        /// <summary>JSON discriminator for ordered reaction sequences. Always <c>"sequence"</c>.</summary>
        public string Kind => "sequence";
        /// <summary>Reactions executed in declaration order.</summary>
        public IReadOnlyList<ReactionGraph> Steps => _steps;

        internal SequenceReaction(IEnumerable<ReactionGraph> steps)
        {
            _steps = ReactionGraph.OrderedSteps(steps);
        }
    }

    /// <summary>Executes a list of reactions concurrently.</summary>
    public sealed class ParallelReaction : ReactionGraph
    {
        private readonly IReadOnlyList<ReactionGraph> _steps;
        private readonly ParallelCompletion _completion;

        /// <summary>JSON discriminator for concurrent reaction groups. Always <c>"parallel"</c>.</summary>
        public string Kind => "parallel";
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

    /// <summary>Base class for parallel completion behavior. Not constructed in application code.</summary>
    [JsonConverter(typeof(PlanNodeDiscriminator<ParallelCompletion>))]
    public abstract class ParallelCompletion
    {
        private protected ParallelCompletion() { }

        internal static ParallelCompletion None { get; } = new NoParallelCompletion();

        /// <summary>JSON discriminator for parallel completion behavior.</summary>
        public abstract string Kind { get; }

        internal static ParallelCompletion OnSettled(ReactionGraph reaction)
        {
            return new SettledParallelCompletion(reaction);
        }
    }

    /// <summary>Represents a parallel reaction with no completion reaction.</summary>
    public sealed class NoParallelCompletion : ParallelCompletion
    {
        /// <summary>JSON discriminator for no completion reaction. Always <c>"none"</c>.</summary>
        public override string Kind => "none";
    }

    /// <summary>Runs a reaction after every parallel branch has settled.</summary>
    public sealed class SettledParallelCompletion : ParallelCompletion
    {
        private readonly ReactionGraph _reaction;

        internal SettledParallelCompletion(ReactionGraph reaction)
        {
            _reaction = reaction;
        }

        /// <summary>JSON discriminator for settled completion reactions. Always <c>"on-settled"</c>.</summary>
        public override string Kind => "on-settled";

        /// <summary>Reaction to run after all parallel branches settle.</summary>
        public ReactionGraph Reaction => _reaction;
    }

    /// <summary>Evaluates conditions and executes the first matching case.</summary>
    public sealed class BranchReaction : ReactionGraph
    {
        private readonly IReadOnlyList<BranchCase> _cases;

        /// <summary>JSON discriminator for branch reactions. Always <c>"branch"</c>.</summary>
        public string Kind => "branch";
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
    [JsonConverter(typeof(BranchCaseJsonConverter))]
    public sealed class BranchCase
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

        internal BranchCase WithReaction(ReactionGraph reaction) =>
            new BranchCase(Guard, reaction);
    }

    internal sealed class BranchCaseJsonConverter : JsonConverter<BranchCase>
    {
        public override void Write(Utf8JsonWriter writer, BranchCase value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            PlanJsonWriter.WriteProperty(writer, options, "guard", value.Guard);
            PlanJsonWriter.WriteProperty(writer, options, "reaction", value.Reaction);
            writer.WriteEndObject();
        }

        public override BranchCase Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
            throw new NotSupportedException("Plan types are write-only.");
    }

    [JsonConverter(typeof(BranchGuardJsonConverter))]
    public abstract class BranchGuard
    {
        private protected BranchGuard() { }

        internal static BranchGuard Else { get; } =
            new DefaultBranchGuard();

        public abstract string Kind { get; }
        internal abstract void WriteGuardPayload(Utf8JsonWriter writer, JsonSerializerOptions options);

        internal static BranchGuard When(ConditionGraph condition) =>
            new ConditionalBranchGuard(condition);

        private sealed class DefaultBranchGuard : BranchGuard
        {
            public override string Kind => "default";

            internal override void WriteGuardPayload(Utf8JsonWriter writer, JsonSerializerOptions options)
            {
            }
        }

        private sealed class ConditionalBranchGuard : BranchGuard
        {
            private readonly ConditionGraph _condition;

            internal ConditionalBranchGuard(ConditionGraph condition)
            {
                _condition = condition;
            }

            public override string Kind => "when";

            internal override void WriteGuardPayload(Utf8JsonWriter writer, JsonSerializerOptions options) =>
                PlanJsonWriter.WriteProperty(writer, options, "condition", _condition);
        }
    }

    internal sealed class BranchGuardJsonConverter : JsonConverter<BranchGuard>
    {
        public override void Write(Utf8JsonWriter writer, BranchGuard value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("kind", value.Kind);
            value.WriteGuardPayload(writer, options);
            writer.WriteEndObject();
        }

        public override BranchGuard Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
            throw new NotSupportedException("Plan types are write-only.");
    }

    /// <summary>Sets a property value on a component or DOM element.</summary>
    public sealed class SetReaction : ReactionGraph
    {
        private readonly MemberName _property;

        /// <summary>JSON discriminator for set reactions. Always <c>"set"</c>.</summary>
        public string Kind => "set";
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
    public sealed class CallReaction : ReactionGraph
    {
        private readonly MemberName _method;
        private readonly IReadOnlyList<ValueExpression> _args;

        /// <summary>JSON discriminator for call reactions. Always <c>"call"</c>.</summary>
        public string Kind => "call";
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
    public sealed class RequestReaction : ReactionGraph
    {
        /// <summary>JSON discriminator for HTTP request reactions. Always <c>"request"</c>.</summary>
        public string Kind => "request";
        /// <summary>HTTP request definition to execute in the async lane.</summary>
        public new RequestPlan Request { get; }

        internal RequestReaction(RequestPlan request) { Request = request; }
    }

    /// <summary>Dispatches a <c>CustomEvent</c>.</summary>
    [JsonConverter(typeof(DispatchReactionJsonConverter))]
    public sealed class DispatchReaction : ReactionGraph
    {
        private readonly EventName _event;
        private readonly DispatchPayload _payload;

        /// <summary>JSON discriminator for custom event dispatch reactions. Always <c>"dispatch"</c>.</summary>
        public string Kind => "dispatch";
        /// <summary>Custom event name to dispatch.</summary>
        public string Event => _event.Value;

        internal DispatchPayload PayloadForJson => _payload;

        internal DispatchReaction(string eventName, DispatchPayload payload)
        {
            _event = EventName.Of(eventName);
            _payload = payload;
        }
    }

    internal sealed class DispatchReactionJsonConverter : JsonConverter<DispatchReaction>
    {
        public override void Write(Utf8JsonWriter writer, DispatchReaction value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("kind", value.Kind);
            writer.WriteString("event", value.Event);
            PlanJsonWriter.WriteProperty(writer, options, "payload", value.PayloadForJson);
            writer.WriteEndObject();
        }

        public override DispatchReaction Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
            throw new NotSupportedException("Plan types are write-only.");
    }

    [JsonConverter(typeof(DispatchPayloadJsonConverter))]
    internal abstract class DispatchPayload
    {
        private protected DispatchPayload() { }

        internal static DispatchPayload None { get; } =
            new NoDispatchPayload();

        public abstract string Kind { get; }

        internal abstract void WritePayload(Utf8JsonWriter writer, JsonSerializerOptions options);

        internal static DispatchPayload Untyped(ValueExpression data) =>
            new PresentDispatchPayload(data, PayloadContract.Untyped);

        internal static DispatchPayload Typed(ValueExpression data, PayloadContract payloadType) =>
            new PresentDispatchPayload(data, payloadType);
    }

    internal sealed class NoDispatchPayload : DispatchPayload
    {
        public override string Kind => "none";

        internal override void WritePayload(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
        }
    }

    internal sealed class PresentDispatchPayload : DispatchPayload
    {
        private readonly ValueExpression _data;
        private readonly PayloadContract _payloadType;

        internal PresentDispatchPayload(ValueExpression data, PayloadContract payloadType)
        {
            _data = data;
            _payloadType = payloadType;
        }

        public override string Kind => "value";

        internal override void WritePayload(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            PlanJsonWriter.WriteProperty(writer, options, "data", _data);
            PlanJsonWriter.WriteProperty(writer, options, "payloadType", _payloadType);
        }
    }

    internal sealed class DispatchPayloadJsonConverter : JsonConverter<DispatchPayload>
    {
        public override void Write(Utf8JsonWriter writer, DispatchPayload value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("kind", value.Kind);
            value.WritePayload(writer, options);
            writer.WriteEndObject();
        }

        public override DispatchPayload Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
            throw new NotSupportedException("Plan types are write-only.");
    }

    /// <summary>Injects HTML into a partial slot.</summary>
    public sealed class InjectReaction : ReactionGraph
    {
        private readonly ComponentKey _slot;

        /// <summary>JSON discriminator for partial-slot injection reactions. Always <c>"inject"</c>.</summary>
        public string Kind => "inject";
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
    public sealed class ShowValidationErrorsReaction : ReactionGraph
    {
        private readonly ComponentId _container;

        /// <summary>JSON discriminator for validation error display reactions. Always <c>"show-validation-errors"</c>.</summary>
        public string Kind => "show-validation-errors";
        /// <summary>Element ID of the validation error container.</summary>
        public string Container => _container.Value;

        internal ShowValidationErrorsReaction(string container)
        {
            _container = ComponentId.Of(container);
        }
    }
}
