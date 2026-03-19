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
    public void Changed_descriptor_has_correct_js_event()
    {
        var descriptor = FusionDateTimePickerEvents.Instance.Changed;
        Assert.That(descriptor.JsEvent, Is.EqualTo("change"));
    }

    [Test]
    public void Changed_descriptor_provides_args_instance()
    {
        var descriptor = FusionDateTimePickerEvents.Instance.Changed;
        Assert.That(descriptor.Args, Is.Not.Null);
        Assert.That(descriptor.Args, Is.TypeOf<FusionDateTimePickerChangeArgs>());
    }

    [Test]
    public void Changed_args_has_expected_properties()
    {
        var args = new FusionDateTimePickerChangeArgs();
        Assert.That(args.Value, Is.Null);
        Assert.That(args.IsInteracted, Is.False);
    }
}
