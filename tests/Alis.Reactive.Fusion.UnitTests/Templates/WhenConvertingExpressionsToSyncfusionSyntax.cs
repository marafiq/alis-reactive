using Alis.Reactive.Fusion.Templates;

namespace Alis.Reactive.Fusion.UnitTests.Templates;

public class TemplateTestModel
{
    public int Id { get; set; }
    public string Subject { get; set; } = "";
    public string StaffRole { get; set; } = "";
    public string StaffPhone { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsUnassigned { get; set; }
    public string ProfileUrl { get; set; } = "";
    public TemplateAddress Address { get; set; } = new();
}

public class TemplateAddress
{
    public string City { get; set; } = "";
    public string State { get; set; } = "";
}

[TestFixture]
public class WhenConvertingExpressionsToTemplateSyntax
{
    [Test]
    public void Simple_property_converts_to_camelCase_binding()
    {
        var result = FusionTemplateExpression.ToBinding<TemplateTestModel, string>(m => m.Subject);
        Assert.That(result, Is.EqualTo("${subject}"));
    }

    [Test]
    public void Nested_property_converts_to_dotted_camelCase_path()
    {
        var result = FusionTemplateExpression.ToBinding<TemplateTestModel, string>(m => m.Address.City);
        Assert.That(result, Is.EqualTo("${address.city}"));
    }

    [Test]
    public void Int_property_converts_to_camelCase_binding()
    {
        var result = FusionTemplateExpression.ToBinding<TemplateTestModel, int>(m => m.Id);
        Assert.That(result, Is.EqualTo("${id}"));
    }

    [Test]
    public void Bool_property_converts_to_camelCase_binding()
    {
        var result = FusionTemplateExpression.ToBinding<TemplateTestModel, bool>(m => m.IsUnassigned);
        Assert.That(result, Is.EqualTo("${isUnassigned}"));
    }

    [Test]
    public void ToPropertyPath_returns_path_without_binding_syntax()
    {
        var result = FusionTemplateExpression.ToPropertyPath<TemplateTestModel, string>(m => m.Subject);
        Assert.That(result, Is.EqualTo("subject"));
    }

    [Test]
    public void Equality_condition_converts_to_triple_equals()
    {
        var result = FusionTemplateExpression.ToCondition<TemplateTestModel>(m => m.StaffRole == "RN");
        Assert.That(result, Is.EqualTo("staffRole === 'RN'"));
    }

    [Test]
    public void Bool_condition_converts_to_property_path()
    {
        var result = FusionTemplateExpression.ToCondition<TemplateTestModel>(m => m.IsUnassigned);
        Assert.That(result, Is.EqualTo("isUnassigned"));
    }

    [Test]
    public void Not_condition_converts_to_negation()
    {
        var result = FusionTemplateExpression.ToCondition<TemplateTestModel>(m => !m.IsUnassigned);
        Assert.That(result, Is.EqualTo("!isUnassigned"));
    }

    [Test]
    public void Inequality_condition_converts_to_not_equals()
    {
        var result = FusionTemplateExpression.ToCondition<TemplateTestModel>(m => m.StaffRole != "RN");
        Assert.That(result, Is.EqualTo("staffRole !== 'RN'"));
    }

    [Test]
    public void Numeric_comparison_converts_correctly()
    {
        var result = FusionTemplateExpression.ToCondition<TemplateTestModel>(m => m.Id > 0);
        Assert.That(result, Is.EqualTo("id > 0"));
    }
}
