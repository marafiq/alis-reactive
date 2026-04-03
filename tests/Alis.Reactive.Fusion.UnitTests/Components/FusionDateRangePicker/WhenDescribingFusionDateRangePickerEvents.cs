using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenDescribingFusionDateRangePickerEvents
{
    [Test]
    public void Singleton_returns_same_instance()
    {
        var a = FusionDateRangePickerEvents.Instance;
        var b = FusionDateRangePickerEvents.Instance;
        Assert.That(a, Is.SameAs(b));
    }

    [Test]
    public void Changed_event_contract_has_correct_js_event()
    {
        var reactiveEvent = FusionDateRangePickerEvents.Instance.Changed;
        Assert.That(reactiveEvent.EventName, Is.EqualTo("change"));
    }

    [Test]
    public void Changed_event_contract_provides_args_instance()
    {
        var reactiveEvent = FusionDateRangePickerEvents.Instance.Changed;
        Assert.That(reactiveEvent.Payload, Is.Not.Null);
        Assert.That(reactiveEvent.Payload, Is.TypeOf<FusionDateRangePickerChangeArgs>());
    }

    [Test]
    public void Changed_args_has_expected_properties()
    {
        var args = new FusionDateRangePickerChangeArgs();
        Assert.That(args.StartDate, Is.Null);
        Assert.That(args.EndDate, Is.Null);
        Assert.That(args.DaySpan, Is.EqualTo(0));
        Assert.That(args.IsInteracted, Is.False);
    }
}
