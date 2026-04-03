using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenDescribingFusionNumericTextBoxEvents
{
    [Test]
    public void Singleton_returns_same_instance()
    {
        var a = FusionNumericTextBoxEvents.Instance;
        var b = FusionNumericTextBoxEvents.Instance;
        Assert.That(a, Is.SameAs(b));
    }

    [Test]
    public void Changed_event_contract_has_correct_js_event()
    {
        var reactiveEvent = FusionNumericTextBoxEvents.Instance.Changed;
        Assert.That(reactiveEvent.EventName, Is.EqualTo("change"));
    }

    [Test]
    public void Changed_event_contract_provides_args_instance()
    {
        var reactiveEvent = FusionNumericTextBoxEvents.Instance.Changed;
        Assert.That(reactiveEvent.Payload, Is.Not.Null);
        Assert.That(reactiveEvent.Payload, Is.TypeOf<FusionNumericTextBoxChangeArgs>());
    }

    [Test]
    public void Changed_args_has_expected_properties()
    {
        var args = new FusionNumericTextBoxChangeArgs();
        Assert.That(args.Value, Is.EqualTo(0m));
        Assert.That(args.PreviousValue, Is.EqualTo(0m));
        Assert.That(args.IsInteracted, Is.False);
    }
}
