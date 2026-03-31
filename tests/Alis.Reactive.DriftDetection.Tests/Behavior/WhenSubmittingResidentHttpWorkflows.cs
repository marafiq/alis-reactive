using Alis.Reactive.DriftDetection.Tests.Infrastructure;
using Alis.Reactive.Native.Components;
using Alis.Reactive.Native.Extensions;

namespace Alis.Reactive.DriftDetection.Tests.Behavior;

[TestFixture]
public class WhenSubmittingResidentHttpWorkflows : DriftTestBase
{
    [Test]
    public void resident_intake_submission_covers_request_handlers_loading_state_and_follow_up_fetches()
    {
        AssertDefinitionPropertiesExactly("HttpReaction", "kind", "preFetch", "request");
        AssertDefinitionPropertiesExactly("RequestDescriptor",
            "verb", "url", "gather", "contentType", "whileLoading",
            "onSuccess", "onError", "chained", "validation");
        AssertDefinitionPropertiesExactly("StatusHandler", "statusCode", "commands", "reaction");
        AssertDefinitionPropertiesExactly("AllGather", "kind");
        AssertDefinitionPropertiesExactly("IntoCommand", "kind", "target");
        AssertDefinitionPropertiesExactly("ValidationErrorsCommand", "kind", "formId");

        var plan = CreatePlan();
        On(plan, t => t.CustomEvent("submit-form", p =>
        {
            p.Post("/api/residents", g => g.IncludeAll())
             .AsFormData()
             .WhileLoading(lp =>
             {
                 lp.Element("spinner").Show();
                 lp.Element("submit-btn").Hide();
             })
             .Response(r =>
             {
                 r.OnSuccess(s => s.Into("result"));
                 r.OnError(400, s => s.ValidationErrors("resident-form"));
                 r.Chained(c => c.Get("/api/residents/latest")
                     .Response(cr => cr.OnSuccess(s => s.Into("latest-resident"))));
             });
        }));

        var json = plan.Render();
        AssertSchemaValid(json);

        AssertPropertiesExactly(json, "entries[0].reaction", "kind", "request");
        AssertPropertiesExactly(json, "entries[0].reaction.request",
            "verb", "url", "gather", "contentType", "whileLoading", "onSuccess", "onError", "chained");
        AssertPropertiesExactly(json, "entries[0].reaction.request.gather[0]", "kind");
        AssertPropertiesExactly(json, "entries[0].reaction.request.onSuccess[0]", "commands");
        AssertPropertiesExactly(json,
            "entries[0].reaction.request.onSuccess[0].commands[0]",
            "kind", "target");
        AssertPropertiesExactly(json, "entries[0].reaction.request.onError[0]", "statusCode", "commands");
        AssertPropertiesExactly(json,
            "entries[0].reaction.request.onError[0].commands[0]",
            "kind", "formId");
        AssertPropertiesExactly(json, "entries[0].reaction.request.chained", "verb", "url", "onSuccess");
    }

    [Test]
    public void resident_update_and_removal_flows_cover_put_and_delete_request_shapes()
    {
        AssertDefinitionPropertiesExactly("RequestDescriptor",
            "verb", "url", "gather", "contentType", "whileLoading",
            "onSuccess", "onError", "chained", "validation");

        var plan = CreatePlan();
        On(plan, t => t.CustomEvent("update-resident", p =>
            p.Put("/api/residents/42", g => g.IncludeAll())));
        On(plan, t => t.CustomEvent("remove-resident", p =>
            p.Delete("/api/residents/42")));

        var json = plan.Render();
        AssertSchemaValid(json);

        AssertPropertiesExactly(json, "entries[0].reaction.request", "verb", "url", "gather");
        AssertPropertiesExactly(json, "entries[1].reaction.request", "verb", "url");
    }

    [Test]
    public void resident_request_gatherers_cover_static_event_and_component_sources()
    {
        AssertDefinitionPropertiesExactly("StaticGather", "kind", "param", "value");
        AssertDefinitionPropertiesExactly("EventGather", "kind", "param", "path");
        AssertDefinitionPropertiesExactly("ComponentGather",
            "kind", "componentId", "vendor", "name", "readExpr");

        var plan = CreatePlan();
        Html.InputField(plan, m => m.Name)
            .NativeTextBox(b => b.Placeholder("Name"));

        On(plan, t => t.CustomEvent("submit-static", p =>
            p.Post("/api/residents", g => g.Static("facilityId", "FAC-001"))));
        On(plan, t => t.CustomEvent<ResidentModel>("submit-event", (args, p) =>
            p.Post("/api/residents", g => g.FromEvent(args, x => x.Name!, "residentName"))));
        On(plan, t => t.CustomEvent("submit-component", p =>
            p.Post("/api/residents", g => g.Include(m => m.Name))));

        var json = plan.Render();
        AssertSchemaValid(json);

        AssertPropertiesExactly(json, "entries[0].reaction.request.gather[0]", "kind", "param", "value");
        AssertPropertiesExactly(json, "entries[1].reaction.request.gather[0]", "kind", "param", "path");
        AssertPropertiesExactly(json, "entries[2].reaction.request.gather[0]",
            "kind", "componentId", "vendor", "name", "readExpr");
    }

    [Test]
    public void resident_response_handlers_can_route_into_conditional_reactions()
    {
        AssertDefinitionPropertiesExactly("StatusHandler", "statusCode", "commands", "reaction");

        var plan = CreatePlan();
        On(plan, t => t.CustomEvent<ResidentModel>("save", (args, p) =>
        {
            p.Post("/api/residents", g => g.IncludeAll())
             .Response(r => r.OnSuccess(s =>
             {
                 s.When(args, x => x.IsVeteran).Truthy()
                  .Then(tp => tp.Element("vet-note").Show())
                  .Else(ep => ep.Element("vet-note").Hide());
             }));
        }));

        var json = plan.Render();
        AssertSchemaValid(json);

        AssertPropertiesExactly(json, "entries[0].reaction.request.onSuccess[0]", "reaction");
    }

    [Test]
    public void resident_parallel_prefetches_cover_parallel_http_reaction_shape()
    {
        AssertDefinitionPropertiesExactly("ParallelHttpReaction",
            "kind", "preFetch", "requests", "onAllSettled");

        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
        {
            p.Element("loader").Show();

            p.Parallel(
                b => b.Get("/api/residents"),
                b => b.Get("/api/facilities"))
             .OnAllSettled(s =>
             {
                 s.Element("loader").Hide();
                 s.Dispatch("data-loaded");
             });
        }));

        var json = plan.Render();
        AssertSchemaValid(json);

        AssertPropertiesExactly(json, "entries[0].reaction",
            "kind", "preFetch", "requests", "onAllSettled");
        AssertPropertiesExactly(json, "entries[0].reaction.requests[0]", "verb", "url");
        AssertPropertiesExactly(json, "entries[0].reaction.requests[1]", "verb", "url");
        AssertPropertiesExactly(json, "entries[0].reaction.onAllSettled[0]",
            "kind", "target", "mutation");
        AssertPropertiesExactly(json, "entries[0].reaction.onAllSettled[1]",
            "kind", "event");
    }
}
