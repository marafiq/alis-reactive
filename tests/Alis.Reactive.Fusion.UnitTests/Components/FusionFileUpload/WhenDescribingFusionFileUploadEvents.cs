using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenDescribingFusionFileUploadEvents
{
    [Test]
    public void Singleton_returns_same_instance()
    {
        var a = FusionFileUploadEvents.Instance;
        var b = FusionFileUploadEvents.Instance;
        Assert.That(a, Is.SameAs(b));
    }

    [Test]
    public void Selected_descriptor_has_correct_js_event()
    {
        var descriptor = FusionFileUploadEvents.Instance.Selected;
        Assert.That(descriptor.JsEvent, Is.EqualTo("selected"));
    }

    [Test]
    public void Selected_descriptor_provides_args_instance()
    {
        var descriptor = FusionFileUploadEvents.Instance.Selected;
        Assert.That(descriptor.Args, Is.Not.Null);
        Assert.That(descriptor.Args, Is.TypeOf<FusionFileUploadSelectedArgs>());
    }

    [Test]
    public void Selected_args_has_expected_properties()
    {
        var args = new FusionFileUploadSelectedArgs();
        Assert.That(args.FilesCount, Is.EqualTo(0));
        Assert.That(args.IsInteracted, Is.False);
    }
}
