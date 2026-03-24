using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors.Reactions;

namespace Alis.Reactive.UnitTests;

public class SegmentPayload
{
    public string? Name { get; set; }
    public int Count { get; set; }
}

/// <summary>
/// F3 — BuildReaction() must throw when the pipeline has multiple segments.
/// Test IDs: F-T4 (BuildReactions count), F-T5 (BuildReaction throws),
/// F-T6 (TriggerBuilder produces correct entry count).
/// </summary>
[TestFixture]
public class WhenEnforcingBuildReactionContract : PlanTestBase
{
    // ═══════════════════════════════════════════════════════════
    // F-T4 — BuildReactions().Count == 2 for two When blocks
    // ═══════════════════════════════════════════════════════════

    [Test]
    public void Two_when_blocks_produce_two_conditional_reactions()
    {
        var pb = new PipelineBuilder<TestModel>();
        var payload = new SegmentPayload();
        pb.When(payload, (SegmentPayload x) => x.Name).Eq("a")
            .Then(t => t.Element("r1").SetText("first"));
        pb.When(payload, (SegmentPayload x) => x.Count).Gt(0)
            .Then(t => t.Element("r2").SetText("second"));

        var reactions = pb.BuildReactions();
        Assert.That(reactions, Has.Count.EqualTo(2));
        Assert.That(reactions[0], Is.InstanceOf<ConditionalReaction>());
        Assert.That(reactions[1], Is.InstanceOf<ConditionalReaction>());
    }

    // ═══════════════════════════════════════════════════════════
    // F-T5 — BuildReaction() throws on multi-segment pipeline
    // ═══════════════════════════════════════════════════════════

    [Test]
    public void BuildReaction_throws_when_multiple_segments_exist()
    {
        var pb = new PipelineBuilder<TestModel>();
        var payload = new SegmentPayload();
        pb.When(payload, (SegmentPayload x) => x.Name).Eq("a")
            .Then(t => t.Element("r1").SetText("first"));
        pb.When(payload, (SegmentPayload x) => x.Count).Gt(0)
            .Then(t => t.Element("r2").SetText("second"));

        var ex = Assert.Throws<InvalidOperationException>(() => pb.BuildReaction());
        Assert.That(ex!.Message, Does.Contain("BuildReactions()"));
        Assert.That(ex.Message, Does.Contain("2"),
            "Error message should include the actual segment count");
    }

    [Test]
    public void Single_conditional_segment_returns_conditional_reaction()
    {
        var pb = new PipelineBuilder<TestModel>();
        var payload = new SegmentPayload();
        pb.When(payload, (SegmentPayload x) => x.Name).Eq("a")
            .Then(t => t.Element("r1").SetText("matched"));

        var reaction = pb.BuildReaction();
        Assert.That(reaction, Is.InstanceOf<ConditionalReaction>());
    }

    [Test]
    public void Sequential_commands_return_sequential_reaction_with_correct_count()
    {
        var pb = new PipelineBuilder<TestModel>();
        pb.Element("a").SetText("one");
        pb.Element("b").SetText("two");
        pb.Dispatch("done");

        var reaction = pb.BuildReaction();
        Assert.That(reaction, Is.InstanceOf<SequentialReaction>());
        var seq = (SequentialReaction)reaction;
        Assert.That(seq.Commands, Has.Count.EqualTo(3));
    }

    // ═══════════════════════════════════════════════════════════
    // F-T6 — TriggerBuilder with 2 When blocks → 2 entries,
    //         plan renders valid JSON with both conditions
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task TriggerBuilder_two_when_blocks_produce_two_plan_entries()
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
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
