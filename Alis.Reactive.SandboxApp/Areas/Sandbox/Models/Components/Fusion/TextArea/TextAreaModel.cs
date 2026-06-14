namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class TextAreaModel
    {
        public string? CareNote { get; set; }
    }

    public sealed class CareNoteEchoRequest
    {
        public string? CareNote { get; set; }
    }

    public sealed class CareNoteEchoResponse
    {
        public string? CareNote { get; set; }

        public string Summary { get; set; } = string.Empty;
    }
}
