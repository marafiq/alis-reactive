namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class TestWidgetModel
    {
        public string? WidgetVal { get; set; }
    }

    public class TestWidgetItemsPayload
    {
        public string[]? Items { get; set; }
    }

    public class TestWidgetDataSourceResponse
    {
        public string? Value { get; set; }
        public string[]? Items { get; set; }
    }
}
