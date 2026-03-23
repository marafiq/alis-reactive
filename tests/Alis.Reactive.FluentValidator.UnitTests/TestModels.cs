namespace Alis.Reactive.FluentValidator.UnitTests;

public enum CareLevel { Independent, Assisted, MemoryCare, Skilled }

public class TestAddress
{
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? ZipCode { get; set; }
}

public class TestCountry
{
    public string? Code { get; set; }
    public string? Name { get; set; }
}

public class TestDeepAddress
{
    public string? Street { get; set; }
    public TestCountry? Country { get; set; }
}

public class TestModel
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public int Age { get; set; }
    public string? Phone { get; set; }
    public decimal Salary { get; set; }
    public string? Website { get; set; }
    public TestAddress? Address { get; set; }
    public TestDeepAddress? DeepAddress { get; set; }
    public bool IsEmployed { get; set; }
    public string? JobTitle { get; set; }
    public string? ConfirmEmail { get; set; }
    public CareLevel CareLevel { get; set; }
    public TimeOnly ShiftStart { get; set; }
    public TimeOnly ShiftEnd { get; set; }
}

public class NestedCrossPropertyModel
{
    public string? Name { get; set; }
    public TestAddress? HomeAddress { get; set; }
    public TestAddress? WorkAddress { get; set; }
}

public class FullCoverageModel
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? ConfirmEmail { get; set; }
    public string? AlternateEmail { get; set; }
    public string? Phone { get; set; }
    public string? CreditCardNumber { get; set; }
    public int Age { get; set; }
    public decimal Score { get; set; }
    public decimal Salary { get; set; }
    public decimal MonthlyRate { get; set; }
    public string? Status { get; set; }
    public string? Nickname { get; set; }
    public DateTime AdmissionDate { get; set; }
    public DateTime DischargeDate { get; set; }
    public bool IsEmployed { get; set; }
}
