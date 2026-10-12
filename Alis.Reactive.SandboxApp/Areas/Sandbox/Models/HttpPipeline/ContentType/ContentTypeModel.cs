namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ContentTypeModel
    {
        public string? NativeValue { get; set; }
        public decimal FusionAmount { get; set; }
    }

    public class FlatResponse
    {
        public string? Message { get; set; }
        public int Count { get; set; }
    }

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

    /// <summary>The document events the ContentType page raises and hears.</summary>
    public static class ContentTypeEvents
    {
        public static readonly AppEvent FetchFlat = AppEvent.Named("fetch-flat");
        public static readonly AppEvent FetchNested = AppEvent.Named("fetch-nested");
        public static readonly AppEvent FetchPartial = AppEvent.Named("fetch-partial");
    }
}
