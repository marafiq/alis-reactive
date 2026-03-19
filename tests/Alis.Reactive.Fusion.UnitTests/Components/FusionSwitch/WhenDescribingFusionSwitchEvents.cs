using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenDescribingFusionSwitchEvents
{
    [Test]
    public void Singleton_returns_same_instance()
    {
        var a = FusionSwitchEvents.Instance;
        var b = FusionSwitchEvents.Instance;
        Assert.That(a, Is.SameAs(b));
    }

    [Test]
    public void Changed_descriptor_has_correct_js_event()
    {
        var descriptor = FusionSwitchEvents.Instance.Changed;
        Assert.That(descriptor.JsEvent, Is.EqualTo("change"));
    }

    [Test]
    public void Changed_descriptor_provides_args_instance()
    {
        var descriptor = FusionSwitchEvents.Instance.Changed;
        Assert.That(descriptor.Args, Is.Not.Null);
        Assert.That(descriptor.Args, Is.TypeOf<FusionSwitchChangeArgs>());
    }

    [Test]
    public void Changed_args_has_expected_properties()
    {
        var args = new FusionSwitchChangeArgs();
        Assert.That(args.Checked, Is.False);
        Assert.That(args.IsInteracted, Is.False);
    }
}
