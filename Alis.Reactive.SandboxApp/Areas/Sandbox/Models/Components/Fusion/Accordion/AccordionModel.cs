namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Backs the resident "My Care Plan" page. The plan is organized into collapsible
    /// sections; the monthly-charges section stays locked until the resident acknowledges
    /// their care agreement, and its detail is fetched on demand when first opened.
    /// </summary>
    public class AccordionModel
    {
        /// <summary>Resident the care plan belongs to, shown in the page heading.</summary>
        public string ResidentName { get; set; } = "Eleanor Whitfield";

        /// <summary>Community the resident lives in.</summary>
        public string CommunityName { get; set; } = "Sunrise at Cedar Grove";
    }

    /// <summary>Server response for the on-demand monthly-charges fetch.</summary>
    public class CareChargesResponse
    {
        /// <summary>Headline the resident sees once charges load, e.g. the billing month.</summary>
        public string Heading { get; set; } = "";
    }
}
