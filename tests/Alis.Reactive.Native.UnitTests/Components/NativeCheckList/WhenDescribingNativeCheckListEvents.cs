using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests;

[TestFixture]
public class WhenDescribingNativeCheckListEvents
{
    [Test]
    public void Singleton_returns_same_instance()
    {
        var a = NativeCheckListEvents.Instance;
        var b = NativeCheckListEvents.Instance;
        Assert.That(a, Is.SameAs(b));
    }

    [Test]
    public void Changed_event_contract_has_correct_js_event()
    {
        var reactiveEvent = NativeCheckListEvents.Instance.Changed;
        Assert.That(reactiveEvent.EventName, Is.EqualTo("change"));
    }

    [Test]
    public void Changed_event_contract_provides_args_instance()
    {
        var reactiveEvent = NativeCheckListEvents.Instance.Changed;
        Assert.That(reactiveEvent.Payload, Is.Not.Null);
        Assert.That(reactiveEvent.Payload, Is.TypeOf<NativeCheckListChangeArgs>());
    }

    [Test]
    public void Changed_args_has_expected_properties()
    {
        var args = new NativeCheckListChangeArgs();
        Assert.That(args.Value, Is.Null);
    }
}
