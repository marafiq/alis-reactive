using System.Text.Json;
using Alis.Reactive.FluentValidator;
using Alis.Reactive.FluentValidator.Validators;
using Alis.Reactive.Native.Extensions;
using Alis.Reactive.Validation;
using FluentValidation;
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
        var html = default(IHtmlHelper<TModel>)!;
        var plan = PlanExtensions.ReactivePlan(html);
        HtmlExtensions.On(html, plan, trigger =>
            trigger.DomReady(pipeline =>
                pipeline.Post("/validate")
                    .Validate<TValidationSource>(FormId)));

        return JsonDocument.Parse(plan.RenderFormatted(Services));
    }

    internal static JsonElement[] RulesFor(JsonElement root, string serverFieldName) =>
        ValidationFor(root, serverFieldName)
            .GetProperty("rules")
            .EnumerateArray()
            .ToArray();

    internal static string[] RuleNames(JsonElement[] rules) =>
        rules.Select(rule => rule.GetProperty("name").GetString()!).ToArray();

    internal static string[] PeerComponents(JsonElement[] rules) =>
        rules.Select(rule => rule
                .GetProperty("execution")
                .GetProperty("value")
                .GetProperty("from")
                .GetProperty("component")
                .GetString()!)
            .ToArray();

    internal static string[] ConditionLeftComponents(JsonElement condition)
    {
        var terms = condition.GetProperty("terms").EnumerateArray();
        return terms
            .Select(term => term
                .GetProperty("left")
                .GetProperty("from")
                .GetProperty("component")
                .GetString()!)
            .ToArray();
    }

    private static JsonElement ValidationFor(JsonElement root, string serverFieldName)
    {
        var rules = root
            .GetProperty("components")
            .GetProperty(FormId)
            .GetProperty("container")
            .GetProperty("validationRules")
            .EnumerateArray()
            .ToArray();

        return rules.Single(rule =>
            rule.GetProperty("serverFieldName").GetString() == serverFieldName);
    }

    private static IServiceProvider BuildServices() =>
        new ServiceCollection()
            .AddSingleton<IClientValidationRuleSource>(_ => new ClientValidationRuleSource(BuildFluentSource()))
            .BuildServiceProvider();

    private static IClientValidationRuleSource BuildFluentSource()
    {
        var services = new ServiceCollection();
        services.AddReactiveFluentValidation(rules =>
        {
            rules.Add<AsyncOnlyValidator>();
            rules.Add<BuiltInPeerComparisonValidator>();
            rules.Add<BuiltInClientRulesValidator>();
            rules.Add<ConfirmEmailValidator>();
            rules.Add<ReactiveAssessmentValidator>();
            rules.Add<ComposedAssessmentValidator>();
            rules.Add<MixedConditionValidator>();
        });

        return services
            .BuildServiceProvider()
            .GetRequiredService<IClientValidationRuleSource>();
    }

    private sealed class ClientValidationRuleSource : IClientValidationRuleSource
    {
        private static readonly IClientValidationRuleSource Core =
            ClientValidationRules.Create(
                ClientValidationRules.For<StayWindowClientValidation, StayWindow>(rules =>
                {
                    rules.Field(x => x.DischargeDate)
                        .GreaterThan(x => x.AdmissionDate, "Discharge must be after admission.")
                        .GreaterThanOrEqualTo(x => x.AdmissionDate, "Discharge must not be before admission.")
                        .LessThan(x => x.ReviewDate, "Discharge must be before review.")
                        .LessThanOrEqualTo(x => x.ReviewDate, "Discharge must not be after review.");
                }),

                ClientValidationRules.For<ResidentAssessmentClientValidation, ResidentAssessment>(rules =>
                {
                    rules.When(
                        fields => fields.Field(x => x.IsVeteran).Truthy()
                            .And(fields.Field(x => x.CareLevel).In("Memory Care", "Skilled Nursing")),
                        rules => rules.Field(x => x.MemoryAssessmentScore)
                            .Required("Memory assessment score is required."));
                }));

        private readonly IClientValidationRuleSource _fluent;

        internal ClientValidationRuleSource(IClientValidationRuleSource fluent)
        {
            _fluent = fluent;
        }

        public IReadOnlyList<ClientValidationField> GetClientRules(Type validationSourceType)
        {
            if (FluentValidatorTypes.Contains(validationSourceType))
                return _fluent.GetClientRules(validationSourceType);

            return Core.GetClientRules(validationSourceType);
        }

        private static readonly Type[] FluentValidatorTypes =
        [
            typeof(AsyncOnlyValidator),
            typeof(BuiltInPeerComparisonValidator),
            typeof(BuiltInClientRulesValidator),
            typeof(ConfirmEmailValidator),
            typeof(ReactiveAssessmentValidator),
            typeof(ComposedAssessmentValidator),
            typeof(MixedConditionValidator)
        ];
    }
}

internal sealed class StayWindowClientValidation { }

internal sealed class ResidentAssessmentClientValidation { }

internal sealed class StayWindow
{
    public DateTime AdmissionDate { get; set; }
    public DateTime DischargeDate { get; set; }
    public DateTime ReviewDate { get; set; }
}

internal sealed class ResidentAssessment
{
    public bool IsVeteran { get; set; }
    public string? CareLevel { get; set; }
    public int? MemoryAssessmentScore { get; set; }
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
            .NotEmpty()
            .Length(2, 10)
            .Matches("^[A-Z]+$");
        ClientRule(model => model.Name)
            .Required("'Name' is required.")
            .MinLength(2, "'Name' must be at least 2 characters.")
            .MaxLength(10, "'Name' must be at most 10 characters.")
            .Regex("^[A-Z]+$", "'Name' format is invalid.");

        RuleFor(model => model.Email)
            .EmailAddress();
        ClientRule(model => model.Email)
            .Email("'Email' must be a valid email address.");

        RuleFor(model => model.Card)
            .CreditCard();
        ClientRule(model => model.Card)
            .CreditCard("'Card' must be a valid credit card number.");

        RuleFor(model => model.EmptyCode)
            .IsEmpty();
        ClientRule(model => model.EmptyCode)
            .Empty("'Empty Code' must be empty.");

        RuleFor(model => model.Score)
            .InclusiveBetween(1, 5)
            .IsExclusiveBetween(0, 10)
            .GreaterThanOrEqualTo(2)
            .LessThanOrEqualTo(8)
            .GreaterThan(1)
            .LessThan(9)
            .Equal(5)
            .NotEqual(3);
        ClientRule(model => model.Score)
            .Range(1, 5, "'Score' must be between 1 and 5.")
            .ExclusiveRange(0, 10, "'Score' must be between 0 and 10 (exclusive).")
            .GreaterThanOrEqualTo(2, "'Score' must be at least 2.")
            .LessThanOrEqualTo(8, "'Score' must be at most 8.")
            .GreaterThan(1, "'Score' must be greater than 1.")
            .LessThan(9, "'Score' must be less than 9.")
            .EqualTo(5, "'Score' must equal 5.")
            .NotEqual(3, "'Score' must not equal 3.");
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
            .GreaterThan(model => model.AdmissionDate)
            .GreaterThanOrEqualTo(model => model.AdmissionDate)
            .LessThan(model => model.ReviewDate)
            .LessThanOrEqualTo(model => model.ReviewDate)
            .Equal(model => model.AdmissionDate)
            .NotEqual(model => model.ReviewDate);
        ClientRule(model => model.DischargeDate)
            .GreaterThan(model => model.AdmissionDate, "Discharge must be after admission.")
            .GreaterThanOrEqualTo(model => model.AdmissionDate, "Discharge must not be before admission.")
            .LessThan(model => model.ReviewDate, "Discharge must be before review.")
            .LessThanOrEqualTo(model => model.ReviewDate, "Discharge must not be after review.")
            .EqualTo(model => model.AdmissionDate, "Discharge must match admission.")
            .NotEqualTo(model => model.ReviewDate, "Discharge must differ from review.");
    }
}

internal sealed class ConfirmEmailValidator : ReactiveValidator<ConfirmEmailModel>
{
    public ConfirmEmailValidator()
    {
        RuleFor(model => model.ConfirmEmail)
            .Must((model, confirmEmail) => confirmEmail == model.Email)
            .WithMessage("Emails must match.");
        ClientRule(model => model.ConfirmEmail)
            .EqualTo(model => model.Email, "Emails must match.");
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
        WhenField(model => model.IsVeteran, () =>
        {
            RuleFor(model => model.Score).NotEmpty()
                .WithMessage("Score required for veterans.");
            ClientRule(model => model.Score)
                .Required("Score required for veterans.");
        });
    }
}

internal sealed class ComposedAssessmentValidator : ReactiveValidator<AssessmentModel>
{
    public ComposedAssessmentValidator()
    {
        WhenFields(fields => fields
            .Field(model => model.IsVeteran).Truthy()
            .And(fields.Field(model => model.Score).Gt(7)), () =>
            {
                RuleFor(model => model.Notes).NotEmpty()
                    .WithMessage("Notes required for high scoring veterans.");
                ClientRule(model => model.Notes)
                    .Required("Notes required for high scoring veterans.");
            });
    }
}

internal sealed class MixedConditionValidator : ReactiveValidator<AssessmentModel>
{
    public MixedConditionValidator()
    {
        WhenField(model => model.IsVeteran, () =>
        {
            When(model => model.ServerFlag, () =>
            {
                RuleFor(model => model.Score).NotEmpty()
                    .WithMessage("Score required by server condition.");
            });
        });
    }
}

internal sealed class AssessmentModel
{
    public bool IsVeteran { get; set; }
    public bool ServerFlag { get; set; }
    public int? Score { get; set; }
    public string? Notes { get; set; }
}
