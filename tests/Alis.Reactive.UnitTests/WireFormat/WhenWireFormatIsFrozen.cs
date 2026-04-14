using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.UnitTests.WireFormat;

/// <summary>
/// Wire format freeze tests (P3). One test per plan model family. Each test serializes a
/// canonical instance covering every concrete variant in the family, then compares the
/// rendered JSON against a manually-committed snapshot file under Snapshots/.
///
/// If a wire format change breaks one of these tests, the failure message says explicitly:
/// "Wire format for {Family} changed. This is a breaking change to the 1.0 contract.
///  Either revert the change or bump the major version."
///
/// Snapshots are NOT auto-accepted. Updating a snapshot requires an explicit edit to the
/// .snap.json file, which is visible in every diff and reviewed manually.
/// </summary>
[TestFixture]
public class WhenWireFormatIsFrozen : PlanTestBase
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private static string SnapshotPath(string family) =>
        System.IO.Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "WireFormat",
            "Snapshots",
            $"{family}.snap.json");

    private static void AssertSnapshot(string family, string actualJson)
    {
        var path = SnapshotPath(family);
        if (!File.Exists(path))
        {
            // First run — write the snapshot for human review, then fail with a clear message.
            // The test author MUST commit the snapshot file by hand after reviewing the JSON.
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
            File.WriteAllText(path, actualJson);
            Assert.Fail(
                $"Wire format snapshot for {family} did not exist. " +
                $"Created at {path}. Review the JSON content, then commit the file. " +
                $"This message will not appear on subsequent runs.");
        }

        var expected = File.ReadAllText(path);
        if (expected != actualJson)
        {
            Assert.Fail(
                $"Wire format for {family} changed. This is a breaking change to the 1.0 " +
                $"contract. Either revert the change or bump the major version.\n\n" +
                $"Expected (committed snapshot at {path}):\n{expected}\n\n" +
                $"Actual:\n{actualJson}");
        }
    }

    private static string SerializeArray<T>(IEnumerable<T> items) where T : class
    {
        // Force each item through the polymorphic converter by serializing via the base type T.
        // This is the same pipeline ReactivePlanSerializer uses inside a real plan render.
        var list = new List<JsonElement>();
        foreach (var item in items)
        {
            var itemJson = JsonSerializer.Serialize<T>(item, Options);
            list.Add(JsonDocument.Parse(itemJson).RootElement.Clone());
        }
        return JsonSerializer.Serialize(list, Options);
    }

    // ── Family snapshot tests ──────────────────────────────────────

    [Test]
    public void WireFormatIsFrozen_Shape()
    {
        var canonical = new Shape[]
        {
            Shape.String,
            Shape.Number,
            Shape.Boolean,
            Shape.Date,
            Shape.Raw,
            Shape.Any,
            Shape.None,
            Shape.ArrayOf(Shape.String),
            Shape.Nullable(Shape.Number),
            Shape.ObjectOf(new Dictionary<string, Shape>
            {
                ["name"] = Shape.String,
                ["count"] = Shape.Number
            })
        };
        AssertSnapshot("Shape", SerializeArray(canonical));
    }

    [Test]
    public void WireFormatIsFrozen_ValueProducer()
    {
        var canonical = new ValueProducer[]
        {
            ValueProducer.Literal("hello"),
            ValueProducer.Literal(42),
            ValueProducer.Literal(true),
            ValueProducer.LiteralRaw(null!, Shape.None),
            ValueProducer.Read(UrlSource.Instance, "tab"),
            ValueProducer.Object(new Dictionary<string, ValueProducer>
            {
                ["name"] = ValueProducer.Literal("Alice")
            }),
            ValueProducer.Array(new List<ValueProducer>
            {
                ValueProducer.Literal(1),
                ValueProducer.Literal(2)
            }),
            ValueProducer.None
        };
        AssertSnapshot("ValueProducer", SerializeArray(canonical));
    }

    [Test]
    public void WireFormatIsFrozen_Reaction()
    {
        var step = Reaction.ShowValidationErrors("c1");
        var canonical = new Reaction[]
        {
            Reaction.Sequence(step),
            Reaction.Parallel(new List<Reaction> { step }, onSettled: null),
            Reaction.Parallel(new List<Reaction> { step }, onSettled: step),
            Reaction.Branch(BranchCase.Default(step)),
            Reaction.Set(UrlSource.Instance, "value", ValueProducer.Literal("x")),
            Reaction.Call(UrlSource.Instance, "method"),
            Reaction.Dispatch("event-name"),
            Reaction.Dispatch("typed-event", ValueProducer.Literal("payload"), "MyPayloadType"),
            Reaction.Inject("comp1", ValueProducer.Literal("html")),
            Reaction.ShowValidationErrors("container1"),
            Reaction.NoOp
        };
        AssertSnapshot("Reaction", SerializeArray(canonical));
    }

    [Test]
    public void WireFormatIsFrozen_Source()
    {
        var canonical = new Source[]
        {
            ComponentSource.Of("comp1"),
            PayloadSource.Event(),
            PayloadSource.Event("MyPayloadType"),
            PayloadSource.Success(),
            PayloadSource.Error(),
            PayloadSource.Request(),
            PayloadSource.Dispatch(),
            PayloadSource.Local(),
            UrlSource.Instance,
            PluginSource.Of("myPlugin")
        };
        AssertSnapshot("Source", SerializeArray(canonical));
    }

    [Test]
    public void WireFormatIsFrozen_Condition()
    {
        var lhs = ValueProducer.Read(ComponentSource.Of("c1"), "value");
        var canonical = new Condition[]
        {
            Condition.Compare(lhs, "eq", ValueProducer.Literal("hello")),
            Condition.Compare(lhs, "truthy"),
            Condition.All(
                Condition.Compare(lhs, "truthy"),
                Condition.Compare(lhs, "eq", ValueProducer.Literal(1))
            ),
            Condition.Any(
                Condition.Compare(lhs, "truthy"),
                Condition.Compare(lhs, "falsy")
            ),
            Condition.Not(Condition.Compare(lhs, "truthy")),
            Condition.Confirm("Are you sure?"),
            Condition.None
        };
        AssertSnapshot("Condition", SerializeArray(canonical));
    }

    [Test]
    public void WireFormatIsFrozen_StartsWhen()
    {
        var canonical = new StartsWhen[]
        {
            StartsWhen.PageReady(),
            StartsWhen.DocumentEvent("custom-event"),
            StartsWhen.DocumentEvent("typed-event", "MyPayloadType"),
            StartsWhen.ComponentEvent("comp1", "change"),
            StartsWhen.ServerPush("/api/sse"),
            StartsWhen.ServerPush("/api/sse", "filtered-event", "MyPayloadType"),
            StartsWhen.SignalR("/hub", "OnUpdate"),
            StartsWhen.SignalR("/hub", "OnUpdateTyped", "MyPayloadType")
        };
        AssertSnapshot("StartsWhen", SerializeArray(canonical));
    }

    [Test]
    public void WireFormatIsFrozen_PathSegment()
    {
        var canonical = new PathSegment[]
        {
            PathSegment.Property("name"),
            PathSegment.Property("nested"),
            PathSegment.AtIndex(0),
            PathSegment.AtIndex(42)
        };
        AssertSnapshot("PathSegment", SerializeArray(canonical));
    }

    [Test]
    public void WireFormatIsFrozen_Plan_envelope()
    {
        // End-to-end: build a minimal but representative plan via the public DSL,
        // render it through ReactivePlanSerializer (the production path), and freeze
        // the entire envelope shape including version, planId, types, components, behaviors.
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Element("test-id").SetText("hello");
        });
        // RenderFormatted produces stable indented JSON suitable for snapshot diffing.
        var json = plan.RenderFormatted();
        // The plan JSON contains a planId that is unique per ReactivePlan instance.
        // Strip it from the snapshot so the test is deterministic across runs.
        var stable = StripPlanId(json);
        AssertSnapshot("Plan_envelope", stable);
    }

    private static string StripPlanId(string json)
    {
        // Replace any "planId": "<value>" with "planId": "<stable>" so the snapshot
        // is deterministic. The plan ID is generated per-instance and doesn't reflect
        // the wire format itself.
        return System.Text.RegularExpressions.Regex.Replace(
            json,
            "\"planId\":\\s*\"[^\"]*\"",
            "\"planId\": \"<stable>\"");
    }
}
