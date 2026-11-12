using Alis.Reactive.RenderingAndDelivery;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Schema;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAViewRendersItsPlan
{
    private const string AWardsPlan =
        "{\"version\":7,\"planId\":\"Alis.Reactive.ModuleTests.Schema.Ward\"," +
        "\"culture\":{\"name\":\"en-US\",\"collation\":\"en-US\",\"decimal\":\".\",\"group\":\",\"," +
        "\"integer\":{\"group\":false,\"exponent\":false},\"fraction\":{\"group\":true,\"exponent\":true}}," +
        "\"timeZone\":{\"kind\":\"clock\"},\"trace\":{\"kind\":\"off\"},\"vendors\":{},\"objects\":{},\"handlers\":[]}";

    [Test]
    public void a_plan_with_no_handler_is_one_script_of_plan_data_with_every_field()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo("<script type=\"application/json\" data-reactive-plan>" + AWardsPlan + "</script>"));
    }

    [Test]
    public void the_plan_a_view_renders_is_valid_by_the_plan_schema()
        => Assert.That(PlanSchema.ProblemsOf(AWardsPlan), Is.Empty);

    [Test]
    public void a_plan_that_lacks_a_field_is_not_valid_by_the_plan_schema()
        => Assert.That(
            PlanSchema.ProblemsOf(AWardsPlan.Replace("\"trace\":{\"kind\":\"off\"},", string.Empty)),
            Is.EqualTo(new[] { "the plan: required" }));
}

public sealed record Ward(string Name);
