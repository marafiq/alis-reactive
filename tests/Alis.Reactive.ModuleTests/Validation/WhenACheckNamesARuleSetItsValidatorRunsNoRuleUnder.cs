using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Validation;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenACheckNamesARuleSetItsValidatorRunsNoRuleUnder
{
    private const string Refused = "WardRotaRules runs no rule under the rule set 'Approval': declare the rules it checks there inside RuleSet(\"Approval\", ..), a rule that holds a child validator among them.";
    private const string RefusedAtNight = "WardRotaRules runs no rule under the rule set 'Night': declare the rules it checks there inside RuleSet(\"Night\", ..), a rule that holds a child validator among them.";

    [Test]
    public void the_endpoints_check_under_it_is_refused()
    {
        var rota = new WardRota { Shifts = [new WardShift { Carer = "Ann", Hours = 10 }] };
        using var controller = ControllerFor(rota);
        Assert.That(() => controller.Validate<WardRota, WardRotaRules>(rota, "Approval"), Throws.InvalidOperationException.With.Message.EqualTo(Refused));
        Assert.That(() => controller.Validate<WardRota, WardRotaRules>(rota, "Night"), Throws.InvalidOperationException.With.Message.EqualTo(RefusedAtNight));
    }

    [Test]
    public void the_endpoints_asynchronous_check_under_it_is_refused()
    {
        var rota = new WardRota { Shifts = [new WardShift { Carer = "Ann", Hours = 10 }] };
        using var controller = ControllerFor(rota);
        Assert.That(async () => await controller.ValidateAsync<WardRota, WardRotaRules>(rota, "Approval"), Throws.InvalidOperationException.With.Message.EqualTo(Refused));
    }

    [Test]
    public void a_requests_check_under_it_is_refused_where_the_view_renders()
        => Assert.That(() => PlanOf<WardRota>(pipeline => pipeline.Post<WardRota>("/rotas", request => request.Inputs(pipeline.Container<WardRota>("rota"))).Validate<WardRotaRules>("Approval")),
            Throws.InvalidOperationException.With.Message.EqualTo(Refused));

    [Test]
    public void a_containers_check_under_it_is_refused_where_the_view_renders()
        => Assert.That(() => PlanOf<WardRota>(pipeline => pipeline.Validate(pipeline.Container<WardRota>("rota"), "Approval")),
            Throws.InvalidOperationException.With.Message.EqualTo(Refused));

    [Test]
    public async Task a_rule_for_each_row_declared_in_the_set_runs_the_rows_rule_of_the_set_and_none_of_no_set()
    {
        var rota = new ApprovedWardRota { Shifts = [new WardShift { Hours = 10 }, new WardShift { Carer = "Ann", Hours = 8 }] };
        using var controller = ControllerFor(rota);
        var checkedNow = controller.Validate<ApprovedWardRota, ApprovedWardRotaRules>(rota, "Approval");
        var checkedLater = await controller.ValidateAsync<ApprovedWardRota, ApprovedWardRotaRules>(rota, "Approval");
        Assert.That(checkedNow.Errors.Select(error => (error.PropertyName, error.ErrorMessage)), Is.EqualTo(new[] { ("Shifts[0].Hours", "An approved shift is 8 hours at most.") }));
        Assert.That(checkedLater.Errors.Select(error => (error.PropertyName, error.ErrorMessage)), Is.EqualTo(new[] { ("Shifts[0].Hours", "An approved shift is 8 hours at most.") }));
        Assert.That(controller.Validate<ApprovedWardRota, ApprovedWardRotaRules>(rota).Errors.Select(error => (error.PropertyName, error.ErrorMessage)),
            Is.EqualTo(new[] { ("Shifts[0].Carer", "Choose a carer.") }));
    }

    [Test]
    public void a_containers_check_under_a_set_its_rule_for_each_row_is_in_is_registered_under_that_set()
    {
        using var plan = JsonDocument.Parse(PlanOf<ApprovedWardRota>(pipeline => pipeline.Validate(pipeline.Container<ApprovedWardRota>("rota"), "Approval")));
        Assert.That(plan.RootElement.GetProperty("objects").EnumerateObject().Select(member => member.Name).Where(name => name.StartsWith("$form:", StringComparison.Ordinal)),
            Is.EqualTo(new[] { "$form:Alis.Reactive.ModuleTests.Validation.WhenACheckNamesARuleSetItsValidatorRunsNoRuleUnder+ApprovedWardRotaRules:Approval" }));
        Assert.That(plan.RootElement.GetProperty("objects").GetProperty("$form:Alis.Reactive.ModuleTests.Validation.WhenACheckNamesARuleSetItsValidatorRunsNoRuleUnder+ApprovedWardRotaRules:Approval")
            .GetProperty("data").GetProperty("registration").GetProperty("ruleSet").GetString(), Is.EqualTo("Approval"));
        Assert.That(PlanSchema.ProblemsOf(plan.RootElement.GetRawText()), Is.Empty);
    }

    [Test]
    public void a_check_under_the_default_set_is_never_refused()
    {
        var rota = new NightRota { Carer = "" };
        using var controller = ControllerFor(rota);
        Assert.That(controller.Validate<NightRota, NightRotaRules>(rota).IsValid, Is.True);
        Assert.That(controller.Validate<NightRota, NightRotaRules>(rota, "Default").IsValid, Is.True);
        Assert.That(controller.Validate<NightRota, NightRotaRules>(rota, "Night").Errors.Select(error => error.ErrorMessage), Is.EqualTo(new[] { "Name the night carer." }));
        Assert.That(PlanOf<NightRota>(pipeline => pipeline.Validate(pipeline.Container<NightRota>("rota"))), Does.Contain("NightRotaRules:default"));
    }

    private static Controller ControllerFor<T>(T model)
    {
        var view = Views.Of(model, _ => { }, Registered);
        return new RotaController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = view.ViewContext.HttpContext.RequestServices },
                ActionDescriptor = new ControllerActionDescriptor(),
            },
        };
    }

    private static string PlanOf<TModel>(Action<Pipeline<TModel>> write) where TModel : new()
    {
        var html = Views.Of(new TModel(), _ => { }, Registered);
        var plan = html.ReactivePlan<TModel>();
        html.On(plan, trigger => trigger.DomReady(write));
        return Views.TextOf(html.RenderPlan(plan))["<script type=\"application/json\" data-reactive-plan>".Length..^"</script>".Length];
    }

    private static void Registered(IServiceCollection services)
    {
        services.AddReactiveRequests(requests => requests.NoAntiforgery());
        services.AddReactiveFluentValidation(validation => validation.Add<WardRotaRules>().Add<ApprovedWardRotaRules>().Add<NightRotaRules>());
    }

    private sealed class RotaController : Controller;

    public sealed class WardShift
    {
        public string? Carer { get; set; }
        public int Hours { get; set; }
    }

    public sealed class WardRota
    {
        public List<WardShift> Shifts { get; set; } = [];
    }

    public sealed class ApprovedWardRota
    {
        public List<WardShift> Shifts { get; set; } = [];
    }

    public sealed class NightRota
    {
        public string? Carer { get; set; }
    }

    public sealed class WardShiftRules : ReactiveValidator<WardShift>
    {
        public WardShiftRules()
        {
            RuleFor(shift => shift.Carer).NotEmpty().WithMessage("Choose a carer.");
            RuleSet("Approval", () => RuleFor(shift => shift.Hours).LessThanOrEqualTo(8).WithMessage("An approved shift is 8 hours at most."));
        }
    }

    public sealed class WardRotaRules : ReactiveValidator<WardRota>
    {
        public WardRotaRules() => RuleForEach(rota => rota.Shifts).SetValidator(new WardShiftRules());
    }

    public sealed class ApprovedWardRotaRules : ReactiveValidator<ApprovedWardRota>
    {
        public ApprovedWardRotaRules() => RuleSet("default, Approval", () => RuleForEach(rota => rota.Shifts).SetValidator(new WardShiftRules()));
    }

    public sealed class NightRotaRules : ReactiveValidator<NightRota>
    {
        public NightRotaRules() => RuleSet("Night", () => RuleFor(rota => rota.Carer).NotEmpty().WithMessage("Name the night carer."));
    }
}
