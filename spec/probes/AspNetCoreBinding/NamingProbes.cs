using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Mvc;

internal static class NamingProbes
{
    public static async Task Run()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddControllers().AddJsonOptions(json =>
        {
            json.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
            json.JsonSerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { LeavePharmacyOut } };
        });
        var app = builder.Build();
        app.MapControllers();
        await app.StartAsync();
        try
        {
            using var client = new HttpClient { BaseAddress = new Uri(app.Urls.First()) };
            Console.WriteLine("core-frombody-naming-policy: under SnakeCaseLower "
                + await Admitted(client, "{\"given_name\":\"Rose\"}") + "; "
                + await Admitted(client, "{\"givenName\":\"Rose\"}") + "; "
                + await Admitted(client, "{\"GivenName\":\"Rose\"}"));
            Console.WriteLine("core-frombody-left-out-member-starts: "
                + await Admitted(client, "{\"given_name\":\"Rose\",\"ward\":\"East\",\"pharmacy\":\"North\"}"));
        }
        finally
        {
            await app.StopAsync();
            await app.DisposeAsync();
        }
    }

    private static void LeavePharmacyOut(JsonTypeInfo contract)
    {
        if (contract.Type != typeof(NamingAdmission)) return;
        var pharmacy = contract.Properties.Single(property => (property.AttributeProvider as MemberInfo)?.Name == nameof(NamingAdmission.Pharmacy));
        contract.Properties.Remove(pharmacy);
    }

    private static async Task<string> Admitted(HttpClient client, string body)
    {
        using var response = await client.PostAsync("/naming/admission", new StringContent(body, Encoding.UTF8, "application/json"));
        return body + " binds " + await response.Content.ReadAsStringAsync();
    }
}

public sealed class NamingAdmission
{
    public string? GivenName { get; set; }

    [JsonIgnore]
    public string Ward { get; set; } = "Rehab";

    public string Pharmacy { get; set; } = "Central";
}

public sealed class NamingProbeController : ControllerBase
{
    [HttpPost("/naming/admission")]
    public string Admission([FromBody] NamingAdmission admission)
        => "GivenName=" + (admission.GivenName ?? "<null>") + " Ward=" + admission.Ward + " Pharmacy=" + admission.Pharmacy;
}
