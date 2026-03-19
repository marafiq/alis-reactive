namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class NativeHiddenFieldModel
    {
        public string? ResidentId { get; set; }
        public string? FormToken { get; set; }
        public string? ResidentName { get; set; }
    }

    public class NativeHiddenFieldEchoResponse
    {
        public string? ResidentId { get; set; }
        public string? FormToken { get; set; }
        public string? ResidentName { get; set; }
        public int FieldCount { get; set; }
    }
}
