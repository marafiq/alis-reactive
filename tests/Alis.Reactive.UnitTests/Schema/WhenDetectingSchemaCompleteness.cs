using System.Text.Json;
using Json.Schema;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Descriptors.Guards;
using Alis.Reactive.Descriptors.Mutations;
using Alis.Reactive.Descriptors.Reactions;
using Alis.Reactive.Descriptors.Requests;
using Alis.Reactive.Descriptors.Sources;
using Alis.Reactive.Descriptors.Triggers;
using Alis.Reactive.Validation;

namespace Alis.Reactive.UnitTests.Schema;

/// <summary>
/// Guards against C# → Schema drift by serializing every descriptor type with all
/// properties populated and validating against the schema.
///
/// Historical drift this catches:
///   b5bb10b — planId added to C# but not schema (additionalProperties: false rejects it)
///   d1fa967 — enriched props added to C# but not schema
///   4be3e5e — componentType in C# but not schema
///
/// If a new property is added to any descriptor, this test will fail because the schema
/// does not know about it (additionalProperties: false). The fix is to update the schema.
/// </summary>
[TestFixture]
public class WhenDetectingSchemaCompleteness : PlanTestBase
{
    private static readonly JsonSerializerOptions SerializeOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    // ── Triggers ──

    [Test]
    public void DomReady_trigger_conforms_to_schema()
    {
        var json = WrapInPlan(new DomReadyTrigger(), SimpleReaction());
        AssertSchemaValid(json);
    }

    [Test]
    public void CustomEvent_trigger_conforms_to_schema()
    {
        var json = WrapInPlan(new CustomEventTrigger("test-event"), SimpleReaction());
        AssertSchemaValid(json);
    }

    [Test]
    public void ComponentEvent_trigger_with_all_properties_conforms_to_schema()
    {
        var trigger = new ComponentEventTrigger(
            componentId: "comp-1",
            jsEvent: "change",
            vendor: "fusion",
            bindingPath: "Address.City",
            readExpr: "value");
        var json = WrapInPlan(trigger, SimpleReaction());
        AssertSchemaValid(json);
    }

    [Test]
    public void ServerPush_trigger_with_all_properties_conforms_to_schema()
    {
        var trigger = new ServerPushTrigger("/api/stream", eventType: "notification");
        var json = WrapInPlan(trigger, SimpleReaction());
        AssertSchemaValid(json);
    }

    [Test]
    public void SignalR_trigger_conforms_to_schema()
    {
        var trigger = new SignalRTrigger("/hubs/data", "ReceiveUpdate");
        var json = WrapInPlan(trigger, SimpleReaction());
        AssertSchemaValid(json);
    }

    // ── Commands ──

    [Test]
    public void Dispatch_command_with_all_properties_conforms_to_schema()
    {
        var cmd = new DispatchCommand("event-name", payload: new { key = "value" }, when: SimpleGuard());
        var json = WrapInPlan(new DomReadyTrigger(), ReactionWith(cmd));
        AssertSchemaValid(json);
    }

    [Test]
    public void MutateElement_command_with_all_properties_conforms_to_schema()
    {
        var cmd = new MutateElementCommand(
            target: "elem-1",
            mutation: new SetPropMutation("textContent"),
            value: "hello",
            source: new EventSource("evt.name"),
            vendor: "native",
            when: SimpleGuard());
        var json = WrapInPlan(new DomReadyTrigger(), ReactionWith(cmd));
        AssertSchemaValid(json);
    }

    [Test]
    public void MutateElement_with_call_mutation_conforms_to_schema()
    {
        var cmd = new MutateElementCommand(
            target: "elem-1",
            mutation: new CallMutation(
                method: "updateData",
                chain: "then",
                args: new MethodArg[]
                {
                    new LiteralArg("data"),
                    new SourceArg(new ComponentSource("comp-1", "fusion", "value"), coerce: "number")
                }),
            vendor: "fusion");
        var json = WrapInPlan(new DomReadyTrigger(), ReactionWith(cmd));
        AssertSchemaValid(json);
    }

    [Test]
    public void MutateEvent_command_with_all_properties_conforms_to_schema()
    {
        var cmd = new MutateEventCommand(
            mutation: new SetPropMutation("preventDefaultAction", coerce: "boolean"),
            value: "true",
            source: new EventSource("evt.text"),
            when: SimpleGuard());
        var json = WrapInPlan(new DomReadyTrigger(), ReactionWith(cmd));
        AssertSchemaValid(json);
    }

    [Test]
    public void ValidationErrors_command_with_all_properties_conforms_to_schema()
    {
        var cmd = new ValidationErrorsCommand("form-1", when: SimpleGuard());
        var json = WrapInPlan(new DomReadyTrigger(), ReactionWith(cmd));
        AssertSchemaValid(json);
    }

    [Test]
    public void Into_command_with_all_properties_conforms_to_schema()
    {
        var cmd = new IntoCommand("container", when: SimpleGuard());
        var json = WrapInPlan(new DomReadyTrigger(), ReactionWith(cmd));
        AssertSchemaValid(json);
    }

    // ── Guards ──

    [Test]
    public void ValueGuard_with_operand_conforms_to_schema()
    {
        var guard = new ValueGuard(
            new EventSource("evt.amount"),
            coerceAs: "number",
            op: "gt",
            operand: 100);
        var cmd = new DispatchCommand("test", when: guard);
        var json = WrapInPlan(new DomReadyTrigger(), ReactionWith(cmd));
        AssertSchemaValid(json);
    }

    [Test]
    public void ValueGuard_with_right_source_conforms_to_schema()
    {
        var guard = new ValueGuard(
            left: new ComponentSource("comp-1", "native", "value"),
            coerceAs: "number",
            op: "lte",
            right: new ComponentSource("comp-2", "native", "value"));
        var cmd = new DispatchCommand("test", when: guard);
        var json = WrapInPlan(new DomReadyTrigger(), ReactionWith(cmd));
        AssertSchemaValid(json);
    }

    [Test]
    public void ValueGuard_with_elementCoerceAs_conforms_to_schema()
    {
        var guard = new ValueGuard(
            source: new ComponentSource("comp-1", "native", "value"),
            coerceAs: "array",
            op: "array-contains",
            operand: "item",
            elementCoerceAs: "string");
        var cmd = new DispatchCommand("test", when: guard);
        var json = WrapInPlan(new DomReadyTrigger(), ReactionWith(cmd));
        AssertSchemaValid(json);
    }

    [Test]
    public void AllGuard_conforms_to_schema()
    {
        var guard = new AllGuard(new Guard[]
        {
            new ValueGuard(new EventSource("evt.a"), "string", "eq", "x"),
            new ValueGuard(new EventSource("evt.b"), "string", "neq", "y")
        });
        var cmd = new DispatchCommand("test", when: guard);
        var json = WrapInPlan(new DomReadyTrigger(), ReactionWith(cmd));
        AssertSchemaValid(json);
    }

    [Test]
    public void AnyGuard_conforms_to_schema()
    {
        var guard = new AnyGuard(new Guard[]
        {
            new ValueGuard(new EventSource("evt.a"), "string", "eq", "x"),
            new ValueGuard(new EventSource("evt.b"), "string", "eq", "y")
        });
        var cmd = new DispatchCommand("test", when: guard);
        var json = WrapInPlan(new DomReadyTrigger(), ReactionWith(cmd));
        AssertSchemaValid(json);
    }

    [Test]
    public void InvertGuard_conforms_to_schema()
    {
        var guard = new InvertGuard(new ValueGuard(new EventSource("evt.a"), "boolean", "truthy"));
        var cmd = new DispatchCommand("test", when: guard);
        var json = WrapInPlan(new DomReadyTrigger(), ReactionWith(cmd));
        AssertSchemaValid(json);
    }

    [Test]
    public void ConfirmGuard_conforms_to_schema()
    {
        var guard = new ConfirmGuard("Are you sure?");
        var cmd = new DispatchCommand("test", when: guard);
        var json = WrapInPlan(new DomReadyTrigger(), ReactionWith(cmd));
        AssertSchemaValid(json);
    }

    // ── Reactions ──

    [Test]
    public void ConditionalReaction_with_all_properties_conforms_to_schema()
    {
        var reaction = new ConditionalReaction(
            commands: new List<Command> { new DispatchCommand("pre") },
            branches: new[]
            {
                new Branch(
                    new ValueGuard(new EventSource("evt.type"), "string", "eq", "A"),
                    new SequentialReaction(new List<Command> { new DispatchCommand("branch-a") })),
                new Branch(null, new SequentialReaction(new List<Command> { new DispatchCommand("else") }))
            });
        var json = WrapInPlan(new DomReadyTrigger(), reaction);
        AssertSchemaValid(json);
    }

    [Test]
    public void HttpReaction_with_all_properties_conforms_to_schema()
    {
        var reaction = new HttpReaction(
            preFetch: new List<Command> { new MutateElementCommand("spinner", new SetPropMutation("className"), value: "visible") },
            request: FullRequestDescriptor());
        var json = WrapInPlan(new DomReadyTrigger(), reaction);
        AssertSchemaValid(json);
    }

    [Test]
    public void ParallelHttpReaction_with_all_properties_conforms_to_schema()
    {
        var reaction = new ParallelHttpReaction(
            preFetch: new List<Command> { new DispatchCommand("loading") },
            requests: new List<RequestDescriptor>
            {
                SimpleRequest(),
                SimpleRequest()
            },
            onAllSettled: new List<Command> { new DispatchCommand("done") });
        var json = WrapInPlan(new DomReadyTrigger(), reaction);
        AssertSchemaValid(json);
    }

    // ── Gather items ──

    [Test]
    public void AllGather_conforms_to_schema()
    {
        var request = RequestWithGather(new AllGather());
        var json = WrapInPlan(new DomReadyTrigger(), new HttpReaction(null, request));
        AssertSchemaValid(json);
    }

    [Test]
    public void ComponentGather_conforms_to_schema()
    {
        var gather = new ComponentGather("comp-1", "native", "fieldName", "value");
        var request = RequestWithGather(gather);
        var json = WrapInPlan(new DomReadyTrigger(), new HttpReaction(null, request));
        AssertSchemaValid(json);
    }

    [Test]
    public void StaticGather_conforms_to_schema()
    {
        var gather = new StaticGather("mode", "edit");
        var request = RequestWithGather(gather);
        var json = WrapInPlan(new DomReadyTrigger(), new HttpReaction(null, request));
        AssertSchemaValid(json);
    }

    [Test]
    public void EventGather_conforms_to_schema()
    {
        var gather = new EventGather("query", "evt.text");
        var request = RequestWithGather(gather);
        var json = WrapInPlan(new DomReadyTrigger(), new HttpReaction(null, request));
        AssertSchemaValid(json);
    }

    // ── Validation ──

    [Test]
    public void Validation_descriptor_with_all_properties_conforms_to_schema()
    {
        var validation = new ValidationDescriptor("form-1", new List<ValidationField>
        {
            FullValidationField()
        });
        validation.PlanId = "TestModel";
        var request = new RequestDescriptor("POST", "/api/submit", validation: validation);
        var json = WrapInPlan(new DomReadyTrigger(), new HttpReaction(null, request));
        AssertSchemaValid(json);
    }

    // ── Request descriptor ──

    [Test]
    public void Request_with_chained_and_form_data_conforms_to_schema()
    {
        var request = new RequestDescriptor(
            verb: "POST",
            url: "/api/upload",
            gather: new List<GatherItem> { new AllGather() },
            contentType: "form-data",
            whileLoading: new List<Command> { new DispatchCommand("loading") },
            onSuccess: new List<StatusHandler> { new StatusHandler(200, new List<Command> { new DispatchCommand("ok") }) },
            onError: new List<StatusHandler> { new StatusHandler(new List<Command> { new DispatchCommand("err") }) },
            chained: SimpleRequest());
        var json = WrapInPlan(new DomReadyTrigger(), new HttpReaction(null, request));
        AssertSchemaValid(json);
    }

    [Test]
    public void StatusHandler_with_reaction_conforms_to_schema()
    {
        var handler = new StatusHandler(200, new ConditionalReaction(
            null,
            new[]
            {
                new Branch(null, new SequentialReaction(new List<Command> { new DispatchCommand("branch") }))
            }));
        var request = new RequestDescriptor("GET", "/api/data",
            onSuccess: new List<StatusHandler> { handler });
        var json = WrapInPlan(new DomReadyTrigger(), new HttpReaction(null, request));
        AssertSchemaValid(json);
    }

    // ── Components map ──

    [Test]
    public void ComponentEntry_with_all_properties_conforms_to_schema()
    {
        var json = JsonSerializer.Serialize(new
        {
            planId = "TestModel",
            components = new Dictionary<string, object>
            {
                ["FieldName"] = new
                {
                    id = "comp-1",
                    vendor = "fusion",
                    readExpr = "value",
                    componentType = "numerictextbox",
                    coerceAs = "number"
                }
            },
            entries = Array.Empty<object>()
        }, SerializeOptions);
        AssertSchemaValid(json);
    }

    // ── Source types ──

    [Test]
    public void EventSource_conforms_to_schema()
    {
        var cmd = new MutateElementCommand(
            "elem",
            new SetPropMutation("textContent"),
            source: new EventSource("evt.name"));
        var json = WrapInPlan(new DomReadyTrigger(), ReactionWith(cmd));
        AssertSchemaValid(json);
    }

    [Test]
    public void ComponentSource_conforms_to_schema()
    {
        var cmd = new MutateElementCommand(
            "elem",
            new SetPropMutation("textContent"),
            source: new ComponentSource("comp-1", "native", "value"));
        var json = WrapInPlan(new DomReadyTrigger(), ReactionWith(cmd));
        AssertSchemaValid(json);
    }

    // ── Mutation types ──

    [Test]
    public void SetPropMutation_with_coerce_conforms_to_schema()
    {
        var cmd = new MutateElementCommand(
            "elem",
            new SetPropMutation("value", coerce: "number"));
        var json = WrapInPlan(new DomReadyTrigger(), ReactionWith(cmd));
        AssertSchemaValid(json);
    }

    [Test]
    public void CallMutation_with_all_properties_conforms_to_schema()
    {
        var cmd = new MutateElementCommand(
            "elem",
            new CallMutation(
                method: "doSomething",
                chain: "refresh",
                args: new MethodArg[]
                {
                    new LiteralArg(42),
                    new SourceArg(new EventSource("evt.data"), coerce: "string")
                }),
            vendor: "fusion");
        var json = WrapInPlan(new DomReadyTrigger(), ReactionWith(cmd));
        AssertSchemaValid(json);
    }

    // ── Helpers ──

    private static Guard SimpleGuard()
        => new ValueGuard(new EventSource("evt.flag"), "boolean", "truthy");

    private static SequentialReaction SimpleReaction()
        => new(new List<Command> { new DispatchCommand("test") });

    private static SequentialReaction ReactionWith(Command cmd)
        => new(new List<Command> { cmd });

    private static RequestDescriptor SimpleRequest()
        => new("GET", "/api/data");

    private static RequestDescriptor RequestWithGather(GatherItem gather)
        => new("POST", "/api/data", gather: new List<GatherItem> { gather });

    private static RequestDescriptor FullRequestDescriptor()
    {
        return new RequestDescriptor(
            verb: "POST",
            url: "/api/data",
            gather: new List<GatherItem>
            {
                new ComponentGather("comp-1", "native", "name", "value"),
                new StaticGather("mode", "edit"),
                new EventGather("q", "evt.text"),
                new AllGather()
            },
            whileLoading: new List<Command> { new DispatchCommand("loading") },
            onSuccess: new List<StatusHandler>
            {
                new(200, new List<Command> { new DispatchCommand("ok") }),
                new(new List<Command> { new DispatchCommand("fallback") })
            },
            onError: new List<StatusHandler>
            {
                new(500, new List<Command> { new DispatchCommand("error") })
            },
            chained: new RequestDescriptor("GET", "/api/next"));
    }

    private static ValidationField FullValidationField()
    {
        var field = new ValidationField("FieldName", new List<ValidationRule>
        {
            new("required", "Field is required"),
            new("minLength", "Too short", constraint: 3),
            new("range", "Out of range", constraint: new[] { 1, 100 }),
            new("equalTo", "Must match", field: "OtherField", coerceAs: "number"),
            new("required", "Required when active", when: new ValidationCondition("IsActive", "truthy"))
        });
        field.FieldId = "field-1";
        field.Vendor = "native";
        field.ReadExpr = "value";
        field.CoerceAs = "string";
        return field;
    }

    private static string WrapInPlan(Trigger trigger, Reaction reaction)
    {
        var entry = new Entry(trigger, reaction);
        var plan = new
        {
            planId = "TestModel",
            components = new Dictionary<string, object>(),
            entries = new[] { entry }
        };
        return JsonSerializer.Serialize(plan, SerializeOptions);
    }

    // ── Verification: proves drift detection works ──
    // This test intentionally creates a plan with an unknown property.
    // It should FAIL schema validation, proving the detection catches drift.

    [Test]
    public void VERIFY_drift_is_detected_when_unknown_property_exists()
    {
        var json = JsonSerializer.Serialize(new
        {
            planId = "TestModel",
            components = new Dictionary<string, object>
            {
                ["Field"] = new
                {
                    id = "c1",
                    vendor = "native",
                    readExpr = "value",
                    componentType = "textbox",
                    coerceAs = "string",
                    extraProp = "this-should-be-rejected" // simulated drift
                }
            },
            entries = Array.Empty<object>()
        }, SerializeOptions);

        using var doc = JsonDocument.Parse(json);
        var result = Schema.Evaluate(doc.RootElement, new EvaluationOptions
        {
            OutputFormat = OutputFormat.List
        });

        Assert.That(result.IsValid, Is.False,
            "Schema should reject unknown property 'extraProp' — drift detection mechanism is broken if this passes");
    }
}
