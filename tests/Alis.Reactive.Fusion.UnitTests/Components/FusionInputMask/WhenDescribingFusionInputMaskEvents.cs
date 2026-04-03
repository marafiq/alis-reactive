using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenDescribingFusionInputMaskEvents
{
    [Test]
    public void Singleton_returns_same_instance()
    {
        var a = FusionInputMaskEvents.Instance;
        var b = FusionInputMaskEvents.Instance;
        Assert.That(a, Is.SameAs(b));
    }

    [Test]
    public void Changed_event_contract_has_correct_js_event()
    {
        var reactiveEvent = FusionInputMaskEvents.Instance.Changed;
        Assert.That(reactiveEvent.EventName, Is.EqualTo("change"));
    }

    [Test]
    public void Changed_event_contract_provides_args_instance()
    {
        var reactiveEvent = FusionInputMaskEvents.Instance.Changed;
        Assert.That(reactiveEvent.Payload, Is.Not.Null);
        Assert.That(reactiveEvent.Payload, Is.TypeOf<FusionInputMaskChangeArgs>());
    }

    [Test]
    public void Changed_args_has_expected_properties()
    {
        var args = new FusionInputMaskChangeArgs();
        Assert.That(args.Value, Is.Null);
        Assert.That(args.IsInteracted, Is.False);
    }
}
