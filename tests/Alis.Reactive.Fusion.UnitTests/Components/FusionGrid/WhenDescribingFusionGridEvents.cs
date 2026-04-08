using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenDescribingFusionGridEvents
{
    [Test]
    public void Singleton_returns_same_instance()
    {
        var a = FusionGridEvents.Instance;
        var b = FusionGridEvents.Instance;
        Assert.That(a, Is.SameAs(b));
    }

    [Test]
    public void DataStateChange_descriptor_has_correct_js_event()
    {
        var descriptor = FusionGridEvents.Instance.DataStateChange;
        Assert.That(descriptor.JsEvent, Is.EqualTo("dataStateChange"));
    }

    [Test]
    public void DataStateChange_descriptor_provides_args_instance()
    {
        var descriptor = FusionGridEvents.Instance.DataStateChange;
        Assert.That(descriptor.Args, Is.Not.Null);
        Assert.That(descriptor.Args, Is.TypeOf<FusionGridDataStateChangeArgs>());
    }

    [Test]
    public void DataStateChange_args_has_expected_defaults()
    {
        var args = new FusionGridDataStateChangeArgs();
        Assert.That(args.Skip, Is.EqualTo(0));
        Assert.That(args.Take, Is.EqualTo(0));
        Assert.That(args.Sorted, Is.Null);
        Assert.That(args.Action, Is.Not.Null);
    }

    [Test]
    public void FusionGridAction_has_expected_constants()
    {
        Assert.That(FusionGridAction.Sorting, Is.EqualTo("sorting"));
        Assert.That(FusionGridAction.Paging, Is.EqualTo("paging"));
        Assert.That(FusionGridAction.Filtering, Is.EqualTo("filtering"));
        Assert.That(FusionGridAction.Refresh, Is.EqualTo("refresh"));
    }
}
