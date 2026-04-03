using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests;

[TestFixture]
public class WhenDescribingNativeHiddenFieldEvents
{
    [Test]
    public void Singleton_returns_same_instance()
    {
        var a = NativeHiddenFieldEvents.Instance;
        var b = NativeHiddenFieldEvents.Instance;
        Assert.That(a, Is.SameAs(b));
    }

    [Test]
    public void Changed_event_contract_has_correct_js_event()
    {
        var reactiveEvent = NativeHiddenFieldEvents.Instance.Changed;
        Assert.That(reactiveEvent.EventName, Is.EqualTo("change"));
    }

    [Test]
    public void Changed_event_contract_provides_args_instance()
    {
        var reactiveEvent = NativeHiddenFieldEvents.Instance.Changed;
        Assert.That(reactiveEvent.Payload, Is.Not.Null);
        Assert.That(reactiveEvent.Payload, Is.TypeOf<NativeHiddenFieldChangeArgs>());
    }

    [Test]
    public void Changed_args_has_expected_properties()
    {
        var args = new NativeHiddenFieldChangeArgs();
        Assert.That(args.Value, Is.Null);
    }
}
