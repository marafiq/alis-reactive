namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Validation.NestedBugs
{
    public class NestedAddressModel
    {
        public string? Name { get; set; }
        public NestedBugAddress Address { get; set; } = new();
    }

    public class NestedBugAddress
    {
        public string? City { get; set; }
        public string? ConfirmCity { get; set; }
    }

    public class ParentChildModel
    {
        public bool ParentFlag { get; set; }
        public ChildSection Child { get; set; } = new();
    }

    public class ChildSection
    {
        public bool ChildFlag { get; set; }
        public string? ChildName { get; set; }
    }

    public class IncludeModel
    {
        public bool IsEmployed { get; set; }
        public string? JobTitle { get; set; }
        public string? Department { get; set; }
    }
}
