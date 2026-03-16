namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class CascadingModel
    {
        public string? Country { get; set; }
        public string? City { get; set; }
    }

    /// <summary>
    /// Response type for city lookup — used in OnSuccess&lt;T&gt; typed binding.
    /// Properties match the camelCase JSON returned by CascadingController.Cities().
    /// </summary>
    public class CityLookupResponse
    {
        public object[]? Cities { get; set; }
        public string? Country { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// Response type for save — used in OnSuccess&lt;T&gt; typed binding.
    /// Properties match the camelCase JSON returned by CascadingController.Save().
    /// </summary>
    public class CascadingSaveResponse
    {
        public string? ReceivedCountry { get; set; }
        public string? ReceivedCity { get; set; }
        public string? Message { get; set; }
    }
}
