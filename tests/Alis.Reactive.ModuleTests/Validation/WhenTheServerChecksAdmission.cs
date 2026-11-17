using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alis.Reactive.Validation;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Validation;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenTheServerChecksAdmission
{
    [Test]
    public void a_predicate_can_check_the_value_and_the_model()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Room).Must(room => room != 13);
        rules.RuleFor(a => a.Room).Must((admission, room) => room > admission.Limit);
        Assert.That(rules.Validate(new Admission { Room = 13, Limit = 12 }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "The specified condition was not met for 'Room'.") }));
        Assert.That(rules.Validate(new Admission { Room = 12, Limit = 12 }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "The specified condition was not met for 'Room'.") }));
        Assert.That(rules.Validate(new Admission { Room = 12, Limit = 11 }).IsValid, Is.True);
    }

    [Test]
    public async Task an_awaited_predicate_finishes_before_its_dependent_rules()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Room).MustAsync((admission, room, cancellation) => Task.FromResult(room > admission.Limit && !cancellation.IsCancellationRequested))
            .DependentRules(() => rules.RuleFor(a => a.ResidentName).NotEmpty());
        var failed = await rules.ValidateAsync(new Admission { Room = 12, Limit = 12 });
        var dependent = await rules.ValidateAsync(new Admission { Room = 13, Limit = 12 });
        Assert.That(failed.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "The specified condition was not met for 'Room'.") }));
        Assert.That(dependent.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("ResidentName", "'Resident Name' must not be empty.") }));
        Assert.That((await rules.ValidateAsync(new Admission { Room = 13, Limit = 12, ResidentName = "Ada" })).IsValid, Is.True);
    }

    [Test]
    public async Task asynchronous_rules_keep_the_selected_set()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.ResidentName).NotEmpty();
        rules.RuleSet("Admit", () => rules.RuleFor(a => a.Room).MustAsync((room, cancellation) => Task.FromResult(room != 13 && !cancellation.IsCancellationRequested)));
        var failed = await rules.ValidateAsync(new Admission { Room = 13 }, "Admit");
        Assert.That(failed.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "The specified condition was not met for 'Room'.") }));
        Assert.That((await rules.ValidateAsync(new Admission { Room = 12 }, "Admit")).IsValid, Is.True);
    }

    [Test]
    public void custom_failures_keep_their_text_and_the_member_they_name()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Room).Custom((room, context) =>
        {
            if (room != 13) return;
            context.AddFailure("Room {PropertyValue} is closed.");
            context.AddFailure(a => a.ResidentName, "Contact admissions.");
        });
        Assert.That(rules.Validate(new Admission { Room = 13 }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "Room {PropertyValue} is closed."), ("ResidentName", "Contact admissions.") }));
        Assert.That(rules.Validate(new Admission { Room = 12 }).IsValid, Is.True);
    }

    [Test]
    public async Task custom_async_checks_receive_the_model_and_can_add_a_failure()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Room).CustomAsync((room, context, cancellation) =>
        {
            cancellation.ThrowIfCancellationRequested();
            if (room <= context.Model.Limit) context.AddFailure("Contact admissions.");
            return Task.CompletedTask;
        });
        var failed = await rules.ValidateAsync(new Admission { Room = 12, Limit = 12 });
        Assert.That(failed.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "Contact admissions.") }));
        Assert.That((await rules.ValidateAsync(new Admission { Room = 13, Limit = 12 })).IsValid, Is.True);
    }

    [Test]
    public void rules_kept_on_the_server_still_check_and_build_their_messages()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Room).GreaterThan(0).ServerOnly().WithMessage(a => "Room " + a.Room + " is closed.");
        Assert.That(rules.Validate(new Admission()).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "Room 0 is closed.") }));
        Assert.That(rules.Validate(new Admission { Room = 12 }).IsValid, Is.True);
    }

    [Test]
    public void server_conditions_and_unless_apply_to_their_own_case()
    {
        var rules = new InlineValidator<Admission>();
        rules.WhenOnServer(a => a.Required, () => rules.RuleFor(a => a.ResidentName).NotEmpty());
        rules.UnlessOnServer(a => a.Required, () => rules.RuleFor(a => a.Room).GreaterThan(0));
        Assert.That(rules.Validate(new Admission { Required = true }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("ResidentName", "'Resident Name' must not be empty.") }));
        Assert.That(rules.Validate(new Admission()).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "'Room' must be greater than '0'.") }));
        Assert.That(rules.Validate(new Admission { Room = 12 }).IsValid, Is.True);
    }

    [Test]
    public void rule_conditions_apply_to_all_preceding_checks_and_guard_member_reads()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Kin!.FullName).NotEmpty().WhenOnServer(a => a.Kin != null).Unless(a => !a.Required).UnlessOnServer(a => a.Room == 13);
        Assert.That(rules.Validate(new Admission { Required = true, Kin = new Contact() }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Kin.FullName", "'Kin Full Name' must not be empty.") }));
        Assert.That(rules.Validate(new Admission { Required = true }).IsValid, Is.True);
        Assert.That(rules.Validate(new Admission { Required = false, Kin = new Contact() }).IsValid, Is.True);
        Assert.That(rules.Validate(new Admission { Required = true, Room = 13, Kin = new Contact() }).IsValid, Is.True);
    }

    [Test]
    public void unless_applies_its_otherwise_to_the_positive_case()
    {
        var rules = new InlineValidator<Admission>();
        rules.Unless(a => a.Required, () => rules.RuleFor(a => a.Room).GreaterThan(0)).Otherwise(() => rules.RuleFor(a => a.ResidentName).NotEmpty());
        Assert.That(rules.Validate(new Admission { Required = true }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("ResidentName", "'Resident Name' must not be empty.") }));
        Assert.That(rules.Validate(new Admission()).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "'Room' must be greater than '0'.") }));
        Assert.That(rules.Validate(new Admission { Room = 12 }).IsValid, Is.True);
    }

    [Test]
    public async Task awaited_conditions_and_their_otherwise_select_one_case()
    {
        var rules = new InlineValidator<Admission>();
        rules.WhenAsync((a, cancellation) => Task.FromResult(a.Required && !cancellation.IsCancellationRequested), () => rules.RuleFor(a => a.ResidentName).NotEmpty()).Otherwise(() => rules.RuleFor(a => a.Room).GreaterThan(0));
        rules.UnlessAsync((a, cancellation) => Task.FromResult(a.Required && !cancellation.IsCancellationRequested), () => rules.RuleFor(a => a.Room).LessThan(13));
        var required = await rules.ValidateAsync(new Admission { Required = true });
        var optional = await rules.ValidateAsync(new Admission());
        var over = await rules.ValidateAsync(new Admission { Room = 13 });
        Assert.That(required.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("ResidentName", "'Resident Name' must not be empty.") }));
        Assert.That(optional.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "'Room' must be greater than '0'.") }));
        Assert.That(over.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "'Room' must be less than '13'.") }));
        Assert.That((await rules.ValidateAsync(new Admission { Room = 12 })).IsValid, Is.True);
    }

    [Test]
    public void a_cancelled_check_runs_no_rule()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Room).MustAsync((room, cancellation) => Task.FromResult(room != 13 && !cancellation.IsCancellationRequested));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Assert.That(async () => await rules.ValidateAsync(new Admission { Room = 13 }, cancellation.Token), Throws.InstanceOf<OperationCanceledException>());
    }
    [Test]
    public void a_custom_child_failure_keeps_the_item_and_typed_member_path()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleForEach(a => a.Contacts).ChildRules(child => child.RuleFor(c => c.FullName).Custom((name, context) =>
        {
            if (!string.IsNullOrEmpty(name)) return;
            context.AddFailure("Contact admissions.");
            context.AddFailure(c => c.FullName, "Provide a contact.");
        }));
        Assert.That(rules.Validate(new Admission { Contacts = [new Contact(), new Contact { FullName = "Ben" }] }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Contacts[0].FullName", "Contact admissions."), ("Contacts[0].FullName", "Provide a contact.") }));
        Assert.That(rules.Validate(new Admission { Contacts = [new Contact { FullName = "Ben" }] }).IsValid, Is.True);
    }

    [Test]
    public async Task validation_waits_for_the_admissions_service_to_answer()
    {
        var answer = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Room).MustAsync((_, _) => answer.Task);
        var pending = rules.ValidateAsync(new Admission { Room = 13 });
        Assert.That(pending.IsCompleted, Is.False);
        answer.SetResult(false);
        var result = await pending;
        Assert.That(result.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "The specified condition was not met for 'Room'.") }));
    }

    [Test]
    public void a_synchronous_call_refuses_an_async_rule_only_when_it_reaches_it()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Room).MustAsync((room, _) => Task.FromResult(room != 13)).When(a => a.Required);
        Assert.That(rules.Validate(new Admission { Required = false }).IsValid, Is.True);
        Assert.That(() => rules.Validate(new Admission { Required = true }), Throws.InvalidOperationException.With.Message.EqualTo("A rule that waits (MustAsync, CustomAsync or WhenAsync) runs only in ValidateAsync."));
    }
    [Test]
    public void a_member_rechecks_its_condition_after_custom_work_but_a_collection_selects_checks_first()
    {
        var member = new InlineValidator<Admission>();
        member.RuleFor(a => a.Room).Custom((_, context) => context.Model.Required = true).GreaterThan(0).When(a => !a.Required);
        Assert.That(member.Validate(new Admission()).IsValid, Is.True);
        var collection = new InlineValidator<Admission>();
        collection.RuleForEach(a => a.Rooms).Custom((_, context) => context.Model.Required = true).GreaterThan(0).When(a => !a.Required);
        Assert.That(collection.Validate(new Admission { Rooms = [0] }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Rooms[0]", "'Rooms' must be greater than '0'.") }));
    }
    [Test]
    public async Task a_validator_with_no_rules_has_no_cancellable_work()
    {
        var rules = new InlineValidator<Admission>();
        Assert.That((await rules.ValidateAsync(new Admission(), new CancellationToken(true))).IsValid, Is.True);
    }

    [Test]
    public void a_check_written_on_a_rule_no_reactive_validator_declares_is_refused_naming_the_rule_it_needs()
        => Assert.That(() => new AnAppsOwnRule().GreaterThan(0),
            Throws.ArgumentException.With.Message.EqualTo("Write a check on a rule a ReactiveValidator declares: RuleFor(x => x.Member). (Parameter 'rule')"));

    private sealed class AnAppsOwnRule : IRuleBuilder<Admission, int>
    {
        public IRuleBuilderOptions<Admission, int> SetValidator(IValidator<int> validator) => throw new NotSupportedException();

        public IRuleBuilder<Admission, int> Cascade(CascadeMode mode) => this;
    }
}
