// PROVER 2 — domain serialize determinism dogfood.
//
// Drives the REAL Alis.Reactive public DSL through the REAL ReactivePlanSerializer
// (ReactivePlan.Render(), camelCase, defined at Alis.Reactive/ReactivePlan.cs:206).
// Public-DSL only: ReactivePlan / Html.On / builders / p.Get / p.When / Element /
// Dispatch / Component / Native extensions. No internal ctors are called.
//
// The plan-build harness mirrors the repo's own behavior-graph Playwright tests
// (tests/Alis.Reactive.PlaywrightTests/Conditions/BehaviorGraph/...): the
// IHtmlHelper<TModel> is null because ReactivePlan(this IHtmlHelper) reads it via
// `html?.ViewContext...` (null-conditional => services: null) and HtmlExtensions.On
// ignores the helper argument entirely.

using System.Text;
using System.Text.Json;
using Alis.Reactive;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Native.Components;
using Alis.Reactive.Native.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Dogfood.Determinism;

// ---- Models / payloads (public DSL inputs) ---------------------------------

public sealed class OrderModel
{
    public string Name { get; set; } = "";
    public int Quantity { get; set; }
    public decimal Total { get; set; }
    public bool IsRush { get; set; }
    public string City { get; set; } = "";
    public Line[] Lines { get; set; } = [];
}

public sealed class Line
{
    public string Sku { get; set; } = "";
    public int Count { get; set; }
    public decimal Price { get; set; }
}

public sealed class ScoreEvent
{
    public int Score { get; set; }
    public bool IsReady { get; set; }
    public string Tier { get; set; } = "";
}

public sealed class AuditPayload
{
    public string Reason { get; set; } = "";
    public int Code { get; set; }
}

public sealed class OrderResponse
{
    public string Status { get; set; } = "";
    public int Count { get; set; }
}

public sealed class ErrorBody
{
    public string Message { get; set; } = "";
}

internal static class Program
{
    // The repo's own proven harness pattern — see file header.
    private static readonly IHtmlHelper<OrderModel> Html = null!;
    private static readonly IHtmlHelper<ScoreEvent> ScoreHtml = null!;

    // Each entry: a stable name + a builder that constructs a fresh plan and renders it.
    // Builders MUST be pure (no captured mutable state) so each invocation rebuilds
    // an independent plan — that is what makes DET-STABLE and DET-CONGRUENT meaningful.
    private static readonly (string Name, Func<string> Build)[] Plans = BuildCatalog();

    private static int Main(string[] args)
    {
        // Hidden inspection mode: `dotnet run -- dump <substring>` prints formatted JSON
        // for every plan whose name contains <substring>. Used to ground the walk rules
        // against real serializer output. Not part of the obligation checks.
        if (args.Length >= 1 && args[0] == "dump")
        {
            var filter = args.Length >= 2 ? args[1] : "";
            foreach (var (name, build) in Plans)
            {
                if (!name.Contains(filter, StringComparison.Ordinal)) continue;
                using var d = JsonDocument.Parse(build());
                Console.WriteLine($"### {name}");
                Console.WriteLine(JsonSerializer.Serialize(d.RootElement, new JsonSerializerOptions { WriteIndented = true }));
                Console.WriteLine();
            }
            return 0;
        }

        // Negative control: prove the walk is NOT vacuous — it must FLAG a non-camelCase
        // property and a union member with no `kind`. Runs with `-- selftest`.
        if (args.Length >= 1 && args[0] == "selftest")
            return SelfTest();

        var failures = new List<string>();

        // DET-STABLE: Render() twice on independently-built plans is byte-identical.
        var stableFails = 0;
        foreach (var (name, build) in Plans)
        {
            var a = build();
            var b = build();
            if (!string.Equals(a, b, StringComparison.Ordinal))
            {
                stableFails++;
                failures.Add($"DET-STABLE FAIL [{name}]: two renders differ. firstDiff={FirstDiff(a, b)}");
            }
        }

        // DET-CAMELCASE + DET-KIND-PRESENT: structural walk over every rendered plan.
        var observedKinds = new SortedSet<string>(StringComparer.Ordinal);
        var camelFails = 0;
        var kindPresentFails = 0;
        foreach (var (name, build) in Plans)
        {
            using var doc = JsonDocument.Parse(build());
            var ctx = new WalkContext(name);
            Walk(doc.RootElement, "$", parentArrayHasKindSibling: false, ctx);
            foreach (var k in ctx.Kinds) observedKinds.Add(k);
            if (ctx.CamelViolations.Count > 0)
            {
                camelFails += ctx.CamelViolations.Count;
                failures.Add($"DET-CAMELCASE FAIL [{name}]: {string.Join(" | ", ctx.CamelViolations.Take(5))}");
            }
            if (ctx.MissingKind.Count > 0)
            {
                kindPresentFails += ctx.MissingKind.Count;
                failures.Add($"DET-KIND-PRESENT FAIL [{name}]: {string.Join(" | ", ctx.MissingKind.Take(5))}");
            }
        }

        // DET-CAMELCASE (token verbatim): the special-cased multi-word tokens must
        // appear verbatim, NOT camelCased, NOT snake-cased. These are the canonical
        // examples called out by the obligation.
        string[] requiredVerbatimTokens =
        [
            "page-ready", "show-validation-errors", "array-op",
            "document-event", "server-push", "route-param",
            "component-event", "follow-up", "on-settled",
            "registered-input", "validation-container",
        ];
        var verbatimFails = 0;
        foreach (var token in requiredVerbatimTokens)
        {
            if (!observedKinds.Contains(token))
            {
                // Only a violation if a camelCased/forbidden variant of a SEEN kind exists,
                // OR if we expected to produce it and did not. We track expected producers
                // separately below; here we only flag forbidden mutated forms.
                var camelVariant = ToCamel(token);
                if (observedKinds.Contains(camelVariant))
                {
                    verbatimFails++;
                    failures.Add($"DET-CAMELCASE FAIL: kind token '{token}' was emitted as camelCased '{camelVariant}'.");
                }
            }
        }

        // DET-CONGRUENT: two independently-built, structurally-identical plans match byte-for-byte.
        var congruentFails = 0;
        foreach (var (name, left, right) in CongruentPairs())
        {
            var lj = left();
            var rj = right();
            if (!string.Equals(lj, rj, StringComparison.Ordinal))
            {
                congruentFails++;
                failures.Add($"DET-CONGRUENT FAIL [{name}]: structurally-identical plans differ. firstDiff={FirstDiff(lj, rj)}");
            }
        }

        // ---- Report ----
        Console.WriteLine("== PROVER 2 — domain serialize determinism ==");
        Console.WriteLine($"plans built (varied)      : {Plans.Length}");
        Console.WriteLine($"distinct kind tokens seen : {observedKinds.Count}");
        Console.WriteLine($"   {string.Join(", ", observedKinds)}");
        Console.WriteLine();
        Console.WriteLine($"DET-STABLE        : {(stableFails == 0 ? "HOLDS" : $"FAILS ({stableFails})")} over {Plans.Length} plans (each rendered twice, ordinal byte compare)");
        Console.WriteLine($"DET-CAMELCASE     : {(camelFails == 0 && verbatimFails == 0 ? "HOLDS" : $"FAILS (props={camelFails}, tokens={verbatimFails})")}");
        Console.WriteLine($"DET-KIND-PRESENT  : {(kindPresentFails == 0 ? "HOLDS" : $"FAILS ({kindPresentFails})")}");
        Console.WriteLine($"DET-CONGRUENT     : {(congruentFails == 0 ? "HOLDS" : $"FAILS ({congruentFails})")} over {CongruentPairs().Count()} pairs");
        Console.WriteLine();

        // Verbatim-token presence report (informational + assertion that the canonical ones were produced).
        var producedRequired = requiredVerbatimTokens.Where(observedKinds.Contains).ToArray();
        var notProducedRequired = requiredVerbatimTokens.Where(t => !observedKinds.Contains(t)).ToArray();
        Console.WriteLine($"verbatim tokens produced  : {string.Join(", ", producedRequired)}");
        if (notProducedRequired.Length > 0)
            Console.WriteLine($"verbatim tokens NOT driven: {string.Join(", ", notProducedRequired)} (NOT_TESTED for this run)");
        Console.WriteLine();

        if (failures.Count == 0)
        {
            Console.WriteLine("ALL OBLIGATIONS HOLD.");
            return 0;
        }

        Console.WriteLine($"FAILURES ({failures.Count}):");
        foreach (var f in failures) Console.WriteLine("  - " + f);
        return 1;
    }

    // Negative control: the walk must catch a non-camelCase property and a kindless union member.
    private static int SelfTest()
    {
        const string badCamel = """{"Behaviors":[{"kind":"x"}]}""";   // "Behaviors" is PascalCase
        const string badKind = """{"steps":[{"kind":"set"},{"property":"p"}]}"""; // 2nd member has no kind

        using var d1 = JsonDocument.Parse(badCamel);
        var c1 = new WalkContext("badCamel");
        Walk(d1.RootElement, "$", false, c1);

        using var d2 = JsonDocument.Parse(badKind);
        var c2 = new WalkContext("badKind");
        Walk(d2.RootElement, "$", false, c2);

        var camelCaught = c1.CamelViolations.Count > 0;
        var kindCaught = c2.MissingKind.Count > 0;
        Console.WriteLine($"selftest camelCaught={camelCaught} ({string.Join(";", c1.CamelViolations)})");
        Console.WriteLine($"selftest kindCaught ={kindCaught} ({string.Join(";", c2.MissingKind)})");
        var ok = camelCaught && kindCaught;
        Console.WriteLine(ok ? "SELFTEST PASS — walk is non-vacuous" : "SELFTEST FAIL — walk would miss real drift");
        return ok ? 0 : 1;
    }

    // ---------------------------------------------------------------------
    // Structural JSON walk
    // ---------------------------------------------------------------------

    private sealed class WalkContext(string planName)
    {
        public string PlanName { get; } = planName;
        public List<string> CamelViolations { get; } = [];
        public List<string> MissingKind { get; } = [];
        public List<string> Kinds { get; } = [];
    }

    // The six value-keyed maps in the plan domain — IReadOnlyDictionary<string,...> whose
    // KEYS are developer data (component/element IDs, vendor type keys, object-member names,
    // payload field names), NOT C# member names. Verified exhaustively from source:
    //   PlanDocument.Types / .Components,
    //   BrowserObjectContract.Properties / .Methods / .Events,
    //   ObjectExpression.Fields.
    // System.Text.Json's PropertyNamingPolicy.CamelCase renames C# member names only;
    // DictionaryKeyPolicy is unset, so these keys serialize verbatim (the same contract as
    // `kind` token values). The camelCase obligation governs C# property names, so the keys
    // of these maps are exempt — but their VALUES are POCO objects and ARE walked.
    private static readonly HashSet<string> ValueKeyedMaps =
        new(StringComparer.Ordinal) { "types", "components", "properties", "methods", "events", "fields" };

    // A "polymorphic node" is an object that is an element of an array in which at least
    // one element carries a non-empty `kind`. Such siblings are a discriminated union, so
    // every member MUST carry a non-empty `kind`. Plain value-bag objects (not array
    // members of a kinded union) are not required to carry `kind`. This catches kind drift
    // without false positives on ordinary nested objects.
    private static void Walk(JsonElement el, string path, bool parentArrayHasKindSibling, WalkContext ctx)
    {
        switch (el.ValueKind)
        {
            case JsonValueKind.Object:
                var hasKind = el.TryGetProperty("kind", out var kindEl) &&
                              kindEl.ValueKind == JsonValueKind.String;
                if (hasKind)
                {
                    var k = kindEl.GetString() ?? "";
                    if (k.Length == 0)
                        ctx.MissingKind.Add($"{path}.kind is empty string");
                    else
                        ctx.Kinds.Add(k);
                }
                else if (parentArrayHasKindSibling)
                {
                    ctx.MissingKind.Add($"{path} is a union member but carries NO kind");
                }

                foreach (var prop in el.EnumerateObject())
                {
                    // `prop.Name` is a serializer-produced C# property name -> must be camelCase.
                    if (!IsCamelCase(prop.Name))
                        ctx.CamelViolations.Add($"{path}.{prop.Name} is not camelCase");

                    // If this property is one of the six value-keyed maps, its CHILD keys are
                    // developer data (verbatim), not C# member names: walk values, skip keys.
                    if (ValueKeyedMaps.Contains(prop.Name) && prop.Value.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var entry in prop.Value.EnumerateObject())
                            Walk(entry.Value, $"{path}.{prop.Name}[\"{entry.Name}\"]", parentArrayHasKindSibling: false, ctx);
                    }
                    else
                    {
                        Walk(prop.Value, $"{path}.{prop.Name}", parentArrayHasKindSibling: false, ctx);
                    }
                }
                break;

            case JsonValueKind.Array:
                var anySiblingHasKind = el.EnumerateArray().Any(e =>
                    e.ValueKind == JsonValueKind.Object &&
                    e.TryGetProperty("kind", out var kk) &&
                    kk.ValueKind == JsonValueKind.String &&
                    (kk.GetString()?.Length ?? 0) > 0);
                var i = 0;
                foreach (var item in el.EnumerateArray())
                    Walk(item, $"{path}[{i++}]", parentArrayHasKindSibling: anySiblingHasKind, ctx);
                break;
        }
    }

    // camelCase = first char lower-or-digit, no underscores, no hyphens, no leading upper.
    // (JSON property names; NOT kind token values, which are walked as string VALUES.)
    private static bool IsCamelCase(string name)
    {
        if (name.Length == 0) return false;
        if (name.Contains('_') || name.Contains('-')) return false;
        var first = name[0];
        return !char.IsUpper(first);
    }

    private static string ToCamel(string token)
    {
        var sb = new StringBuilder();
        var upperNext = false;
        foreach (var c in token)
        {
            if (c is '-' or '_') { upperNext = true; continue; }
            sb.Append(upperNext ? char.ToUpperInvariant(c) : c);
            upperNext = false;
        }
        return sb.ToString();
    }

    private static string FirstDiff(string a, string b)
    {
        var n = Math.Min(a.Length, b.Length);
        for (var i = 0; i < n; i++)
            if (a[i] != b[i])
                return $"@{i}: '{Snippet(a, i)}' vs '{Snippet(b, i)}'";
        return a.Length != b.Length ? $"length {a.Length} vs {b.Length}" : "none";
    }

    private static string Snippet(string s, int i)
    {
        var start = Math.Max(0, i - 10);
        var len = Math.Min(25, s.Length - start);
        return s.Substring(start, len);
    }

    // ---------------------------------------------------------------------
    // Plan catalog — 30+ varied plans spanning many node families.
    // Every builder is pure: it constructs a brand-new plan from scratch.
    // ---------------------------------------------------------------------

    private static (string, Func<string>)[] BuildCatalog()
    {
        var list = new List<(string, Func<string>)>
        {
            // --- triggers (page-ready / document-event / server-push / signalr) ---
            ("trigger.page-ready", () => Order(p =>
                p.DomReady(pl => pl.Element("status").SetText("Ready")))),

            ("trigger.document-event", () => Order(p =>
                p.CustomEvent("refresh", pl => pl.Element("box").AddClass("active")))),

            ("trigger.document-event.typed", () => Score(p =>
                p.CustomEvent<ScoreEvent>("score", (args, pl) =>
                    pl.Element("grade").SetText(args, x => x.Tier)))),

            ("trigger.server-push", () => Order(p =>
                p.ServerPush("/sse/orders", pl => pl.Element("live").SetText("tick")))),

            ("trigger.server-push.typed", () => Score(p =>
                p.ServerPush<ScoreEvent>("/sse/score", "score-evt", (args, pl) =>
                    pl.Element("g").SetText(args, x => x.Tier)))),

            ("trigger.signalr", () => Order(p =>
                p.SignalR("/hub/notify", "OnPing", pl => pl.Element("n").SetText("ping")))),

            // --- element commands (set element members) ---
            ("element.set-text-html-class-show-hide", () => Order(p =>
                p.DomReady(pl =>
                {
                    pl.Element("a").SetText("hello");
                    pl.Element("b").SetHtml("<i>x</i>");
                    pl.Element("c").AddClass("on");
                    pl.Element("d").RemoveClass("off");
                    pl.Element("e").ToggleClass("t");
                    pl.Element("f").Show();
                    pl.Element("g").Hide();
                }))),

            // --- dispatch (literal payload / source-backed payload / bare) ---
            ("dispatch.bare", () => Order(p =>
                p.DomReady(pl => pl.Dispatch("opened")))),

            ("dispatch.literal-payload", () => Order(p =>
                p.DomReady(pl => pl.Dispatch("audit", new AuditPayload { Reason = "manual", Code = 7 })))),

            ("dispatch.source-payload", () => Order(p =>
                p.DomReady(pl =>
                    pl.DispatchWith<AuditPayload>("audit2", b =>
                    {
                        b.Set(x => x.Reason, "auto");
                        b.Set(x => x.Code, 9);
                    })))),

            // --- conditions / branch / compare / literal (When/Then/ElseIf/Else) ---
            ("branch.payload.then-elseif-else", () => Score(p =>
                p.CustomEvent<ScoreEvent>("score", (args, pl) =>
                    pl.When(args, x => x.Score).Gte(90)
                      .Then(hit => hit.Element("grade").SetText("A"))
                      .ElseIf(args, x => x.Score).Gte(80)
                      .Then(hit => hit.Element("grade").SetText("B"))
                      .Else(miss => miss.Element("grade").SetText("C"))))),

            ("branch.truthy-falsy", () => Score(p =>
                p.CustomEvent<ScoreEvent>("score", (args, pl) =>
                    pl.When(args, x => x.IsReady).Truthy()
                      .Then(t => t.Dispatch("go"))
                      .Else(e => e.Dispatch("wait"))))),

            ("branch.url-source.in", () => Order(p =>
                p.DomReady(pl =>
                    pl.When(pl.FromUrl<string>("mode")).In("a", "b", "c")
                      .Then(t => t.Element("m").SetText("ok"))
                      .Else(e => e.Element("m").SetText("no"))))),

            ("branch.guard-and-or-not", () => Score(p =>
                p.CustomEvent<ScoreEvent>("score", (args, pl) =>
                    pl.When(args, x => x.Score).Gt(0)
                      .And(args, x => x.IsReady).Truthy()
                      .Or(args, x => x.Tier).Eq("gold")
                      .Not()
                      .Then(t => t.Dispatch("matched"))))),

            ("branch.string-ops", () => Order(p =>
                p.DomReady(pl =>
                    pl.When(pl.FromUrl<string>("q")).Contains("abc")
                      .Then(t => t.Element("r").SetText("found"))))),

            ("branch.null-ops", () => Order(p =>
                p.DomReady(pl =>
                    pl.When(pl.FromUrl<string>("opt")).NotNull()
                      .Then(t => t.Element("r").SetText("present"))
                      .Else(e => e.Element("r").SetText("absent"))))),

            // --- confirm guard ---
            ("confirm.guard", () => Order(p =>
                p.DomReady(pl =>
                    pl.Confirm("Delete this order?")
                      .Then(t => t.Dispatch("confirmed-delete"))))),

            // --- HTTP request / gather / header / route-param / static ---
            ("http.get.simple", () => Order(p =>
                p.DomReady(pl => pl.Get("/api/orders")))),

            ("http.post.gather-static", () => Order(p =>
                p.DomReady(pl =>
                    pl.Post("/api/orders", g => g.Static("source", "web").Static("v", 2))))),

            ("http.get.header-and-route", () => Order(p =>
                p.DomReady(pl =>
                    pl.Get("/api/orders/{id}")
                      .Gather(g => g
                          .Header("X-Trace", "abc")
                          .RouteParam("id", 42)
                          .FromUrl("ref"))))),

            ("http.post.from-event", () => Score(p =>
                p.CustomEvent<ScoreEvent>("score", (args, pl) =>
                    pl.Post("/api/score", g => g.FromEvent(args, x => x.Score, "score"))))),

            ("http.response.success-error-into", () => Order(p =>
                p.DomReady(pl =>
                    pl.Get("/api/orders")
                      .Response(r => r
                          .OnSuccess<OrderResponse>((body, ok) =>
                              ok.Element("status").SetText(body, x => x.Status))
                          .OnError<ErrorBody>((err, fail) =>
                              fail.Element("err").SetText(err, x => x.Message))
                          .OnError(404, nf => nf.Element("err").SetText("not found")))))),

            ("http.response.into-inject", () => Order(p =>
                p.DomReady(pl =>
                    pl.Get("/api/fragment")
                      .Response(r => r.OnSuccess(ok => ok.Into("slot")))))),

            ("http.chained", () => Order(p =>
                p.DomReady(pl =>
                    pl.Post("/api/start", g => g.Static("kick", true))
                      .Response(r => r.OnSuccess(ok =>
                          ok.Get("/api/next")))))),

            ("http.parallel", () => Order(p =>
                p.DomReady(pl =>
                    pl.Parallel(
                        a => a.Get("/api/a"),
                        b => b.Get("/api/b"),
                        c => c.Post("/api/c").Gather(g => g.Static("z", 1)))))),

            ("http.while-loading", () => Order(p =>
                p.DomReady(pl =>
                    pl.Get("/api/slow")
                      .WhileLoading(load => load.Element("spinner").Show())
                      .Finally(done => done.Element("spinner").Hide())))),

            // --- component set / call / read (set + call + component-event via Native) ---
            ("component.set-call", () => Order(p =>
                p.DomReady(pl =>
                    pl.Component<NativeTextBox>("name-box").SetValue("Ada").FocusIn()))),

            ("component.read-in-branch", () => Order(p =>
                p.DomReady(pl =>
                {
                    var box = pl.Component<NativeTextBox>("name-box");
                    pl.When(box.Value()).Eq("admin")
                      .Then(t => t.Element("flag").SetText("is-admin"));
                }))),

            // --- arrays: array + array-op (Where/Select/OrderBy/Count) used as value ---
            ("array.count-into-element", () => Order(p =>
                p.CustomEvent<OrderModel>("lines-changed", (args, pl) =>
                {
                    var arr = pl.From(args, x => x.Lines);
                    pl.Element("line-count").SetText(arr.Count());
                }))),

            ("array.where-select-order-count", () => Order(p =>
                p.CustomEvent<OrderModel>("lines-changed", (args, pl) =>
                {
                    var arr = pl.From(args, x => x.Lines)
                        .Where(l => l.Count > 0)
                        .OrderBy(l => l.Sku)
                        .Select(l => l.Sku);
                    pl.Element("skus").SetText(arr.Count());
                }))),

            ("array.sum-decimal", () => Order(p =>
                p.CustomEvent<OrderModel>("lines-changed", (args, pl) =>
                {
                    var arr = pl.From(args, x => x.Lines);
                    pl.Element("total").SetText(arr.Sum(l => l.Price));
                }))),

            ("array.from-dom", () => Order(p =>
                p.DomReady(pl =>
                {
                    var arr = pl.FromDom("card", "classList");
                    pl.Element("klass-count").SetText(arr.Count());
                }))),

            // --- validation errors (show-validation-errors) ---
            ("validation.show-errors", () => Order(p =>
                p.DomReady(pl => pl.ValidationErrors("order-form")))),

            // --- mixed mega-plan: many families in one reaction sequence ---
            ("mega.mixed-families", () => Score(p =>
                p.CustomEvent<ScoreEvent>("score", (args, pl) =>
                {
                    pl.Element("start").SetText("start");
                    pl.When(args, x => x.Score).Gte(50)
                      .Then(t => t.Element("ok").AddClass("pass"))
                      .Else(e => e.Element("ok").AddClass("fail"));
                    pl.Post("/api/audit", g => g.FromEvent(args, x => x.Score, "score").Header("X-K", "v"));
                    pl.Dispatch("scored", new AuditPayload { Reason = "auto", Code = args.Score });
                    pl.Component<NativeTextBox>("tier").SetValue("computed");
                    pl.ValidationErrors("form");
                    pl.Element("done").SetText("done");
                }))),
        };

        // Two extra triggers chained to push count and variety well past 30.
        list.Add(("chained-triggers.dom+custom+sse", () => Order(p =>
            p.DomReady(d => d.Element("x").SetText("a"))
             .CustomEvent("e1", c => c.Element("y").SetText("b"))
             .ServerPush("/sse", s => s.Element("z").SetText("c")))));

        list.Add(("element.into-after-post", () => Order(p =>
            p.DomReady(pl =>
                pl.Post("/api/x", g => g.Static("k", 1))
                  .Response(r => r.OnSuccess(ok => ok.Into("target")))))));

        return list.ToArray();
    }

    // DET-CONGRUENT: independently-built but structurally-identical plan pairs.
    // Distinct lambdas, distinct plan instances, identical authored shape.
    private static IEnumerable<(string Name, Func<string> Left, Func<string> Right)> CongruentPairs()
    {
        yield return ("congruent.element-set",
            () => Order(p => p.DomReady(pl => pl.Element("a").SetText("hi"))),
            () => Order(p => p.DomReady(pl => pl.Element("a").SetText("hi"))));

        yield return ("congruent.branch",
            () => Score(p => p.CustomEvent<ScoreEvent>("s", (a, pl) =>
                pl.When(a, x => x.Score).Gte(90).Then(t => t.Element("g").SetText("A")).Else(e => e.Element("g").SetText("B")))),
            () => Score(p => p.CustomEvent<ScoreEvent>("s", (a, pl) =>
                pl.When(a, x => x.Score).Gte(90).Then(t => t.Element("g").SetText("A")).Else(e => e.Element("g").SetText("B")))));

        yield return ("congruent.http-gather",
            () => Order(p => p.DomReady(pl => pl.Post("/api/o/{id}", g => g.Static("source", "web").RouteParam("id", 1)))),
            () => Order(p => p.DomReady(pl => pl.Post("/api/o/{id}", g => g.Static("source", "web").RouteParam("id", 1)))));

        yield return ("congruent.component-set",
            () => Order(p => p.DomReady(pl => pl.Component<NativeTextBox>("box").SetValue("v").FocusIn())),
            () => Order(p => p.DomReady(pl => pl.Component<NativeTextBox>("box").SetValue("v").FocusIn())));

        yield return ("congruent.array-op",
            () => Order(p => p.CustomEvent<OrderModel>("lc", (a, pl) =>
                pl.Element("c").SetText(pl.From(a, x => x.Lines).Where(l => l.Count > 0).Count()))),
            () => Order(p => p.CustomEvent<OrderModel>("lc", (a, pl) =>
                pl.Element("c").SetText(pl.From(a, x => x.Lines).Where(l => l.Count > 0).Count()))));
    }

    // ---- plan builders (real public DSL entry points) ----

    private static string Order(Action<Alis.Reactive.Builders.TriggerBuilder<OrderModel>> trigger)
    {
        var plan = PlanExtensions.ReactivePlan(Html);
        HtmlExtensions.On(Html, plan, trigger);
        return plan.Render();
    }

    private static string Score(Action<Alis.Reactive.Builders.TriggerBuilder<ScoreEvent>> trigger)
    {
        var plan = PlanExtensions.ReactivePlan(ScoreHtml);
        HtmlExtensions.On(ScoreHtml, plan, trigger);
        return plan.Render();
    }
}
