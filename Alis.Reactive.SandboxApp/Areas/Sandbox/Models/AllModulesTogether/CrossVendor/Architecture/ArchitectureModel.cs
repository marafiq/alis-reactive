namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ArchitectureModel
    {
        public string? NativeValue { get; set; }
        public string? FusionValue { get; set; }

        // Section 6: Cross-vendor validation
        public string? NativeRequired { get; set; }
        public string? FusionRequired { get; set; }

        // Section 7: Cross-vendor equalTo
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
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
        public DeepWalkResult Result { get; set; } = new();
    }

    public class DeepWalkResult
    {
        public DeepWalkDetail Detail { get; set; } = new();
    }

    public class DeepWalkDetail
    {
        public string? Total { get; set; }
        public DeepWalkAddress Address { get; set; } = new();
    }

    public class DeepWalkAddress
    {
        public string? City { get; set; }
    }
}
