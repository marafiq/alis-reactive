using System.Text.Json;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Fusion.Components;
using Alis.Reactive.Native.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.PlaywrightTests.Conditions.BehaviorGraph;

[TestFixture]
public sealed class WhenConditionDslBuildsBehaviorGraph
{
    private static readonly IHtmlHelper<ConditionBehaviorModel> Html = null!;

    [Test]
    public void repeated_branch_blocks_mixed_with_requests_keep_declaration_order()
    {
        var plan = PlanExtensions.ReactivePlan(Html);

        HtmlExtensions.On(Html, plan, trigger =>
            trigger.CustomEvent<ConditionBehaviorEvent>("score-changed", (args, pipeline) =>
            {
                pipeline.Element("start").SetText("start");

                pipeline.When(args, x => x.Score).Gte(90)
                    .Then(hit => hit.Element("grade").SetText("A"))
                    .ElseIf(args, x => x.Score).Gte(80)
                    .Then(hit => hit.Element("grade").SetText("B"))
                    .Else(miss => miss.Element("grade").SetText("Other"));

                pipeline.Post("/audit", gather => gather.FromEvent(args, x => x.Score, "score"));

                pipeline.When(args, x => x.IsReady).Truthy()
                    .Then(ready => ready.Dispatch("ready"))
                    .Else(notReady => notReady.Dispatch("not-ready"));

                pipeline.Element("done").SetText("done");
            }));

        using var doc = JsonDocument.Parse(plan.RenderFormatted());
        var statements = doc.RootElement
            .GetProperty("handlers")[0]
            .GetProperty("do")
            .EnumerateArray()
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(statements.Select(StatementKind), Is.EqualTo(new[]
            {
                "set",
                "if",
                "let",
                "if",
                "set"
            }));

            Assert.That(ArmCount(statements[1]), Is.EqualTo(2));
            Assert.That(statements[1].GetProperty("else").GetArrayLength(), Is.GreaterThan(0));

            Assert.That(ArmCount(statements[3]), Is.EqualTo(1));
            Assert.That(statements[3].GetProperty("else").GetArrayLength(), Is.GreaterThan(0));

            Assert.That(SentValueNames(statements[2]), Is.EqualTo(new[] { "score" }));
        });
    }

    [Test]
    public void condition_guards_preserve_typed_value_sources()
    {
        var plan = PlanExtensions.ReactivePlan(Html);
        var plugin = plan.RegisterPlugin<ConditionBehaviorPlugin>();

        HtmlExtensions.On(Html, plan, trigger =>
            trigger.CustomEvent<ConditionBehaviorEvent>("score-changed", (args, pipeline) =>
            {
                var schedule = pipeline.Component<FusionSchedule>("shift-schedule");
                TypedPluginSource<string> normalizedStatus =
                    pipeline.Plugin(plugin.NormalizeStatus)
                        .Arg(args, x => x.Status);
                var expectedStatus = pipeline.Plugin(plugin.ExpectedStatus);

                pipeline.When(args, x => x.Score).Gte(pipeline.FromUrl<int>("minScore"))
                    .And(schedule.Get(c => c.CurrentView)).Eq("Week")
                    .And(inner => inner
                        .When(schedule.Get(c => c.GetEvents<object>())).NotEmpty()
                        .Or(normalizedStatus).Eq(expectedStatus))
                    .Then(hit => hit.Element("event-branch").SetText("accepted"))
                    .ElseIf(args, x => x.Status).Eq("manual")
                    .Then(hit => hit.Element("event-branch").SetText("manual"))
                    .Else(miss => miss.Element("event-branch").SetText("rejected"));

                pipeline.Get("/status")
                    .Response(response => response.OnSuccess<ConditionBehaviorResponse>((json, success) =>
                    {
                        success.When(json, x => x.Status).Eq("approved")
                            .Then(hit => hit.Post("/resident/{residentId}/approve", gather => gather
                                .RouteParam("residentId", json.Read(x => x.ResidentId))))
                            .Else(miss => miss.Element("response-branch").SetText(json, x => x.Status));
                    }));
            }));

        using var doc = JsonDocument.Parse(plan.RenderFormatted());
        var comparisons = AllComparisons(doc.RootElement).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(comparisons, Does.Contain(new ConditionComparison(
                "gte",
                "payload:event",
                "score",
                "property",
                "url",
                "minScore",
                "property")));
            Assert.That(comparisons, Does.Contain(new ConditionComparison(
                "eq",
                "component",
                "currentView",
                "property",
                null,
                null,
                null)));
            Assert.That(comparisons, Does.Contain(new ConditionComparison(
                "not-empty",
                "component",
                "getEvents",
                "method",
                null,
                null,
                null)));
            Assert.That(comparisons, Does.Contain(new ConditionComparison(
                "eq",
                "plugin",
                "normalizeStatus",
                "method",
                "plugin",
                "expectedStatus",
                "property")));
            Assert.That(comparisons, Does.Contain(new ConditionComparison(
                "eq",
                "payload:success",
                "status",
                "property",
                null,
                null,
                null)));
        });
    }

    private static string StatementKind(JsonElement statement) =>
        statement.GetProperty("kind").GetString()!;

    private static int ArmCount(JsonElement ifStatement) =>
        ifStatement.GetProperty("arms").GetArrayLength();

    // let res = await http.send({ ..., values: { … } }): the names the request sends.
    private static string[] SentValueNames(JsonElement letStatement) =>
        letStatement.GetProperty("expr").GetProperty("args")[0]
            .GetProperty("entries").EnumerateArray()
            .Single(entry => entry.GetProperty("name").GetString() == "values")
            .GetProperty("value").GetProperty("entries").EnumerateArray()
            .Select(entry => entry.GetProperty("name").GetString()!)
            .ToArray();

    private static readonly string[] ComparisonOperators = { "eq", "neq", "lt", "lte", "gt", "gte" };

    // Every comparison in the plan: an ordering or equality operator, or a test for emptiness.
    private static List<ConditionComparison> AllComparisons(JsonElement root)
    {
        var comparisons = new List<ConditionComparison>();
        CollectComparisons(root, comparisons);
        return comparisons;
    }

    private static void CollectComparisons(JsonElement element, List<ConditionComparison> comparisons)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                if (ConditionComparison.TryFrom(element, out var comparison))
                    comparisons.Add(comparison);

                foreach (var property in element.EnumerateObject())
                    CollectComparisons(property.Value, comparisons);
                return;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    CollectComparisons(item, comparisons);
                return;
        }
    }

    private sealed record ConditionComparison(
        string Op,
        string LeftSource,
        string LeftMember,
        string LeftAccess,
        string? RightSource,
        string? RightMember,
        string? RightAccess)
    {
        internal static bool TryFrom(JsonElement node, out ConditionComparison comparison)
        {
            comparison = null!;
            if (!IsOp(node, out var op)) return false;

            if (op == "not" && IsOp(node.GetProperty("args")[0], out var inner) && inner == "empty")
                return TryUnary("not-empty", node.GetProperty("args")[0].GetProperty("args")[0], out comparison);
            if (op == "empty")
                return TryUnary("is-empty", node.GetProperty("args")[0], out comparison);
            if (!ComparisonOperators.Contains(op)) return false;

            var left = Read.From(node.GetProperty("args")[0]);
            var right = Read.From(node.GetProperty("args")[1]);
            if (left == null) return false;
            comparison = new ConditionComparison(op, left.Source, left.Member, left.Access, right?.Source, right?.Member, right?.Access);
            return true;
        }

        private static bool TryUnary(string op, JsonElement operand, out ConditionComparison comparison)
        {
            comparison = null!;
            var read = Read.From(operand);
            if (read == null) return false;
            comparison = new ConditionComparison(op, read.Source, read.Member, read.Access, null, null, null);
            return true;
        }

        private static bool IsOp(JsonElement node, out string op)
        {
            op = "";
            if (node.ValueKind != JsonValueKind.Object || !node.TryGetProperty("kind", out var kind) || kind.GetString() != "op")
                return false;
            op = node.GetProperty("op").GetString()!;
            return true;
        }
    }

    // A read in the grammar: a get or call on a binding or a located object; a literal is no read.
    private sealed record Read(string Source, string Member, string Access)
    {
        internal static Read? From(JsonElement node)
        {
            var kind = node.GetProperty("kind").GetString();
            if (kind != "get" && kind != "call") return null;

            var path = node.GetProperty("path").EnumerateArray().Select(segment => segment.GetString()!).ToArray();
            var target = node.GetProperty("target");
            if (path.Length == 0) return From(target);

            var access = kind == "call" ? "method" : "property";
            return new Read(SourceOf(target, path), path[^1], access);
        }

        private static string SourceOf(JsonElement target, string[] path)
        {
            var kind = target.GetProperty("kind").GetString();
            if (kind == "ref")
                return path[0] == "body" ? "payload:success" : "payload:event";
            if (kind == "get" && target.GetProperty("path")[0].GetString() == "query")
                return "url";

            var id = target.GetProperty("id").GetString()!;
            return id.StartsWith("$plugin:", StringComparison.Ordinal) ? "plugin" : "component";
        }
    }

    private sealed class ConditionBehaviorModel
    {
    }

    private sealed class ConditionBehaviorEvent
    {
        public int Score { get; set; }
        public bool IsReady { get; set; }
        public string Status { get; set; } = "";
    }

    private sealed class ConditionBehaviorResponse
    {
        public int ResidentId { get; set; }
        public string Status { get; set; } = "";
    }

    private sealed class ConditionBehaviorPlugin : Plugin
    {
        public ConditionBehaviorPlugin()
            : base("conditionBehavior")
        {
            ExpectedStatus = Property<string>("expectedStatus");
            NormalizeStatus = Function<string>("normalizeStatus").Arg<string>();
        }

        public PluginProperty<string> ExpectedStatus { get; }
        public PluginFunction<string> NormalizeStatus { get; }
    }
}
