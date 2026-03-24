namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ArchitectureModel
    {
        public string? NativeValue { get; set; }
        public string? FusionValue { get; set; }
    }

    // Typed payloads for architecture regression tests (Sections 4 & 5)

    public class NativeChangePayload
    {
        public string? Value { get; set; }
    }

    public class FusionChangePayload
    {
        public string? NewValue { get; set; }
    }

    public class DeepWalkPayload
    {
        public DeepWalkResult? Result { get; set; }
    }

    public class DeepWalkResult
    {
        public DeepWalkDetail? Detail { get; set; }
    }

    public class DeepWalkDetail
    {
        public string? Total { get; set; }
        public DeepWalkAddress? Address { get; set; }
    }

    public class DeepWalkAddress
    {
        public string? City { get; set; }
    }
}
