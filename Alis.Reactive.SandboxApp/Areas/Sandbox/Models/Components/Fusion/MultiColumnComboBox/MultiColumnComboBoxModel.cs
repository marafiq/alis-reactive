namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.MultiColumnComboBox
{
    public class MultiColumnComboBoxModel
    {
        public string? Facility { get; set; }
        public string? Resident { get; set; }
    }

    /// <summary>
    /// Typed DataSource item for the facility multi-column combo box.
    /// PascalCase C# -> camelCase JSON via global JsonConvert.DefaultSettings in Program.cs.
    /// Fields reference the camelCase names: text, value, city, capacity.
    /// </summary>
    public class FacilityItem
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
        public string City { get; set; } = "";
        public int Capacity { get; set; }
    }
}
