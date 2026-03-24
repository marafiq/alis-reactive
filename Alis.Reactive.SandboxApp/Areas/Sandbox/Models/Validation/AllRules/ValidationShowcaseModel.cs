namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ValidationShowcaseModel
    {
        public AllRulesSection AllRules { get; set; } = new();
        public BasicSection Server { get; set; } = new();
        public NestedSection Nested { get; set; } = new();
        public ConditionalSection Conditional { get; set; } = new();
        public BasicSection Live { get; set; } = new();
        public CombinedSection Combined { get; set; } = new();
        public HiddenFieldsSection Hidden { get; set; } = new();
        public BasicSection Db { get; set; } = new();
    }

    public class BasicSection
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
    }

    public class AllRulesSection : BasicSection
    {
        public int? Age { get; set; }
        public string? Phone { get; set; }
        public decimal? Salary { get; set; }
        public string? Password { get; set; }
    }

    public class CombinedSection : BasicSection
    {
        public int? Age { get; set; }
        public string? Phone { get; set; }
    }

    public class HiddenFieldsSection
    {
        public bool ShowExtras { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public decimal? Salary { get; set; }
    }

    public class ConditionalSection
    {
        public bool IsEmployed { get; set; }
        public string? JobTitle { get; set; }
    }

    public class NestedSection
    {
        public ValidationAddress? Address { get; set; }
        public DeliveryNote? Delivery { get; set; }
    }

    public class ValidationAddress
    {
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? ZipCode { get; set; }
    }

    public class DeliveryNote
    {
        public string? Instructions { get; set; }
        public string? ContactPhone { get; set; }
    }
}
