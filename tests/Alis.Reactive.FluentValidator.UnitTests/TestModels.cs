using System.Collections.Generic;
using Alis.Reactive;

namespace Alis.Reactive.FluentValidator.UnitTests;

/// <summary>
/// Builds a ComponentsMap for TestModel — mirrors what plan-aware builder overloads
/// do in production. Every field registered as native/value.
/// </summary>
public static class TestComponentsMap
{
    public static IReadOnlyDictionary<string, ComponentRegistration> ForTestModel()
    {
        var map = new Dictionary<string, ComponentRegistration>();
        Register(map, "Name");
        Register(map, "Email");
        Register(map, "Age");
        Register(map, "Phone");
        Register(map, "Salary");
        Register(map, "Website");
        Register(map, "JobTitle");
        Register(map, "IsEmployed");
        Register(map, "Address.Street");
        Register(map, "Address.City");
        Register(map, "Address.ZipCode");
        Register(map, "DeepAddress.Street");
        Register(map, "DeepAddress.Country.Code");
        Register(map, "DeepAddress.Country.Name");
        return map;
    }

    private static void Register(Dictionary<string, ComponentRegistration> map, string propertyPath)
    {
        var elementId = IdGenerator.For(typeof(TestModel), propertyPath);
        map[propertyPath] = new ComponentRegistration(elementId, "native", propertyPath, "value");
    }
}

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
