using Alis.Reactive;
using Alis.Reactive.Native.Components;
using static VerifyNUnit.Verifier;

namespace Alis.Reactive.Native.UnitTests;

[TestFixture]
public class WhenMutatingANativeDropDown : NativeTestBase
{
    [Test]
    public Task SetValue_produces_dom_mutation()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<NativeDropDown>(m => m.Status).SetValue("active"));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task FocusIn_produces_dom_call()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<NativeDropDown>(m => m.Status).FocusIn());
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task SetValue_followed_by_element_mutation()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Component<NativeDropDown>(m => m.Status).SetValue("pending");
            p.Element("echo").SetText("Status updated");
        });
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public void Value_returns_bind_expr()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var expr = p.Component<NativeDropDown>(m => m.Status).Value();
            Assert.That(expr, Is.EqualTo("ref:Alis_Reactive_Native_UnitTests_NativeTestModel__Status.value"));
        });
    }

    [Test]
    public Task String_ref_overload()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<NativeDropDown>("filterDropdown").SetValue("B"));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
