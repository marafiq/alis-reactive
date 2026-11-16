using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;

namespace Alis.Reactive.Conformance.App;

internal static class AppHost
{
    internal static void SetJsonOptions(string host, JsonSerializerOptions json)
    {
        switch (host)
        {
            case HostNames.Net10:
            case HostNames.NoRequests:
            case HostNames.NoAntiforgeryChoice:
            case HostNames.SiteHeader:
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
                throw new InvalidOperationException($"The conformance app serves no host '{host}'; it serves {HostNames.Net10}, {HostNames.NumbersAsText}, {HostNames.DateConverter}, {HostNames.SnakeCase}, {HostNames.PascalCase}, {HostNames.EnumsAsNumbers}, {HostNames.JsonContract}, {HostNames.NoRequests} and {HostNames.NoAntiforgeryChoice}, {HostNames.SiteHeader}.");
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

    internal static void AddValidation(IServiceCollection services)
        => services.AddReactiveFluentValidation(validators => validators.Flags<Components.CareDays>().Flags<Care.CareDays>().Flags<Care.RotaDays>());

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
