namespace Alis.Reactive.Conformance.World;

public sealed partial class MoveInChange
{
    public bool DepositConnectionRestored { get; set; }

    public decimal? RequiredDeposit { get; set; }

    public int? ImportedDiet { get; set; }
}

public static partial class MoveInChanges
{
    public static Seed TheDepositConnectionIsRestored() => new() { MoveIn = new MoveInChange { DepositConnectionRestored = true } };

    public static Seed TheRequiredDepositChanges(decimal amount) => new() { MoveIn = new MoveInChange { RequiredDeposit = amount } };

    public static Seed AReferralCarriesDiet(int needs) => new() { MoveIn = new MoveInChange { ImportedDiet = needs } };
}

public static partial class CareEndpoints
{
    public const string MoveInDeposits = CarePages.MoveIn + "/deposits";
    public const string MoveInDiet = CarePages.MoveIn + "/diet";
    public const string MoveInMealPlan = CarePages.MoveIns + "meal-plan";
}
