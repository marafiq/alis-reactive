using Alis.Reactive.PlanTests.Support;

namespace Alis.Reactive.PlanTests.Refusals;

/// <summary>An array operation is a C# lambda the plan carries for the browser; one the browser cannot run is refused where it is written.</summary>
[TestFixture]
public class WhenAnArrayOperationCannotRunInTheBrowser
{
    [Test]
    public void an_order_by_a_record_is_refused_naming_a_scalar_key()
    {
        var plan = Page.Plan<Admission>();

        var refused = Assert.Throws<InvalidOperationException>(() =>
            plan.Triggers().DomReady(p => p.Get("/residents").Response(r =>
                r.OnSuccess<List<Admission>>((rows, s) => s.From(rows).OrderBy(x => x.NextOfKin)))));

        Assert.That(refused!.Message, Does.Contain("OrderBy key must project to a scalar"));
        Assert.That(refused.Message, Does.Contain("'Contact'"));
    }

    [Test]
    public void a_text_test_against_another_member_of_the_item_is_refused()
    {
        var plan = Page.Plan<Admission>();

        var refused = Assert.Throws<InvalidOperationException>(() =>
            plan.Triggers().DomReady(p => p.Get("/residents").Response(r =>
                r.OnSuccess<List<Admission>>((rows, s) => s.From(rows).Where(x => x.Name!.Contains(x.NextOfKin.Name!))))));

        Assert.That(refused!.Message, Does.Contain("must be constants or captured values"));
    }

    [Test]
    public void a_method_with_no_pure_javascript_form_is_refused_naming_the_pure_ones()
    {
        var plan = Page.Plan<Admission>();

        var refused = Assert.Throws<InvalidOperationException>(() =>
            plan.Triggers().DomReady(p => p.Get("/residents").Response(r =>
                r.OnSuccess<List<Admission>>((rows, s) => s.From(rows).Select(x => x.Name!.Insert(0, "Dr "))))));

        Assert.That(refused!.Message, Does.Contain("'String.Insert' has no pure JavaScript form"));
        Assert.That(refused.Message, Does.Contain("String.Trim"));
    }

    [Test]
    public void a_projection_that_builds_an_object_is_refused()
    {
        var plan = Page.Plan<Admission>();

        var refused = Assert.Throws<InvalidOperationException>(() =>
            plan.Triggers().DomReady(p => p.Get("/residents").Response(r =>
                r.OnSuccess<List<Admission>>((rows, s) => s.From(rows).Select(x => new { x.Name })))));

        Assert.That(refused!.Message, Does.Contain("Element selector must be a member access"));
    }
}
