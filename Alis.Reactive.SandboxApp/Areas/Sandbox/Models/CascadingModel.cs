using Newtonsoft.Json;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class CascadingModel
    {
        public string? Country { get; set; }
        public string? City { get; set; }
    }

    /// <summary>
    /// Typed DataSource item for the country dropdown.
    /// JsonProperty ensures Syncfusion (Newtonsoft) serializes PascalCase to camelCase.
    /// Fields reference the camelCase names: text, value, continent.
    /// </summary>
    public class CountryItem
    {
        [JsonProperty("value")]
        public string Value { get; set; } = "";

        [JsonProperty("text")]
        public string Text { get; set; } = "";

        [JsonProperty("continent")]
        public string Continent { get; set; } = "";
    }

    /// <summary>
    /// Typed DataSource item for the city dropdown.
    /// JsonProperty ensures Syncfusion (Newtonsoft) serializes PascalCase to camelCase.
    /// Fields reference the camelCase names: text, value, state, population.
    /// </summary>
    public class CityItem
    {
        [JsonProperty("value")]
        public string Value { get; set; } = "";

        [JsonProperty("text")]
        public string Text { get; set; } = "";

        [JsonProperty("state")]
        public string State { get; set; } = "";

        [JsonProperty("population")]
        public int Population { get; set; }
    }

    /// <summary>
    /// Response type for city lookup — used in OnSuccess&lt;T&gt; typed binding.
    /// Properties match the camelCase JSON returned by CascadingController.Cities().
    /// </summary>
    public class CityLookupResponse
    {
        public object[]? Cities { get; set; }
        public string? Country { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// Response type for save — used in OnSuccess&lt;T&gt; typed binding.
    /// Properties match the camelCase JSON returned by CascadingController.Save().
    /// </summary>
    public class CascadingSaveResponse
    {
        public string? ReceivedCountry { get; set; }
        public string? ReceivedCity { get; set; }
        public string? Message { get; set; }
    }
}
