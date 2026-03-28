namespace Alis.Reactive.UnitTests;

[TestFixture]
public class WhenMutatingAnElement : PlanTestBase
{
    [Test]
    public Task AddClass_produces_mutate_element_command() =>
        VerifyJson(Build(p => p.Element("status").AddClass("active")).Render());

    [Test]
    public Task RemoveClass_produces_mutate_element_command() =>
        VerifyJson(Build(p => p.Element("status").RemoveClass("hidden")).Render());

    [Test]
    public Task SetText_produces_mutate_element_command() =>
        VerifyJson(Build(p => p.Element("output").SetText("hello")).Render());

    [Test]
    public Task Multiple_mutations_on_same_element() =>
        VerifyJson(Build(p =>
        {
            p.Element("panel").RemoveClass("text-muted");
            p.Element("panel").AddClass("text-green");
            p.Element("panel").SetText("done");
        }).Render());

    [Test]
    public Task Mutations_mixed_with_dispatch() =>
        VerifyJson(Build(p =>
        {
            p.Element("step-1").AddClass("complete");
            p.Dispatch("step-done");
            p.Element("step-2").SetText("next");
        }).Render());

    [Test]
    public Task Show_and_hide_produce_correct_actions() =>
        VerifyJson(Build(p =>
        {
            p.Element("loader").Show();
            p.Element("content").Hide();
        }).Render());

    [Test]
    public Task ToggleClass_produces_correct_action() =>
        VerifyJson(Build(p => p.Element("menu").ToggleClass("expanded")).Render());

    [Test]
    public Task SetHtml_produces_correct_action() =>
        VerifyJson(Build(p => p.Element("container").SetHtml("<strong>done</strong>")).Render());

    private static ReactivePlan<TestModel> Build(Action<Builders.PipelineBuilder<TestModel>> pipeline)
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(pipeline);
        return plan;
    }
}
