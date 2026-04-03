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
    public void Selected_event_contract_has_correct_js_event()
    {
        var reactiveEvent = FusionFileUploadEvents.Instance.Selected;
        Assert.That(reactiveEvent.EventName, Is.EqualTo("selected"));
    }

    [Test]
    public void Selected_event_contract_provides_args_instance()
    {
        var reactiveEvent = FusionFileUploadEvents.Instance.Selected;
        Assert.That(reactiveEvent.Payload, Is.Not.Null);
        Assert.That(reactiveEvent.Payload, Is.TypeOf<FusionFileUploadSelectedArgs>());
    }

    [Test]
    public void Selected_args_has_expected_properties()
    {
        var args = new FusionFileUploadSelectedArgs();
        Assert.That(args.FilesCount, Is.EqualTo(0));
        Assert.That(args.IsInteracted, Is.False);
    }
}
