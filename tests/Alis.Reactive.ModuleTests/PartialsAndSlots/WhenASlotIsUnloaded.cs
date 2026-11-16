using System;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;
using static Alis.Reactive.ModuleTests.PartialsAndSlots.CardPlans;

namespace Alis.Reactive.ModuleTests.PartialsAndSlots;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenASlotIsUnloaded
{
    [Test]
    public void an_unload_calls_the_slot_the_element_id_keys_with_no_argument_by_one_step_that_is_not_awaited()
    {
        var expected = Loaded(Document + "," + Slot("card"), Unload("card"));

        Assert.That(Rendered(pipeline => pipeline.Unload("card")), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_load_and_an_unload_of_one_element_reach_one_slot_and_each_element_keys_its_own()
    {
        var expected = Loaded(
            Document + "," + Slot("card") + "," + Slot("notes"),
            Unload("notes") + "," + HtmlRequest("GET", "/cards", "ward=North", Load("card", HtmlOfTheReply("reply1"))) + "," + Unload("card"));

        Assert.That(
            Rendered(pipeline =>
            {
                pipeline.Unload("notes");
                pipeline.Get("/cards?ward=North").Into("card").NoDefaults();
                pipeline.Unload("card");
            }),
            Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_null_element_id_is_refused_by_its_parameters_name()
        => Assert.That(() => Rendered(pipeline => pipeline.Unload(null!)), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("elementId"));
}
