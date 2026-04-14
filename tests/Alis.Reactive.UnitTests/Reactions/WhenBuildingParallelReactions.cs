using System.Collections.Generic;
using System.Text.Json;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.UnitTests.Reactions;

[TestFixture]
public class WhenBuildingParallelReactions : PlanTestBase
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static string SerializeReaction(Reaction r) =>
        JsonSerializer.Serialize<Reaction>(r, Options);

    private static Reaction ASimpleStep() =>
        Reaction.ShowValidationErrors("c1");

    // ── NoOpReaction rendering and identity ──────────────────────

    [Test]
    public void NoOpReaction_renders_kind_only()
    {
        var json = SerializeReaction(Reaction.NoOp);
        Assert.That(json, Is.EqualTo("{\"kind\":\"noop\"}"));
    }

    [Test]
    public void Reaction_NoOp_is_a_NoOpReaction_instance()
    {
        Assert.That(Reaction.NoOp, Is.InstanceOf<NoOpReaction>());
    }

    // ── ParallelReaction default-onSettled behavior (R15-style invariant) ──

    [Test]
    public void ParallelReaction_with_null_onSettled_defaults_to_NoOp_singleton()
    {
        // Constructed via the public factory (the documented entry point for builders).
        var par = (ParallelReaction)Reaction.Parallel(
            new List<Reaction> { ASimpleStep() },
            onSettled: null);
        Assert.That(par.OnSettled, Is.SameAs(Reaction.NoOp));
    }

    [Test]
    public void ParallelReaction_with_omitted_onSettled_defaults_to_NoOp_singleton()
    {
        // The default-parameter overload (no onSettled argument) takes the `null` default.
        var par = (ParallelReaction)Reaction.Parallel(new List<Reaction> { ASimpleStep() });
        Assert.That(par.OnSettled, Is.SameAs(Reaction.NoOp));
    }

    [Test]
    public void ParallelReaction_with_explicit_onSettled_preserves_the_passed_reaction()
    {
        var explicitOnSettled = ASimpleStep();
        var par = (ParallelReaction)Reaction.Parallel(
            new List<Reaction> { ASimpleStep() },
            onSettled: explicitOnSettled);
        Assert.That(par.OnSettled, Is.SameAs(explicitOnSettled));
        Assert.That(par.OnSettled, Is.Not.SameAs(Reaction.NoOp));
    }

    [Test]
    public void ParallelReaction_throws_when_steps_is_null()
    {
        Assert.Throws<System.ArgumentNullException>(
            () => Reaction.Parallel((List<Reaction>)null!));
    }

    // ── Wire format: onSettled is always present (R2-style widening) ──

    [Test]
    public void ParallelReaction_default_onSettled_renders_with_explicit_noop_object()
    {
        var par = Reaction.Parallel(new List<Reaction> { ASimpleStep() });
        var json = SerializeReaction(par);
        Assert.That(json, Does.Contain("\"onSettled\":{\"kind\":\"noop\"}"));
    }

    [Test]
    public void ParallelReaction_with_explicit_onSettled_renders_the_explicit_kind()
    {
        var par = Reaction.Parallel(
            new List<Reaction> { ASimpleStep() },
            onSettled: Reaction.ShowValidationErrors("done"));
        var json = SerializeReaction(par);
        Assert.That(json, Does.Contain("\"onSettled\":{\"kind\":\"show-validation-errors\""));
        Assert.That(json, Does.Not.Contain("\"kind\":\"noop\""));
    }

    [Test]
    public void ParallelReaction_renders_with_onSettled_field_present_in_both_default_and_explicit_cases()
    {
        var defaultOnSettled = SerializeReaction(Reaction.Parallel(new List<Reaction> { ASimpleStep() }));
        var explicitOnSettled = SerializeReaction(
            Reaction.Parallel(
                new List<Reaction> { ASimpleStep() },
                onSettled: Reaction.ShowValidationErrors("c2")));
        Assert.That(defaultOnSettled, Does.Contain("\"onSettled\":"));
        Assert.That(explicitOnSettled, Does.Contain("\"onSettled\":"));
    }

    // ── Schema validation: NoOpReaction is accepted at the wire layer ──

    [Test]
    public void Plan_with_real_ParallelReaction_passes_schema_validation_end_to_end()
    {
        // End-to-end gate: build a plan with a real ParallelReaction via the public DSL,
        // render it, validate against reactive-plan.schema.json. Proves the C# wire format
        // (which now always emits "onSettled") satisfies the tightened schema requirement
        // ("required": ["kind", "steps", "onSettled"]). If the wire format and schema drift,
        // this test fails.
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Parallel(
                b => b.Post("/api/a"),
                b => b.Post("/api/b")
            ).OnAllSettled(oas =>
            {
                oas.Element("status").SetText("Done");
            });
        });
        var json = plan.RenderFormatted();
        AssertSchemaValid(json);
        // Also assert the wire format actually contains the onSettled object
        Assert.That(json, Does.Contain("\"onSettled\""));
    }
}
