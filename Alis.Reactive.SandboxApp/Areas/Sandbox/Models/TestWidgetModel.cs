namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class TestWidgetModel
    {
        public string? WidgetValue { get; set; }
    }

    public class DataSourceResponse
    {
        public string? Name { get; set; }
        public int Count { get; set; }
        public string? Selected { get; set; }
        public string[]? Items { get; set; }
        public DataSourceDetail? Detail { get; set; }
    }

    public class DataSourceDetail
    {
        public string? Region { get; set; }
        public DataSourceMetadata? Metadata { get; set; }
    }

    public class DataSourceMetadata
    {
        public int Version { get; set; }
    }
}
