using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests;

[TestFixture]
public class WhenDescribingNativeTextAreaEvents
{
    [Test]
    public void Singleton_returns_same_instance()
    {
        Assert.That(NativeTextAreaEvents.Instance, Is.SameAs(NativeTextAreaEvents.Instance));
    }

    [Test]
    public void Changed_descriptor_has_correct_js_event()
    {
        Assert.That(NativeTextAreaEvents.Instance.Changed.JsEvent, Is.EqualTo("change"));
    }

    [Test]
    public void Changed_descriptor_provides_args_instance()
    {
        var args = NativeTextAreaEvents.Instance.Changed.Args;
        Assert.That(args, Is.Not.Null);
        Assert.That(args, Is.TypeOf<NativeTextAreaChangeArgs>());
    }
}
