using System.Text.Json;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.UnitTests;

public class SegmentPayload
{
    public string? Name { get; set; }
    public int Count { get; set; }
}

/// <summary>
/// F3 — BuildAction() must throw when the pipeline has multiple segments.
/// Test IDs: F-T4 (BuildActions count), F-T5 (BuildAction throws),
/// F-T6 (TriggerBuilder produces correct workflow count).
/// </summary>
[TestFixture]
public class WhenEnforcingBuildActionContract : PlanTestBase
{
    [Test]
    public void Two_when_blocks_produce_two_branch_actions()
    {
        var pb = CreateDomReadyPipeline();
        var payload = new SegmentPayload();
        pb.When(payload, (SegmentPayload x) => x.Name).Eq("a")
            .Then(t => t.Element("r1").SetText("first"));
        pb.When(payload, (SegmentPayload x) => x.Count).Gt(0)
            .Then(t => t.Element("r2").SetText("second"));

        var actions = pb.BuildActions();
        Assert.That(actions, Has.Count.EqualTo(2));
        Assert.That(actions[0], Is.InstanceOf<BranchAction>());
        Assert.That(actions[1], Is.InstanceOf<BranchAction>());
    }

    [Test]
    public void BuildAction_throws_when_multiple_segments_exist()
    {
        var pb = CreateDomReadyPipeline();
        var payload = new SegmentPayload();
        pb.When(payload, (SegmentPayload x) => x.Name).Eq("a")
            .Then(t => t.Element("r1").SetText("first"));
        pb.When(payload, (SegmentPayload x) => x.Count).Gt(0)
            .Then(t => t.Element("r2").SetText("second"));

        var ex = Assert.Throws<InvalidOperationException>(() => pb.BuildAction());
        Assert.That(ex!.Message, Does.Contain("BuildActions()"));
        Assert.That(ex.Message, Does.Contain("2"));
    }

    [Test]
    public void Single_conditional_segment_returns_branch_action()
    {
        var pb = CreateDomReadyPipeline();
        var payload = new SegmentPayload();
        pb.When(payload, (SegmentPayload x) => x.Name).Eq("a")
            .Then(t => t.Element("r1").SetText("matched"));

        var action = pb.BuildAction();
        Assert.That(action, Is.InstanceOf<BranchAction>());
    }

    [Test]
    public void Sequential_commands_return_sequence_action_with_correct_count()
    {
        var pb = CreateDomReadyPipeline();
        pb.Element("a").SetText("one");
        pb.Element("b").SetText("two");
        pb.Dispatch("done");

        var action = pb.BuildAction();
        Assert.That(action, Is.InstanceOf<SequenceAction>());
        var sequence = (SequenceAction)action;
        Assert.That(sequence.Steps, Has.Count.EqualTo(3));
    }

    [Test]
    public Task TriggerBuilder_two_when_blocks_produce_two_workflows()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<SegmentPayload>("test", (args, p) =>
        {
            p.When(args, x => x.Name).Eq("a")
                .Then(t => t.Element("r1").SetText("first"));
            p.When(args, x => x.Count).Gt(0)
                .Then(t => t.Element("r2").SetText("second"));
        });

        var json = plan.Render();
        using var doc = JsonDocument.Parse(json);
        Assert.That(doc.RootElement.GetProperty("workflows").GetArrayLength(), Is.EqualTo(2));
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
