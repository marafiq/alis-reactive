using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

internal static class RowsProbes
{
    private const string Rows = "{\"rows\":[{\"name\":\"Ada\",\"status\":1,\"held\":\"by the vendor\"}]}";

    public static async Task Run()
    {
        var namesAndNumbers = await Bound(json => json.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));
        var namesAlone = await Bound(json => json.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false)));
        Console.WriteLine("core-frombody-row-enum-number: a row's enum sent as its number under an options-added JsonStringEnumConverter binds " + namesAndNumbers
            + " | with allowIntegerValues false binds " + namesAlone);
        var skipped = await Bound(_ => { });
        var disallowed = await Bound(json => json.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow);
        Console.WriteLine("core-frombody-row-unmapped-member: a row member its class lacks, under the default options, binds " + skipped
            + " | under UnmappedMemberHandling.Disallow binds " + disallowed);
    }

    private static async Task<string> Bound(Action<JsonSerializerOptions> options)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddControllers().AddJsonOptions(json => options(json.JsonSerializerOptions));
        var app = builder.Build();
        app.MapControllers();
        await app.StartAsync();
        try
        {
            using var client = new HttpClient { BaseAddress = new Uri(app.Urls.First()) };
            using var response = await client.PostAsync("/rows/billing", new StringContent(Rows, Encoding.UTF8, "application/json"));
            return await response.Content.ReadAsStringAsync();
        }
        finally
        {
            await app.StopAsync();
            await app.DisposeAsync();
        }
    }
}

public enum RowsProbeStatus
{
    Due,
    Paid,
}

public sealed class RowsProbeRow
{
    public string Name { get; set; } = "";

    public RowsProbeStatus Status { get; set; }
}

public sealed class RowsProbeSave
{
    public IEnumerable<RowsProbeRow> Rows { get; set; } = [];
}

public sealed class RowsProbeController : ControllerBase
{
    [HttpPost("/rows/billing")]
    public string Billing([FromBody] RowsProbeSave? save)
        => (save is null ? "nothing" : "Rows=[" + string.Join(",", save.Rows.Select(row => row.Name + " " + row.Status)) + "]")
           + (ModelState.IsValid ? "" : " (errors " + string.Join(", ", ModelState.Where(entry => entry.Value!.Errors.Count > 0).Select(entry => entry.Key)) + ")");
}
