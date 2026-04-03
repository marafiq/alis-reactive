using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests;

[TestFixture]
public class WhenDescribingNativeTextAreaEvents
{
    [Test]
    public void Singleton_returns_same_instance()
    {
        var first = NativeTextAreaEvents.Instance;
        var second = NativeTextAreaEvents.Instance;

        Assert.That(first, Is.SameAs(second));
    }

    [Test]
    public void Changed_event_contract_has_correct_js_event()
    {
        Assert.That(NativeTextAreaEvents.Instance.Changed.EventName, Is.EqualTo("change"));
    }

    [Test]
    public void Changed_event_contract_provides_args_instance()
    {
        var args = NativeTextAreaEvents.Instance.Changed.Payload;
        Assert.That(args, Is.Not.Null);
        Assert.That(args, Is.TypeOf<NativeTextAreaChangeArgs>());
    }
}
