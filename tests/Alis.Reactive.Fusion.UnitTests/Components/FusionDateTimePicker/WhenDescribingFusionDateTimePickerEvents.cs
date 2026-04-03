using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenDescribingFusionDateTimePickerEvents
{
    [Test]
    public void Singleton_returns_same_instance()
    {
        var a = FusionDateTimePickerEvents.Instance;
        var b = FusionDateTimePickerEvents.Instance;
        Assert.That(a, Is.SameAs(b));
    }

    [Test]
    public void Changed_event_contract_has_correct_js_event()
    {
        var reactiveEvent = FusionDateTimePickerEvents.Instance.Changed;
        Assert.That(reactiveEvent.EventName, Is.EqualTo("change"));
    }

    [Test]
    public void Changed_event_contract_provides_args_instance()
    {
        var reactiveEvent = FusionDateTimePickerEvents.Instance.Changed;
        Assert.That(reactiveEvent.Payload, Is.Not.Null);
        Assert.That(reactiveEvent.Payload, Is.TypeOf<FusionDateTimePickerChangeArgs>());
    }

    [Test]
    public void Changed_args_has_expected_properties()
    {
        var args = new FusionDateTimePickerChangeArgs();
        Assert.That(args.Value, Is.Null);
        Assert.That(args.IsInteracted, Is.False);
    }
}
