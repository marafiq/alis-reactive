using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenDescribingFusionDateRangePickerEvents
{
    [Test]
    public void Singleton_returns_same_instance()
    {
        var a = FusionDateRangePickerEvents.Instance;
        var b = FusionDateRangePickerEvents.Instance;
        Assert.That(a, Is.SameAs(b));
    }

    [Test]
    public void Changed_descriptor_has_correct_js_event()
    {
        var descriptor = FusionDateRangePickerEvents.Instance.Changed;
        Assert.That(descriptor.JsEvent, Is.EqualTo("change"));
    }

    [Test]
    public void Changed_descriptor_provides_args_instance()
    {
        var descriptor = FusionDateRangePickerEvents.Instance.Changed;
        Assert.That(descriptor.Args, Is.Not.Null);
        Assert.That(descriptor.Args, Is.TypeOf<FusionDateRangePickerChangeArgs>());
    }

    [Test]
    public void Changed_args_has_expected_properties()
    {
        var args = new FusionDateRangePickerChangeArgs();
        Assert.That(args.StartDate, Is.Null);
        Assert.That(args.EndDate, Is.Null);
        Assert.That(args.DaySpan, Is.EqualTo(0));
        Assert.That(args.IsInteracted, Is.False);
    }
}
