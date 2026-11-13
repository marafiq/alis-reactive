using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App;

internal static class AppHost
{
    internal static void SetJsonOptions(string host, JsonSerializerOptions json)
    {
        switch (host)
        {
            case HostNames.Net10:
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
            default:
                throw new InvalidOperationException($"The conformance app serves no host '{host}'; it serves {HostNames.Net10}, {HostNames.NumbersAsText}, {HostNames.DateConverter}, {HostNames.SnakeCase}, {HostNames.PascalCase} and {HostNames.EnumsAsNumbers}.");
        }
    }

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
