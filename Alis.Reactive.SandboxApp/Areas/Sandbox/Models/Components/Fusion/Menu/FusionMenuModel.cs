namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Menu
{
    public sealed class FusionMenuModel
    {
        public string CurrentPage { get; set; } = "";

        public StaffMember SignedIn { get; init; } = StaffMember.MedTech;

        public StaffMember SwitchTo { get; init; } = StaffMember.BusinessOffice;
    }

    public sealed class StaffPageQuery
    {
        public string Page { get; set; } = "";
    }

    public sealed class StaffPage
    {
        public string Summary { get; set; } = "";
    }

    public sealed record StaffMember(string Key, string Name, string Role, bool OpensBilling)
    {
        public static StaffMember MedTech { get; } = new("med-tech", "Jordan Lee", "Med tech", false);

        public static StaffMember BusinessOffice { get; } = new("business-office", "Pat Kim", "Business office", true);
    }
}
