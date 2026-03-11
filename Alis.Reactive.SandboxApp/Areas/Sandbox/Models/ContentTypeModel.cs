namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ContentTypeModel
    {
        public string? NativeValue { get; set; }
        public decimal FusionAmount { get; set; }
    }

    /// <summary>Flat JSON response — no nesting.</summary>
    public class FlatResponse
    {
        public string? Message { get; set; }
        public int Count { get; set; }
    }

    /// <summary>Nested JSON response — multiple levels deep.</summary>
    public class NestedResponse
    {
        public ResponseData? Data { get; set; }
    }

    public class ResponseData
    {
        public ResponseUser? User { get; set; }
        public decimal Total { get; set; }
    }

    public class ResponseUser
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
    }
}
