using System.Text.Json;
using FluentValidation;

namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenBindingProjectedFieldShapeEvidence
{
    [SetUp]
    public void SetUp()
    {
        ReactivePlanConfig.Reset();
        ReactivePlanConfig.UseClientValidationProjectionSource(AdapterFactory.Create());
    }

    [TearDown]
    public void TearDown()
    {
        ReactivePlanConfig.Reset();
    }

    [Test]
    public void Fluent_validation_projection_binds_deferred_fields_from_rule_member_shape_not_model_field_lookup()
    {
        var plan = new ReactivePlan<ProjectedShapeModel>();
        var trigger = new Builders.TriggerBuilder<ProjectedShapeModel>(plan, plan.Context);
        trigger.DomReady(p =>
            p.Post("/save", gather => gather.IncludeAll())
                .Validate<ProjectedShapeValidator>("resident-form"));

        using var document = JsonDocument.Parse(plan.Render());

        var validation = document.RootElement
            .GetProperty("components")
            .GetProperty("resident-form")
            .GetProperty("container")
            .GetProperty("validationRules")[0];

        Assert.That(validation.GetProperty("serverFieldName").GetString(), Is.EqualTo("MoveInDate"));
        Assert.That(validation.GetProperty("component").GetString(), Does.EndWith("__MoveInDate"));
        Assert.That(
            validation.GetProperty("value").GetProperty("shape").GetProperty("kind").GetString(),
            Is.EqualTo("date"));
    }

    private sealed class ProjectedShapeModel
    {
        public DateTime AdmissionDate { get; set; }
    }

    private sealed class ProjectedShapeValidator : AbstractValidator<ProjectedShapeModel>
    {
        public ProjectedShapeValidator()
        {
            RuleFor(model => model.AdmissionDate)
                .NotEmpty()
                .OverridePropertyName("MoveInDate");
        }
    }
}
