using System;
using System.Linq.Expressions;
using Alis.Reactive.ArrayLambdas;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.ArrayLambdas;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAListsLambdaReadsADatesMembers
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";

    private const string AnyDose =
        "{\"version\":7,\"planId\":\"Alis.Reactive.ModuleTests.ArrayLambdas.MedicationRound\"," +
        "\"culture\":{\"name\":\"en-US\",\"collation\":\"en-US\",\"decimal\":\".\",\"group\":\",\"," +
        "\"integer\":{\"group\":false,\"exponent\":false},\"fraction\":{\"group\":true,\"exponent\":true}}," +
        "\"timeZone\":{\"kind\":\"clock\"},\"trace\":{\"kind\":\"off\"}," +
        "\"vendors\":{\"native\":{\"locate\":{\"kind\":\"fn\",\"params\":[\"element\"],\"body\":{\"kind\":\"ref\",\"name\":\"element\"}}," +
        "\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"}}}," +
        "\"objects\":{" +
        "\"$document\":{\"kind\":\"plugin\",\"name\":\"alis.document\",\"key\":\"\"," +
        "\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"},\"data\":{\"kind\":\"none\"}}," +
        "\"cart\":{\"kind\":\"element\",\"id\":\"cart\",\"vendor\":\"native\"}}," +
        "\"handlers\":[{\"on\":\"$document\",\"path\":[],\"event\":\"ready\",\"payload\":[],\"members\":\"required\",\"policy\":\"overlap\",\"do\":[" +
        "{\"kind\":\"set\",\"path\":[\"flagged\"],\"members\":\"required\",\"content\":false,\"target\":{\"kind\":\"obj\",\"id\":\"cart\"},\"value\":" +
        "{\"kind\":\"call\",\"path\":[\"some\"],\"members\":\"requiredIfHeld\"," +
        "\"target\":{\"kind\":\"get\",\"path\":[\"doses\"],\"members\":\"required\",\"target\":{\"kind\":\"obj\",\"id\":\"cart\"}}," +
        "\"args\":[{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":";

    private const string Ends = "}]}}]}]}";

    private const string Item = "{\"kind\":\"ref\",\"name\":\"item1\"}";

    private const string DueKey = "{\"kind\":\"op\",\"op\":\"dateKey\",\"args\":[" +
        "{\"kind\":\"data\",\"path\":[{\"name\":\"due\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"0001-01-01T00:00:00\"}}],\"target\":" + Item + "}]}";

    private const string RecordedKey = "{\"kind\":\"op\",\"op\":\"dateKey\",\"args\":[{\"kind\":\"op\",\"op\":\"nullableValue\",\"args\":[" +
        "{\"kind\":\"data\",\"path\":[{\"name\":\"recorded\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":null}}],\"target\":" + Item + "}]}]}";

    private const string SeenWallKey = "{\"kind\":\"op\",\"op\":\"wallKey\",\"args\":[" +
        "{\"kind\":\"data\",\"path\":[{\"name\":\"seen\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"0001-01-01T00:00:00\\u002B00:00\"}}],\"target\":" + Item + "}]}";

    private const string RoundKey = "{\"kind\":\"op\",\"op\":\"dayKey\",\"args\":[" +
        "{\"kind\":\"data\",\"path\":[{\"name\":\"round\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"0001-01-01\"}}],\"target\":" + Item + "}]}";

    private const string ShiftKey = "{\"kind\":\"op\",\"op\":\"timeKey\",\"args\":[" +
        "{\"kind\":\"data\",\"path\":[{\"name\":\"shift\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"00:00:00\"}}],\"target\":" + Item + "}]}";

    [Test]
    public void year_reads_the_year_of_the_dates_key()
        => Assert.That(Rendered(d => d.Due.Year == 2026), Is.EqualTo(Plan("{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + Part("year", DueKey) + ",{\"kind\":\"lit\",\"value\":2026}]}")));

    [Test]
    public void month_reads_the_month_of_the_dates_key()
        => Assert.That(Rendered(d => d.Due.Month > 10), Is.EqualTo(Plan("{\"kind\":\"op\",\"op\":\"gt\",\"args\":[" + Part("month", DueKey) + ",{\"kind\":\"lit\",\"value\":10}]}")));

    [Test]
    public void day_reads_the_day_of_the_dates_key()
        => Assert.That(Rendered(d => d.Due.Day == 9), Is.EqualTo(Plan("{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + Part("day", DueKey) + ",{\"kind\":\"lit\",\"value\":9}]}")));

    [Test]
    public void hour_reads_the_hour_of_the_dates_key_as_the_server_wrote_it()
        => Assert.That(Rendered(d => d.Due.Hour < 12), Is.EqualTo(Plan("{\"kind\":\"op\",\"op\":\"lt\",\"args\":[" + Part("hour", DueKey) + ",{\"kind\":\"lit\",\"value\":12}]}")));

    [Test]
    public void minute_reads_the_minute_of_the_dates_key()
        => Assert.That(Rendered(d => d.Due.Minute == 30), Is.EqualTo(Plan("{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + Part("minute", DueKey) + ",{\"kind\":\"lit\",\"value\":30}]}")));

    [Test]
    public void second_reads_the_second_of_the_dates_key()
        => Assert.That(Rendered(d => d.Due.Second == 0), Is.EqualTo(Plan("{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + Part("second", DueKey) + ",{\"kind\":\"lit\",\"value\":0}]}")));

    [Test]
    public void a_day_of_the_week_is_compared_with_the_days_number_from_sunday_at_zero()
        => Assert.That(Rendered(d => d.Due.DayOfWeek == DayOfWeek.Monday), Is.EqualTo(Plan("{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + Part("dayOfWeek", DueKey) + ",{\"kind\":\"lit\",\"value\":1}]}")));

    [Test]
    public void a_day_of_the_week_the_view_holds_is_written_as_its_number()
    {
        var restDay = DayOfWeek.Sunday;

        Assert.That(Rendered(d => d.Due.DayOfWeek != restDay), Is.EqualTo(Plan("{\"kind\":\"op\",\"op\":\"neq\",\"args\":[" + Part("dayOfWeek", DueKey) + ",{\"kind\":\"lit\",\"value\":0}]}")));
    }

    [Test]
    public void a_day_of_the_week_the_view_holds_as_null_is_written_as_null_and_not_as_sundays_number()
    {
        DayOfWeek? none = null;

        Assert.That(Rendered(d => d.Due.DayOfWeek == none), Is.EqualTo(Plan("{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + Part("dayOfWeek", DueKey) + ",{\"kind\":\"lit\",\"value\":null}]}")));
    }

    [Test]
    public void a_date_time_offsets_day_is_its_day_at_its_own_offset()
        => Assert.That(Rendered(d => d.Seen.Day == 9), Is.EqualTo(Plan("{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + Part("day", SeenWallKey) + ",{\"kind\":\"lit\",\"value\":9}]}")));

    [Test]
    public void a_date_time_offsets_other_members_read_its_wall_at_its_own_offset()
        => Assert.That(
            Rendered(d => d.Seen.Year == 2026 && d.Seen.Month == 11 && d.Seen.Hour == 23 && d.Seen.Minute == 30 && d.Seen.Second == 5 && d.Seen.DayOfWeek == DayOfWeek.Monday),
            Is.EqualTo(Plan(And(And(And(And(And(Equal("year", SeenWallKey, "2026"), Equal("month", SeenWallKey, "11")), Equal("hour", SeenWallKey, "23")),
                Equal("minute", SeenWallKey, "30")), Equal("second", SeenWallKey, "5")), Equal("dayOfWeek", SeenWallKey, "1")))));

    [Test]
    public void a_date_only_reads_its_month_its_day_and_its_day_of_the_week_from_its_key()
        => Assert.That(
            Rendered(d => d.Round.Month == 11 && d.Round.Day == 16 && d.Round.DayOfWeek == DayOfWeek.Monday),
            Is.EqualTo(Plan(And(And(Equal("month", RoundKey, "11"), Equal("day", RoundKey, "16")), Equal("dayOfWeek", RoundKey, "1")))));

    [Test]
    public void a_time_only_reads_its_minute_and_its_second_from_its_key()
        => Assert.That(
            Rendered(d => d.Shift.Minute == 30 && d.Shift.Second == 5),
            Is.EqualTo(Plan(And(Equal("minute", ShiftKey, "30"), Equal("second", ShiftKey, "5")))));

    [Test]
    public void a_nullables_value_reads_its_dates_member_and_nothing_where_it_holds_null()
        => Assert.That(Rendered(d => d.Recorded!.Value.Year == 2026), Is.EqualTo(Plan("{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + Part("year", RecordedKey) + ",{\"kind\":\"lit\",\"value\":2026}]}")));

    [Test]
    public void a_date_only_reads_its_year_from_its_key()
        => Assert.That(Rendered(d => d.Round.Year == 2026), Is.EqualTo(Plan("{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + Part("year", RoundKey) + ",{\"kind\":\"lit\",\"value\":2026}]}")));

    [Test]
    public void a_time_only_reads_its_hour_from_its_key()
        => Assert.That(Rendered(d => d.Shift.Hour >= 7), Is.EqualTo(Plan("{\"kind\":\"op\",\"op\":\"gte\",\"args\":[" + Part("hour", ShiftKey) + ",{\"kind\":\"lit\",\"value\":7}]}")));

    private static string Part(string part, string key) => "{\"kind\":\"op\",\"op\":\"" + part + "\",\"args\":[" + key + "]}";

    private static string Equal(string part, string key, string number) => "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + Part(part, key) + ",{\"kind\":\"lit\",\"value\":" + number + "}]}";

    private static string And(string left, string right) => "{\"kind\":\"op\",\"op\":\"and\",\"args\":[" + left + "," + right + "]}";

    private static string Plan(string body) => ScriptOpen + AnyDose + body + Ends + ScriptClose;

    private static string Rendered(Expression<Func<CartDose, bool>> predicate)
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var cart = pipeline.Component<DoseCart>("cart");
            cart.Flagged.Set(cart.Doses.Any(predicate));
        }));
        return Views.TextOf(html.RenderPlan(plan));
    }
}
