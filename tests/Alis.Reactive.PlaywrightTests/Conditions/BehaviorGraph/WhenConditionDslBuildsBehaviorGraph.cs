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
        var reaction = doc.RootElement
            .GetProperty("behaviors")[0]
            .GetProperty("reaction");
        var steps = reaction.GetProperty("steps")
            .EnumerateArray()
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(reaction.GetProperty("kind").GetString(), Is.EqualTo("sequence"));
            Assert.That(steps.Select(ReactionKind), Is.EqualTo(new[]
            {
                "sequence",
                "branch",
                "request",
                "branch",
                "sequence"
            }));

            Assert.That(BranchGuardKinds(steps[1]), Is.EqualTo(new[]
            {
                "when",
                "when",
                "default"
            }));

            Assert.That(BranchGuardKinds(steps[3]), Is.EqualTo(new[]
            {
                "when",
                "default"
            }));

            Assert.That(steps[2]
                .GetProperty("request")
                .GetProperty("input")
                .GetProperty("assignments")
                .GetArrayLength(), Is.EqualTo(1));
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
                    .And(schedule.CurrentView()).Eq("Week")
                    .And(inner => inner
                        .When(schedule.GetEvents()).NotEmpty()
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
        var comparisons = AllCompareConditions(doc.RootElement)
            .Select(ConditionComparison.From)
            .ToArray();

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

    private static string ReactionKind(JsonElement reaction) =>
        reaction.GetProperty("kind").GetString()!;

    private static string[] BranchGuardKinds(JsonElement branch)
    {
        return branch
            .GetProperty("cases")
            .EnumerateArray()
            .Select(branchCase => branchCase
                .GetProperty("guard")
                .GetProperty("kind")
                .GetString()!)
            .ToArray();
    }

    private static List<JsonElement> AllCompareConditions(JsonElement root)
    {
        var comparisons = new List<JsonElement>();
        CollectCompareConditions(root, comparisons);
        return comparisons;
    }

    private static void CollectCompareConditions(JsonElement element, List<JsonElement> comparisons)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                if (element.TryGetProperty("kind", out var kind)
                    && kind.GetString() == "compare")
                {
                    comparisons.Add(element);
                }

                foreach (var property in element.EnumerateObject())
                    CollectCompareConditions(property.Value, comparisons);
                return;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    CollectCompareConditions(item, comparisons);
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
        internal static ConditionComparison From(JsonElement condition)
        {
            var left = condition.GetProperty("left");
            var right = RightReadOrNull(condition);

            return new ConditionComparison(
                condition.GetProperty("op").GetString()!,
                SourceLabel(left),
                left.GetProperty("member").GetString()!,
                AccessLabel(left),
                right.HasValue ? SourceLabel(right.Value) : null,
                right.HasValue ? right.Value.GetProperty("member").GetString() : null,
                right.HasValue ? AccessLabel(right.Value) : null);
        }

        private static JsonElement? RightReadOrNull(JsonElement condition)
        {
            var right = condition.GetProperty("right");
            if (right.GetProperty("kind").GetString() != "value")
                return null;

            var value = right.GetProperty("value");
            if (!value.TryGetProperty("kind", out var kind) || kind.GetString() != "read")
                return null;

            return value;
        }

        private static string SourceLabel(JsonElement read)
        {
            var source = read.GetProperty("from");
            var kind = source.GetProperty("kind").GetString();
            if (kind != "payload") return kind!;

            return "payload:" + source.GetProperty("scope").GetString();
        }

        private static string AccessLabel(JsonElement read) =>
            read.GetProperty("access").GetProperty("kind").GetString()!;
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

    private sealed class ConditionBehaviorPlugin : ReactivePlugin
    {
        public ConditionBehaviorPlugin()
            : base("conditionBehavior")
        {
            ExpectedStatus = Property<string>("expectedStatus");
            NormalizeStatus = Function<string, string>("normalizeStatus");
        }

        public PluginProperty<string> ExpectedStatus { get; }
        public PluginFunction<string> NormalizeStatus { get; }
    }
}
