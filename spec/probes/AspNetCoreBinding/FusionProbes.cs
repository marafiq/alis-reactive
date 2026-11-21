using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

internal static class FusionProbes
{
    public static async Task Run(HttpClient client)
    {
        var unsetPage = await client.GetStringAsync("/fusion/allergies");
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy { ProcessDictionaryKeys = true, OverrideSpecifiedNames = false } }
        };
        var camelPage = await client.GetStringAsync("/fusion/allergies");
        var rawPage = await client.GetStringAsync("/fusion/allergies-raw");
        JsonConvert.DefaultSettings = null;
        Console.WriteLine("fusion-ej2-builder-newtonsoft-defaults: EJ2's MultiSelect writes its C# dataSource as " + DataSource(unsetPage)
            + " with no Newtonsoft default settings, and as " + DataSource(camelPage) + " under AddReactiveFusion's camel case; System.Text.Json writes "
            + System.Text.Json.JsonSerializer.Serialize(FusionAllergyProbe.Allergies, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web)));
        Console.WriteLine("fusion-ej2-builder-raw-json: a Newtonsoft JRaw of System.Text.Json's text, under AddReactiveFusion's camel case, is written as "
            + DataSource(rawPage));
        Console.WriteLine("fusion-ej2-script-text-escaped: EJ2 writes the label " + FusionAllergyProbe.Allergies[1].DisplayLabel
            + " into the page's script element with < and > as \\u003c and \\u003e, and as is " + camelPage.Contains("\"" + FusionAllergyProbe.Allergies[1].DisplayLabel + "\"", StringComparison.Ordinal));
    }

    public static string WrittenByTheAppsOptions()
        => System.Text.Json.JsonSerializer.Serialize(FusionAllergyProbe.Allergies, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));

    private static string DataSource(string page)
    {
        var match = Regex.Match(page, "\"dataSource\": ejs\\.data\\.DataUtil\\.parse\\.isJson\\((\\[.*?\\])\\)", RegexOptions.Singleline);
        return match.Success ? Regex.Replace(match.Groups[1].Value, "\\s*\\n\\s*", "") : "nothing";
    }
}

public sealed class FusionAllergyProbe
{
    public static readonly IReadOnlyList<FusionAllergyProbe> Allergies =
        [new() { AllergyCode = 7, DisplayLabel = "Penicillin" }, new() { AllergyCode = 9, DisplayLabel = "</script><b>Ada</b>" }];

    public int AllergyCode { get; init; }

    [JsonPropertyName("label")]
    public string DisplayLabel { get; init; } = "";
}

public sealed class FusionProbeController : Controller
{
    [HttpGet("/fusion/allergies")]
    public IActionResult Allergies() => View("~/Views/Probe/Fusion.cshtml", FusionAllergyProbe.Allergies);

    [HttpGet("/fusion/allergies-raw")]
    public IActionResult AllergiesRaw() => View("~/Views/Probe/FusionRaw.cshtml", new Newtonsoft.Json.Linq.JRaw(FusionProbes.WrittenByTheAppsOptions()));
}
