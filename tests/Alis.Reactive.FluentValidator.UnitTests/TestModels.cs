namespace Alis.Reactive.FluentValidator.UnitTests;

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
}
