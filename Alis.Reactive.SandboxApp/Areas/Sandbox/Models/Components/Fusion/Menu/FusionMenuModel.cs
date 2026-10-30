namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Menu
{
    /// <summary>
    /// The staff navigation on the Maple Grove nurses' station tablet. Who is signed in decides which sections
    /// open; the navigation stays open until a page is chosen.
    /// </summary>
    public sealed class FusionMenuModel
    {
        /// <summary>The page the signed-in staff member chose, by its menu item; empty until one is chosen.</summary>
        public string CurrentPage { get; set; } = "";

        /// <summary>Who is signed in on the tablet.</summary>
        public StaffMember SignedIn { get; init; } = StaffMember.MedTech;

        /// <summary>Who the tablet switches to.</summary>
        public StaffMember SwitchTo { get; init; } = StaffMember.BusinessOffice;
    }

    /// <summary>The page the tablet loads, by the menu item that names it.</summary>
    public sealed class StaffPageQuery
    {
        public string Page { get; set; } = "";
    }

    /// <summary>What a page says when it opens.</summary>
    public sealed class StaffPage
    {
        public string Summary { get; set; } = "";
    }

    /// <summary>A staff member who signs in on the tablet; only the business office opens Billing.</summary>
    public sealed record StaffMember(string Key, string Name, string Role, bool OpensBilling)
    {
        public static StaffMember MedTech { get; } = new("med-tech", "Jordan Lee", "Med tech", false);

        public static StaffMember BusinessOffice { get; } = new("business-office", "Pat Kim", "Business office", true);
    }
}
