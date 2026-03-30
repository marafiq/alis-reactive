using Alis.Reactive.DriftDetection.Tests.Infrastructure;

namespace Alis.Reactive.DriftDetection.Tests.Reactions;

[TestFixture]
public class WhenDetectingReactionSchemaDrift : DriftTestBase
{
    [Test]
    public void sequential_reaction_conforms()
    {
        // SequentialReaction: kind, commands
        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
        {
            p.Element("step-1").AddClass("complete");
            p.Dispatch("init");
        }));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "SequentialReaction", "entries[0].reaction");
    }

    [Test]
    public void conditional_with_pre_commands_conforms()
    {
        // ConditionalReaction: kind, commands (pre-branch), branches
        var plan = CreatePlan();
        On(plan, t => t.CustomEvent<ResidentModel>("assess", (args, p) =>
        {
            // Pre-branch commands (these go into ConditionalReaction.commands)
            p.Element("status").SetText("Assessing...");

            // Conditional branches
            p.When(args, x => x.CareLevel!).Eq("Memory Care")
             .Then(tp => tp.Element("rate").SetText("$5,200"))
             .Else(ep => ep.Element("rate").SetText("$2,400"));
        }));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "ConditionalReaction", "entries[0].reaction");
    }

    [Test]
    public void http_with_pre_fetch_conforms()
    {
        // HttpReaction: kind, preFetch, request
        var plan = CreatePlan();
        On(plan, t => t.CustomEvent("save-resident", p =>
        {
            // Pre-fetch command (goes into HttpReaction.preFetch)
            p.Element("save-btn").Hide();

            p.Post("/api/residents", g => g.IncludeAll())
             .Response(r => r.OnSuccess(s => s.Element("save-btn").Show()));
        }));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "HttpReaction", "entries[0].reaction");
    }

    [Test]
    public void parallel_with_all_properties_conforms()
    {
        // ParallelHttpReaction: kind, preFetch, requests, onAllSettled
        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
        {
            // Pre-fetch commands
            p.Element("loader").Show();

            p.Parallel(
                b => b.Get("/api/residents"),
                b => b.Get("/api/facilities")
            ).OnAllSettled(s =>
            {
                s.Element("loader").Hide();
                s.Dispatch("data-loaded");
            });
        }));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "ParallelHttpReaction", "entries[0].reaction");
    }

    [Test]
    public void branch_with_and_without_guard_conforms()
    {
        // Branch: guard (present on When/ElseIf), reaction
        // Else branch has guard: null (omitted by JsonIgnoreCondition.WhenWritingNull)
        var plan = CreatePlan();
        On(plan, t => t.CustomEvent<ResidentModel>("classify", (args, p) =>
        {
            p.When(args, x => x.IsVeteran).Truthy()
             .Then(tp => tp.Element("vet-badge").Show())
             .Else(ep => ep.Element("vet-badge").Hide());
        }));

        var json = plan.Render();
        AssertSchemaValid(json);
        // First branch (When) has both guard and reaction
        AssertAllPropertiesPresent(json, "Branch",
            "entries[0].reaction.branches[0]");
    }
}
