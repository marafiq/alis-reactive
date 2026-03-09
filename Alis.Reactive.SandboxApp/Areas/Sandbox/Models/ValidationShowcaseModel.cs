namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ValidationShowcaseModel
    {
        // Basic fields
        public string? Name { get; set; }
        public string? Email { get; set; }
        public int? Age { get; set; }
        public string? Phone { get; set; }
        public decimal? Salary { get; set; }
        public string? Website { get; set; }

        // Password pair (equalTo)
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }

        // Conditional field
        public bool IsEmployed { get; set; }
        public string? JobTitle { get; set; }

        // Nested address
        public ValidationAddress? Address { get; set; }
    }

    public class ValidationAddress
    {
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? ZipCode { get; set; }
    }
}
