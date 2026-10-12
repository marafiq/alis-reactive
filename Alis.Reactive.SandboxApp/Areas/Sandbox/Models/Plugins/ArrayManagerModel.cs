namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ArrayManagerModel { }

    public class ResidentsListResponse
    {
        public object[]? Items { get; set; }
    }

    /// <summary>What the page sends to the PluginEcho endpoint: <c>?count=</c>, the array plugin's count.</summary>
    public sealed class PluginEchoQuery
    {
        public int? Count { get; set; }
    }

    public class PluginArrayEchoResponse
    {
        public int? ReceivedCount { get; set; }
        public string? ReceivedHeader { get; set; }
    }
}
