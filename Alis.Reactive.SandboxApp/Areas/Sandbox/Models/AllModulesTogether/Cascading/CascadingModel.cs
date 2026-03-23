namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.AllModulesTogether.Cascading
{
    public class CascadingModel
    {
        public string? Country { get; set; }
        public string? City { get; set; }
    }

    /// <summary>
    /// Typed DataSource item for the country dropdown.
    /// PascalCase C# → camelCase JSON via global JsonConvert.DefaultSettings in Program.cs.
    /// No [JsonProperty] needed. Fields reference camelCase: text, value, continent.
    /// </summary>
    public class CountryItem
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
        public string Continent { get; set; } = "";
    }

    /// <summary>
    /// Typed DataSource item for the city dropdown.
    /// PascalCase C# → camelCase JSON via global JsonConvert.DefaultSettings in Program.cs.
    /// No [JsonProperty] needed. Fields reference camelCase: text, value, state, population.
    /// </summary>
    public class CityItem
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
        public string State { get; set; } = "";
        public int Population { get; set; }
    }

    public class CityLookupResponse
    {
        public object[]? Cities { get; set; }
        public string? Country { get; set; }
        public int Count { get; set; }
    }

    public class CascadingSaveResponse
    {
        public string? ReceivedCountry { get; set; }
        public string? ReceivedCity { get; set; }
        public string? Message { get; set; }
    }
}
