using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests.Components.NativeDatePicker;

[TestFixture]
public class WhenDescribingNativeDatePickerEvents
{
    [Test]
    public void Singleton_returns_same_instance()
    {
        var a = NativeDatePickerEvents.Instance;
        var b = NativeDatePickerEvents.Instance;
        Assert.That(a, Is.SameAs(b));
    }

    [Test]
    public void Changed_descriptor_has_correct_js_event()
    {
        var descriptor = NativeDatePickerEvents.Instance.Changed;
        Assert.That(descriptor.JsEvent, Is.EqualTo("change"));
    }

    [Test]
    public void Changed_descriptor_provides_args_instance()
    {
        var descriptor = NativeDatePickerEvents.Instance.Changed;
        Assert.That(descriptor.Args, Is.Not.Null);
        Assert.That(descriptor.Args, Is.TypeOf<NativeDatePickerChangeArgs>());
    }
}
