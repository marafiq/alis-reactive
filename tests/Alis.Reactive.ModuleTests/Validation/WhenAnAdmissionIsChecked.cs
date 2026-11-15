using System.Linq;
using Alis.Reactive.Validation;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Validation;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAnAdmissionIsChecked
{
    [Test]
    public void a_required_name_cannot_be_null()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.ResidentName).NotNull();
        var failed = rules.Validate(new Admission { ResidentName = null });
        Assert.That(failed.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("ResidentName", "'Resident Name' must not be empty.") }));
        Assert.That(failed.IsValid, Is.False);
        Assert.That(rules.Validate(new Admission { ResidentName = "" }).IsValid, Is.True);
    }

    [Test]
    public void a_required_name_cannot_be_blank()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.ResidentName).NotEmpty();
        var failed = rules.Validate(new Admission { ResidentName = " \t" });
        Assert.That(failed.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("ResidentName", "'Resident Name' must not be empty.") }));
        Assert.That(failed.IsValid, Is.False);
        Assert.That(rules.Validate(new Admission { ResidentName = "Ada" }).IsValid, Is.True);
    }

    [Test]
    public void an_empty_name_cannot_hold_text()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.ResidentName).Empty();
        var failed = rules.Validate(new Admission { ResidentName = "Ada" });
        Assert.That(failed.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("ResidentName", "'Resident Name' must be empty.") }));
        Assert.That(failed.IsValid, Is.False);
        Assert.That(rules.Validate(new Admission { ResidentName = " \t" }).IsValid, Is.True);
    }

    [Test]
    public void a_name_meets_its_minimum_length()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.ResidentName).MinimumLength(3);
        var failed = rules.Validate(new Admission { ResidentName = "Al" });
        Assert.That(failed.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("ResidentName", "The length of 'Resident Name' must be at least 3 characters. You entered 2 characters.") }));
        Assert.That(failed.IsValid, Is.False);
        Assert.That(rules.Validate(new Admission { ResidentName = "Ada" }).IsValid, Is.True);
    }

    [Test]
    public void a_name_meets_its_maximum_length()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.ResidentName).MaximumLength(3);
        var failed = rules.Validate(new Admission { ResidentName = "Alice" });
        Assert.That(failed.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("ResidentName", "The length of 'Resident Name' must be 3 characters or fewer. You entered 5 characters.") }));
        Assert.That(failed.IsValid, Is.False);
        Assert.That(rules.Validate(new Admission { ResidentName = "Ada" }).IsValid, Is.True);
    }

    [Test]
    public void a_name_meets_both_length_bounds()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.ResidentName).Length(2, 4);
        var failed = rules.Validate(new Admission { ResidentName = "Alice" });
        Assert.That(failed.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("ResidentName", "'Resident Name' must be between 2 and 4 characters. You entered 5 characters.") }));
        Assert.That(failed.IsValid, Is.False);
        Assert.That(rules.Validate(new Admission { ResidentName = "Ada" }).IsValid, Is.True);
    }

    [Test]
    public void a_name_meets_its_exact_length()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.ResidentName).Length(3);
        var failed = rules.Validate(new Admission { ResidentName = "Al" });
        Assert.That(failed.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("ResidentName", "'Resident Name' must be 3 characters in length. You entered 2 characters.") }));
        Assert.That(failed.IsValid, Is.False);
        Assert.That(rules.Validate(new Admission { ResidentName = "Ada" }).IsValid, Is.True);
    }

    [Test]
    public void an_email_has_one_at_sign_between_text()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.ResidentName).EmailAddress();
        var failed = rules.Validate(new Admission { ResidentName = "a@@b" });
        Assert.That(failed.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("ResidentName", "'Resident Name' is not a valid email address.") }));
        Assert.That(failed.IsValid, Is.False);
        Assert.That(rules.Validate(new Admission { ResidentName = "a@b" }).IsValid, Is.True);
    }

    [Test]
    public void a_card_keeps_its_checksum_through_spaces_and_dashes()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.ResidentName).CreditCard();
        var failed = rules.Validate(new Admission { ResidentName = "4111-1111-1111-1112" });
        Assert.That(failed.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("ResidentName", "'Resident Name' is not a valid credit card number.") }));
        Assert.That(failed.IsValid, Is.False);
        Assert.That(rules.Validate(new Admission { ResidentName = "4111 1111 1111 1111" }).IsValid, Is.True);
    }

    [Test]
    public void equals_its_assigned_room()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Room).Equal(12);
        var failed = rules.Validate(new Admission { Room = 11 });
        Assert.That(failed.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "'Room' must be equal to '12'.") }));
        Assert.That(rules.Validate(new Admission { Room = 12 }).IsValid, Is.True);
    }

    [Test]
    public void excludes_a_reserved_room()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Room).NotEqual(12);
        var failed = rules.Validate(new Admission { Room = 12 });
        Assert.That(failed.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "'Room' must not be equal to '12'.") }));
        Assert.That(rules.Validate(new Admission { Room = 11 }).IsValid, Is.True);
    }

    [Test]
    public void requires_a_room_above_the_boundary()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Room).GreaterThan(12);
        var failed = rules.Validate(new Admission { Room = 12 });
        Assert.That(failed.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "'Room' must be greater than '12'.") }));
        Assert.That(rules.Validate(new Admission { Room = 13 }).IsValid, Is.True);
    }

    [Test]
    public void accepts_the_minimum_room()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Room).GreaterThanOrEqualTo(12);
        var failed = rules.Validate(new Admission { Room = 11 });
        Assert.That(failed.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "'Room' must be greater than or equal to '12'.") }));
        Assert.That(rules.Validate(new Admission { Room = 12 }).IsValid, Is.True);
    }

    [Test]
    public void requires_a_room_below_the_boundary()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Room).LessThan(12);
        var failed = rules.Validate(new Admission { Room = 12 });
        Assert.That(failed.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "'Room' must be less than '12'.") }));
        Assert.That(rules.Validate(new Admission { Room = 11 }).IsValid, Is.True);
    }

    [Test]
    public void accepts_the_maximum_room()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Room).LessThanOrEqualTo(12);
        var failed = rules.Validate(new Admission { Room = 13 });
        Assert.That(failed.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "'Room' must be less than or equal to '12'.") }));
        Assert.That(rules.Validate(new Admission { Room = 12 }).IsValid, Is.True);
    }

    [Test]
    public void includes_the_rooms_at_the_range_boundaries()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Room).InclusiveBetween(10, 12);
        var failed = rules.Validate(new Admission { Room = 13 });
        Assert.That(failed.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "'Room' must be between 10 and 12. You entered 13.") }));
        Assert.That(rules.Validate(new Admission { Room = 12 }).IsValid, Is.True);
    }

    [Test]
    public void excludes_the_rooms_at_the_range_boundaries()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Room).ExclusiveBetween(10, 12);
        var failed = rules.Validate(new Admission { Room = 12 });
        Assert.That(failed.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "'Room' must be between 10 and 12 (exclusive). You entered 12.") }));
        Assert.That(rules.Validate(new Admission { Room = 11 }).IsValid, Is.True);
    }

    [Test]
    public void the_fee_counts_digits_and_decimals()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Fee).PrecisionScale(4, 2, false);
        var failed = rules.Validate(new Admission { Fee = 123.456m });
        Assert.That(failed.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Fee", "'Fee' must not be more than 4 digits in total, with allowance for 2 decimals. 6 digits and 3 decimals were found.") }));
        Assert.That(rules.Validate(new Admission { Fee = 0.001m }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Fee", "'Fee' must not be more than 4 digits in total, with allowance for 2 decimals. 4 digits and 3 decimals were found.") }));
        Assert.That(rules.Validate(new Admission { Fee = 100m }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Fee", "'Fee' must not be more than 4 digits in total, with allowance for 2 decimals. 5 digits and 2 decimals were found.") }));
        Assert.That(rules.Validate(new Admission { Fee = 1.2m }).IsValid, Is.True);
        Assert.That(rules.Validate(new Admission { Fee = 12.34m }).IsValid, Is.True);
    }

    [Test]
    public void the_fee_can_ignore_trailing_zeroes()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Fee).PrecisionScale(4, 2, false);
        var failed = rules.Validate(new Admission { Fee = 1.230m });
        Assert.That(failed.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Fee", "'Fee' must not be more than 4 digits in total, with allowance for 2 decimals. 4 digits and 3 decimals were found.") }));
        var trimmed = new InlineValidator<Admission>();
        trimmed.RuleFor(a => a.Fee).PrecisionScale(4, 2, true);
        Assert.That(trimmed.Validate(new Admission { Fee = 1.230m }).IsValid, Is.True);
        Assert.That(trimmed.Validate(new Admission { Fee = 1.234m }).Errors.Select(e => e.ErrorMessage), Is.EqualTo(new[] { "'Fee' must not be more than 4 digits in total, with allowance for 2 decimals. 4 digits and 3 decimals were found." }));
    }

    [Test]
    public void care_must_be_a_declared_value()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Care).IsInEnum();
        var failed = rules.Validate(new Admission { Care = (CareLevel)7 });
        Assert.That(failed.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Care", "'Care' has a range of values which does not include '7'.") }));
        Assert.That(rules.Validate(new Admission { Care = CareLevel.Residential }).IsValid, Is.True);
    }

    [Test]
    public void null_text_passes_the_checks_that_do_not_require_it()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.ResidentName).MinimumLength(3).MaximumLength(10).Length(3, 10).Length(5).EmailAddress().CreditCard();
        Assert.That(rules.Validate(new Admission()).IsValid, Is.True);
        Assert.That(rules.Validate(new Admission { ResidentName = "a" }).Errors.Select(e => e.ErrorMessage), Is.EqualTo(new[]
        {
            "The length of 'Resident Name' must be at least 3 characters. You entered 1 characters.",
            "'Resident Name' must be between 3 and 10 characters. You entered 1 characters.",
            "'Resident Name' must be 5 characters in length. You entered 1 characters.",
            "'Resident Name' is not a valid email address.",
            "'Resident Name' is not a valid credit card number.",
        }));
    }
    [Test]
    public void a_contact_url_uses_the_http_or_https_scheme()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.ResidentName).Url();
        Assert.That(rules.Validate(new Admission { ResidentName = "ftp://care.test/admit" }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("ResidentName", "'Resident Name' must be a valid URL.") }));
        Assert.That(rules.Validate(new Admission { ResidentName = "https://care.test/admit" }).IsValid, Is.True);
        Assert.That(rules.Validate(new Admission { ResidentName = "http://care.test/admit" }).IsValid, Is.True);
        Assert.That(rules.Validate(new Admission()).IsValid, Is.True);
    }

    [Test]
    public void a_required_room_and_its_choices_cannot_be_empty()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Rooms).NotEmpty();
        rules.RuleFor(a => a.Room).NotEmpty();
        Assert.That(rules.Validate(new Admission()).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Rooms", "'Rooms' must not be empty."), ("Room", "'Room' must not be empty.") }));
        Assert.That(rules.Validate(new Admission { Rooms = [12], Room = 12 }).IsValid, Is.True);
        var empty = new InlineValidator<Admission>();
        empty.RuleFor(a => a.Rooms).Empty();
        empty.RuleFor(a => a.Room).Empty();
        Assert.That(empty.Validate(new Admission { Rooms = [12], Room = 12 }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Rooms", "'Rooms' must be empty."), ("Room", "'Room' must be empty.") }));
        Assert.That(empty.Validate(new Admission()).IsValid, Is.True);
    }

    [Test]
    public void optional_values_pass_when_absent_and_are_checked_when_present()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.OptionalRoom).GreaterThan(10).GreaterThanOrEqualTo(11).LessThan(15).LessThanOrEqualTo(14).InclusiveBetween(11, 14).ExclusiveBetween(10, 15);
        rules.RuleFor(a => a.OptionalFee).PrecisionScale(4, 2, false);
        rules.RuleFor(a => a.OptionalCare).IsInEnum();
        Assert.That(rules.Validate(new Admission { OptionalRoom = 15, OptionalFee = 123.456m, OptionalCare = (CareLevel)7 }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[]
        {
            ("OptionalRoom", "'Optional Room' must be less than '15'."),
            ("OptionalRoom", "'Optional Room' must be less than or equal to '14'."),
            ("OptionalRoom", "'Optional Room' must be between 11 and 14. You entered 15."),
            ("OptionalRoom", "'Optional Room' must be between 10 and 15 (exclusive). You entered 15."),
            ("OptionalFee", "'Optional Fee' must not be more than 4 digits in total, with allowance for 2 decimals. 6 digits and 3 decimals were found."),
            ("OptionalCare", "'Optional Care' has a range of values which does not include '7'."),
        }));
        Assert.That(rules.Validate(new Admission { OptionalRoom = 10 }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[]
        {
            ("OptionalRoom", "'Optional Room' must be greater than '10'."),
            ("OptionalRoom", "'Optional Room' must be greater than or equal to '11'."),
            ("OptionalRoom", "'Optional Room' must be between 11 and 14. You entered 10."),
            ("OptionalRoom", "'Optional Room' must be between 10 and 15 (exclusive). You entered 10."),
        }));
        Assert.That(rules.Validate(new Admission()).IsValid, Is.True);
        Assert.That(rules.Validate(new Admission { OptionalRoom = 12, OptionalFee = 12.34m, OptionalCare = CareLevel.Nursing }).IsValid, Is.True);
    }

    [Test]
    public void nullable_and_required_room_bounds_read_the_other_member()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.OptionalRoom).GreaterThan(a => a.Limit).GreaterThanOrEqualTo(a => a.Limit).LessThan(a => a.OptionalLimit).LessThanOrEqualTo(a => a.OptionalLimit);
        rules.RuleFor(a => a.Room).GreaterThan(a => a.OptionalLimit).GreaterThanOrEqualTo(a => a.OptionalLimit);
        Assert.That(rules.Validate(new Admission { OptionalRoom = 12, Limit = 13, OptionalLimit = 11, Room = 10 }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[]
        {
            ("OptionalRoom", "'Optional Room' must be greater than '13'."),
            ("OptionalRoom", "'Optional Room' must be greater than or equal to '13'."),
            ("OptionalRoom", "'Optional Room' must be less than '11'."),
            ("OptionalRoom", "'Optional Room' must be less than or equal to '11'."),
            ("Room", "'Room' must be greater than '11'."),
            ("Room", "'Room' must be greater than or equal to '11'."),
        }));
        Assert.That(rules.Validate(new Admission { OptionalLimit = 11, Room = 12 }).IsValid, Is.True);
    }

    [Test]
    public void room_upper_bounds_and_nullable_lower_bounds_read_the_other_member()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.OptionalRoom).GreaterThan(a => a.OptionalLimit).GreaterThanOrEqualTo(a => a.OptionalLimit).LessThan(a => a.Limit).LessThanOrEqualTo(a => a.Limit);
        rules.RuleFor(a => a.Room).LessThan(a => a.OptionalLimit).LessThanOrEqualTo(a => a.OptionalLimit);
        rules.RuleFor(a => a.Room).LessThan(a => a.Limit).LessThanOrEqualTo(a => a.Limit);
        Assert.That(rules.Validate(new Admission { OptionalRoom = 12, OptionalLimit = 13, Limit = 11, Room = 14 }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[]
        {
            ("OptionalRoom", "'Optional Room' must be greater than '13'."),
            ("OptionalRoom", "'Optional Room' must be greater than or equal to '13'."),
            ("OptionalRoom", "'Optional Room' must be less than '11'."),
            ("OptionalRoom", "'Optional Room' must be less than or equal to '11'."),
            ("Room", "'Room' must be less than '13'."),
            ("Room", "'Room' must be less than or equal to '13'."),
            ("Room", "'Room' must be less than '11'."),
            ("Room", "'Room' must be less than or equal to '11'."),
        }));
        Assert.That(rules.Validate(new Admission { OptionalLimit = 13, Limit = 11, Room = 10 }).IsValid, Is.True);
    }

    [Test]
    public void equality_and_inequality_read_the_other_member_and_preserve_null()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.OptionalRoom).Equal(a => a.OptionalLimit);
        rules.RuleFor(a => a.Room).NotEqual(a => a.Limit);
        Assert.That(rules.Validate(new Admission { OptionalRoom = 12, OptionalLimit = 13, Room = 12, Limit = 12 }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("OptionalRoom", "'Optional Room' must be equal to '13'."), ("Room", "'Room' must not be equal to '12'.") }));
        Assert.That(rules.Validate(new Admission { Room = 12, Limit = 13 }).IsValid, Is.True);
    }

    [Test]
    [SetCulture("en-US")]
    public void message_operands_are_formatted_in_english_culture()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Fee).GreaterThan(12.5m).WithMessage("{PropertyName}: {PropertyValue:F2}; {ComparisonValue:F2}; {Unknown}");
        Assert.That(rules.Validate(new Admission { Fee = 12.4m }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Fee", "Fee: 12.40; 12.50; {Unknown}") }));
        Assert.That(rules.Validate(new Admission { Fee = 12.6m }).IsValid, Is.True);
    }

    [Test]
    [SetCulture("de-DE")]
    public void message_operands_are_formatted_in_the_calling_culture()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.Fee).GreaterThan(12.5m).WithMessage("{PropertyName}: {PropertyValue:F2}; {ComparisonValue:F2}; {Unknown}");
        Assert.That(rules.Validate(new Admission { Fee = 12.4m }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Fee", "Fee: 12,40; 12,50; {Unknown}") }));
        Assert.That(rules.Validate(new Admission { Fee = 12.6m }).IsValid, Is.True);
    }
    [Test]
    public void a_rule_set_requires_a_nonempty_name()
    {
        var rules = new InlineValidator<Admission>();
        Assert.That(() => rules.RuleSet("", () => rules.RuleFor(a => a.Room).GreaterThan(0)), Throws.ArgumentException.With.Message.EqualTo("The value cannot be an empty string (Parameter 'ruleSetName')"));
        rules.RuleSet("Admit", () => rules.RuleFor(a => a.Room).GreaterThan(0));
        Assert.That(rules.Validate(new Admission(), "Admit").Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("Room", "'Room' must be greater than '0'.") }));
    }

    [Test]
    public void an_explicit_display_name_cannot_be_empty()
    {
        var rules = new InlineValidator<Admission>();
        var rule = rules.RuleFor(a => a.ResidentName).NotEmpty();
        Assert.That(() => rule.WithName(""), Throws.ArgumentException.With.Message.EqualTo("The value cannot be an empty string (Parameter 'overridePropertyName')"));
        rule.WithName("Resident Name");
        Assert.That(rules.Validate(new Admission()).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("ResidentName", "'Resident Name' must not be empty.") }));
    }

    [Test]
    public void an_explicit_literal_message_cannot_be_empty()
    {
        var rules = new InlineValidator<Admission>();
        var rule = rules.RuleFor(a => a.ResidentName).NotEmpty();
        Assert.That(() => rule.WithMessage(""), Throws.ArgumentException.With.Message.EqualTo("The value cannot be an empty string (Parameter 'errorMessage')"));
        rule.WithMessage("Contact admissions.");
        Assert.That(rules.Validate(new Admission()).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("ResidentName", "Contact admissions.") }));
    }

    [Test]
    public void length_minus_one_leaves_the_upper_bound_open()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.ResidentName).Length(3, -1);
        Assert.That(rules.Validate(new Admission { ResidentName = "Al" }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[] { ("ResidentName", "'Resident Name' must be between 3 and -1 characters. You entered 2 characters.") }));
        Assert.That(rules.Validate(new Admission { ResidentName = "Alice" }).IsValid, Is.True);
        var unlimited = new InlineValidator<Admission>();
        unlimited.RuleFor(a => a.ResidentName).MinimumLength(-1).MaximumLength(-1);
        Assert.That(unlimited.Validate(new Admission { ResidentName = "" }).IsValid, Is.True);
        Assert.That(unlimited.Validate(new Admission { ResidentName = "Ada" }).IsValid, Is.True);
    }

    [Test]
    public void reversed_length_bounds_are_refused()
    {
        var rules = new InlineValidator<Admission>();
        Assert.That(() => rules.RuleFor(a => a.ResidentName).Length(4, 2), Throws.TypeOf<System.ArgumentOutOfRangeException>().With.Message.EqualTo("Max should be larger than min. (Parameter 'max')"));
        Assert.That(() => rules.RuleFor(a => a.ResidentName).MaximumLength(-2), Throws.TypeOf<System.ArgumentOutOfRangeException>().With.Message.EqualTo("Max should be larger than min. (Parameter 'max')"));
        rules.RuleFor(a => a.ResidentName).Length(2, 4);
        Assert.That(rules.Validate(new Admission { ResidentName = "Ada" }).IsValid, Is.True);
    }

    [Test]
    public void reversed_room_bounds_are_refused()
    {
        var rules = new InlineValidator<Admission>();
        Assert.That(() => rules.RuleFor(a => a.Room).InclusiveBetween(12, 10), Throws.TypeOf<System.ArgumentOutOfRangeException>().With.Message.EqualTo("To should be larger than from. (Parameter 'to')"));
        rules.RuleFor(a => a.Room).InclusiveBetween(10, 12);
        Assert.That(rules.Validate(new Admission { Room = 12 }).IsValid, Is.True);
    }

    [Test]
    public void precision_and_scale_require_nonnegative_compatible_bounds()
    {
        var rules = new InlineValidator<Admission>();
        Assert.That(() => rules.RuleFor(a => a.Fee).PrecisionScale(-1, 0, false), Throws.TypeOf<System.ArgumentOutOfRangeException>().With.Message.EqualTo("Precision must be a positive integer. [value:-1]. (Parameter 'precision')"));
        Assert.That(() => rules.RuleFor(a => a.Fee).PrecisionScale(2, -1, false), Throws.TypeOf<System.ArgumentOutOfRangeException>().With.Message.EqualTo("Scale must be a positive integer. [value:-1]. (Parameter 'scale')"));
        Assert.That(() => rules.RuleFor(a => a.Fee).PrecisionScale(2, 3, false), Throws.TypeOf<System.ArgumentOutOfRangeException>().With.Message.EqualTo("Scale must be less than precision. [scale:3, precision:2]. (Parameter 'scale')"));
        rules.RuleFor(a => a.Fee).PrecisionScale(4, 2, false);
        Assert.That(rules.Validate(new Admission { Fee = 12.34m }).IsValid, Is.True);
    }
}
