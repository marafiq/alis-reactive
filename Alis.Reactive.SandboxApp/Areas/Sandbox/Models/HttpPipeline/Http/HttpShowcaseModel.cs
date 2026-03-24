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

    // ── Response DTOs for typed OnSuccess<T> bindings ──────────────

    public class ResidentsResponse
    {
        public string? First { get; set; }
        public string? Second { get; set; }
        public int Count { get; set; }
    }

    public class SaveResponse
    {
        public string? Message { get; set; }
        public string? ReceivedName { get; set; }
    }

    public class UpdateResponse
    {
        public string? ReceivedName { get; set; }
        public string? ReceivedFacilityId { get; set; }
        public bool Updated { get; set; }
    }

    public class DeleteResponse
    {
        public bool Deleted { get; set; }
        public int DeletedId { get; set; }
    }

    public class FormDataResponse
    {
        public int Count { get; set; }
        public string? ReceivedFields { get; set; }
    }

    public class SearchResponse
    {
        public string? Query { get; set; }
        public int MatchCount { get; set; }
    }

    public class FacilitiesResponse
    {
        public string? First { get; set; }
        public string? Second { get; set; }
        public int Count { get; set; }
    }

}
