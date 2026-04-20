using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenDescribingFusionInPlaceEditorEvents
{
    [Test]
    public void Singleton_returns_same_instance()
    {
        Assert.That(FusionInPlaceEditorEvents.Instance, Is.SameAs(FusionInPlaceEditorEvents.Instance));
    }

    [Test]
    public void BeginEdit_descriptor_maps_to_sf_event_name()
    {
        var d = FusionInPlaceEditorEvents.Instance.BeginEdit;
        Assert.That(d.JsEvent, Is.EqualTo("beginEdit"));
        Assert.That(d.Args, Is.TypeOf<FusionInPlaceEditorBeginEditArgs>());
    }

    [Test]
    public void EndEdit_descriptor_maps_to_sf_event_name()
    {
        var d = FusionInPlaceEditorEvents.Instance.EndEdit;
        Assert.That(d.JsEvent, Is.EqualTo("endEdit"));
        Assert.That(d.Args, Is.TypeOf<FusionInPlaceEditorEndEditArgs>());
    }

    [Test]
    public void Changed_descriptor_maps_to_sf_event_name()
    {
        var d = FusionInPlaceEditorEvents.Instance.Changed;
        Assert.That(d.JsEvent, Is.EqualTo("change"));
        Assert.That(d.Args, Is.TypeOf<FusionInPlaceEditorChangeArgs>());
    }

    [Test]
    public void Validating_descriptor_maps_to_sf_event_name()
    {
        var d = FusionInPlaceEditorEvents.Instance.Validating;
        Assert.That(d.JsEvent, Is.EqualTo("validating"));
        Assert.That(d.Args, Is.TypeOf<FusionInPlaceEditorValidatingArgs>());
    }

    [Test]
    public void ActionBegin_descriptor_maps_to_sf_event_name()
    {
        var d = FusionInPlaceEditorEvents.Instance.ActionBegin;
        Assert.That(d.JsEvent, Is.EqualTo("actionBegin"));
        Assert.That(d.Args, Is.TypeOf<FusionInPlaceEditorActionBeginArgs>());
    }

    [Test]
    public void ActionSuccess_descriptor_maps_to_sf_event_name()
    {
        var d = FusionInPlaceEditorEvents.Instance.ActionSuccess;
        Assert.That(d.JsEvent, Is.EqualTo("actionSuccess"));
        Assert.That(d.Args, Is.TypeOf<FusionInPlaceEditorActionSuccessArgs>());
    }

    [Test]
    public void ActionFailure_descriptor_maps_to_sf_event_name()
    {
        var d = FusionInPlaceEditorEvents.Instance.ActionFailure;
        Assert.That(d.JsEvent, Is.EqualTo("actionFailure"));
        Assert.That(d.Args, Is.TypeOf<FusionInPlaceEditorActionFailureArgs>());
    }

    [Test]
    public void SubmitClick_descriptor_maps_to_sf_event_name()
    {
        var d = FusionInPlaceEditorEvents.Instance.SubmitClick;
        Assert.That(d.JsEvent, Is.EqualTo("submitClick"));
        Assert.That(d.Args, Is.TypeOf<FusionInPlaceEditorSubmitClickArgs>());
    }

    [Test]
    public void CancelClick_descriptor_maps_to_sf_event_name()
    {
        var d = FusionInPlaceEditorEvents.Instance.CancelClick;
        Assert.That(d.JsEvent, Is.EqualTo("cancelClick"));
        Assert.That(d.Args, Is.TypeOf<FusionInPlaceEditorCancelClickArgs>());
    }

    [Test]
    public void BeginEdit_args_has_expected_defaults()
    {
        var args = new FusionInPlaceEditorBeginEditArgs();
        Assert.Multiple(() =>
        {
            Assert.That(args.Cancel, Is.False);
            Assert.That(args.CancelFocus, Is.False);
            Assert.That(args.Mode, Is.Null);
        });
    }

    [Test]
    public void EndEdit_args_has_expected_defaults()
    {
        var args = new FusionInPlaceEditorEndEditArgs();
        Assert.Multiple(() =>
        {
            Assert.That(args.Action, Is.Null);
            Assert.That(args.Cancel, Is.False);
            Assert.That(args.Mode, Is.Null);
        });
    }

    [Test]
    public void Change_args_has_expected_defaults()
    {
        var args = new FusionInPlaceEditorChangeArgs();
        Assert.Multiple(() =>
        {
            Assert.That(args.Value, Is.Null);
            Assert.That(args.PreviousValue, Is.Null);
        });
    }

    [Test]
    public void Validating_args_has_expected_defaults()
    {
        var args = new FusionInPlaceEditorValidatingArgs();
        Assert.Multiple(() =>
        {
            Assert.That(args.Data, Is.Null);
            Assert.That(args.Cancel, Is.False);
            Assert.That(args.ErrorMessage, Is.Null);
        });
    }

    [Test]
    public void ActionBegin_args_has_expected_defaults()
    {
        var args = new FusionInPlaceEditorActionBeginArgs();
        Assert.Multiple(() =>
        {
            Assert.That(args.Data, Is.Null);
            Assert.That(args.Cancel, Is.False);
        });
    }

    [Test]
    public void ActionSuccess_args_has_expected_defaults()
    {
        var args = new FusionInPlaceEditorActionSuccessArgs();
        Assert.Multiple(() =>
        {
            Assert.That(args.Value, Is.Null);
            Assert.That(args.Data, Is.Null);
        });
    }

    [Test]
    public void ActionFailure_args_has_expected_defaults()
    {
        var args = new FusionInPlaceEditorActionFailureArgs();
        Assert.Multiple(() =>
        {
            Assert.That(args.Value, Is.Null);
            Assert.That(args.Data, Is.Null);
        });
    }
}
