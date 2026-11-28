using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.OwnershipAndState;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenATemplateRendersTwiceOnOnePage
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";

    private const string EventTargets = "{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"}";

    private const string AnAddressPlanUnderEnglish =
        "{\"version\":7,\"planId\":\"Alis.Reactive.ModuleTests.OwnershipAndState.Address\"," +
        "\"culture\":{\"name\":\"en-US\",\"collation\":\"en-US\",\"decimal\":\".\",\"group\":\",\"," +
        "\"integer\":{\"group\":false,\"exponent\":false},\"fraction\":{\"group\":true,\"exponent\":true}}," +
        "\"timeZone\":{\"kind\":\"clock\"},\"trace\":{\"kind\":\"off\"}," +
        "\"vendors\":{\"native\":{\"locate\":{\"kind\":\"fn\",\"params\":[\"element\"],\"body\":{\"kind\":\"ref\",\"name\":\"element\"}},\"listen\":" + EventTargets + "}}," +
        "\"objects\":{" +
        "\"$document\":{\"kind\":\"plugin\",\"name\":\"alis.document\",\"key\":\"\",\"listen\":" + EventTargets + ",\"data\":{\"kind\":\"none\"}},";

    private const string OnLoad = "\"handlers\":[{\"on\":\"$document\",\"path\":[],\"event\":\"ready\",\"payload\":[],\"members\":\"required\",\"policy\":\"overlap\",";

    private const string TheHomeAddressPlan = AnAddressPlanUnderEnglish +
        "\"home-street-note\":{\"kind\":\"element\",\"id\":\"home-street-note\",\"vendor\":\"native\"}}," +
        OnLoad +
        "\"do\":[{\"kind\":\"set\",\"path\":[\"textContent\"],\"members\":\"required\",\"content\":true," +
        "\"target\":{\"kind\":\"obj\",\"id\":\"home-street-note\"},\"value\":{\"kind\":\"lit\",\"value\":\"Where the resident lives\"}}]}]}";

    private const string ThePostalAddressPlan = AnAddressPlanUnderEnglish +
        "\"postal-street-note\":{\"kind\":\"element\",\"id\":\"postal-street-note\",\"vendor\":\"native\"}}," +
        OnLoad +
        "\"do\":[{\"kind\":\"set\",\"path\":[\"textContent\"],\"members\":\"required\",\"content\":true," +
        "\"target\":{\"kind\":\"obj\",\"id\":\"postal-street-note\"},\"value\":{\"kind\":\"lit\",\"value\":\"Where letters go\"}}]}]}";

    [Test]
    public void each_rendering_writes_a_plan_of_its_own_though_both_carry_one_plan_id()
    {
        var html = Views.Of(new Address("Main Road"));
        var home = html.ReactivePlan<Address>();
        var postal = html.ReactivePlan<Address>();
        html.On(home, trigger => trigger.DomReady(pipeline => pipeline.Element("home-street-note").Text.Set("Where the resident lives")));
        html.On(postal, trigger => trigger.DomReady(pipeline => pipeline.Element("postal-street-note").Text.Set("Where letters go")));

        var rendered = Views.TextOf(html.RenderPlan(home)) + Views.TextOf(html.RenderPlan(postal));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + TheHomeAddressPlan + ScriptClose + ScriptOpen + ThePostalAddressPlan + ScriptClose));
    }

    [Test]
    public void the_plan_of_the_first_rendering_is_valid_by_the_plan_schema()
        => Assert.That(PlanSchema.ProblemsOf(TheHomeAddressPlan), Is.Empty);

    [Test]
    public void the_plan_of_the_second_rendering_is_valid_by_the_plan_schema()
        => Assert.That(PlanSchema.ProblemsOf(ThePostalAddressPlan), Is.Empty);
}

public sealed record Address(string Street);
