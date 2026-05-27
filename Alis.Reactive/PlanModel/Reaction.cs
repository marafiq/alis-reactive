using System;
using System.Collections.Generic;
using System.Text.Json;
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
            new SequenceReaction(steps);

        internal static Reaction Sequence(List<Reaction> steps) =>
            new SequenceReaction(steps);

        internal static Reaction Parallel(List<Reaction> steps, ParallelCompletion completion) =>
            new ParallelReaction(steps, completion);

        internal static Reaction Branch(params BranchCase[] cases) =>
            new BranchReaction(cases);

        internal static Reaction Branch(List<BranchCase> cases) =>
            new BranchReaction(cases);

        internal static Reaction Set(Source on, string property, ValueProducer value) =>
            new SetReaction(on, property, value);

        internal static Reaction Call(Source on, string method) =>
            new CallReaction(on, method, Array.Empty<ValueProducer>());

        internal static Reaction Call(Source on, string method, List<ValueProducer> args) =>
            new CallReaction(on, method, args);

        internal static Reaction Call(Source on, string method, IReadOnlyList<ValueProducer> args) =>
            new CallReaction(on, method, args);

        internal static Reaction Request(RequestPlan request) =>
            new RequestReaction(request);

        internal static Reaction Dispatch(string eventName) =>
            new DispatchReaction(eventName, DispatchPayload.None);

        internal static Reaction Dispatch(string eventName, ValueProducer data) =>
            new DispatchReaction(eventName, DispatchPayload.Untyped(data));

        internal static Reaction Dispatch(string eventName, ValueProducer data, PayloadContract payloadType) =>
            new DispatchReaction(eventName, DispatchPayload.Typed(data, payloadType));

        internal static Reaction Inject(string component, ValueProducer value) =>
            new InjectReaction(InjectionTarget.PartialSlot(component), value);

        internal static Reaction ShowValidationErrors(string container) =>
            new ShowValidationErrorsReaction(container);

        internal static IReadOnlyList<Reaction> OrderedSteps(IEnumerable<Reaction> steps)
        {
            if (steps == null) throw new ArgumentNullException(nameof(steps));

            var snapshot = new List<Reaction>();
            foreach (var step in steps)
            {
                if (step == null)
                    throw new ArgumentException("Reaction step must not be null.", nameof(steps));

                snapshot.Add(step);
            }

            return snapshot.Count == 0
                ? Array.Empty<Reaction>()
                : snapshot;
        }
    }

    /// <summary>Executes a list of reactions in declaration order.</summary>
    public sealed class SequenceReaction : Reaction
    {
        private readonly IReadOnlyList<Reaction> _steps;

        /// <summary>Gets the kind. Always <c>"sequence"</c>.</summary>
        public string Kind => "sequence";
        /// <summary>Gets the ordered reactions to execute.</summary>
        public IReadOnlyList<Reaction> Steps => _steps;

        internal SequenceReaction(IEnumerable<Reaction> steps)
        {
            _steps = Reaction.OrderedSteps(steps);
        }
    }

    /// <summary>Executes a list of reactions concurrently.</summary>
    public sealed class ParallelReaction : Reaction
    {
        private readonly IReadOnlyList<Reaction> _steps;
        private readonly ParallelCompletion _completion;

        /// <summary>Gets the kind. Always <c>"parallel"</c>.</summary>
        public string Kind => "parallel";
        /// <summary>Gets the reactions to execute concurrently.</summary>
        public IReadOnlyList<Reaction> Steps => _steps;
        /// <summary>Gets the completion behavior to run after all steps settle.</summary>
        public ParallelCompletion Completion => _completion;

        internal ParallelReaction(IEnumerable<Reaction> steps, ParallelCompletion completion)
        {
            _steps = Reaction.OrderedSteps(steps);
            _completion = completion ?? throw new ArgumentNullException(nameof(completion));
        }
    }

    /// <summary>Base class for parallel completion behavior. Not constructed in application code.</summary>
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<ParallelCompletion>))]
    public abstract class ParallelCompletion
    {
        private protected ParallelCompletion() { }

        internal static ParallelCompletion None { get; } = new NoParallelCompletion();

        /// <summary>Gets the completion kind.</summary>
        public abstract string Kind { get; }

        internal static ParallelCompletion OnSettled(Reaction reaction)
        {
            if (reaction == null) throw new ArgumentNullException(nameof(reaction));
            return new SettledParallelCompletion(reaction);
        }
    }

    /// <summary>Represents a parallel reaction with no completion reaction.</summary>
    public sealed class NoParallelCompletion : ParallelCompletion
    {
        /// <summary>Gets the kind. Always <c>"none"</c>.</summary>
        public override string Kind => "none";
    }

    /// <summary>Runs a reaction after every parallel branch has settled.</summary>
    public sealed class SettledParallelCompletion : ParallelCompletion
    {
        private readonly Reaction _reaction;

        internal SettledParallelCompletion(Reaction reaction)
        {
            _reaction = reaction ?? throw new ArgumentNullException(nameof(reaction));
        }

        /// <summary>Gets the kind. Always <c>"on-settled"</c>.</summary>
        public override string Kind => "on-settled";

        /// <summary>Gets the reaction to run after all branches settle.</summary>
        public Reaction Reaction => _reaction;
    }

    /// <summary>Evaluates conditions and executes the first matching case.</summary>
    public sealed class BranchReaction : Reaction
    {
        private readonly IReadOnlyList<BranchCase> _cases;

        /// <summary>Gets the kind. Always <c>"branch"</c>.</summary>
        public string Kind => "branch";
        /// <summary>Gets the ordered cases to evaluate.</summary>
        public IReadOnlyList<BranchCase> Cases => _cases;

        internal BranchReaction(IEnumerable<BranchCase> cases)
        {
            _cases = OrderedCases(cases);
        }

        private static IReadOnlyList<BranchCase> OrderedCases(IEnumerable<BranchCase> cases)
        {
            if (cases == null) throw new ArgumentNullException(nameof(cases));

            var snapshot = new List<BranchCase>();
            foreach (var branchCase in cases)
            {
                if (branchCase == null)
                    throw new ArgumentException("Branch case must not be null.", nameof(cases));

                snapshot.Add(branchCase);
            }

            var hasNoCases = snapshot.Count == 0;
            if (hasNoCases)
                throw new InvalidOperationException(
                    "Branch reaction requires at least one branch case.");

            EnsureDefaultCaseIsUniqueAndLast(snapshot);
            return snapshot;
        }

        private static void EnsureDefaultCaseIsUniqueAndLast(IReadOnlyList<BranchCase> cases)
        {
            var defaultCaseIndex = -1;
            for (var i = 0; i < cases.Count; i++)
            {
                var branchCase = cases[i];
                var caseIsDefault = branchCase.GuardForJson.Kind == "default";
                if (!caseIsDefault) continue;

                if (defaultCaseIndex >= 0)
                    throw new InvalidOperationException(
                        "Branch reaction can have only one default branch case.");

                defaultCaseIndex = i;
            }

            var defaultCaseWasDeclared = defaultCaseIndex >= 0;
            var defaultCaseIsLast = defaultCaseIndex == cases.Count - 1;
            if (defaultCaseWasDeclared && !defaultCaseIsLast)
                throw new InvalidOperationException(
                    "Branch reaction default branch case must be last.");
        }
    }

    /// <summary>Pairs an explicit guard with a reaction to execute. A default guard is the else case.</summary>
    [JsonConverter(typeof(BranchCaseJsonConverter))]
    public sealed class BranchCase
    {
        private readonly BranchGuard _guard;

        /// <summary>Gets the reaction to execute when the condition is met.</summary>
        public Reaction Reaction { get; }

        internal BranchGuard GuardForJson => _guard;

        private BranchCase(BranchGuard guard, Reaction reaction)
        {
            _guard = guard ?? throw new ArgumentNullException(nameof(guard));
            Reaction = reaction ?? throw new ArgumentNullException(nameof(reaction));
        }

        internal static BranchCase Of(Condition when, Reaction reaction) =>
            new BranchCase(BranchGuard.When(when), reaction);

        internal static BranchCase Default(Reaction reaction) =>
            new BranchCase(BranchGuard.Else, reaction);

        internal BranchCase WithReaction(Reaction reaction) =>
            new BranchCase(_guard, reaction);
    }

    internal sealed class BranchCaseJsonConverter : JsonConverter<BranchCase>
    {
        public override void Write(Utf8JsonWriter writer, BranchCase value, JsonSerializerOptions options)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            writer.WriteStartObject();
            WriteProperty(writer, options, "guard", value.GuardForJson);
            WriteProperty(writer, options, "reaction", value.Reaction);
            writer.WriteEndObject();
        }

        public override BranchCase Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
            throw new NotSupportedException("Plan types are write-only.");

        private static void WriteProperty<T>(
            Utf8JsonWriter writer,
            JsonSerializerOptions options,
            string name,
            T value)
        {
            writer.WritePropertyName(name);
            JsonSerializer.Serialize(writer, value, options);
        }
    }

    [JsonConverter(typeof(BranchGuardJsonConverter))]
    internal abstract class BranchGuard
    {
        private protected BranchGuard() { }

        internal static BranchGuard Else { get; } =
            new DefaultBranchGuard();

        public abstract string Kind { get; }
        internal abstract void WriteGuardPayload(Utf8JsonWriter writer, JsonSerializerOptions options);

        internal static BranchGuard When(Condition condition) =>
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
            private readonly Condition _condition;

            internal ConditionalBranchGuard(Condition condition)
            {
                _condition = condition ?? throw new ArgumentNullException(nameof(condition));
            }

            public override string Kind => "when";
            public Condition Condition => _condition;

            internal override void WriteGuardPayload(Utf8JsonWriter writer, JsonSerializerOptions options) =>
                BranchGuardJsonConverter.WriteProperty(writer, options, "condition", _condition);
        }
    }

    internal sealed class BranchGuardJsonConverter : JsonConverter<BranchGuard>
    {
        public override void Write(Utf8JsonWriter writer, BranchGuard value, JsonSerializerOptions options)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

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

        internal static void WriteProperty<T>(
            Utf8JsonWriter writer,
            JsonSerializerOptions options,
            string name,
            T value)
        {
            writer.WritePropertyName(name);
            JsonSerializer.Serialize(writer, value, options);
        }
    }

    /// <summary>Sets a property value on a component or DOM element.</summary>
    public sealed class SetReaction : Reaction
    {
        private readonly MemberName _property;

        /// <summary>Gets the kind. Always <c>"set"</c>.</summary>
        public string Kind => "set";
        /// <summary>Gets the target source to set the property on.</summary>
        public Source On { get; }
        /// <summary>Gets the property name to set.</summary>
        public string Property => _property.Value;
        /// <summary>Gets the value to assign.</summary>
        public ValueProducer Value { get; }

        internal SetReaction(Source on, string property, ValueProducer value)
        {
            On = on ?? throw new ArgumentNullException(nameof(on));
            _property = MemberName.Of(property);
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
    }

    /// <summary>Calls a method on a component or DOM element.</summary>
    public sealed class CallReaction : Reaction
    {
        private readonly MemberName _method;
        private readonly IReadOnlyList<ValueProducer> _args;

        /// <summary>Gets the kind. Always <c>"call"</c>.</summary>
        public string Kind => "call";
        /// <summary>Gets the target source to call the method on.</summary>
        public Source On { get; }
        /// <summary>Gets the method name to call.</summary>
        public string Method => _method.Value;
        /// <summary>Gets the method arguments. Empty when the method takes no arguments.</summary>
        public IReadOnlyList<ValueProducer> Args => _args;

        internal CallReaction(Source on, string method, IReadOnlyList<ValueProducer> args)
        {
            On = on ?? throw new ArgumentNullException(nameof(on));
            _method = MemberName.Of(method);
            _args = OrderedArguments(args);
        }

        private static IReadOnlyList<ValueProducer> OrderedArguments(IReadOnlyList<ValueProducer> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (items.Count == 0)
                return Array.Empty<ValueProducer>();

            var snapshot = new List<ValueProducer>();
            foreach (var item in items)
            {
                if (item == null)
                    throw new ArgumentException("Call argument must not be null.", nameof(items));

                snapshot.Add(item);
            }

            return snapshot;
        }
    }

    /// <summary>Sends an HTTP request as defined by the enclosed <see cref="RequestPlan"/>.</summary>
    public sealed class RequestReaction : Reaction
    {
        /// <summary>Gets the kind. Always <c>"request"</c>.</summary>
        public string Kind => "request";
        /// <summary>Gets the HTTP request definition.</summary>
        public new RequestPlan Request { get; }

        internal RequestReaction(RequestPlan request) { Request = request ?? throw new ArgumentNullException(nameof(request)); }
    }

    /// <summary>Dispatches a custom browser event.</summary>
    [JsonConverter(typeof(DispatchReactionJsonConverter))]
    public sealed class DispatchReaction : Reaction
    {
        private readonly EventName _event;
        private readonly DispatchPayload _payload;

        /// <summary>Gets the kind. Always <c>"dispatch"</c>.</summary>
        public string Kind => "dispatch";
        /// <summary>Gets the event name to dispatch.</summary>
        public string Event => _event.Value;

        internal DispatchPayload PayloadForJson => _payload;

        internal DispatchReaction(string eventName, DispatchPayload payload)
        {
            _event = EventName.Of(eventName);
            _payload = payload ?? throw new ArgumentNullException(nameof(payload));
        }
    }

    internal sealed class DispatchReactionJsonConverter : JsonConverter<DispatchReaction>
    {
        public override void Write(Utf8JsonWriter writer, DispatchReaction value, JsonSerializerOptions options)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            writer.WriteStartObject();
            writer.WriteString("kind", value.Kind);
            writer.WriteString("event", value.Event);
            WriteProperty(writer, options, "payload", value.PayloadForJson);
            writer.WriteEndObject();
        }

        public override DispatchReaction Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
            throw new NotSupportedException("Plan types are write-only.");

        private static void WriteProperty<T>(
            Utf8JsonWriter writer,
            JsonSerializerOptions options,
            string name,
            T value)
        {
            writer.WritePropertyName(name);
            JsonSerializer.Serialize(writer, value, options);
        }
    }

    [JsonConverter(typeof(DispatchPayloadJsonConverter))]
    internal abstract class DispatchPayload
    {
        private protected DispatchPayload() { }

        internal static DispatchPayload None { get; } =
            new NoDispatchPayload();

        public abstract string Kind { get; }

        internal abstract void WritePayload(Utf8JsonWriter writer, JsonSerializerOptions options);

        internal static DispatchPayload Untyped(ValueProducer data) =>
            new PresentDispatchPayload(data, PayloadContract.Untyped);

        internal static DispatchPayload Typed(ValueProducer data, PayloadContract payloadType) =>
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
        private readonly ValueProducer _data;
        private readonly PayloadContract _payloadType;

        internal PresentDispatchPayload(ValueProducer data, PayloadContract payloadType)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _payloadType = payloadType ?? throw new ArgumentNullException(nameof(payloadType));
        }

        public override string Kind => "value";

        internal override void WritePayload(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            DispatchPayloadJsonConverter.WriteProperty(writer, options, "data", _data);
            DispatchPayloadJsonConverter.WriteProperty(writer, options, "payloadType", _payloadType);
        }
    }

    internal sealed class DispatchPayloadJsonConverter : JsonConverter<DispatchPayload>
    {
        public override void Write(Utf8JsonWriter writer, DispatchPayload value, JsonSerializerOptions options)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

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

        internal static void WriteProperty<T>(
            Utf8JsonWriter writer,
            JsonSerializerOptions options,
            string name,
            T value)
        {
            writer.WritePropertyName(name);
            JsonSerializer.Serialize(writer, value, options);
        }
    }

    /// <summary>Base class for HTML injection targets. Not constructed in application code.</summary>
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<InjectionTarget>))]
    public abstract class InjectionTarget
    {
        private protected InjectionTarget() { }

        /// <summary>Gets the target kind.</summary>
        public abstract string Kind { get; }

        /// <summary>Gets the target component key.</summary>
        public abstract string Component { get; }

        internal static InjectionTarget PartialSlot(string component) =>
            new PartialSlotInjectionTarget(ComponentKey.Of(component));
    }

    /// <summary>Replaces a browser partial slot with injected HTML and embedded plans.</summary>
    public sealed class PartialSlotInjectionTarget : InjectionTarget
    {
        private readonly ComponentKey _component;

        internal PartialSlotInjectionTarget(ComponentKey component)
        {
            _component = component ?? throw new ArgumentNullException(nameof(component));
        }

        /// <summary>Gets the kind. Always <c>"partial-slot"</c>.</summary>
        public override string Kind => "partial-slot";
        /// <summary>Gets the component whose HTML is replaced.</summary>
        public override string Component => _component.Value;
    }

    /// <summary>Injects a value into a declared target.</summary>
    public sealed class InjectReaction : Reaction
    {
        private readonly InjectionTarget _target;

        /// <summary>Gets the kind. Always <c>"inject"</c>.</summary>
        public string Kind => "inject";
        /// <summary>Gets the target component and lifecycle semantics.</summary>
        public InjectionTarget Target => _target;
        /// <summary>Gets the value to inject.</summary>
        public ValueProducer Value { get; }

        internal InjectReaction(InjectionTarget target, ValueProducer value)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
    }

    /// <summary>Displays accumulated validation errors in the target container.</summary>
    public sealed class ShowValidationErrorsReaction : Reaction
    {
        private readonly ComponentId _container;

        /// <summary>Gets the kind. Always <c>"show-validation-errors"</c>.</summary>
        public string Kind => "show-validation-errors";
        /// <summary>Gets the element ID of the validation error container.</summary>
        public string Container => _container.Value;

        internal ShowValidationErrorsReaction(string container)
        {
            _container = ComponentId.Of(container);
        }
    }
}
