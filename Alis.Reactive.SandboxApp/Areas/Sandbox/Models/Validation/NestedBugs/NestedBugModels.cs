namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Validation.NestedBugs
{
    // --- Claim 1 & 3: Nested address with condition + cross-property ---
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

    // --- Claim 2: Parent + child condition composition ---
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

    // --- Claim 4: Include inside WhenField ---
    public class IncludeModel
    {
        public bool IsEmployed { get; set; }
        public string? JobTitle { get; set; }
        public string? Department { get; set; }
    }
}
