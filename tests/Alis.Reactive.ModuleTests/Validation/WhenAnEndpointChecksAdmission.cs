using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Validation;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAnEndpointChecksAdmission
{
    [Test]
    public async Task the_default_check_reads_the_model_MVC_bound()
    {
        var request = new Admission();
        using var controller = ControllerFor(request, services => services.AddReactiveFluentValidation(validation => validation.Add<AdmissionRules>()));
        Assert.That(await Bind(controller, request, ("ResidentName", "Ada"), ("Room", "2")), Is.True);
        var result = controller.Validate<Admission, AdmissionRules>(request);
        Assert.That(result.Errors.Select(error => (error.PropertyName, error.ErrorMessage)), Is.EqualTo(new[] { ("Room", "Choose room 10 or above.") }));
        Assert.That(request.ResidentName, Is.EqualTo("Ada"));
        Assert.That(request.Room, Is.EqualTo(2));
    }

    [Test]
    public async Task the_named_check_runs_only_its_rule_set()
    {
        var request = new Admission();
        using var controller = ControllerFor(request, services => services.AddReactiveFluentValidation(validation => validation.Add<AdmissionRules>()));
        Assert.That(await Bind(controller, request, ("ResidentName", "Ada"), ("Room", "2")), Is.True);
        var result = controller.Validate<Admission, AdmissionRules>(request, "Move");
        Assert.That(result.Errors.Select(error => (error.PropertyName, error.ErrorMessage)), Is.EqualTo(new[] { ("ResidentName", "A move needs a longer name.") }));
    }

    [Test]
    public async Task a_binding_refusal_keeps_its_key_and_skips_that_members_rules()
    {
        var request = new Admission();
        using var controller = ControllerFor(request, services => services.AddReactiveFluentValidation(validation => validation.Add<AdmissionRules>()));
        Assert.That(await Bind(controller, request, ("ResidentName", ""), ("Room", "twelve")), Is.False);
        var result = controller.Validate<Admission, AdmissionRules>(request);
        Assert.That(result.ErrorsByField["Room"], Is.EqualTo(new[] { "The value 'twelve' is not valid for Room." }));
        Assert.That(result.ErrorsByField["ResidentName"], Is.EqualTo(new[] { "Enter the resident's name." }));
        Assert.That(result.Errors.Count, Is.EqualTo(2));
    }

    [Test]
    public void a_binder_exception_uses_the_attempted_value_and_member_name()
    {
        var request = new Admission { ResidentName = "Ada" };
        using var controller = ControllerFor(request, services => services.AddReactiveFluentValidation(validation => validation.Add<AdmissionRules>()));
        controller.ModelState.SetModelValue("Room", new ValueProviderResult("twelve"));
        Assert.That(controller.ModelState.TryAddModelException("Room", new FormatException("Parser details stay inside MVC.")), Is.True);
        var result = controller.Validate<Admission, AdmissionRules>(request);
        Assert.That(result.Errors.Select(error => (error.PropertyName, error.ErrorMessage)), Is.EqualTo(new[] { ("Room", "The value 'twelve' is not valid for Room.") }));
    }

    [Test]
    public async Task another_binding_path_does_not_suppress_the_requests_member()
    {
        var request = new Admission();
        using var controller = ControllerFor(request, services => services.AddReactiveFluentValidation(validation => validation.Add<AdmissionRules>()));
        Assert.That(await Bind(controller, request, ("ResidentName", "Ada"), ("Room", "2")), Is.True);
        Assert.That(await controller.TryUpdateModelAsync(new Admission(), "other", Query(("other.ResidentName", "Grace"), ("other.Room", "twelve"))), Is.False);
        var result = controller.Validate<Admission, AdmissionRules>(request);
        Assert.That(result.Errors.Select(error => (error.PropertyName, error.ErrorMessage)), Is.EqualTo(new[] { ("other.Room", "The value 'twelve' is not valid for Room."), ("Room", "Choose room 10 or above.") }));
    }

    [Test]
    public async Task a_binding_refusal_skips_the_rules_guard_and_dependents()
    {
        var request = new Admission { ResidentName = "Ada" };
        using var controller = ControllerFor(request, services => services.AddReactiveFluentValidation(validation => validation.Add<GuardedRules>()));
        Assert.That(await Bind(controller, request, ("Room", "twelve")), Is.False);
        var result = controller.Validate<Admission, GuardedRules>(request);
        Assert.That(result.Errors.Select(error => (error.PropertyName, error.ErrorMessage)), Is.EqualTo(new[] { ("Room", "The value 'twelve' is not valid for Room.") }));
    }

    [Test]
    public async Task a_child_binding_refusal_skips_only_the_refused_child_member()
    {
        var request = new Household();
        using var controller = ControllerFor(request, services => services.AddReactiveFluentValidation(validation => validation.Add<HouseholdRules>()));
        Assert.That(await Bind(controller, request, ("Residents[0].Room", "twelve"), ("Residents[0].ResidentName", ""), ("Residents[1].Room", "2"), ("Residents[1].ResidentName", "Ada")), Is.False);
        var result = controller.Validate<Household, HouseholdRules>(request);
        Assert.That(result.ErrorsByField["Residents[0].Room"], Is.EqualTo(new[] { "The value 'twelve' is not valid for Room." }));
        Assert.That(result.ErrorsByField["Residents[0].ResidentName"], Is.EqualTo(new[] { "Enter the resident's name." }));
        Assert.That(result.ErrorsByField["Residents[1].Room"], Is.EqualTo(new[] { "Choose room 10 or above." }));
        Assert.That(result.Errors.Count, Is.EqualTo(3));
    }

    [Test]
    public async Task a_binding_refusal_skips_one_collection_item_and_checks_the_next()
    {
        var request = new RoomSelection();
        using var controller = ControllerFor(request, services => services.AddReactiveFluentValidation(validation => validation.Add<RoomSelectionRules>()));
        var values = new QueryStringValueProvider(BindingSource.Query, new QueryCollection(new Dictionary<string, StringValues>
        {
            ["Rooms.index"] = new StringValues(["0", "1"]),
            ["Rooms[0]"] = "twelve",
            ["Rooms[1]"] = "2",
        }), CultureInfo.InvariantCulture);
        Assert.That(await controller.TryUpdateModelAsync(request, "", values), Is.False);
        var result = controller.Validate<RoomSelection, RoomSelectionRules>(request);
        Assert.That(result.Errors.Select(error => (error.PropertyName, error.ErrorMessage)), Is.EqualTo(new[] { ("Rooms[0]", "The value 'twelve' is not valid."), ("Rooms[1]", "Choose room 10 or above.") }));
        Assert.That(request.Rooms, Is.EqualTo(new[] { 0, 2 }));
    }

    [Test]
    public async Task registered_models_skip_MVC_annotations_and_implicit_required()
    {
        var request = new AnnotatedAdmission();
        using var controller = ControllerFor(request, services => services.AddReactiveFluentValidation(validation => validation.Add<AnnotatedRules>()));
        Assert.That(await Bind(controller, request, ("ResidentName", ""), ("Reference", ""), ("Room", "2")), Is.True);
        Assert.That(controller.ModelState.IsValid, Is.True);
        var result = controller.Validate<AnnotatedAdmission, AnnotatedRules>(request);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public async Task registration_before_MVC_also_suppresses_its_own_validation()
    {
        var services = WebApplication.CreateEmptyBuilder(new WebApplicationOptions()).Services;
        services.AddReactiveFluentValidation(validation => validation.Add<AnnotatedRules>());
        services.AddControllersWithViews();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        using var controller = ControllerUsing(scope.ServiceProvider);
        var request = new AnnotatedAdmission();
        Assert.That(await Bind(controller, request, ("ResidentName", ""), ("Reference", ""), ("Room", "2")), Is.True);
        Assert.That(controller.Validate<AnnotatedAdmission, AnnotatedRules>(request).IsValid, Is.True);
    }

    [Test]
    public async Task an_unregistered_model_keeps_MVC_validation()
    {
        var request = new AnnotatedAdmission();
        using var controller = ControllerFor(request, services => services.AddReactiveFluentValidation(validation => validation.Add<AdmissionRules>()));
        Assert.That(await Bind(controller, request, ("ResidentName", ""), ("Reference", ""), ("Room", "2")), Is.False);
        Assert.That(controller.ModelState["ResidentName"]!.Errors.Select(error => error.ErrorMessage), Is.EqualTo(new[] { "The ResidentName field is required." }));
        Assert.That(controller.ModelState["Reference"]!.Errors.Select(error => error.ErrorMessage), Is.EqualTo(new[] { "MVC requires a reference." }));
    }

    [Test]
    public void duplicate_models_refuse_with_the_model_and_cure()
    {
        var services = new ServiceCollection();
        services.AddReactiveFluentValidation(validation => validation.Add<AdmissionRules>().Add<OtherAdmissionRules>());
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var scope = provider.CreateScope();
        using var controller = ControllerUsing(scope.ServiceProvider);
        Assert.That(() => controller.Validate<Admission, AdmissionRules>(new Admission()), Throws.InvalidOperationException.With.Message.EqualTo("Admission has more than one registered validator: AdmissionRules and OtherAdmissionRules. Register one validator for Admission with Add<TValidator>(); put its alternate checks in rule sets."));
    }

    [Test]
    public void adding_the_same_validator_twice_also_refuses()
    {
        var services = new ServiceCollection();
        services.AddReactiveFluentValidation(validation => validation.Add<AdmissionRules>());
        services.AddReactiveFluentValidation(validation => validation.Add<AdmissionRules>());
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        using var controller = ControllerUsing(scope.ServiceProvider);
        Assert.That(() => controller.Validate<Admission, AdmissionRules>(new Admission()), Throws.InvalidOperationException.With.Message.EqualTo("Admission has more than one registered validator: AdmissionRules and AdmissionRules. Register one validator for Admission with Add<TValidator>(); put its alternate checks in rule sets."));
    }

    [Test]
    public void an_unregistered_model_names_the_registration_it_needs()
    {
        using var controller = ControllerFor(new Admission(), services => services.AddReactiveFluentValidation(_ => { }));
        Assert.That(() => controller.Validate<Admission, AdmissionRules>(new Admission()), Throws.InvalidOperationException.With.Message.EqualTo("Admission has no registered AdmissionRules validator. Register it with services.AddReactiveFluentValidation(validation => validation.Add<AdmissionRules>())."));
    }

    [Test]
    public void a_missing_startup_names_the_registration_it_needs()
    {
        using var controller = ControllerFor(new Admission(), _ => { });
        Assert.That(() => controller.Validate<Admission, AdmissionRules>(new Admission()), Throws.InvalidOperationException.With.Message.EqualTo("Admission has no registered AdmissionRules validator. Register it with services.AddReactiveFluentValidation(validation => validation.Add<AdmissionRules>())."));
    }

    [Test]
    public void the_named_validator_must_be_the_models_registered_validator()
    {
        using var controller = ControllerFor(new Admission(), services => services.AddReactiveFluentValidation(validation => validation.Add<OtherAdmissionRules>()));
        Assert.That(() => controller.Validate<Admission, AdmissionRules>(new Admission()), Throws.InvalidOperationException.With.Message.EqualTo("Admission has no registered AdmissionRules validator. Register it with services.AddReactiveFluentValidation(validation => validation.Add<AdmissionRules>())."));
    }

    [Test]
    public void the_check_uses_the_existing_registration_in_its_own_scope_and_disposes_discovery()
    {
        var services = new ServiceCollection();
        var lifetime = new Lifetime();
        services.AddSingleton(lifetime);
        services.AddScoped<Policy>();
        services.AddScoped(provider => new ScopedRules(provider.GetRequiredService<Policy>(), 12));
        services.AddReactiveFluentValidation(validation => validation.Add<ScopedRules>());
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var first = provider.CreateScope();
        using var second = provider.CreateScope();
        first.ServiceProvider.GetRequiredService<Policy>().Minimum = 20;
        second.ServiceProvider.GetRequiredService<Policy>().Minimum = 30;
        using var firstController = ControllerUsing(first.ServiceProvider);
        using var secondController = ControllerUsing(second.ServiceProvider);
        var request = new Admission { Room = 25 };
        Assert.That(firstController.Validate<Admission, ScopedRules>(request).IsValid, Is.True);
        Assert.That(secondController.Validate<Admission, ScopedRules>(request).Errors.Select(error => error.ErrorMessage), Is.EqualTo(new[] { "The scoped policy refused the room." }));
        Assert.That(lifetime.Created, Is.EqualTo(3));
        Assert.That(lifetime.Disposed, Is.EqualTo(1));
    }

    [Test]
    public void a_new_validator_registration_uses_the_requests_scope()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new Lifetime());
        services.AddScoped<Policy>();
        services.AddReactiveFluentValidation(validation => validation.Add<DefaultScopedRules>());
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var first = provider.CreateScope();
        using var second = provider.CreateScope();
        first.ServiceProvider.GetRequiredService<Policy>().Minimum = 20;
        second.ServiceProvider.GetRequiredService<Policy>().Minimum = 30;
        using var firstController = ControllerUsing(first.ServiceProvider);
        using var secondController = ControllerUsing(second.ServiceProvider);
        var request = new Admission { Room = 25 };
        Assert.That(firstController.Validate<Admission, DefaultScopedRules>(request).IsValid, Is.True);
        Assert.That(secondController.Validate<Admission, DefaultScopedRules>(request).Errors.Select(error => error.ErrorMessage), Is.EqualTo(new[] { "The request's policy refused the room." }));
    }

    [Test]
    public async Task an_async_check_awaits_its_rule_and_keeps_binding_refusals()
    {
        var request = new Admission();
        var decision = new Decision();
        using var controller = ControllerFor(request, services =>
        {
            services.AddSingleton(decision);
            services.AddReactiveFluentValidation(validation => validation.Add<AwaitedRules>());
        });
        Assert.That(await Bind(controller, request, ("Room", "twelve"), ("ResidentName", "Ada")), Is.False);
        var checking = controller.ValidateAsync<Admission, AwaitedRules>(request);
        await decision.Started.Task;
        Assert.That(checking.IsCompleted, Is.False);
        decision.Answer.SetResult(false);
        var result = await checking;
        Assert.That(result.ErrorsByField["Room"], Is.EqualTo(new[] { "The value 'twelve' is not valid for Room." }));
        Assert.That(result.ErrorsByField["ResidentName"], Is.EqualTo(new[] { "The admission was declined." }));
        Assert.That(result.Errors.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task the_async_rule_receives_the_callers_token()
    {
        var request = new Admission { ResidentName = "Ada", Room = 10 };
        using var cancellation = new CancellationTokenSource();
        using var controller = ControllerFor(request, services =>
        {
            var rules = new InlineValidator<Admission>();
            rules.RuleFor(admission => admission.Room).MustAsync((_, token) => Task.FromResult(token == cancellation.Token));
            services.AddSingleton(rules);
            services.AddReactiveFluentValidation(validation => validation.Add<InlineValidator<Admission>>());
        });
        var result = await controller.ValidateAsync<Admission, InlineValidator<Admission>>(request, cancellation.Token);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public async Task a_named_async_check_uses_its_rule_set()
    {
        var request = new Admission { ResidentName = "Ada" };
        var decision = new Decision();
        decision.Answer.SetResult(false);
        using var controller = ControllerFor(request, services =>
        {
            services.AddSingleton(decision);
            services.AddReactiveFluentValidation(validation => validation.Add<AwaitedRules>());
        });
        var result = await controller.ValidateAsync<Admission, AwaitedRules>(request, "Move");
        Assert.That(result.Errors.Select(error => (error.PropertyName, error.ErrorMessage)), Is.EqualTo(new[] { ("Room", "A move needs room 20 or above.") }));
    }

    [Test]
    public async Task cancellation_reaches_the_async_rule()
    {
        var request = new Admission { ResidentName = "Ada" };
        var decision = new Decision();
        using var controller = ControllerFor(request, services =>
        {
            services.AddSingleton(decision);
            services.AddReactiveFluentValidation(validation => validation.Add<AwaitedRules>());
        });
        using var cancellation = new CancellationTokenSource();
        var checking = controller.ValidateAsync<Admission, AwaitedRules>(request, cancellation.Token);
        await decision.Started.Task;
        cancellation.Cancel();
        Assert.That(async () => await checking, Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public void a_synchronous_check_refuses_an_async_rule()
    {
        using var controller = ControllerFor(new Admission(), services =>
        {
            services.AddSingleton(new Decision());
            services.AddReactiveFluentValidation(validation => validation.Add<AwaitedRules>());
        });
        Assert.That(() => controller.Validate<Admission, AwaitedRules>(new Admission()), Throws.InvalidOperationException.With.Message.EqualTo("A rule that waits (MustAsync, CustomAsync or WhenAsync) runs only in ValidateAsync."));
    }

    [Test]
    public void a_combination_passes_over_an_enum_flags_marks_and_is_one_value_over_one_it_does_not()
    {
        var request = new Visiting { Days = VisitingDays.Morning | VisitingDays.Evening, OptionalDays = VisitingDays.Morning | VisitingDays.Evening };
        var hours = new VisitingHours { Hours = VisitingHour.Morning | VisitingHour.Evening };
        using var flags = ControllerFor(request, services => services.AddReactiveFluentValidation(validation => validation.Add<VisitingRules>()));
        using var single = ControllerFor(hours, services => services.AddReactiveFluentValidation(validation => validation.Add<VisitingHoursRules>()));
        Assert.That(flags.Validate<Visiting, VisitingRules>(request).IsValid, Is.True);
        Assert.That(single.Validate<VisitingHours, VisitingHoursRules>(hours).Errors.Select(error => (error.PropertyName, error.ErrorMessage)), Is.EqualTo(new[] { ("Hours", "'Hours' has a range of values which does not include '7'.") }));
    }

    [Test]
    public void flags_require_whole_declared_values_and_a_declared_zero()
    {
        using var controller = ControllerFor(new Visiting(), services => services.AddReactiveFluentValidation(validation => validation.Add<VisitingRules>()));
        Assert.That(controller.Validate<Visiting, VisitingRules>(new Visiting { Days = (VisitingDays)1 }).Errors.Select(error => (error.PropertyName, error.ErrorMessage)), Is.EqualTo(new[] { ("Days", "'Days' has a range of values which does not include '1'.") }));
        Assert.That(controller.Validate<Visiting, VisitingRules>(new Visiting { Days = 0 }).Errors.Select(error => (error.PropertyName, error.ErrorMessage)), Is.EqualTo(new[] { ("Days", "'Days' has a range of values which does not include '0'.") }));
        Assert.That(controller.Validate<Visiting, VisitingRules>(new Visiting { Days = VisitingDays.All }).IsValid, Is.True);
    }

    [Test]
    public void a_declared_zero_and_a_childs_flags_are_each_read_by_their_mark()
    {
        var request = new VisitingHousehold { Visit = new Visiting { Days = VisitingDays.Morning | VisitingDays.Evening } };
        using var controller = ControllerFor(request, services => services.AddReactiveFluentValidation(validation => validation.Add<VisitingHouseholdRules>()));
        Assert.That(controller.Validate<VisitingHousehold, VisitingHouseholdRules>(request).IsValid, Is.True);
    }

    private static Controller ControllerFor<T>(T model, Action<IServiceCollection> configure)
    {
        var view = Views.Of(model, _ => { }, configure);
        return ControllerUsing(view.ViewContext.HttpContext.RequestServices);
    }

    private static Controller ControllerUsing(IServiceProvider services)
        => new AdmissionController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = services },
                ActionDescriptor = new ControllerActionDescriptor(),
            },
        };

    private static Task<bool> Bind<T>(Controller controller, T request, params (string Key, string Value)[] values) where T : class
        => controller.TryUpdateModelAsync(request, "", Query(values));

    private static QueryStringValueProvider Query(params (string Key, string Value)[] values)
        => new(BindingSource.Query, new QueryCollection(values.ToDictionary(value => value.Key, value => new StringValues(value.Value))), CultureInfo.InvariantCulture);

    private sealed class AdmissionController : Controller;

    public sealed class AdmissionRules : ReactiveValidator<Admission>
    {
        public AdmissionRules()
        {
            RuleFor(request => request.ResidentName).NotEmpty().WithMessage("Enter the resident's name.");
            RuleFor(request => request.Room).GreaterThanOrEqualTo(10).WithMessage("Choose room 10 or above.");
            RuleSet("Move", () => RuleFor(request => request.ResidentName).MinimumLength(5).WithMessage("A move needs a longer name."));
        }
    }

    public sealed class OtherAdmissionRules : ReactiveValidator<Admission>;

    public sealed class GuardedRules : ReactiveValidator<Admission>
    {
        public GuardedRules()
        {
            WhenOnServer(request => 10 / request.Room > 0, () =>
                RuleFor(request => request.Room).GreaterThan(0).DependentRules(() => RuleFor(request => request.ResidentName).Equal("Grace")));
        }
    }

    public sealed class Household
    {
        public List<Admission> Residents { get; set; } = [];
    }

    public sealed class HouseholdRules : ReactiveValidator<Household>
    {
        public HouseholdRules() => RuleForEach(request => request.Residents).SetValidator(new AdmissionRules());
    }

    public sealed class RoomSelection
    {
        public List<int> Rooms { get; set; } = [];
    }

    public sealed class RoomSelectionRules : ReactiveValidator<RoomSelection>
    {
        public RoomSelectionRules() => RuleForEach(request => request.Rooms).GreaterThanOrEqualTo(10).WithMessage("Choose room 10 or above.");
    }

    public sealed class AnnotatedAdmission : IValidatableObject
    {
        public string ResidentName { get; set; } = "";
        [Required(ErrorMessage = "MVC requires a reference.")]
        public string? Reference { get; set; }
        [System.ComponentModel.DataAnnotations.Range(100, 200)]
        public int Room { get; set; }

        public IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
            => [new("MVC model validation ran.")];
    }

    public sealed class AnnotatedRules : ReactiveValidator<AnnotatedAdmission>;

    public sealed class Lifetime
    {
        public int Created { get; set; }
        public int Disposed { get; set; }
    }

    public sealed class Policy : IDisposable
    {
        private readonly Lifetime _lifetime;

        public Policy(Lifetime lifetime)
        {
            _lifetime = lifetime;
            lifetime.Created++;
        }

        public int Minimum { get; set; }
        public void Dispose() => _lifetime.Disposed++;
    }

    public sealed class ScopedRules : ReactiveValidator<Admission>
    {
        public ScopedRules(Policy policy, int floor)
            => RuleFor(request => request.Room).Must(room => room >= Math.Max(policy.Minimum, floor)).WithMessage("The scoped policy refused the room.");
    }

    public sealed class DefaultScopedRules : ReactiveValidator<Admission>
    {
        public DefaultScopedRules(Policy policy)
            => RuleFor(request => request.Room).Must(room => room >= policy.Minimum).WithMessage("The request's policy refused the room.");
    }

    public sealed class Decision
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource<bool> Answer { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public sealed class AwaitedRules : ReactiveValidator<Admission>
    {
        public AwaitedRules(Decision decision)
        {
            RuleFor(request => request.ResidentName).MustAsync(async (_, token) =>
            {
                decision.Started.TrySetResult();
                return await decision.Answer.Task.WaitAsync(token);
            }).WithMessage("The admission was declined.");
            RuleFor(request => request.Room).GreaterThan(0);
            RuleSet("Move", () => RuleFor(request => request.Room).GreaterThanOrEqualTo(20).WithMessage("A move needs room 20 or above."));
        }
    }

    [Flags]
    public enum VisitingDays
    {
        Morning = 3,
        Evening = 4,
        All = 15,
    }

    public sealed class Visiting
    {
        public VisitingDays Days { get; set; }
        public VisitingDays? OptionalDays { get; set; }
    }

    [Flags]
    public enum VisitOptions
    {
        None = 0,
        Escort = 1,
    }

    public sealed class VisitingHousehold
    {
        public Visiting Visit { get; set; } = new();
        public VisitOptions Options { get; set; }
    }

    public sealed class VisitingHouseholdRules : ReactiveValidator<VisitingHousehold>
    {
        public VisitingHouseholdRules()
        {
            RuleFor(request => request.Visit).SetValidator(new VisitingRules());
            RuleFor(request => request.Options).IsInEnum();
        }
    }

    public sealed class VisitingRules : ReactiveValidator<Visiting>
    {
        public VisitingRules()
        {
            RuleFor(request => request.Days).IsInEnum();
            RuleFor(request => request.OptionalDays).IsInEnum();
        }
    }

    public enum VisitingHour
    {
        Morning = 3,
        Evening = 4,
        All = 15,
    }

    public sealed class VisitingHours
    {
        public VisitingHour Hours { get; set; }
    }

    public sealed class VisitingHoursRules : ReactiveValidator<VisitingHours>
    {
        public VisitingHoursRules() => RuleFor(request => request.Hours).IsInEnum();
    }
}

