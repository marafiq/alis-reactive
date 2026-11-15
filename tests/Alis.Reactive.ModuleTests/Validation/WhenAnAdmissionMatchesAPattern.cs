using System;
using System.Linq;
using System.Text.RegularExpressions;
using Alis.Reactive.Validation;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Validation;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAnAdmissionMatchesAPattern
{
    [Test]
    public void a_name_matches_the_developers_pattern_and_null_needs_no_match()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.ResidentName).Matches("^[A-Z][a-z]+$");
        Assert.That(rules.Validate(new Admission { ResidentName = "ADA" }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)),
            Is.EqualTo(new[] { ("ResidentName", "'Resident Name' is not in the correct format.") }));
        Assert.That(rules.Validate(new Admission { ResidentName = "" }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)),
            Is.EqualTo(new[] { ("ResidentName", "'Resident Name' is not in the correct format.") }));
        Assert.That(rules.Validate(new Admission { ResidentName = "Ada" }).IsValid, Is.True);
        Assert.That(rules.Validate(new Admission()).IsValid, Is.True);
    }

    [Test]
    public void a_server_pattern_reads_the_developers_options()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.ResidentName).Matches("^ada$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).ServerOnly();
        Assert.That(rules.Validate(new Admission { ResidentName = "Ben" }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)),
            Is.EqualTo(new[] { ("ResidentName", "'Resident Name' is not in the correct format.") }));
        Assert.That(rules.Validate(new Admission { ResidentName = "ADA" }).IsValid, Is.True);
    }

    [Test]
    public void a_dollar_reads_the_original_server_pattern()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.ResidentName).Matches("^Ada$");
        Assert.That(rules.Validate(new Admission { ResidentName = "Ada\r\n" }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)),
            Is.EqualTo(new[] { ("ResidentName", "'Resident Name' is not in the correct format.") }));
        Assert.That(rules.Validate(new Admission { ResidentName = "Ada\n" }).IsValid, Is.True);
    }

    [Test]
    public void the_pattern_has_its_message_placeholder()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.ResidentName).Matches("^[A-Z][a-z]+$").WithMessage("{PropertyName} must match {RegularExpression}.");
        Assert.That(rules.Validate(new Admission { ResidentName = "ADA" }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)),
            Is.EqualTo(new[] { ("ResidentName", "Resident Name must match ^[A-Z][a-z]+$.") }));
        Assert.That(rules.Validate(new Admission { ResidentName = "Ada" }).IsValid, Is.True);
    }

    [Test]
    public void an_invalid_pattern_is_refused_when_the_rule_is_declared()
    {
        var rules = new InlineValidator<Admission>();
        var failure = Assert.Throws<RegexParseException>(() => rules.RuleFor(a => a.ResidentName).Matches("["));
        Assert.That(failure!.Message, Is.EqualTo("Invalid pattern '[' at offset 1. Unterminated [] set."));
    }

    [Test]
    public void an_empty_pattern_matches_any_text()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.ResidentName).Matches("");
        Assert.That(rules.Validate(new Admission { ResidentName = "" }).IsValid, Is.True);
        Assert.That(rules.Validate(new Admission { ResidentName = "Ada" }).IsValid, Is.True);
    }

    [Test]
    public void a_pattern_has_the_packages_two_second_timeout()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.ResidentName).Matches("^(a+)+$");
        var failure = Assert.Throws<RegexMatchTimeoutException>(() => rules.Validate(new Admission { ResidentName = new string('a', 64) + "!" }));
        Assert.That(failure!.MatchTimeout.TotalSeconds, Is.EqualTo(2));
    }
}
