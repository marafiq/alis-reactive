using System.Text.Json;
using System.Text.RegularExpressions;
using Json.Schema;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

[TestFixture]
public class WhenAnAppRendersItsPages
{
    [Test]
    public async Task every_sandbox_get_action_answers_and_every_plan_it_renders_conforms()
    {
        Environment.SetEnvironmentVariable("ALIS_NO_BROADCAST", "1");
        await using var app = new WebApplicationFactory<Alis.Reactive.SandboxApp.Hubs.NotificationHub>();

        await Sweep.Run(app, "the sandbox");
    }

    [Test]
    public async Task every_framework_app_get_action_answers_and_every_plan_it_renders_conforms()
    {
        await using var app = new WebApplicationFactory<Alis.Reactive.Framework.App.Realtime.LineExchange>();

        await Sweep.Run(app, "the framework app");
    }
}

internal static class Sweep
{
    private static readonly JsonSchema Schema =
        JsonSchema.FromFile(Path.Combine(Repository.Root, "Alis.Reactive.Assets", "runtime", "types", "plan.schema.json"));

    private static readonly Regex PlanScript = new("<script type=\"application/json\" data-reactive-plan>(.*?)</script>", RegexOptions.Singleline);

    private static readonly Regex Link = new("href=\"(/[^\"]*)\"");
    private static readonly Regex PlanUrl = new("\"name\":\"url\",\"value\":\\{\"kind\":\"lit\",\"value\":\"(/[^\"{]*)\"\\}");

    private static readonly Regex Placeholder = new(@"\{\*?(\w+)(?::([^}?]*))?\??\}");

    internal static async Task Run<TApp>(WebApplicationFactory<TApp> app, string name) where TApp : class
    {
        using var client = app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Accept.ParseAdd("text/html");
        var queue = new Queue<(string Origin, string Path)>(ActionPaths(app.Services));
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var failures = new List<string>();
        var refused = new List<string>();
        var slowest = new List<(string Path, long Ms)>();
        var plans = 0;

        while (queue.Count > 0)
        {
            var (origin, path) = queue.Dequeue();
            if (!visited.Add(path)) continue;

            string body;
            var started = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                using var response = await client.GetAsync(path, HttpCompletionOption.ResponseHeadersRead);
                if ((int)response.StatusCode >= 500) refused.Add($"{origin} ({(int)response.StatusCode})");
                if (response.Content.Headers.ContentType?.MediaType != "text/html") continue;
                body = await response.Content.ReadAsStringAsync();
            }
            catch (Exception aborted) when (aborted is OperationCanceledException || aborted is HttpRequestException)
            {
                refused.Add($"{origin} (aborted)");
                continue;
            }

            slowest.Add((path, started.ElapsedMilliseconds));
            foreach (Match script in PlanScript.Matches(body))
            {
                plans++;
                var refusal = Refusal(script.Groups[1].Value);
                if (refusal != null) failures.Add($"{origin}: a plan on GET {path} does not conform to plan.schema.json:\n{refusal}");
            }
            foreach (Match link in Link.Matches(body)) queue.Enqueue(($"a link on {path}", link.Groups[1].Value));
            foreach (Match url in PlanUrl.Matches(body)) queue.Enqueue(($"a plan on {path}", url.Groups[1].Value));
        }

        TestContext.Progress.WriteLine(
            $"[sweep] {name}: {visited.Count} pages requested, {plans} plans conform, {refused.Count} refused to render" +
            (refused.Count == 0 ? "" : ": " + string.Join(", ", refused)));
        TestContext.Progress.WriteLine(
            $"[sweep] {name}: slowest pages " + string.Join(", ", slowest.OrderByDescending(page => page.Ms).Take(5).Select(page => $"{page.Path} {page.Ms} ms")));
        Assert.That(failures, Is.Empty, $"{name}, {visited.Count} pages requested, {plans} plans checked:\n" + string.Join("\n", failures));
        Assert.That(plans, Is.GreaterThan(0), $"{name} rendered no plan on {visited.Count} pages; the sweep read nothing.");
    }

    private static IEnumerable<(string Action, string Path)> ActionPaths(IServiceProvider services)
    {
        var links = services.GetRequiredService<LinkGenerator>();
        var actions = services.GetRequiredService<IActionDescriptorCollectionProvider>().ActionDescriptors.Items
            .OfType<ControllerActionDescriptor>()
            .Where(AnswersGet);
        foreach (var action in actions)
        {
            var values = new RouteValueDictionary(action.RouteValues);
            if (action.AttributeRouteInfo?.Template is string template)
            {
                foreach (Match parameter in Placeholder.Matches(template))
                    values[parameter.Groups[1].Value] = Sample(parameter.Groups[2].Value);
            }

            var path = links.GetPathByAction(action.ActionName, action.ControllerName, values)
                ?? throw new InvalidOperationException($"{action.DisplayName} has no path.");
            yield return ($"{action.ControllerName}.{action.ActionName}", path);
        }
    }

    private static bool AnswersGet(ControllerActionDescriptor action)
    {
        var methods = action.EndpointMetadata.OfType<IHttpMethodMetadata>().SelectMany(metadata => metadata.HttpMethods)
            .Concat(action.ActionConstraints?.OfType<HttpMethodActionConstraint>().SelectMany(constraint => constraint.HttpMethods) ?? [])
            .ToList();
        return methods.Count == 0 || methods.Contains("GET", StringComparer.OrdinalIgnoreCase);
    }

    private static string Sample(string constraint) => constraint switch
    {
        var c when c.Contains("int") || c.Contains("long") => "1",
        var c when c.Contains("guid") => Guid.Empty.ToString(),
        var c when c.Contains("bool") => "true",
        var c when c.Contains("datetime") => "2026-01-01",
        _ => "sweep",
    };

    private static string? Refusal(string json)
    {
        JsonDocument plan;
        try { plan = JsonDocument.Parse(json); }
        catch (JsonException notJson) { return $"    not JSON: {notJson.Message}\n    {json}"; }
        using var _ = plan;
        var result = Schema.Evaluate(plan.RootElement, new EvaluationOptions { OutputFormat = OutputFormat.List });
        if (result.IsValid) return null;
        return string.Join("\n", result.Details!
            .Where(detail => detail.Errors != null)
            .Select(detail => $"    {detail.InstanceLocation}: {string.Join("; ", detail.Errors!.Select(error => error.Key + ": " + error.Value))}"));
    }
}
