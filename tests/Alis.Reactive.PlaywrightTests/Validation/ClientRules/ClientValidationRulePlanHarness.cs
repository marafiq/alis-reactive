using System.Text.Json;
using Alis.Reactive.FluentValidator;
using Alis.Reactive.Native.Extensions;
using Alis.Reactive.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.PlaywrightTests.Validation.ClientRules;

internal static class ClientValidationRulePlanHarness
{
    private const string FormId = "validation-form";
    private static readonly object ConfigGate = new();
    private static bool _configured;

    internal static JsonDocument RenderPlan<TModel, TValidationSource>()
        where TModel : class
        where TValidationSource : class
    {
        EnsureRuleSource();

        var html = default(IHtmlHelper<TModel>)!;
        var plan = PlanExtensions.ReactivePlan(html);
        HtmlExtensions.On(html, plan, trigger =>
            trigger.DomReady(pipeline =>
                pipeline.Post("/validate")
                    .Validate<TValidationSource>(FormId)));

        return JsonDocument.Parse(plan.RenderFormatted());
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

    private static void EnsureRuleSource()
    {
        if (_configured) return;

        lock (ConfigGate)
        {
            if (_configured) return;

            ReactivePlanConfig.UseClientValidationRuleSource(
                new ClientValidationRuleSource());
            _configured = true;
        }
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

        private static readonly FluentValidationAdapter Fluent =
            new(CreateValidator);

        public IReadOnlyList<ClientValidationField> GetClientRules(Type validationSourceType)
        {
            if (FluentValidatorTypes.Contains(validationSourceType))
                return Fluent.GetClientRules(validationSourceType);

            return Core.GetClientRules(validationSourceType);
        }

        private static readonly Type[] FluentValidatorTypes =
        [
            typeof(AsyncOnlyValidator),
            typeof(ConfirmEmailValidator),
            typeof(ReactiveAssessmentValidator),
            typeof(ComposedAssessmentValidator),
            typeof(MixedConditionValidator)
        ];

        private static IValidator? CreateValidator(Type type)
        {
            if (type == typeof(AsyncOnlyValidator)) return new AsyncOnlyValidator();
            if (type == typeof(ConfirmEmailValidator)) return new ConfirmEmailValidator();
            if (type == typeof(ReactiveAssessmentValidator)) return new ReactiveAssessmentValidator();
            if (type == typeof(ComposedAssessmentValidator)) return new ComposedAssessmentValidator();
            if (type == typeof(MixedConditionValidator)) return new MixedConditionValidator();
            return null;
        }
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

internal sealed class AsyncOnlyValidator : AbstractValidator<AsyncOnlyModel>
{
    public AsyncOnlyValidator()
    {
        RuleFor(model => model.Code)
            .MustAsync((_, _, _) => Task.FromResult(false))
            .WithMessage("Code is checked on the server.")
            .ClientRule(rule => rule.Required());
    }
}

internal sealed class AsyncOnlyModel
{
    public string? Code { get; set; }
}

internal sealed class ConfirmEmailValidator : AbstractValidator<ConfirmEmailModel>
{
    public ConfirmEmailValidator()
    {
        RuleFor(model => model.ConfirmEmail)
            .Must((model, confirmEmail) => confirmEmail == model.Email)
            .WithMessage("Emails must match.")
            .ClientRule(rule => rule.EqualTo(model => model.Email));
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
