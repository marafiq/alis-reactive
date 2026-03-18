using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests;

[TestFixture]
public class WhenDescribingNativeRadioGroupEvents
{
    [Test]
    public void Singleton_returns_same_instance()
    {
        var a = NativeRadioGroupEvents.Instance;
        var b = NativeRadioGroupEvents.Instance;
        Assert.That(a, Is.SameAs(b));
    }

    [Test]
    public void Changed_descriptor_has_correct_js_event()
    {
        var descriptor = NativeRadioGroupEvents.Instance.Changed;
        Assert.That(descriptor.JsEvent, Is.EqualTo("change"));
    }

    [Test]
    public void Changed_descriptor_provides_args_instance()
    {
        var descriptor = NativeRadioGroupEvents.Instance.Changed;
        Assert.That(descriptor.Args, Is.Not.Null);
        Assert.That(descriptor.Args, Is.TypeOf<NativeRadioGroupChangeArgs>());
    }

    [Test]
    public void Changed_args_has_expected_properties()
    {
        var args = new NativeRadioGroupChangeArgs();
        Assert.That(args.Value, Is.Null);
    }
}
