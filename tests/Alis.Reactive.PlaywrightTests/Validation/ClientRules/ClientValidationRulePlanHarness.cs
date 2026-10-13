using System.Text.Json;
using Alis.Reactive.FluentValidator;
using Alis.Reactive.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.PlaywrightTests.Validation.ClientRules;

internal static class ClientValidationRulePlanHarness
{
    private const string FormId = "validation-form";
    private static readonly IServiceProvider Services = BuildServices();

    internal static JsonDocument RenderPlan<TModel, TValidationSource>()
        where TModel : class
        where TValidationSource : class
    {
        return RenderPlan<TModel, TValidationSource>(Services);
    }

    internal static JsonDocument RenderPlan<TModel, TValidationSource>(IServiceProvider services)
        where TModel : class
        where TValidationSource : class
    {
        var html = default(IHtmlHelper<TModel>)!;
        var plan = PlanExtensions.ReactivePlan(html);
        HtmlExtensions.On(html, plan, trigger =>
            trigger.DomReady(pipeline =>
                pipeline.Post("/validate")
                    .Validate<TValidationSource>(FormId)));

        return JsonDocument.Parse(plan.RenderFormatted(services));
    }

    /// <summary>The tests the form runs on the field the server reports errors under this name.</summary>
    internal static JsonElement[] RulesFor(JsonElement root, string serverFieldName) =>
        Fields(root)
            .Single(field => field.GetProperty("name").GetString() == serverFieldName)
            .GetProperty("tests")
            .EnumerateArray()
            .ToArray();

    internal static JsonElement[] Fields(JsonElement root) =>
        root.GetProperty("objects")
            .GetProperty("$form:" + FormId)
            .GetProperty("data")
            .GetProperty("fields")
            .EnumerateArray()
            .ToArray();

    internal static string[] RuleNames(JsonElement[] rules) =>
        rules.Select(rule => rule.GetProperty("rule").GetString()!).ToArray();

    /// <summary>The other field each test's check reads: its peer.</summary>
    internal static string[] PeerComponents(JsonElement[] rules) =>
        rules.Select(rule => ComponentsReadBy(rule.GetProperty("check")).First()).ToArray();

    /// <summary>The field each term of a composed condition reads, in order.</summary>
    internal static string[] ConditionLeftComponents(JsonElement condition) =>
        condition.GetProperty("args")
            .EnumerateArray()
            .Select(term => ComponentsReadBy(term).First())
            .ToArray();

    /// <summary>
    /// The components an expression reads: a located element, or a field another plan renders, which
    /// it reads through the model by its id.
    /// </summary>
    internal static List<string> ComponentsReadBy(JsonElement expression)
    {
        var components = new List<string>();
        Collect(expression, components);
        return components;
    }

    private static void Collect(JsonElement element, List<string> components)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var kind = element.TryGetProperty("kind", out var kindProperty) ? kindProperty.GetString() : null;
                if (kind == "obj")
                {
                    var id = element.GetProperty("id").GetString()!;
                    if (!id.StartsWith("$", StringComparison.Ordinal)) components.Add(id);
                }

                var readsModel = kind == "call"
                    && element.GetProperty("target").TryGetProperty("id", out var target) && target.GetString() == "$model";
                if (readsModel)
                {
                    components.Add(element.GetProperty("args")[0].GetProperty("value").GetString()!);
                    return;
                }

                foreach (var property in element.EnumerateObject())
                    Collect(property.Value, components);
                return;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    Collect(item, components);
                return;
        }
    }

    private static IServiceProvider BuildServices() =>
        BuildServices(new ServiceCollection()).BuildServiceProvider();

    private static ServiceCollection BuildServices(ServiceCollection services)
    {
        services.AddReactiveFluentValidation(rules =>
        {
            rules.Add<AsyncOnlyValidator>();
            rules.Add<BuiltInPeerComparisonValidator>();
            rules.Add<CrossObjectPeerValidator>();
            rules.Add<BuiltInClientRulesValidator>();
            rules.Add<ConfirmEmailValidator>();
            rules.Add<ReactiveAssessmentValidator>();
            rules.Add<ComposedAssessmentValidator>();
            rules.Add<MixedConditionValidator>();
            rules.Add<RuleConditionValidator>();
            rules.Add<ServerOnlyRuleValidator>();
            rules.Add<TaggedAssessmentValidator>();
        });

        return services;
    }
}

internal sealed class StayWindow
{
    public DateTime AdmissionDate { get; set; }
    public DateTime DischargeDate { get; set; }
    public DateTime ReviewDate { get; set; }
}

internal sealed class AsyncOnlyValidator : ReactiveValidator<AsyncOnlyModel>
{
    public AsyncOnlyValidator()
    {
        RuleFor(model => model.Code)
            .MustAsync((_, _, _) => Task.FromResult(false))
            .WithMessage("Code is checked on the server.");
    }
}

internal sealed class AsyncOnlyModel
{
    public string? Code { get; set; }
}

internal sealed class BuiltInClientRulesValidator : ReactiveValidator<BuiltInClientRulesModel>
{
    public BuiltInClientRulesValidator()
    {
        RuleFor(model => model.Name)
            .NotEmpty().WithMessage("'Name' is required.")
            .MinimumLength(2).WithMessage("'Name' must be at least 2 characters.")
            .MaximumLength(10).WithMessage("'Name' must be at most 10 characters.")
            .Matches("^[A-Z]+$").WithMessage("'Name' format is invalid.");

        RuleFor(model => model.Email).EmailAddress().WithMessage("'Email' must be a valid email address.");

        RuleFor(model => model.Card).CreditCard().WithMessage("'Card' must be a valid credit card number.");

        RuleFor(model => model.EmptyCode).Empty().WithMessage("'Empty Code' must be empty.");

        RuleFor(model => model.Score)
            .InclusiveBetween(1, 5).WithMessage("'Score' must be between 1 and 5.")
            .ExclusiveBetween(0, 10).WithMessage("'Score' must be between 0 and 10 (exclusive).")
            .GreaterThanOrEqualTo(2).WithMessage("'Score' must be at least 2.")
            .LessThanOrEqualTo(8).WithMessage("'Score' must be at most 8.")
            .GreaterThan(1).WithMessage("'Score' must be greater than 1.")
            .LessThan(9).WithMessage("'Score' must be less than 9.")
            .Equal(5).WithMessage("'Score' must equal 5.")
            .NotEqual(3).WithMessage("'Score' must not equal 3.");
    }
}

internal sealed class BuiltInClientRulesModel
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Card { get; set; }
    public string? EmptyCode { get; set; }
    public int Score { get; set; }
}

internal sealed class BuiltInPeerComparisonValidator : ReactiveValidator<StayWindow>
{
    public BuiltInPeerComparisonValidator()
    {
        RuleFor(model => model.DischargeDate)
            .GreaterThan(model => model.AdmissionDate).WithMessage("Discharge must be after admission.")
            .GreaterThanOrEqualTo(model => model.AdmissionDate).WithMessage("Discharge must not be before admission.")
            .LessThan(model => model.ReviewDate).WithMessage("Discharge must be before review.")
            .LessThanOrEqualTo(model => model.ReviewDate).WithMessage("Discharge must not be after review.")
            .Equal(model => model.AdmissionDate).WithMessage("Discharge must match admission.")
            .NotEqual(model => model.ReviewDate).WithMessage("Discharge must differ from review.");
    }
}

internal sealed class StayPlanModel
{
    public StayWindow Stay { get; set; } = new StayWindow();
    public CarePlanReview Review { get; set; } = new CarePlanReview();
}

internal sealed class CarePlanReview
{
    public DateTime ReviewDate { get; set; }
}

internal sealed class CrossObjectPeerValidator : ReactiveValidator<StayPlanModel>
{
    public CrossObjectPeerValidator()
    {
        RuleFor(model => model.Review.ReviewDate)
            .GreaterThan(model => model.Stay.DischargeDate).WithMessage("Review the care plan after discharge.");
    }
}

internal sealed class ConfirmEmailValidator : ReactiveValidator<ConfirmEmailModel>
{
    public ConfirmEmailValidator()
    {
        RuleFor(model => model.ConfirmEmail).Equal(model => model.Email).WithMessage("Emails must match.");
    }
}

internal sealed class ConfirmEmailModel
{
    public string? Email { get; set; }
    public string? ConfirmEmail { get; set; }
}

internal sealed class ReactiveAssessmentValidator : ReactiveValidator<AssessmentModel>
{
    public ReactiveAssessmentValidator()
    {
        When(model => model.IsVeteran, () =>
        {
            RuleFor(model => model.Score).NotEmpty().WithMessage("Score required for veterans.");
        });
    }
}

internal sealed class ComposedAssessmentValidator : ReactiveValidator<AssessmentModel>
{
    public ComposedAssessmentValidator()
    {
        When(model => model.IsVeteran && model.Score > 7, () =>
            {
                RuleFor(model => model.Notes).NotEmpty().WithMessage("Notes required for high scoring veterans.");
            });
    }
}

internal sealed class MixedConditionValidator : ReactiveValidator<AssessmentModel>
{
    public MixedConditionValidator()
    {
        When(model => model.IsVeteran, () =>
        {
            When(model => IsServerFlagged(model), () =>
            {
                RuleFor(model => model.Score).NotEmpty()
                    .WithMessage("Score required by server condition.");
            });
        });
    }

    // A method is C# only the server runs; the browser has no form of a condition that calls one.
    private static bool IsServerFlagged(AssessmentModel model) => model.ServerFlag;
}

internal sealed class RuleConditionValidator : ReactiveValidator<AssessmentModel>
{
    public RuleConditionValidator()
    {
        RuleFor(model => model.Notes)
            .NotEmpty().WithMessage("Notes are required for veterans.")
            .When(model => model.IsVeteran)
            .MaximumLength(200).WithMessage("Keep notes under 200 characters.");
    }
}

internal sealed class ServerOnlyRuleValidator : ReactiveValidator<AssessmentModel>
{
    public ServerOnlyRuleValidator()
    {
        RuleFor(model => model.Notes).NotEmpty().WithMessage("Notes are recorded by the server.").ServerOnly();
        RuleFor(model => model.Score).NotEmpty().WithMessage("Score is required.");
    }
}

internal sealed class AssessmentModel
{
    public bool IsVeteran { get; set; }
    public bool ServerFlag { get; set; }
    public int? Score { get; set; }
    public string? Notes { get; set; }
}

internal sealed class TaggedAssessmentValidator : ReactiveValidator<TaggedAssessmentModel>
{
    public TaggedAssessmentValidator()
    {
        When(model => model.Tags.Contains("fall-risk"), () =>
        {
            RuleFor(model => model.ReviewNote).NotEmpty().WithMessage("Review note is required for fall-risk residents.");
        });
    }
}

internal sealed class TaggedAssessmentModel
{
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string? ReviewNote { get; set; }
}
