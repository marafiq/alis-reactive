using Alis.Reactive.PlanTests.Support;

namespace Alis.Reactive.PlanTests.Requests;

/// <summary>A request is one send of alis.http: its route, query or body gathered from the page, its routes on the answer.</summary>
[TestFixture]
public class WhenAPageAuthorsRequests
{
    [Test]
    public void a_get_fills_the_route_placeholder_from_the_member_named_like_it_and_sends_the_rest_as_the_query()
    {
        var plan = Page.Plan<Admission>();
        plan.Triggers().DomReady(p =>
            p.Get("/residents/{Id}").Gather<ResidentQuery>(g => g.Set(x => x.Id, 1042).Set(x => x.Wing, "Garden")));

        var rendered = plan.Rendered();

        var send = rendered.Handlers()[0]!["do"]![0]!["expr"]!["args"]![0]!;
        var fields = send["fields"]!.AsArray().ToDictionary(field => field!["name"]!.GetValue<string>(), field => field!["value"]!);
        Assert.That(fields["route"]!["fields"]!.AsArray().Single()!["name"]!.GetValue<string>(), Is.EqualTo("Id"));
        Assert.That(fields["values"]!["fields"]!.AsArray().Single()!["name"]!.GetValue<string>(), Is.EqualTo("Wing"));
        Golden.Matches(rendered, "Requests", nameof(a_get_fills_the_route_placeholder_from_the_member_named_like_it_and_sends_the_rest_as_the_query));
    }

    [Test]
    public void a_post_of_the_form_reads_the_success_body_where_the_route_names_it()
    {
        var plan = Page.Plan<Admission>().TextInput(m => m.Name);
        plan.Triggers().DomReady(p =>
            p.Post("/admit").Gather(g => g.Form("admission"))
             .Response(r => r.OnSuccess<AdmissionSaved>((saved, s) => s.Element("status").Text.Set(saved.Get(x => x.Confirmation)))));

        var rendered = plan.Rendered();

        var send = rendered.Handlers()[0]!["do"]![0]!;
        Assert.That(send["kind"]!.GetValue<string>(), Is.EqualTo("let"));
        Assert.That(send["await"]!.GetValue<bool>(), Is.True);
        Golden.Matches(rendered, "Requests", nameof(a_post_of_the_form_reads_the_success_body_where_the_route_names_it));
    }

    [Test]
    public void a_chained_request_runs_after_the_first_succeeds_and_parallel_ones_wait_for_all()
    {
        var plan = Page.Plan<Admission>();
        plan.Triggers().DomReady(p =>
        {
            p.Get("/first").Response(r => r.Chained(start => start.Get("/second")));
            p.Parallel(start => start.Get("/a"), start => start.Get("/b"));
        });

        var rendered = plan.Rendered();

        var statements = rendered.Handlers()[0]!["do"]!.AsArray();
        Assert.That(statements.Last()!["kind"]!.GetValue<string>(), Is.EqualTo("all"));
        Golden.Matches(rendered, "Requests", nameof(a_chained_request_runs_after_the_first_succeeds_and_parallel_ones_wait_for_all));
    }
}
