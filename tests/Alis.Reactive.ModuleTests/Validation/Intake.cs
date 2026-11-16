using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Validation;

namespace Alis.Reactive.ModuleTests.Validation;

public sealed class Supplies
{
    public int Count { get; set; }
}

public sealed class SuppliesRules : ReactiveValidator<Supplies>
{
    public SuppliesRules() => RuleFor(supplies => supplies.Count).GreaterThan(0);
}

public sealed class DeputyCard
{
    public Deputy? Deputy { get; set; }
}

public sealed class Deputy
{
    public string? Name { get; set; } = "Unnamed";
    public string? Phone { get; set; }
}

public sealed class DeputyRules : ReactiveValidator<Deputy>
{
    public DeputyRules() => RuleFor(deputy => deputy.Name).NotEmpty().WithMessage("Name the deputy.");
}

public sealed class DeputyCardRules : ReactiveValidator<DeputyCard>
{
    public DeputyCardRules() => RuleFor(card => card.Deputy!).SetValidator(new DeputyRules());
}

public sealed class IntakeCard
{
    public string? Name { get; set; } = "Unnamed";
    public string? Status { get; set; }
    public bool Urgent { get; set; }
    public List<Carer> Carers { get; set; } = [new Carer { Name = "Ruth" }];
}

public sealed class Carer
{
    public string? Name { get; set; }
}

public sealed class IntakeCardRules : ReactiveValidator<IntakeCard>
{
    public IntakeCardRules()
    {
        RuleFor(card => card.Status).NotEmpty().When(card => card.Urgent);
        RuleFor(card => card.Name).NotEmpty().WithMessage("Name the resident.");
        RuleForEach(card => card.Carers).ChildRules(carer => carer.RuleFor(c => c.Name).NotEmpty().WithMessage("Name the carer."));
        RuleFor(card => card.Name).Must(name => name != "Nobody").WithMessage("Only the server reads this.");
    }
}

public sealed class HomeCard
{
    public Lead Lead { get; set; } = new() { Name = "Ruth" };
}

public sealed class PopulatedHomeCard
{
    [System.Text.Json.Serialization.JsonObjectCreationHandling(System.Text.Json.Serialization.JsonObjectCreationHandling.Populate)]
    public Lead Lead { get; set; } = new() { Name = "Ruth" };
}

public sealed class Lead
{
    public string? Name { get; set; } = "";
    public string? Phone { get; set; }
}

public sealed class HomeCardRules : ReactiveValidator<HomeCard>
{
    public HomeCardRules() => RuleFor(card => card.Lead.Name).NotEmpty().WithMessage("Lead name is required.");
}

public sealed class PopulatedHomeCardRules : ReactiveValidator<PopulatedHomeCard>
{
    public PopulatedHomeCardRules() => RuleFor(card => card.Lead.Name).NotEmpty().WithMessage("Lead name is required.");
}

public sealed class SignedHomeCard
{
    [JsonConverter(typeof(LeadSignature))]
    public Lead Lead { get; set; } = new() { Name = "Ruth" };
}

public sealed class LeadSignature : JsonConverter<Lead>
{
    public override Lead Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => new() { Name = reader.GetString() };

    public override void Write(Utf8JsonWriter writer, Lead value, JsonSerializerOptions options) => writer.WriteStringValue(value.Name);
}

public sealed class SignedLeadNameRules : ReactiveValidator<SignedHomeCard>
{
    public SignedLeadNameRules() => RuleFor(card => card.Lead.Name).NotEmpty().WithMessage("Lead name is required.");
}

public sealed class SignedLeadNameServerRules : ReactiveValidator<SignedHomeCard>
{
    public SignedLeadNameServerRules() => RuleFor(card => card.Lead.Name).NotEmpty().WithMessage("Lead name is required.").ServerOnly();
}

public sealed class LeadRules : ReactiveValidator<Lead>
{
    public LeadRules() => RuleFor(lead => lead.Name).NotEmpty().WithMessage("Name the lead.");
}

public sealed class IncludedLeadRules : ReactiveValidator<Lead>
{
    public IncludedLeadRules() => Include(new LeadRules());
}

public sealed class ServerLeadRules : ReactiveValidator<Lead>
{
    public ServerLeadRules() => RuleFor(lead => lead.Name).NotEmpty().WithMessage("Name the lead.").ServerOnly();
}

public sealed class SignedLeadRules : ReactiveValidator<SignedHomeCard>
{
    public SignedLeadRules() => RuleFor(card => card.Lead).SetValidator(new LeadRules());
}

public sealed class SignedLeadChildRules : ReactiveValidator<SignedHomeCard>
{
    public SignedLeadChildRules() => RuleFor(card => card.Lead).ChildRules(lead => lead.RuleFor(l => l.Name).NotEmpty().WithMessage("Name the lead."));
}

public sealed class SignedIncludedLeadRules : ReactiveValidator<SignedHomeCard>
{
    public SignedIncludedLeadRules() => RuleFor(card => card.Lead).SetValidator(new IncludedLeadRules());
}

public sealed class SignedServerLeadRules : ReactiveValidator<SignedHomeCard>
{
    public SignedServerLeadRules() => RuleFor(card => card.Lead).SetValidator(new ServerLeadRules());
}

public sealed class HomeLeadRules : ReactiveValidator<HomeCard>
{
    public HomeLeadRules() => RuleFor(card => card.Lead).SetValidator(new LeadRules());
}

public sealed class HomeLeadChildRules : ReactiveValidator<HomeCard>
{
    public HomeLeadChildRules() => RuleFor(card => card.Lead).ChildRules(lead => lead.RuleFor(l => l.Name).NotEmpty().WithMessage("Name the lead."));
}

public sealed class HomeIncludedLeadRules : ReactiveValidator<HomeCard>
{
    public HomeIncludedLeadRules() => RuleFor(card => card.Lead).SetValidator(new IncludedLeadRules());
}

public sealed class Household
{
    public List<string> Phones { get; set; } = [];
}

public sealed class HouseholdCard
{
    public Household Household { get; set; } = new();
}

public sealed class SignedHouseholdCard
{
    [JsonConverter(typeof(HouseholdSignature))]
    public Household Household { get; set; } = new();
}

public sealed class HouseholdSignature : JsonConverter<Household>
{
    public override Household Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new() { Phones = [.. (reader.GetString() ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries)] };

    public override void Write(Utf8JsonWriter writer, Household value, JsonSerializerOptions options) => writer.WriteStringValue(string.Join(",", value.Phones));
}

public sealed class HouseholdPhonesRules : ReactiveValidator<HouseholdCard>
{
    public HouseholdPhonesRules() => RuleForEach(card => card.Household.Phones).NotEmpty().WithMessage("Give each phone.");
}

public sealed class SignedHouseholdPhonesRules : ReactiveValidator<SignedHouseholdCard>
{
    public SignedHouseholdPhonesRules() => RuleForEach(card => card.Household.Phones).NotEmpty().WithMessage("Give each phone.");
}

public sealed class LeadCard
{
    public string? Resident { get; set; }
    public Lead Lead { get; set; } = new() { Name = "Ruth" };
}

public sealed class SignedLeadCard
{
    public string? Resident { get; set; }

    [JsonConverter(typeof(LeadSignature))]
    public Lead Lead { get; set; } = new() { Name = "Ruth" };
}

public sealed class LeadCardRules : ReactiveValidator<LeadCard>
{
    public LeadCardRules() => RuleFor(card => card.Resident).NotEqual(c => c.Lead.Name).WithMessage("The lead is someone other than the resident.");
}

public sealed class SignedLeadCardRules : ReactiveValidator<SignedLeadCard>
{
    public SignedLeadCardRules() => RuleFor(card => card.Resident).NotEqual(c => c.Lead.Name).WithMessage("The lead is someone other than the resident.");
}

public sealed class RoundCard
{
    public string? Ward { get; set; }
    public int Beds { get; set; }
    public int Staff { get; set; }
}

public sealed class RoundCardRules : ReactiveValidator<RoundCard>
{
    public RoundCardRules()
    {
        RuleFor(card => card.Ward).Matches("^[A-Z][0-9]$").MaximumLength(2);
        RuleFor(card => card.Beds).InclusiveBetween(1, 40).WhenOnServer(card => card.Staff > 0);
        RuleFor(card => card.Staff).LessThanOrEqualTo(card => card.Beds).WithMessage("No more staff than beds.");
        RuleFor(card => card.Ward).NotEmpty().ServerOnly();
    }
}

public sealed class StaffedRoundRules : ReactiveValidator<RoundCard>
{
    public StaffedRoundRules() => RuleFor(card => card.Staff).LessThanOrEqualTo(card => card.Beds).WithMessage("No more staff than beds.");
}

public sealed class HiddenBedsCard
{
    [JsonIgnore]
    public int Beds { get; set; }

    public int Staff { get; set; }
}

public sealed class HiddenBedsRules : ReactiveValidator<HiddenBedsCard>
{
    public HiddenBedsRules() => RuleFor(card => card.Staff).LessThanOrEqualTo(card => card.Beds).WithMessage("No more staff than beds.");
}

public sealed class HiddenBedsBesideStaffRules : ReactiveValidator<HiddenBedsCard>
{
    public HiddenBedsBesideStaffRules() => RuleFor(card => card.Beds).GreaterThanOrEqualTo(card => card.Staff).WithMessage("A bed for each of the staff.");
}

public sealed class HiddenBedsCountRules : ReactiveValidator<HiddenBedsCard>
{
    public HiddenBedsCountRules() => RuleFor(card => card.Beds).Equal(4).WithMessage("Four beds on the round.").When(card => card.Beds > 0);
}

public sealed class RoundBedsCountRules : ReactiveValidator<RoundCard>
{
    public RoundBedsCountRules() => RuleFor(card => card.Beds).Equal(4).WithMessage("Four beds on the round.").When(card => card.Beds > 0);
}

public abstract class StaffedCard
{
    public abstract int Beds { get; set; }

    public int Staff { get; set; }
}

public sealed class OverriddenBedsCard : StaffedCard
{
    public override int Beds { get; set; }
}

public sealed class OverriddenBedsRules : ReactiveValidator<OverriddenBedsCard>
{
    public OverriddenBedsRules() => RuleFor(card => card.Staff).LessThanOrEqualTo(card => card.Beds).WithMessage("No more staff than beds.");
}

public sealed class BedsBesideStaffRules : ReactiveValidator<RoundCard>
{
    public BedsBesideStaffRules() => RuleFor(card => card.Staff).LessThanOrEqualTo(c => c.Beds).WithMessage("No more staff than beds.");
}

public sealed class CountedBedsCard
{
    [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
    public int Beds { get; set; }

    public int Staff { get; set; }
}

public sealed class CountedBedsRules : ReactiveValidator<CountedBedsCard>
{
    public CountedBedsRules() => RuleFor(card => card.Staff).LessThanOrEqualTo(c => c.Beds).WithMessage("No more staff than beds.");
}

public sealed class HashedBedsRules : ReactiveValidator<RoundCard>
{
    public HashedBedsRules() => RuleFor(card => card.Staff).LessThanOrEqualTo(c => c.Beds.GetHashCode()).WithMessage("No more staff than beds.");
}

public sealed class WingCard
{
    public long Beds { get; set; }
    public int Staff { get; set; }
}

public sealed class WingCardRules : ReactiveValidator<WingCard>
{
    public WingCardRules() => RuleFor(card => card.Staff).LessThanOrEqualTo(c => (int)c.Beds).WithMessage("No more staff than beds.");
}

public sealed class SmallWingCard
{
    public short Beds { get; set; }
    public int Staff { get; set; }
}

public sealed class SmallWingCardRules : ReactiveValidator<SmallWingCard>
{
    public SmallWingCardRules() => RuleFor(card => card.Staff).LessThanOrEqualTo(c => (int)c.Beds).WithMessage("No more staff than beds.");
}

public sealed class OpenBedsCard
{
    public int? Beds { get; set; }
    public int Staff { get; set; }
}

public sealed class OpenBedsRules : ReactiveValidator<OpenBedsCard>
{
    public OpenBedsRules() => RuleFor(card => card.Staff).LessThanOrEqualTo(c => c.Beds ?? 0).WithMessage("No more staff than beds.");
}

public sealed class HiddenOpenBedsCard
{
    [JsonIgnore]
    public int? Beds { get; set; }

    public int Staff { get; set; }
}

public sealed class HiddenOpenBedsRules : ReactiveValidator<HiddenOpenBedsCard>
{
    public HiddenOpenBedsRules() => RuleFor(card => card.Staff).LessThanOrEqualTo(c => c.Beds ?? 0).WithMessage("No more staff than beds.");
}

public sealed class HiddenOpenBedsBesideStaffRules : ReactiveValidator<HiddenOpenBedsCard>
{
    public HiddenOpenBedsBesideStaffRules() => RuleFor(card => card.Beds ?? 0).GreaterThanOrEqualTo(c => c.Staff).WithMessage("A bed for each of the staff.");
}

public sealed class OpenBedsBesideStaffRules : ReactiveValidator<OpenBedsCard>
{
    public OpenBedsBesideStaffRules() => RuleFor(card => card.Beds ?? 0).GreaterThanOrEqualTo(c => c.Staff).WithMessage("A bed for each of the staff.");
}

public sealed class HiddenOpenLeastRound
{
    [JsonIgnore]
    public decimal? Least { get; set; }

    public List<decimal> Amounts { get; set; } = [];
}

public sealed class HiddenOpenLeastRoundRules : ReactiveValidator<HiddenOpenLeastRound>
{
    public HiddenOpenLeastRoundRules() => RuleForEach(round => round.Amounts).GreaterThanOrEqualTo(r => r.Least ?? 0).WithMessage("Give at least the least dose.");
}

public sealed class OpenLeastRoundRules : ReactiveValidator<OpenMedicationRound>
{
    public OpenLeastRoundRules() => RuleForEach(round => round.Amounts).GreaterThanOrEqualTo(r => r.Least ?? 0).WithMessage("Give at least the least dose.");
}

public sealed class RotaCard
{
    public int Staff { get; set; }
    public List<Carer> Carers { get; set; } = [];
}

public sealed class RotaCardRules : ReactiveValidator<RotaCard>
{
    public RotaCardRules() => RuleFor(card => card.Staff).GreaterThanOrEqualTo(c => c.Carers.Count(carer => carer.Name != null)).WithMessage("A shift for each named carer.");
}

public sealed class UnreadRoundRules : ReactiveValidator<RoundCard>
{
    public UnreadRoundRules() => RuleFor(card => card.Ward).NotEmpty().When(card => card.Ward!.GetHashCode() > 0);
}

public sealed class FoldedRoundRules : ReactiveValidator<RoundCard>
{
    public FoldedRoundRules() => RuleFor(card => card.Ward).Matches("^a[0-9]$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
}

public sealed class InvariantWardRules : ReactiveValidator<RoundCard>
{
    public InvariantWardRules() => RuleFor(card => card.Ward).Matches("^[0-9]+$", System.Text.RegularExpressions.RegexOptions.CultureInvariant);
}

public sealed class PlainWardRules : ReactiveValidator<RoundCard>
{
    public PlainWardRules() => RuleFor(card => card.Ward).Matches("^[0-9]+$");
}

public sealed class MultilineWardRules : ReactiveValidator<RoundCard>
{
    public MultilineWardRules() => RuleFor(card => card.Ward).Matches("^[0-9]+$", System.Text.RegularExpressions.RegexOptions.Multiline);
}

public sealed class OrderedWardRules : ReactiveValidator<RoundCard>
{
    public OrderedWardRules() => RuleFor(card => card.Ward!).GreaterThan("A1");
}

public sealed class BoundedWardRules : ReactiveValidator<RoundCard>
{
    public BoundedWardRules() => RuleFor(card => card.Ward!).InclusiveBetween("A1", "C9");
}

public sealed class NamedStaffRules : ReactiveValidator<RoundCard>
{
    public NamedStaffRules() => RuleFor(card => card.Staff).LessThanOrEqualTo(card => card.Beds).WithMessage("No more than {ComparisonValue} staff.");
}

public sealed class CountedWardRules : ReactiveValidator<RoundCard>
{
    public CountedWardRules() => RuleFor(card => card.Ward).MaximumLength(2).WithMessage("You entered {TotalLength} letters.");
}

public sealed class DependentWardRules : ReactiveValidator<RoundCard>
{
    public DependentWardRules() => RuleFor(card => card.Ward).NotEmpty().DependentRules(() => RuleFor(card => card.Beds).GreaterThan(0));
}

public sealed class StoppedWardRules : ReactiveValidator<RoundCard>
{
    public StoppedWardRules() => RuleFor(card => card.Ward).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(2);
}

public sealed class StoppedAfterTheServerWardRules : ReactiveValidator<RoundCard>
{
    public StoppedAfterTheServerWardRules() => RuleFor(card => card.Ward).Cascade(CascadeMode.Stop).Must(ward => ward != "Z9").NotEmpty();
}

public sealed class StoppedBeforeTheServerWardRules : ReactiveValidator<RoundCard>
{
    public StoppedBeforeTheServerWardRules() => RuleFor(card => card.Ward).Cascade(CascadeMode.Stop).NotEmpty().Must(ward => ward != "Z9");
}

public sealed class StoppedDeputyRules : ReactiveValidator<DeputyCard>
{
    public StoppedDeputyRules() => RuleFor(card => card.Deputy!).Cascade(CascadeMode.Stop).NotNull().SetValidator(new DeputyRules());
}

public sealed class TrimmedWardRules : ReactiveValidator<RoundCard>
{
    public TrimmedWardRules() => RuleFor(card => card.Ward!.Trim()).NotEmpty();
}

public sealed class WardCard
{
    public string? Ward { get; set; }
}

public sealed class WardCardRules : ReactiveValidator<WardCard>
{
    public WardCardRules() => RuleFor(card => card.Ward).NotNull();
}

public sealed class OrderedStatusRules : ReactiveValidator<IntakeCard>
{
    public OrderedStatusRules() => RuleFor(card => card.Status!).GreaterThan(card => card.Name!);
}

public sealed class OrderedUrgencyRules : ReactiveValidator<IntakeCard>
{
    public OrderedUrgencyRules() => RuleFor(card => card.Urgent).GreaterThan(false);
}

public sealed class StoppedCarersRules : ReactiveValidator<IntakeCard>
{
    public StoppedCarersRules() => RuleForEach(card => card.Carers).Cascade(CascadeMode.Stop).ChildRules(carer => carer.RuleFor(c => c.Name).NotEmpty());
}

public sealed class DoseCard
{
    public decimal Dose { get; set; }
    public Dictionary<string, int> Rounds { get; set; } = [];
}

public sealed class CountedZerosRules : ReactiveValidator<DoseCard>
{
    public CountedZerosRules() => RuleFor(card => card.Dose).PrecisionScale(4, 1, false);
}

public sealed class ScaledDoseRules : ReactiveValidator<DoseCard>
{
    public ScaledDoseRules() => RuleFor(card => card.Dose).PrecisionScale(4, 2, true);
}

public sealed class EmailWardRules : ReactiveValidator<RoundCard>
{
    public EmailWardRules() => RuleFor(card => card.Ward).EmailAddress();
}

public sealed class CardWardRules : ReactiveValidator<RoundCard>
{
    public CardWardRules() => RuleFor(card => card.Ward).CreditCard();
}

public sealed class WebWardRules : ReactiveValidator<RoundCard>
{
    public WebWardRules() => RuleFor(card => card.Ward).Url();
}

[Flags]
public enum RoundDays
{
    Mon = 1,
    Tue = 2,
    Wed = 4,
}

public sealed class DayCard
{
    public RoundDays Days { get; set; } = RoundDays.Mon | RoundDays.Tue;
}

public sealed class DayCardRules : ReactiveValidator<DayCard>
{
    public DayCardRules() => RuleFor(card => card.Days).IsInEnum();
}

public sealed class EmptyRoundsRules : ReactiveValidator<DoseCard>
{
    public EmptyRoundsRules() => RuleFor(card => card.Rounds).NotEmpty();
}

public sealed class MedicationRound
{
    public decimal Least { get; set; }
    public List<decimal> Amounts { get; set; } = [];
}

public sealed class MedicationRoundRules : ReactiveValidator<MedicationRound>
{
    public MedicationRoundRules() => RuleForEach(round => round.Amounts).GreaterThanOrEqualTo(round => round.Least).WithMessage("Give at least the least dose.");
}

public sealed class OpenMedicationRound
{
    public decimal? Least { get; set; }
    public List<decimal> Amounts { get; set; } = [];
}

public sealed class OpenMedicationRoundRules : ReactiveValidator<OpenMedicationRound>
{
    public OpenMedicationRoundRules() => RuleForEach(round => round.Amounts).GreaterThanOrEqualTo(round => round.Least).WithMessage("Give at least the least dose.");
}

public sealed class HiddenLeastRound
{
    [JsonIgnore]
    public decimal Least { get; set; }

    public List<decimal> Amounts { get; set; } = [];
}

public sealed class HiddenLeastRoundRules : ReactiveValidator<HiddenLeastRound>
{
    public HiddenLeastRoundRules() => RuleForEach(round => round.Amounts).GreaterThanOrEqualTo(round => round.Least).WithMessage("Give at least the least dose.");
}

public sealed class ChartedRound
{
    public decimal Least { get; set; }

    [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
    public List<decimal> Amounts { get; set; } = [];
}

public sealed class ChartedRoundRules : ReactiveValidator<ChartedRound>
{
    public ChartedRoundRules() => RuleForEach(round => round.Amounts).GreaterThanOrEqualTo(round => round.Least).WithMessage("Give at least the least dose.");
}

public sealed class SealedRound
{
    public decimal? Least { get; set; }

    [JsonConverter(typeof(AmountsText))]
    public List<decimal> Amounts { get; set; } = [];
}

public sealed class SealedRoundRules : ReactiveValidator<SealedRound>
{
    public SealedRoundRules() => RuleForEach(round => round.Amounts).GreaterThanOrEqualTo(round => round.Least).WithMessage("Give at least the least dose.");
}

public sealed class AmountsText : JsonConverter<List<decimal>>
{
    public override List<decimal> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => [.. (reader.GetString() ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(amount => decimal.Parse(amount, CultureInfo.InvariantCulture))];

    public override void Write(Utf8JsonWriter writer, List<decimal> value, JsonSerializerOptions options)
        => writer.WriteStringValue(string.Join(",", value.Select(amount => amount.ToString(CultureInfo.InvariantCulture))));
}

public sealed class Visit
{
    public DateTime At { get; set; }
}

public sealed class VisitRules : ReactiveValidator<Visit>
{
    public VisitRules() => RuleFor(v => v.At).GreaterThan(new DateTime(2026, 11, 1)).WithMessage("Book a visit after November 1.");
}

public sealed class ArrivedVisitRules : ReactiveValidator<Visit>
{
    public ArrivedVisitRules() => RuleFor(v => v.At).NotEmpty().WithMessage("Give the visit's day.");
}

public sealed class DayText : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => DateTime.ParseExact(reader.GetString() ?? "01.01.0001", "dd.MM.yyyy", CultureInfo.InvariantCulture);

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture));
}

public enum DoseStatus
{
    Due,
    Given,
    Held,
}

public sealed class DoseChart
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DoseStatus Charted { get; set; }

    public DoseStatus Planned { get; set; }

    [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
    public int Vials { get; set; }

    public int Doses { get; set; }

    [JsonConverter(typeof(MilligramsText))]
    public decimal Milligrams { get; set; }

    [JsonConverter(typeof(WardCode))]
    public string? Ward { get; set; }
}

public sealed class ChartedDoseRules : ReactiveValidator<DoseChart>
{
    public ChartedDoseRules() => RuleFor(chart => chart.Charted).Equal(DoseStatus.Given).WithMessage("Chart the dose as given.")
        .IsInEnum().WithMessage("Chart a status the round knows.")
        .NotEmpty().WithMessage("Chart the dose.");
}

public sealed class PlannedDoseRules : ReactiveValidator<DoseChart>
{
    public PlannedDoseRules() => RuleFor(chart => chart.Planned).Equal(DoseStatus.Given).WithMessage("Chart the dose as given.")
        .IsInEnum().WithMessage("Chart a status the round knows.")
        .NotEmpty().WithMessage("Chart the dose.");
}

public sealed class CountedVialsRules : ReactiveValidator<DoseChart>
{
    public CountedVialsRules() => RuleFor(chart => chart.Vials).GreaterThan(0);
}

public sealed class BoundedVialsRules : ReactiveValidator<DoseChart>
{
    public BoundedVialsRules() => RuleFor(chart => chart.Vials).InclusiveBetween(1, 4);
}

public sealed class WeighedMilligramsRules : ReactiveValidator<DoseChart>
{
    public WeighedMilligramsRules() => RuleFor(chart => chart.Milligrams).GreaterThan(0m);
}

public sealed class ExactMilligramsRules : ReactiveValidator<DoseChart>
{
    public ExactMilligramsRules() => RuleFor(chart => chart.Milligrams).Equal(5m);
}

public sealed class SuppliedVialsRules : ReactiveValidator<DoseChart>
{
    public SuppliedVialsRules() => RuleFor(chart => chart.Vials).LessThanOrEqualTo(chart => chart.Doses).WithMessage("No more vials than doses.");
}

public sealed class ShortWardRules : ReactiveValidator<DoseChart>
{
    public ShortWardRules() => RuleFor(chart => chart.Ward).MaximumLength(3);
}

public sealed class CodedWardRules : ReactiveValidator<DoseChart>
{
    public CodedWardRules() => RuleFor(chart => chart.Ward).Matches("^[A-Z][0-9]$");
}

public sealed class SetDoseRules : ReactiveValidator<ChartedRound>
{
    public SetDoseRules() => RuleForEach(round => round.Amounts).Equal(2.5m).WithMessage("Give the set dose.");
}

public sealed class PlainSetDoseRules : ReactiveValidator<MedicationRound>
{
    public PlainSetDoseRules() => RuleForEach(round => round.Amounts).Equal(2.5m).WithMessage("Give the set dose.");
}

public sealed class PositiveAmountsRules : ReactiveValidator<ChartedRound>
{
    public PositiveAmountsRules() => RuleForEach(round => round.Amounts).GreaterThan(0m);
}

public sealed class ChartedAmountsRules : ReactiveValidator<ChartedRound>
{
    public ChartedAmountsRules() => RuleFor(round => round.Amounts).NotNull().WithMessage("Chart the amounts.");
}

public sealed class PlainAmountsRules : ReactiveValidator<MedicationRound>
{
    public PlainAmountsRules() => RuleFor(round => round.Amounts).NotNull().WithMessage("Chart the amounts.");
}

public sealed class FilledAmountsRules : ReactiveValidator<ChartedRound>
{
    public FilledAmountsRules() => RuleFor(round => round.Amounts).NotEmpty().WithMessage("Give a dose.");
}

public sealed class SealedSetDoseRules : ReactiveValidator<SealedRound>
{
    public SealedSetDoseRules() => RuleForEach(round => round.Amounts).Equal(2.5m);
}

public sealed class SealedAmountsRules : ReactiveValidator<SealedRound>
{
    public SealedAmountsRules() => RuleFor(round => round.Amounts).NotEmpty();
}

public sealed class MilligramsText : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => decimal.Parse((reader.GetString() ?? "0").Split(' ')[0], CultureInfo.InvariantCulture);

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture) + " mg");
}

public sealed class WardCode : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.GetString()?.Substring("Ward ".Length);

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) => writer.WriteStringValue("Ward " + value);
}
