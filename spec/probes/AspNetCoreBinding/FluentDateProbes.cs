using FluentValidation;

static class FluentDateProbes
{
    public static void Run()
    {
        var shift = new FluentShiftRules();
        var empty = shift.Validate(new FluentShift());
        var early = shift.Validate(new FluentShift { Starts = new TimeOnly(6, 30), Length = new TimeSpan(1, 2, 0, 0) });
        var inside = shift.Validate(new FluentShift { Starts = new TimeOnly(7, 0), Length = TimeSpan.FromHours(23) });
        var tickPast = shift.Validate(new FluentShift { Seen = new DateTimeOffset(2026, 11, 9, 18, 0, 0, TimeSpan.Zero).AddTicks(1) });
        var otherOffset = shift.Validate(new FluentShift { Seen = new DateTimeOffset(2026, 11, 9, 10, 0, 0, TimeSpan.FromHours(-8)) });
        var day = new FluentDayRules();
        var stay = new FluentStayRules();
        var arrived = new DateTime(2026, 10, 27, 8, 0, 0);
        Console.WriteLine("fv-date-comparisons: FluentValidation 12.1.1 over null members: GreaterThanOrEqualTo, LessThanOrEqualTo and InclusiveBetween pass "
            + empty.IsValid
            + ", Equal fails " + !day.Validate(new FluentDay()).IsValid
            + ", NotEqual passes " + new FluentDayNotRules().Validate(new FluentDay()).IsValid
            + "; a TimeOnly of 06:30 fails GreaterThanOrEqualTo 07:00 " + early.Errors.Any(e => e.PropertyName == "Starts")
            + ", a TimeSpan of 1.02:00:00 fails LessThanOrEqualTo 23:00:00 " + early.Errors.Any(e => e.PropertyName == "Length")
            + ", 07:00 and 23:00:00 pass " + inside.IsValid
            + "; a DateTimeOffset one tick past the upper bound fails InclusiveBetween " + !tickPast.IsValid
            + ", the bound written at -08:00 passes " + otherOffset.IsValid
            + "; a DateTime one second before its peer fails GreaterThanOrEqualTo " + !stay.Validate(new FluentStay { Arrived = arrived, Left = arrived.AddSeconds(-1) }).IsValid
            + ", a Utc and a Local DateTime of one wall time pass it " + stay.Validate(new FluentStay { Arrived = DateTime.SpecifyKind(arrived, DateTimeKind.Utc), Left = DateTime.SpecifyKind(arrived, DateTimeKind.Local) }).IsValid);
    }
}

public sealed class FluentShift
{
    public TimeOnly? Starts { get; set; }
    public TimeSpan? Length { get; set; }
    public DateTimeOffset? Seen { get; set; }
}

public sealed class FluentShiftRules : AbstractValidator<FluentShift>
{
    public FluentShiftRules()
    {
        RuleFor(s => s.Starts).GreaterThanOrEqualTo(new TimeOnly(7, 0));
        RuleFor(s => s.Length).LessThanOrEqualTo(TimeSpan.FromHours(23));
        RuleFor(s => s.Seen).InclusiveBetween(new DateTimeOffset(2026, 11, 9, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 11, 9, 18, 0, 0, TimeSpan.Zero));
    }
}

public sealed class FluentDay
{
    public DateOnly? Day { get; set; }
}

public sealed class FluentDayRules : AbstractValidator<FluentDay>
{
    public FluentDayRules() => RuleFor(v => v.Day).Equal(new DateOnly(2026, 10, 27));
}

public sealed class FluentDayNotRules : AbstractValidator<FluentDay>
{
    public FluentDayNotRules() => RuleFor(v => v.Day).NotEqual(new DateOnly(2026, 10, 27));
}

public sealed class FluentStay
{
    public DateTime Arrived { get; set; }
    public DateTime? Left { get; set; }
}

public sealed class FluentStayRules : AbstractValidator<FluentStay>
{
    public FluentStayRules() => RuleFor(s => s.Left).GreaterThanOrEqualTo(s => s.Arrived);
}
