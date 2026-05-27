using System.Text.Json;
using Alis.Reactive.Native.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.PlaywrightTests.Conditions.Projection;

[TestFixture]
public sealed class WhenConditionDslBuildsBehaviorGraph
{
    private static readonly IHtmlHelper<ConditionProjectionModel> Html = null!;

    [Test]
    public void mixed_conditions_requests_and_commands_keep_declaration_order()
    {
        var plan = PlanExtensions.ReactivePlan(Html);

        HtmlExtensions.On(Html, plan, trigger =>
            trigger.CustomEvent<ConditionProjectionEvent>("score-changed", (args, pipeline) =>
            {
                pipeline.Element("start").SetText("start");

                pipeline.When(args, x => x.Score).Gte(90)
                    .Then(hit => hit.Element("grade").SetText("A"))
                    .ElseIf(args, x => x.Score).Gte(80)
                    .Then(hit => hit.Element("grade").SetText("B"))
                    .Else(miss => miss.Element("grade").SetText("Other"));

                pipeline.Post("/audit", gather => gather.FromEvent(args, x => x.Score, "score"));

                pipeline.When(args, x => x.IsReady).Truthy()
                    .Then(ready => ready.Dispatch("ready"));

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
                "when"
            }));

            Assert.That(steps[2]
                .GetProperty("request")
                .GetProperty("input")
                .GetProperty("assignments")
                .GetArrayLength(), Is.EqualTo(1));
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

    private sealed class ConditionProjectionModel
    {
    }

    private sealed class ConditionProjectionEvent
    {
        public int Score { get; set; }
        public bool IsReady { get; set; }
    }
}
