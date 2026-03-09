using Alis.Reactive;
using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests;

[TestFixture]
public class WhenDescribingNativeDropDownEvents
{
    [Test]
    public void Singleton_returns_same_instance()
    {
        var a = NativeDropDownEvents.Instance;
        var b = NativeDropDownEvents.Instance;
        Assert.That(a, Is.SameAs(b));
    }

    [Test]
    public void Changed_descriptor_has_correct_js_event()
    {
        var descriptor = NativeDropDownEvents.Instance.Changed;
        Assert.That(descriptor.JsEvent, Is.EqualTo("change"));
    }

    [Test]
    public void Changed_descriptor_provides_args_instance()
    {
        var descriptor = NativeDropDownEvents.Instance.Changed;
        Assert.That(descriptor.Args, Is.Not.Null);
        Assert.That(descriptor.Args, Is.TypeOf<NativeDropDownChangeArgs>());
    }

    [Test]
    public void Changed_args_has_expected_properties()
    {
        var args = new NativeDropDownChangeArgs();
        Assert.That(args.Value, Is.Null);
    }
}
