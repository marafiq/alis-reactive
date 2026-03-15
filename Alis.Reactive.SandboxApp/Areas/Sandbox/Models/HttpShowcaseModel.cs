namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class HttpShowcaseModel
    {
        public string? Name { get; set; }
        public int? FacilityId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public IReadOnlyList<HttpActionLinkRow> ActionRows { get; set; } = Array.Empty<HttpActionLinkRow>();
    }

    public sealed class HttpActionLinkRow
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
