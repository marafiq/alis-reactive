using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Reactions;
using Alis.Reactive.Descriptors.Triggers;

namespace Alis.Reactive.UnitTests;

/// <summary>
/// F1 — Entry constructor must reject null trigger and null reaction.
/// Test IDs: F-T1 (null trigger → ANE), F-T1b (null reaction → ANE).
/// F-T2 (grep: no new Entry(null, …) in production code) verified manually.
/// </summary>
[TestFixture]
public class WhenEnforcingEntryInvariants
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
    public void Valid_entry_assigns_both_properties()
    {
        var trigger = new DomReadyTrigger();
        var reaction = new SequentialReaction(new List<Descriptors.Commands.Command>());
        var entry = new Entry(trigger, reaction);
        Assert.That(entry.Trigger, Is.SameAs(trigger));
        Assert.That(entry.Reaction, Is.SameAs(reaction));
    }
}
