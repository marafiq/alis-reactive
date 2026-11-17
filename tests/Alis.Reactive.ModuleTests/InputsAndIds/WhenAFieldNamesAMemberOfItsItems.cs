using System.Text.Json.Serialization;
using Alis.Reactive.RenderingAndDelivery;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.InputsAndIds;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAFieldNamesAMemberOfItsItems
{
    [Test]
    public void a_member_that_carries_its_json_name_is_named_by_it()
    {
        var html = Views.Of(new AllergyForm());
        var plan = html.ReactivePlan<AllergyForm>();
        Assert.That(html.InputField(plan, m => m.AllergyCodes).JsonName((AllergyItem a) => a.DisplayLabel), Is.EqualTo("label"));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_member_is_named_in_camel_case_under_the_default_options()
    {
        var html = Views.Of(new AllergyForm());
        var plan = html.ReactivePlan<AllergyForm>();
        Assert.That(html.InputField(plan, m => m.AllergyCodes).JsonName((AllergyItem a) => a.AllergyCode), Is.EqualTo("allergyCode"));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_path_past_one_member_is_refused()
    {
        var html = Views.Of(new AllergyForm());
        var plan = html.ReactivePlan<AllergyForm>();
        var field = html.InputField(plan, m => m.AllergyCodes);
        Assert.That(() => field.JsonName((AllergyItem a) => a.Source.Ward), Throws.InvalidOperationException.With.Message.EqualTo("'a => a.Source.Ward' is not one member of the item, so the component would name a field its data does not hold; name one member, such as item => item.Name."));
        Assert.That(() => field.JsonName((AllergyItem a) => a.Source), Throws.Nothing);
        html.RenderPlan(plan);
    }
}

public sealed class AllergyForm
{
    public int[]? AllergyCodes { get; set; }
}

public sealed record AllergyItem([property: JsonPropertyName("label")] string DisplayLabel, int AllergyCode, AllergySource Source);

public sealed record AllergySource(string Ward);
