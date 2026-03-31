using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors.Reactions;

namespace Alis.Reactive.UnitTests;

/// <summary>
/// Proves that commands declared BETWEEN two When/Then blocks preserve their
/// declaration order — they must run after the first condition, not before it.
///
/// Bug: FlushSegment bundles all accumulated Commands into one SequentialReaction,
/// merging pre-condition and between-condition commands. A between-condition command
/// silently moves before the first condition.
/// </summary>
[TestFixture]
public class WhenCommandsFallBetweenTwoConditions : PlanTestBase
{
    [Test]
    public void Commands_between_conditions_produce_correct_segment_order()
    {
        // Arrange: before → cond1 → between → cond2 → after
        var pb = new PipelineBuilder<TestModel>();
        var payload = new SegmentPayload();

        pb.Element("before").SetText("step1");

        pb.When(payload, (SegmentPayload x) => x.Name).Eq("a")
            .Then(t => t.Element("r1").SetText("cond1"));

        pb.Element("between").SetText("step3");

        pb.When(payload, (SegmentPayload x) => x.Count).Gt(0)
            .Then(t => t.Element("r2").SetText("cond2"));

        pb.Element("after").SetText("step5");

        // Act
        var reactions = pb.BuildReactions();

        // Assert: 4 segments in declaration order
        // 1. ConditionalReaction with "before" as pre-command + cond1 branches
        // 2. SequentialReaction with "between"
        // 3. ConditionalReaction with cond2 branches
        // 4. SequentialReaction with "after"
        Assert.That(reactions, Has.Count.EqualTo(4),
            $"Expected 4 segments (cond1 with pre-cmd, between, cond2, after) but got {reactions.Count}");

        Assert.That(reactions[0], Is.InstanceOf<ConditionalReaction>(),
            "Segment 0 must be cond1 (ConditionalReaction)");

        var cond1 = (ConditionalReaction)reactions[0];
        Assert.That(cond1.Commands, Is.Not.Null,
            "cond1 must carry 'before' as a pre-command");
        Assert.That(cond1.Commands, Has.Count.EqualTo(1),
            "cond1 pre-commands must contain exactly 'before'");

        Assert.That(reactions[1], Is.InstanceOf<SequentialReaction>(),
            "Segment 1 must be 'between' (SequentialReaction)");
        var between = (SequentialReaction)reactions[1];
        Assert.That(between.Commands, Has.Count.EqualTo(1));

        Assert.That(reactions[2], Is.InstanceOf<ConditionalReaction>(),
            "Segment 2 must be cond2 (ConditionalReaction)");

        Assert.That(reactions[3], Is.InstanceOf<SequentialReaction>(),
            "Segment 3 must be 'after' (SequentialReaction)");
    }

    [Test]
    public Task Plan_json_preserves_command_ordering_across_conditions()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<SegmentPayload>("test", (args, p) =>
        {
            p.Element("before").SetText("step1");

            p.When(args, x => x.Name).Eq("a")
                .Then(t => t.Element("r1").SetText("cond1"));

            p.Element("between").SetText("step3");

            p.When(args, x => x.Count).Gt(0)
                .Then(t => t.Element("r2").SetText("cond2"));

            p.Element("after").SetText("step5");
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public void Single_condition_with_pre_commands_is_not_affected()
    {
        // Ensure the fix doesn't break the common single-condition case
        var pb = new PipelineBuilder<TestModel>();
        var payload = new SegmentPayload();

        pb.Element("before").SetText("step1");

        pb.When(payload, (SegmentPayload x) => x.Name).Eq("a")
            .Then(t => t.Element("r1").SetText("matched"))
            .Else(e => e.Element("r1").SetText("no match"));

        pb.Element("after").SetText("step3");

        var reaction = pb.BuildReaction();
        Assert.That(reaction, Is.InstanceOf<ConditionalReaction>());

        var cond = (ConditionalReaction)reaction;
        Assert.That(cond.Commands, Is.Not.Null, "Pre-commands must be present");
        Assert.That(cond.Commands, Has.Count.EqualTo(2),
            "Single condition: both 'before' and 'after' are pre/post commands in the same reaction");
    }
}
