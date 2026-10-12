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

    /// <summary>The HTTP page's own query string: <c>?tab=medications&amp;facilityId=7&amp;page=3&amp;residentId=42&amp;wing=garden</c>.</summary>
    public sealed class HttpShowcaseQuery
    {
        public string? Tab { get; set; }
        public int FacilityId { get; set; }
        public int Page { get; set; }
        public int ResidentId { get; set; }
        public string[] Wing { get; set; } = Array.Empty<string>();
    }

    /// <summary>The query <c>Search?q=</c> binds: the text a resident's name contains.</summary>
    public sealed class ResidentSearchQuery
    {
        public string Q { get; set; } = "";
    }

    /// <summary>The query <c>UrlParamEcho</c> binds and echoes back.</summary>
    public sealed class UrlParamEchoQuery
    {
        public string? Tab { get; set; }
        public int FacilityId { get; set; }
        public string? Page { get; set; }
    }

    /// <summary>The request <c>ComposeEcho/{id}?facility=</c> binds: the resident in the path, the facility in the query.</summary>
    public sealed class ComposeEchoQuery
    {
        public int Id { get; set; }
        public int Facility { get; set; }
    }

    /// <summary>The lookup <c>Residents/{id}</c> binds: the resident in the path.</summary>
    public sealed class ResidentLookup
    {
        public int Id { get; set; }
    }

    /// <summary>The lookup <c>Facilities/{facilityId}/Residents/{residentId}</c> binds: both in the path.</summary>
    public sealed class FacilityResidentLookup
    {
        public int FacilityId { get; set; }
        public int ResidentId { get; set; }
    }

    /// <summary>The lookup <c>ResidentByName/{name}</c> binds: the name in the path.</summary>
    public sealed class ResidentNameLookup
    {
        public string Name { get; set; } = "";
    }

    /// <summary>The body the <c>EchoHeaders</c> POST accepts; it answers with the headers only.</summary>
    public sealed class EchoHeadersBody
    {
        public string Payload { get; set; } = "";
    }

    public sealed class HttpActionLinkRow
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

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

    public class ResidentByIdResponse
    {
        public int ResidentId { get; set; }
        public string? Name { get; set; }
    }

    public class FacilityResidentResponse
    {
        public int FacilityId { get; set; }
        public int ResidentId { get; set; }
        public string? Name { get; set; }
    }

    public class ResidentByNameResponse
    {
        public string? ReceivedName { get; set; }
    }

    public class UrlParamEchoResponse
    {
        public string? Tab { get; set; }
        public string? FacilityId { get; set; }
        public string? Page { get; set; }
    }

    public class ComposeEchoResponse
    {
        public int ResidentId { get; set; }
        public string? Name { get; set; }
        public string? ReceivedTab { get; set; }
        public string? ReceivedFacility { get; set; }
    }

    public class EchoHeadersResponse
    {
        public string? ApiVersion { get; set; }
        public string? RequestId { get; set; }
        public string? TenantId { get; set; }
    }

    /// <summary>The document events the HttpShowcase page raises and hears.</summary>
    public static class HttpShowcaseEvents
    {
        public static readonly AppEvent NativeActionLinkGridRefresh = AppEvent.Named("native-action-link-grid-refresh");
    }
}
