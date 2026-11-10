using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

internal static class ViewProbes
{
    public static async Task Run(HttpClient client)
    {
        Console.WriteLine("core-partial-model-prefix: at the page a partial for Kin names " + Names(await client.GetStringAsync("/views/partial-model"))
            + "; inside the template for Kin a partial for its Address names " + Names(await client.GetStringAsync("/views/partial-in-template")));
        Console.WriteLine("core-template-visited: a null Next renders " + Chain(await client.GetStringAsync("/views/visited?shape=null"))
            + "; a Next that is the model itself renders " + Chain(await client.GetStringAsync("/views/visited?shape=loop"))
            + "; a distinct Next renders " + Chain(await client.GetStringAsync("/views/visited?shape=distinct")));
    }

    private static string Names(string page)
        => "[" + string.Join(", ", Regex.Matches(page, "name=\"([^\"]+)\"").Select(match => match.Groups[1].Value)) + "]";

    private static string Chain(string page)
        => "[" + string.Join(", ", Regex.Matches(page, "<i>chain ([^<]*)</i>").Select(match => match.Groups[1].Value)) + "]";
}

public sealed class ProbeKin
{
    public string Name { get; set; } = "Sam";

    public ProbeAddress Address { get; set; } = new();
}

public sealed class ProbeAddress
{
    public string City { get; set; } = "York";
}

public sealed class ProbeFamily
{
    public ProbeKin Kin { get; set; } = new();
}

public sealed class ProbeChain
{
    public string Name { get; set; } = "";

    public ProbeChain? Next { get; set; }
}

public sealed class ViewProbeController : Controller
{
    [HttpGet("/views/partial-model")]
    public IActionResult PartialModel() => View("~/Views/Probe/PartialModel.cshtml", new ProbeFamily());

    [HttpGet("/views/partial-in-template")]
    public IActionResult PartialInTemplate() => View("~/Views/Probe/PartialInTemplate.cshtml", new ProbeFamily());

    [HttpGet("/views/visited")]
    public IActionResult Visited(string shape)
    {
        var chain = new ProbeChain { Name = "first" };
        chain.Next = shape switch
        {
            "loop" => chain,
            "distinct" => new ProbeChain { Name = "second" },
            _ => null,
        };
        return View("~/Views/Probe/Visited.cshtml", chain);
    }
}
