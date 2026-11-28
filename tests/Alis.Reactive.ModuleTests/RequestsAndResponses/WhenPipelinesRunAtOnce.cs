using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.RequestsAndResponses;

[TestFixture]
public sealed class WhenPipelinesRunAtOnce
{
    [Test]
    public void each_branch_is_a_block_of_one_all_and_the_steps_after_it_follow_it_in_its_block()
    {
        var plan = RequestPage.Rendered(pipeline =>
        {
            pipeline.Parallel(pipeline => pipeline.Get("/beds"), pipeline => pipeline.Element("ward").Text.Set("North"));
            pipeline.Element("done").Text.Set("settled");
        });
        var steps = RequestPage.Steps(plan);
        Assert.That(steps.GetArrayLength(), Is.EqualTo(2));
        Assert.That(steps[0].GetProperty("kind").GetString(), Is.EqualTo("all"));
        var blocks = steps[0].GetProperty("blocks");
        Assert.That(blocks.GetArrayLength(), Is.EqualTo(2));
        Assert.That(blocks[0].GetArrayLength(), Is.EqualTo(1));
        Assert.That(blocks[0][0].GetProperty("kind").GetString(), Is.EqualTo("request"));
        Assert.That(blocks[1].GetRawText(), Is.EqualTo(
            """[{"kind":"set","path":["textContent"],"members":"required","content":true,"target":{"kind":"obj","id":"ward"},"value":{"kind":"lit","value":"North"}}]"""));
        Assert.That(steps[1].GetRawText(), Is.EqualTo(
            """{"kind":"set","path":["textContent"],"members":"required","content":true,"target":{"kind":"obj","id":"done"},"value":{"kind":"lit","value":"settled"}}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_parallel_of_no_branch_is_an_all_of_no_block()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Parallel());
        Assert.That(RequestPage.Steps(plan)[0].GetRawText(), Is.EqualTo("""{"kind":"all","blocks":[]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }
}
