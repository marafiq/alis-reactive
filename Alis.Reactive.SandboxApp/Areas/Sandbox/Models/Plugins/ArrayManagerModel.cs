namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ArrayManagerModel { }

    public class ResidentsListResponse
    {
        public object[]? Items { get; set; }
    }

    public class PluginArrayEchoResponse
    {
        public int? ReceivedCount { get; set; }
        public string? ReceivedHeader { get; set; }
    }
}
