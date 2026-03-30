using Alis.Reactive.DriftDetection.Tests.Infrastructure;

namespace Alis.Reactive.DriftDetection.Tests.Http;

[TestFixture]
public class WhenDetectingRequestSchemaDrift : DriftTestBase
{
    [Test]
    public void get_request_conforms()
    {
        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
            p.Get("/api/residents")));

        var json = plan.Render();
        AssertSchemaValid(json);
    }

    [Test]
    public void post_with_all_properties_conforms()
    {
        // RequestDescriptor: verb, url, gather, contentType, whileLoading,
        //   onSuccess, onError, chained, validation
        // Note: validation requires a registered IValidationExtractor to populate.
        // Without it, Validate<T>() sets an empty descriptor that will be populated at Render().
        // We exercise all other properties.
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
                 r.Chained(c => c.Get("/api/residents/latest"));
             });
        }));

        var json = plan.Render();
        AssertSchemaValid(json);
    }

    [Test]
    public void put_request_conforms()
    {
        var plan = CreatePlan();
        On(plan, t => t.CustomEvent("update-resident", p =>
            p.Put("/api/residents/42", g => g.IncludeAll())));

        var json = plan.Render();
        AssertSchemaValid(json);
    }

    [Test]
    public void delete_request_conforms()
    {
        var plan = CreatePlan();
        On(plan, t => t.CustomEvent("remove-resident", p =>
            p.Delete("/api/residents/42")));

        var json = plan.Render();
        AssertSchemaValid(json);
    }

    [Test]
    public void form_data_content_type_conforms()
    {
        var plan = CreatePlan();
        On(plan, t => t.CustomEvent("upload", p =>
            p.Post("/api/documents/upload", g => g.IncludeAll())
             .AsFormData()));

        var json = plan.Render();
        AssertSchemaValid(json);
    }

    [Test]
    public void while_loading_conforms()
    {
        var plan = CreatePlan();
        On(plan, t => t.CustomEvent("fetch", p =>
        {
            p.Get("/api/residents")
             .WhileLoading(lp => lp.Element("spinner").Show());
        }));

        var json = plan.Render();
        AssertSchemaValid(json);
    }

    [Test]
    public void success_handler_with_commands_conforms()
    {
        // StatusHandler with commands (sequential handler)
        var plan = CreatePlan();
        On(plan, t => t.CustomEvent("save", p =>
        {
            p.Post("/api/residents", g => g.IncludeAll())
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("status").SetText("Saved!");
                 s.Dispatch("resident-saved");
             }));
        }));

        var json = plan.Render();
        AssertSchemaValid(json);
    }

    [Test]
    public void success_handler_with_reaction_conforms()
    {
        // StatusHandler with reaction (conditional handler inside response)
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
    }

    [Test]
    public void error_handler_without_status_conforms()
    {
        // OnError with a specific status code
        var plan = CreatePlan();
        On(plan, t => t.CustomEvent("save", p =>
        {
            p.Post("/api/residents", g => g.IncludeAll())
             .Response(r =>
             {
                 r.OnSuccess(s => s.Dispatch("saved"));
                 r.OnError(500, s => s.Element("error").SetText("Server error"));
             });
        }));

        var json = plan.Render();
        AssertSchemaValid(json);
    }

    [Test]
    public void chained_request_conforms()
    {
        var plan = CreatePlan();
        On(plan, t => t.CustomEvent("save-and-load", p =>
        {
            p.Post("/api/residents", g => g.IncludeAll())
             .Response(r =>
             {
                 r.OnSuccess(s => s.Dispatch("saved"));
                 r.Chained(c => c.Get("/api/residents/latest")
                     .Response(cr => cr.OnSuccess(s => s.Into("latest-resident"))));
             });
        }));

        var json = plan.Render();
        AssertSchemaValid(json);
    }
}
