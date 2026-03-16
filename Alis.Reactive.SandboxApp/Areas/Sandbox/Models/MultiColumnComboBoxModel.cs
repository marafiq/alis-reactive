using Newtonsoft.Json;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class MultiColumnComboBoxModel
    {
        public string? Facility { get; set; }
        public string? Resident { get; set; }
    }

    /// <summary>
    /// Typed DataSource item for the facility multi-column combo box.
    /// JsonProperty ensures Syncfusion (Newtonsoft) serializes PascalCase to camelCase.
    /// Fields reference the camelCase names: text, value, city, capacity.
    /// </summary>
    public class FacilityItem
    {
        [JsonProperty("value")]
        public string Value { get; set; } = "";

        [JsonProperty("text")]
        public string Text { get; set; } = "";

        [JsonProperty("city")]
        public string City { get; set; } = "";

        [JsonProperty("capacity")]
        public int Capacity { get; set; }
    }
}
