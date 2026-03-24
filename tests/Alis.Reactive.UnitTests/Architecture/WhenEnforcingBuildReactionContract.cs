using Alis.Reactive.Builders;

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
    public void Two_when_blocks_produce_two_reactions()
    {
        var pb = new PipelineBuilder<TestModel>();
        var payload = new SegmentPayload();
        pb.When(payload, (SegmentPayload x) => x.Name).Eq("a")
            .Then(t => t.Element("r1").SetText("first"));
        pb.When(payload, (SegmentPayload x) => x.Count).Gt(0)
            .Then(t => t.Element("r2").SetText("second"));

        var reactions = pb.BuildReactions();
        Assert.That(reactions.Count, Is.EqualTo(2));
    }

    // ═══════════════════════════════════════════════════════════
    // F-T5 — BuildReaction() throws on multi-segment pipeline
    // ═══════════════════════════════════════════════════════════

    [Test]
    public void BuildReaction_throws_when_multiple_segments()
    {
        var pb = new PipelineBuilder<TestModel>();
        var payload = new SegmentPayload();
        pb.When(payload, (SegmentPayload x) => x.Name).Eq("a")
            .Then(t => t.Element("r1").SetText("first"));
        pb.When(payload, (SegmentPayload x) => x.Count).Gt(0)
            .Then(t => t.Element("r2").SetText("second"));

        var ex = Assert.Throws<InvalidOperationException>(() => pb.BuildReaction());
        Assert.That(ex!.Message, Does.Contain("BuildReactions()"));
    }

    [Test]
    public void BuildReaction_succeeds_for_single_segment()
    {
        var pb = new PipelineBuilder<TestModel>();
        var payload = new SegmentPayload();
        pb.When(payload, (SegmentPayload x) => x.Name).Eq("a")
            .Then(t => t.Element("r1").SetText("matched"));

        var reaction = pb.BuildReaction();
        Assert.That(reaction, Is.Not.Null);
    }

    [Test]
    public void BuildReaction_succeeds_for_sequential_commands()
    {
        var pb = new PipelineBuilder<TestModel>();
        pb.Element("a").SetText("one");
        pb.Element("b").SetText("two");

        var reaction = pb.BuildReaction();
        Assert.That(reaction, Is.Not.Null);
    }

    // ═══════════════════════════════════════════════════════════
    // F-T6 — TriggerBuilder with 2 When blocks → 2 entries
    // ═══════════════════════════════════════════════════════════

    [Test]
    public void TriggerBuilder_two_when_blocks_produce_two_entries()
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

        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var entries = doc.RootElement.GetProperty("entries");
        Assert.That(entries.GetArrayLength(), Is.EqualTo(2));
    }
}
