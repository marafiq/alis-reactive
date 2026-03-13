namespace Alis.Reactive.UnitTests;

/// <summary>
/// WhileLoading and OnAllSettled are command-list surfaces by design.
/// They must reject conditions, HTTP, and parallel pipelines at build time
/// instead of silently dropping them.
/// </summary>
[TestFixture]
public class WhenRejectingNonSequentialInCommandListSurfaces : PlanTestBase
{
    public class Payload
    {
        public string Role { get; set; } = "";
    }

    [Test]
    public void WhileLoading_rejects_conditional_pipeline()
    {
        var plan = CreatePlan();
        Assert.Throws<InvalidOperationException>(() =>
        {
            Trigger(plan).CustomEvent<Payload>("save", (args, p) =>
            {
                p.Post("/api/save")
                 .WhileLoading(wl =>
                 {
                     wl.When(args, x => x.Role).Eq("admin")
                       .Then(t => t.Element("spinner").Show());
                 });
            });
        });
    }

    [Test]
    public void WhileLoading_rejects_http_pipeline()
    {
        var plan = CreatePlan();
        Assert.Throws<InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Post("/api/save")
                 .WhileLoading(wl =>
                 {
                     wl.Get("/api/other");
                 });
            });
        });
    }

    [Test]
    public void WhileLoading_allows_plain_commands()
    {
        var plan = CreatePlan();
        Assert.DoesNotThrow(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Post("/api/save")
                 .WhileLoading(wl =>
                 {
                     wl.Element("spinner").Show();
                     wl.Element("save-btn").Hide();
                 });
            });
        });
    }

    [Test]
    public void OnAllSettled_rejects_conditional_pipeline()
    {
        var plan = CreatePlan();
        Assert.Throws<InvalidOperationException>(() =>
        {
            Trigger(plan).CustomEvent<Payload>("batch", (args, p) =>
            {
                p.Parallel(
                    b => b.Post("/api/a"),
                    b => b.Post("/api/b")
                ).OnAllSettled(oas =>
                {
                    oas.When(args, x => x.Role).Eq("admin")
                       .Then(t => t.Element("result").Show());
                });
            });
        });
    }

    [Test]
    public void OnAllSettled_rejects_http_pipeline()
    {
        var plan = CreatePlan();
        Assert.Throws<InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Parallel(
                    b => b.Post("/api/a"),
                    b => b.Post("/api/b")
                ).OnAllSettled(oas =>
                {
                    oas.Get("/api/other");
                });
            });
        });
    }

    [Test]
    public void OnAllSettled_allows_plain_commands()
    {
        var plan = CreatePlan();
        Assert.DoesNotThrow(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Parallel(
                    b => b.Post("/api/a"),
                    b => b.Post("/api/b")
                ).OnAllSettled(oas =>
                {
                    oas.Element("spinner").Hide();
                    oas.Element("status").SetText("Done");
                });
            });
        });
    }
}
