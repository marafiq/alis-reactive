using Alis.Reactive.NativeTagHelpers.Tests.Infrastructure;
using Alis.Reactive.NativeTagHelpers.ValidationSummary;
using NUnit.Framework;

namespace Alis.Reactive.NativeTagHelpers.Tests;

[TestFixture]
public class WhenRenderingValidationSummary : TagHelperTestBase
{
    [Test]
    public void Renders_div_with_validation_summary_attribute()
    {
        var tagHelper = new NativeValidationSummaryTagHelper { PlanId = "my-plan" };
        var context = CreateContext("native-validation-summary");
        var output = CreateOutput("native-validation-summary");

        tagHelper.Process(context, output);

        Assert.That(output.TagName, Is.EqualTo("div"));
        Assert.That(output.Attributes["data-alis-validation-summary"]?.Value?.ToString(), Is.EqualTo("my-plan"));
    }

    [Test]
    public void Starts_hidden()
    {
        var tagHelper = new NativeValidationSummaryTagHelper { PlanId = "my-plan" };
        var context = CreateContext("native-validation-summary");
        var output = CreateOutput("native-validation-summary");

        tagHelper.Process(context, output);

        Assert.That(output.Attributes.ContainsName("hidden"), Is.True);
    }

    [Test]
    public void Uses_empty_string_when_plan_id_is_null()
    {
        var tagHelper = new NativeValidationSummaryTagHelper();
        var context = CreateContext("native-validation-summary");
        var output = CreateOutput("native-validation-summary");

        tagHelper.Process(context, output);

        Assert.That(output.Attributes["data-alis-validation-summary"]?.Value?.ToString(), Is.EqualTo(""));
    }

    [Test]
    public void Applies_user_css_class()
    {
        var tagHelper = new NativeValidationSummaryTagHelper { PlanId = "plan-1", CssClass = "mt-4" };
        var context = CreateContext("native-validation-summary");
        var output = CreateOutput("native-validation-summary");

        tagHelper.Process(context, output);

        Assert.That(output.Attributes["class"]?.Value?.ToString(), Is.EqualTo("mt-4"));
    }

    [Test]
    public void Omits_class_attribute_when_no_css_class()
    {
        var tagHelper = new NativeValidationSummaryTagHelper { PlanId = "plan-1" };
        var context = CreateContext("native-validation-summary");
        var output = CreateOutput("native-validation-summary");

        tagHelper.Process(context, output);

        Assert.That(output.Attributes.ContainsName("class"), Is.False);
    }
}
