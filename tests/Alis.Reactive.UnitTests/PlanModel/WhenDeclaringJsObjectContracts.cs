using Alis.Reactive.PlanModel;
using PlanPath = Alis.Reactive.PlanModel.Path;

namespace Alis.Reactive.UnitTests.PlanModel;

[TestFixture]
public sealed class WhenDeclaringJsObjectContracts
{
    [Test]
    public void property_members_cannot_be_redeclared_with_a_different_runtime_path()
    {
        var jsType = new JsType();
        jsType.Declare(JsPropertyContract.Create(
            MemberName.Of("value"),
            PlanPath.Parse("value"),
            Shape.String,
            MemberAccess.Read));

        var ex = Assert.Throws<InvalidOperationException>(() =>
            jsType.Declare(JsPropertyContract.Create(
                MemberName.Of("value"),
                PlanPath.Parse("textContent"),
                Shape.String,
                MemberAccess.Write)));

        Assert.That(ex!.Message, Does.Contain("Property 'value'"));
        Assert.That(ex.Message, Does.Contain("path 'value'"));
        Assert.That(ex.Message, Does.Contain("path 'textContent'"));
    }

    [Test]
    public void property_redeclaration_merges_compatible_shape_and_access_when_path_matches()
    {
        var jsType = new JsType();
        jsType.Declare(JsPropertyContract.Create(
            MemberName.Of("value"),
            PlanPath.Parse("value"),
            Shape.String,
            MemberAccess.Read));

        jsType.Declare(JsPropertyContract.Create(
            MemberName.Of("value"),
            PlanPath.Parse("value"),
            Shape.Nullable(Shape.String),
            MemberAccess.Write));

        Assert.That(jsType.Properties["value"].Shape, Is.EqualTo(Shape.Nullable(Shape.String)));
        Assert.That(jsType.Properties["value"].Access, Is.EqualTo("readwrite"));
    }

    [Test]
    public void method_members_report_both_paths_when_redeclared_with_a_different_runtime_path()
    {
        var jsType = new JsType();
        jsType.Declare(JsMethodContract.Create(
            MemberName.Of("setValue"),
            PlanPath.Parse("setValue"),
            MethodSignature.Exact(new[] { Shape.String }, Shape.None)));

        var ex = Assert.Throws<InvalidOperationException>(() =>
            jsType.Declare(JsMethodContract.Create(
                MemberName.Of("setValue"),
                PlanPath.Parse("editor.setValue"),
                MethodSignature.Exact(new[] { Shape.String }, Shape.None))));

        Assert.That(ex!.Message, Does.Contain("Method 'setValue'"));
        Assert.That(ex.Message, Does.Contain("path 'setValue'"));
        Assert.That(ex.Message, Does.Contain("path 'editor.setValue'"));
    }
}
