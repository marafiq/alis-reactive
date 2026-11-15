using System.Linq;
using Alis.Reactive.Validation;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Validation;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAdmissionRulesAreComposed
{
    [Test]
    public void a_display_name_changes_the_message_and_keeps_the_binding_path()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Kin!.FullName).NotEmpty().WithName("Next of kin");
        var failed = rules.Validate(new Admission { Kin = new Contact() });
        Assert.That(failed.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Kin.FullName", "'Next of kin' must not be empty.") }));
        Assert.That(failed.ErrorsByField, Is.EquivalentTo(new[] { new System.Collections.Generic.KeyValuePair<string, string[]>("Kin.FullName", ["'Next of kin' must not be empty."]) }));
        Assert.That(rules.Validate(new Admission { Kin = new Contact { FullName = "Ben" } }).IsValid, Is.True);
    }

    [Test]
    public void a_direct_nested_member_names_its_whole_path_in_the_message()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Kin!.FullName).NotEmpty();
        Assert.That(rules.Validate(new Admission { Kin = new Contact() }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Kin.FullName", "'Kin Full Name' must not be empty.") }));
        Assert.That(rules.Validate(new Admission { Kin = new Contact { FullName = "Ben" } }).IsValid, Is.True);
    }

    [Test]
    public void a_message_can_name_the_member_its_value_and_its_comparison()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Room).GreaterThan(a => a.Limit).WithMessage("{PropertyName}: {PropertyValue}, {ComparisonProperty} {ComparisonValue}, {PropertyPath}");
        Assert.That(rules.Validate(new Admission { Room = 12, Limit = 12 }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "Room: 12, Limit 12, Room") }));
        Assert.That(rules.Validate(new Admission { Room = 13, Limit = 12 }).IsValid, Is.True);
    }

    [Test]
    public void a_condition_on_a_chain_applies_to_checks_already_written()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.ResidentName).NotEmpty().When(a => a.Required).MinimumLength(3);
        Assert.That(rules.Validate(new Admission { ResidentName = "", Required = false }).Errors.Select(e => e.ErrorMessage), Is.EqualTo(new[] { "The length of 'Resident Name' must be at least 3 characters. You entered 0 characters." }));
        Assert.That(rules.Validate(new Admission { ResidentName = "Ada", Required = true }).IsValid, Is.True);
    }

    [Test]
    public void the_otherwise_branch_checks_the_other_case()
    {
        var rules = new InlineValidator<Admission>();
        rules.When(a => a.Required, () => rules.RuleFor(a => a.ResidentName).NotEmpty()).Otherwise(() => rules.RuleFor(a => a.Room).GreaterThan(0));
        Assert.That(rules.Validate(new Admission { Required = true }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("ResidentName", "'Resident Name' must not be empty.") }));
        Assert.That(rules.Validate(new Admission()).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "'Room' must be greater than '0'.") }));
        Assert.That(rules.Validate(new Admission { Room = 12 }).IsValid, Is.True);
        Assert.That(rules.Validate(new Admission { Required = true, ResidentName = "Ada" }).IsValid, Is.True);
    }

    [Test]
    public void a_rule_set_checks_its_own_rules_and_an_include_keeps_that_selection()
    {
        var included = new InlineValidator<Admission>();
        included.RuleFor(a => a.ResidentName).NotEmpty();
        included.RuleSet("Admit", () => included.RuleFor(a => a.Room).GreaterThan(0));
        var rules = new InlineValidator<Admission>();
        rules.Include(included);
        Assert.That(rules.Validate(new Admission()).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("ResidentName", "'Resident Name' must not be empty.") }));
        Assert.That(rules.Validate(new Admission(), "Admit").Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "'Room' must be greater than '0'.") }));
        Assert.That(rules.Validate(new Admission { Room = 12 }, "admit").IsValid, Is.True);
        Assert.That(rules.Validate(new Admission(), "Missing").IsValid, Is.True);
    }

    [Test]
    public void a_nested_rule_set_keeps_its_name_and_restores_the_outer_set()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleSet("Outer", () =>
        {
            rules.RuleSet("Inner", () => rules.RuleFor(a => a.Room).GreaterThan(0));
            rules.RuleFor(a => a.ResidentName).NotEmpty();
        });
        Assert.That(rules.Validate(new Admission(), "Inner").Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "'Room' must be greater than '0'.") }));
        Assert.That(rules.Validate(new Admission(), "Outer").Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("ResidentName", "'Resident Name' must not be empty.") }));
        Assert.That(rules.Validate(new Admission { Room = 12 }, "Inner").IsValid, Is.True);
        Assert.That(rules.Validate(new Admission { ResidentName = "Ada" }, "Outer").IsValid, Is.True);
    }

    [Test]
    public void a_child_validator_skips_null_and_prefixes_a_present_members_failures()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Kin!).SetValidator(new ContactRules());
        Assert.That(rules.Validate(new Admission { Kin = new Contact() }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Kin.FullName", "'Full Name' must not be empty.") }));
        Assert.That(rules.Validate(new Admission()).IsValid, Is.True);
        Assert.That(rules.Validate(new Admission { Kin = new Contact { FullName = "Ben" } }).IsValid, Is.True);
    }

    [Test]
    public void inline_child_rules_skip_null_and_prefix_a_present_members_failures()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Kin!).ChildRules(child => child.RuleFor(k => k.FullName).NotEmpty());
        Assert.That(rules.Validate(new Admission { Kin = new Contact() }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Kin.FullName", "'Full Name' must not be empty.") }));
        Assert.That(rules.Validate(new Admission()).IsValid, Is.True);
        Assert.That(rules.Validate(new Admission { Kin = new Contact { FullName = "Ben" } }).IsValid, Is.True);
    }

    [Test]
    public void inline_children_inherit_the_rule_set_and_named_validators_keep_their_own_sets()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleSet("Admit", () =>
        {
            rules.RuleFor(a => a.Kin!).SetValidator(new ContactRules());
            rules.RuleFor(a => a.Kin!).ChildRules(child => child.RuleFor(k => k.FullName).NotEmpty());
        });
        Assert.That(rules.Validate(new Admission { Kin = new Contact() }, "Admit").Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Kin.FullName", "'Full Name' must not be empty.") }));
        Assert.That(rules.Validate(new Admission { Kin = new Contact { FullName = "Ben" } }, "Admit").IsValid, Is.True);
    }

    [Test]
    public void each_item_failure_uses_its_binding_index()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleForEach(a => a.Rooms).GreaterThan(0);
        Assert.That(rules.Validate(new Admission { Rooms = [0, 12, -1] }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Rooms[0]", "'Rooms' must be greater than '0'."), ("Rooms[2]", "'Rooms' must be greater than '0'.") }));
        Assert.That(rules.Validate(new Admission { Rooms = [10, 12] }).IsValid, Is.True);
        Assert.That(rules.Validate(new Admission { Rooms = null! }).IsValid, Is.True);
    }

    [Test]
    public void each_child_failure_uses_its_item_and_member_path()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleForEach(a => a.Contacts).SetValidator(new ContactRules());
        Assert.That(rules.Validate(new Admission { Contacts = [new Contact { FullName = "Ben" }, new Contact()] }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Contacts[1].FullName", "'Full Name' must not be empty.") }));
        Assert.That(rules.Validate(new Admission { Contacts = [new Contact { FullName = "Ben" }] }).IsValid, Is.True);
    }

    [Test]
    public void stopping_cascade_ends_a_collection_rule_at_its_first_failure()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleForEach(a => a.Rooms).Cascade(CascadeMode.Stop).GreaterThan(0).LessThan(10);
        Assert.That(rules.Validate(new Admission { Rooms = [0, 12] }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Rooms[0]", "'Rooms' must be greater than '0'.") }));
        Assert.That(rules.Validate(new Admission { Rooms = [1, 2] }).IsValid, Is.True);
    }

    [Test]
    public void continuing_cascade_reports_every_failed_check_in_declaration_order()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.ResidentName).NotEmpty().MinimumLength(3);
        var failed = rules.Validate(new Admission { ResidentName = "" });
        Assert.That(failed.Errors.Select(e => e.ErrorMessage), Is.EqualTo(new[] { "'Resident Name' must not be empty.", "The length of 'Resident Name' must be at least 3 characters. You entered 0 characters." }));
        Assert.That(failed.ErrorsByField["ResidentName"], Is.EqualTo(new[] { "'Resident Name' must not be empty.", "The length of 'Resident Name' must be at least 3 characters. You entered 0 characters." }));
        Assert.That(rules.Validate(new Admission { ResidentName = "Ada" }).IsValid, Is.True);
    }

    [Test]
    public void a_validator_default_cascade_can_be_set_after_its_rules()
    {
        var rules = new AdmissionRules();
        Assert.That(rules.Validate(new Admission { ResidentName = "" }).Errors.Select(e => e.ErrorMessage), Is.EqualTo(new[] { "'Resident Name' must not be empty.", "'Room' must be greater than '0'." }));
        Assert.That(rules.Validate(new Admission { ResidentName = "Ada", Room = 12 }).IsValid, Is.True);
    }

    [Test]
    public void dependent_rules_run_only_after_their_parent_passes()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.ResidentName).NotEmpty().DependentRules(() => rules.RuleFor(a => a.Room).GreaterThan(0));
        Assert.That(rules.Validate(new Admission()).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("ResidentName", "'Resident Name' must not be empty.") }));
        Assert.That(rules.Validate(new Admission { ResidentName = "Ada" }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "'Room' must be greater than '0'.") }));
        Assert.That(rules.Validate(new Admission { ResidentName = "Ada", Room = 12 }).IsValid, Is.True);
    }

    private sealed class ContactRules : ReactiveValidator<Contact>
    {
        internal ContactRules() => RuleFor(c => c.FullName).NotEmpty();
    }

    private sealed class AdmissionRules : ReactiveValidator<Admission>
    {
        internal AdmissionRules()
        {
            RuleFor(a => a.ResidentName).NotEmpty().MinimumLength(3);
            RuleFor(a => a.Room).GreaterThan(0);
            RuleLevelCascadeMode = CascadeMode.Stop;
        }
    }
    [Test]
    public void a_condition_applies_to_dependents_only_when_they_precede_it()
    {
        var before = new InlineValidator<Admission>();
        before.RuleFor(a => a.ResidentName).NotEmpty().When(a => a.Required).DependentRules(() => before.RuleFor(a => a.Room).GreaterThan(0));
        Assert.That(before.Validate(new Admission()).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "'Room' must be greater than '0'.") }));
        var after = new InlineValidator<Admission>();
        after.RuleFor(a => a.ResidentName).NotEmpty().DependentRules(() => after.RuleFor(a => a.Room).GreaterThan(0)).When(a => a.Required);
        Assert.That(after.Validate(new Admission()).IsValid, Is.True);
    }
    [Test]
    public void outer_conditions_guard_both_nested_branches_before_the_child_is_read()
    {
        var present = new InlineValidator<Admission>();
        present.When(a => a.Kin != null, () => present.When(a => a.Kin!.FullName == "", () => present.RuleFor(a => a.Room).GreaterThan(0)));
        Assert.That(present.Validate(new Admission()).IsValid, Is.True);
        Assert.That(present.Validate(new Admission { Kin = new Contact { FullName = "" } }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "'Room' must be greater than '0'.") }));
        var otherwise = new InlineValidator<Admission>();
        otherwise.When(a => a.Kin == null, () => otherwise.RuleFor(a => a.ResidentName).NotEmpty())
            .Otherwise(() => otherwise.When(a => a.Kin!.FullName == "", () => otherwise.RuleFor(a => a.Room).GreaterThan(0)));
        Assert.That(otherwise.Validate(new Admission { ResidentName = "Ada" }).IsValid, Is.True);
        Assert.That(otherwise.Validate(new Admission { Kin = new Contact { FullName = "" } }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "'Room' must be greater than '0'.") }));
    }

    [Test]
    public void a_later_inline_condition_guards_the_conditions_it_wraps()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Room).GreaterThan(0).When(a => a.Kin!.FullName == "").When(a => a.Kin != null);
        Assert.That(rules.Validate(new Admission()).IsValid, Is.True);
        Assert.That(rules.Validate(new Admission { Kin = new Contact { FullName = "" } }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "'Room' must be greater than '0'.") }));
        Assert.That(rules.Validate(new Admission { Kin = new Contact { FullName = "Ben" } }).IsValid, Is.True);
    }

    [Test]
    public void a_selected_rule_set_is_one_exact_name()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.ResidentName).NotEmpty();
        rules.RuleSet("Admit", () => rules.RuleFor(a => a.Room).GreaterThan(0));
        Assert.That(rules.Validate(new Admission(), "default,Admit").IsValid, Is.True);
        Assert.That(rules.Validate(new Admission(), " Admit ").IsValid, Is.True);
        Assert.That(rules.Validate(new Admission(), "Admit").Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "'Room' must be greater than '0'.") }));
    }

    [Test]
    public void a_skipped_collection_check_skips_its_dependents_but_an_absent_collection_does_not()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleForEach(a => a.Rooms).GreaterThan(0).When(a => a.Required).DependentRules(() => rules.RuleFor(a => a.ResidentName).NotEmpty());
        Assert.That(rules.Validate(new Admission { Rooms = [12] }).IsValid, Is.True);
        Assert.That(rules.Validate(new Admission { Rooms = null!, Required = true }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("ResidentName", "'Resident Name' must not be empty.") }));
        Assert.That(rules.Validate(new Admission { Rooms = [12], Required = true, ResidentName = "Ada" }).IsValid, Is.True);
    }
}
