using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Descriptors.Guards;
using Alis.Reactive.Descriptors.Reactions;
using Alis.Reactive.Descriptors.Sources;
using Alis.Reactive.Descriptors.Triggers;

namespace Alis.Reactive.UnitTests;

/// <summary>
/// F1 — Entry constructor must reject null trigger and null reaction.
/// A-T1 — Double guard on same command must throw.
/// </summary>
[TestFixture]
public class WhenEnforcingEntryInvariants : PlanTestBase
{
    [Test]
    public void Null_trigger_throws_ArgumentNullException()
    {
        var reaction = new SequentialReaction(new List<Descriptors.Commands.Command>());
        var ex = Assert.Throws<ArgumentNullException>(() => new Entry(null!, reaction));
        Assert.That(ex!.ParamName, Is.EqualTo("trigger"));
    }

    [Test]
    public void Null_reaction_throws_ArgumentNullException()
    {
        var trigger = new DomReadyTrigger();
        var ex = Assert.Throws<ArgumentNullException>(() => new Entry(trigger, null!));
        Assert.That(ex!.ParamName, Is.EqualTo("reaction"));
    }

    [Test]
    public Task Valid_entry_renders_correct_plan_json()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Element("status").SetText("loaded");
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ═══════════════════════════════════════════════════════════
    // A-T1 — Double guard on same command must throw
    // ═══════════════════════════════════════════════════════════

    [Test]
    public void WithGuard_on_already_guarded_command_throws()
    {
        var guard = new ValueGuard(
            new EventSource("evt.name"), "string", GuardOp.Eq, "test");
        var cmd = new DispatchCommand("test");
        var guarded = cmd.WithGuard(guard);

        var ex = Assert.Throws<InvalidOperationException>(() => guarded.WithGuard(guard));
        Assert.That(ex!.Message, Does.Contain("already has a guard"));
    }
}
