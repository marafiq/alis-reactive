using System.Collections.Generic;

namespace Alis.Reactive.Conformance.World;

public sealed class Seed
{
    public IList<string> Residents { get; set; } = new List<string>();

    public string? Word { get; set; }

    public int? CachedRoundsPlanVersion { get; set; }

    public int? CachedVitalsPlanVersion { get; set; }

    public bool? CachedVitalsCutOff { get; set; }

    public string? ComponentText { get; set; }

    public string? ComponentOuterText { get; set; }

    public string? ComponentReadOnlyText { get; set; }

    public string? ComponentCurrentItem { get; set; }

    public int? HeldNamedItems { get; set; }

    public int? HeldTotal { get; set; }

    public int? ComponentOuterCount { get; set; }

    public int? ComponentActiveItems { get; set; }

    public int? ComponentCount { get; set; }

    public string? ComponentRecordDetail { get; set; }

    public ComponentVersion? ComponentVersion { get; set; }

    public string? NotesGroup { get; set; }

    public int? MoveStatus { get; set; }

    public bool? CodeRefuses { get; set; }

    public int? HeldMatchingItems { get; set; }

    public bool? TextRefuses { get; set; }

    public bool? HeldCountRefuses { get; set; }

    public bool? SettleRefuses { get; set; }

    public bool? CountRefuses { get; set; }

    public bool? ItemRefuses { get; set; }

    public bool? BedsDropTheirConnection { get; set; }

    public bool? ComponentHoldsNoRecords { get; set; }

    public string? ComponentStartsPlace { get; set; }

    public string? ComponentBetaConstructor { get; set; }

    public int? RecordCount { get; set; }

    public static Seed Resident(string name) => new Seed().AndResident(name);

    public static Seed TheWord(string word) => new() { Word = word };

    public static Seed CachedRoundsOfPlanVersion(int version) => new() { CachedRoundsPlanVersion = version };

    public static Seed CachedVitalsOfPlanVersion(int version) => new() { CachedVitalsPlanVersion = version };

    public static Seed CachedVitalsCutOffInsideAnObject() => new() { CachedVitalsCutOff = true };

    public static Seed TheComponentsText(string text) => new() { ComponentText = text };

    public static Seed TheComponentsOuterText(string text) => new() { ComponentOuterText = text };

    public static Seed TheComponentsReadOnlyText(string text) => new() { ComponentReadOnlyText = text };

    public static Seed TheComponentsCurrentItem(string name) => new() { ComponentCurrentItem = name };

    public static Seed TheHeldObjectsNamedItems(int named) => new() { HeldNamedItems = named };

    public static Seed TheHeldObjectsTotal(int total) => new() { HeldTotal = total };

    public static Seed TheComponentsOuterCount(int count) => new() { ComponentOuterCount = count };

    public static Seed TheComponentsActiveItems(int active) => new() { ComponentActiveItems = active };

    public static Seed TheComponentsCount(int count) => new() { ComponentCount = count };

    public static Seed TheComponentsRecordDetail(string name) => new() { ComponentRecordDetail = name };

    public static Seed TheComponentsVersion(ComponentVersion version) => new() { ComponentVersion = version };

    public static Seed TheNotesGroup(string group) => new() { NotesGroup = group };

    public static Seed TheMoveEndpointAnswers(int status) => new() { MoveStatus = status };

    public static Seed TheCodeEndpointRefuses() => new() { CodeRefuses = true };

    public static Seed TheHeldObjectsMatchingItems(int matching) => new() { HeldMatchingItems = matching };

    public static Seed TheTextEndpointRefuses() => new() { TextRefuses = true };

    public static Seed TheHeldCountEndpointRefuses() => new() { HeldCountRefuses = true };

    public static Seed TheSettleEndpointRefuses() => new() { SettleRefuses = true };

    public static Seed TheCountEndpointRefuses() => new() { CountRefuses = true };

    public static Seed TheItemEndpointRefuses() => new() { ItemRefuses = true };

    public static Seed TheBedsEndpointDropsItsConnection() => new() { BedsDropTheirConnection = true };

    public static Seed TheComponentHoldsNoRecords() => new() { ComponentHoldsNoRecords = true };

    public static Seed TheComponentsStartsPlace(string place) => new() { ComponentStartsPlace = place };

    public static Seed TheComponentsBetaConstructor(string constructor) => new() { ComponentBetaConstructor = constructor };

    public static Seed TheRecordEndpointsCount(int count) => new() { RecordCount = count };

    public Seed AndResident(string name)
    {
        Residents.Add(name);
        return this;
    }
}
