using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Tracing;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Net.Http.Headers;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Trace;

namespace Alis.Reactive.Conformance.App;

internal static class AppHost
{
    private const string Urls = "urls";
    private const string ASecondPort = "http://127.0.0.1:0";

    internal static void SetJsonOptions(string host, JsonSerializerOptions json)
    {
        switch (host)
        {
            case HostNames.Net10:
            case HostNames.NoRequests:
            case HostNames.NoAntiforgeryChoice:
            case HostNames.SiteHeader:
            case HostNames.DefaultHooks:
            case HostNames.Tracing:
                WriteEnumNames(json);
                return;
            case HostNames.NumbersAsText:
                WriteEnumNames(json);
                json.NumberHandling = JsonNumberHandling.WriteAsString;
                return;
            case HostNames.DateConverter:
                WriteEnumNames(json);
                json.Converters.Add(new DayFirstDates());
                return;
            case HostNames.SnakeCase:
                WriteEnumNames(json);
                json.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                return;
            case HostNames.PascalCase:
                WriteEnumNames(json);
                json.PropertyNamingPolicy = null;
                return;
            case HostNames.EnumsAsNumbers:
                return;
            case HostNames.JsonContract:
                WriteEnumNames(json);
                json.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
                json.RespectRequiredConstructorParameters = true;
                return;
            default:
                throw new InvalidOperationException($"The conformance app serves no host '{host}'; it serves {HostNames.Net10}, {HostNames.NumbersAsText}, {HostNames.DateConverter}, {HostNames.SnakeCase}, {HostNames.PascalCase}, {HostNames.EnumsAsNumbers}, {HostNames.JsonContract}, {HostNames.NoRequests}, {HostNames.NoAntiforgeryChoice}, {HostNames.SiteHeader}, {HostNames.DefaultHooks} and {HostNames.Tracing}.");
        }
    }

    internal static void AddRequests(string host, IServiceCollection services)
    {
        switch (host)
        {
            case HostNames.NoRequests:
                return;
            case HostNames.NoAntiforgeryChoice:
                services.AddReactiveRequests(requests => { });
                return;
            case HostNames.SiteHeader:
                services.AddReactiveRequests(requests => TheDefaults(requests).Header("Care-Site", "North"));
                return;
            case HostNames.DefaultHooks:
                services.AddReactiveRequests(requests => TheDefaults(requests)
                    .WhileLoading(pipeline => pipeline.Plugin<CareSession>().Renew())
                    .Finally(pipeline =>
                    {
                        pipeline.Element(DefaultHooks.RequestLog).Text.Append("defaults finally ");
                        pipeline.Plugin<CareSession>().Flush();
                        pipeline.Element(DefaultHooks.FlushLine).Text.Set("flushed");
                    }));
                return;
            default:
                services.AddReactiveRequests(requests => TheDefaults(requests));
                return;
        }
    }

    private static RequestDefaults TheDefaults(RequestDefaults requests) => requests
        .Antiforgery(AntiforgeryIn.Header)
        .ShowBusy()
        .ShowValidationErrors()
        .OnError(401, Content.None, (expired, pipeline) => pipeline.Notice().Warning("Your session has ended."))
        .OnError((failed, pipeline) => pipeline.Notice().Error("The request failed."))
        .Retry(3, TimeSpan.FromSeconds(2));

    internal static void AddTracing(string host, WebApplicationBuilder builder)
    {
        if (host != HostNames.Tracing) return;
        if (!builder.Environment.IsDevelopment()) throw new InvalidOperationException("The tracing host exports to the World area's collector, which the app serves in Development alone.");

        var urls = builder.Configuration[Urls] ?? throw new InvalidOperationException("The tracing host adds a second port to the address --urls names.");
        builder.WebHost.UseUrls(urls, ASecondPort);
        builder.Services.AddOpenTelemetry().WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation()
            .AddSource(ReactiveTracing.SourceName)
            .SetSampler(services => new ParentBasedSampler(new RendersSampler(services.GetRequiredService<IHttpContextAccessor>(), services.GetRequiredService<Worlds>())))
            .AddProcessor(services => new SimpleActivityExportProcessor(new InMemoryExporter<Activity>(services.GetRequiredService<ExportedActivities>()))));
        builder.Services.AddCors();
        builder.Services.AddOptions<CorsOptions>().Configure<SecondOrigin>((cors, second) => cors.AddPolicy(SecondOrigin.Policy, policy => policy
            .SetIsOriginAllowed(second.IsThisServers)
            .WithMethods(HttpMethods.Get)
            .WithHeaders(HeaderNames.TraceParent, HeaderNames.TraceState, HeaderNames.Baggage)));
    }

    internal static void UseTracing(string host, WebApplication app)
    {
        if (host != HostNames.Tracing) return;

        app.UseRouting();
        app.UseSecondOrigin();
        app.UseCors();
        app.MapCollector();
    }

    internal static void AddValidation(IServiceCollection services)
        => services.AddReactiveFluentValidation(validators => validators
            .Flags<Components.CareDays>()
            .Flags<Care.CareDays>()
            .Flags<Care.RotaDays>()
            .Add<Bench.CodeRules>()
            .Add<Bench.StrictCodeRules>()
            .Add<Bench.EntriesRules>()
            .Add<Bench.FirstRules>()
            .Add<Care.ArrivalIntakeRules>()
            .Add<Care.BedAssignmentRules>()
            .Add<Care.DeputyAppointmentRules>()
            .Add<Care.DeputyNamelessAppointmentRules>()
            .Add<Care.HomeLeadRules>()
            .Add<Care.HouseholdContactsRules>()
            .Add<Care.IntakeAssessmentRules>()
            .Add<Care.QuickAdmissionRules>()
            .Add<Care.ResidentAdmissionRules>()
            .Add<Care.RoomAdmissionRules>()
            .Add<Care.RoomNumberingRules>()
            .Add<Care.StateMoveInTexasRules>()
            .Add<Care.StateMoveInMassachusettsRules>()
            .Add<Care.VisitorSignInRules>()
            .Add<Care.WardAssignmentRules>()
            .Add<Care.CarePlanRules>()
            .Add<Care.CareRotaRules>()
            .Add<Care.EarlyCareRotaRules>()
            .Add<Care.RoundDosingRules>()
            .Add<Care.SupplyCountRules>()
            .Add<Refusals.VisitValidator>());

    private static void WriteEnumNames(JsonSerializerOptions json) => json.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

    private sealed class DayFirstDates : JsonConverter<DateTime>
    {
        private const string Form = "dd.MM.yyyy";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => DateTime.ParseExact(reader.GetString()!, Form, CultureInfo.InvariantCulture);

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(Form, CultureInfo.InvariantCulture));
    }
}
