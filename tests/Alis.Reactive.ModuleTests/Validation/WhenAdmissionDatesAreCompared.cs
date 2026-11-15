using System;
using System.Linq;
using Alis.Reactive.Validation;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Validation;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAdmissionDatesAreCompared
{
    [Test]
    public void a_date_is_compared_to_the_tick_and_null_time_passes_an_ordering()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.AdmittedAt).GreaterThan(a => a.ExpectedAt);
        rules.RuleFor(a => a.Starts).GreaterThanOrEqualTo(new TimeOnly(7, 0));
        rules.RuleFor(a => a.Stay).LessThanOrEqualTo(TimeSpan.FromHours(23));
        var failed = rules.Validate(new Admission
        {
            AdmittedAt = new DateTime(2026, 11, 15).AddTicks(1),
            ExpectedAt = new DateTime(2026, 11, 15).AddTicks(2),
            Starts = new TimeOnly(6, 30),
            Stay = TimeSpan.FromHours(26),
        });
        Assert.That(failed.Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[]
        {
            ("AdmittedAt", "'Admitted At' must be greater than '11/15/2026 12:00:00\u202fAM'."),
            ("Starts", "'Starts' must be greater than or equal to '7:00\u202fAM'."),
            ("Stay", "'Stay' must be less than or equal to '23:00:00'."),
        }));
        Assert.That(rules.Validate(new Admission
        {
            AdmittedAt = new DateTime(2026, 11, 15).AddTicks(2),
            ExpectedAt = new DateTime(2026, 11, 15).AddTicks(1),
            Stay = TimeSpan.FromHours(23),
        }).IsValid, Is.True);
    }
    [Test]
    public void offsets_compare_the_instant_to_the_tick_and_dates_compare_the_day()
    {
        var rules = new InlineValidator<Admission>();
        rules.RuleFor(a => a.SeenAt).Equal(a => a.ExpectedSeenAt);
        rules.RuleFor(a => a.AdmissionDay).LessThan(new DateOnly(2026, 11, 16));
        Assert.That(rules.Validate(new Admission
        {
            SeenAt = new DateTimeOffset(2026, 11, 15, 7, 0, 0, TimeSpan.FromHours(-5)).AddTicks(1),
            ExpectedSeenAt = new DateTimeOffset(2026, 11, 15, 12, 0, 0, TimeSpan.Zero).AddTicks(2),
            AdmissionDay = new DateOnly(2026, 11, 16),
        }).Errors.Select(e => (e.PropertyName, e.ErrorMessage)), Is.EqualTo(new[]
        {
            ("SeenAt", "'Seen At' must be equal to '11/15/2026 12:00:00\u202fPM +00:00'."),
            ("AdmissionDay", "'Admission Day' must be less than '11/16/2026'."),
        }));
        Assert.That(rules.Validate(new Admission
        {
            SeenAt = new DateTimeOffset(2026, 11, 15, 7, 0, 0, TimeSpan.FromHours(-5)).AddTicks(2),
            ExpectedSeenAt = new DateTimeOffset(2026, 11, 15, 12, 0, 0, TimeSpan.Zero).AddTicks(2),
            AdmissionDay = new DateOnly(2026, 11, 15),
        }).IsValid, Is.True);
    }
}
