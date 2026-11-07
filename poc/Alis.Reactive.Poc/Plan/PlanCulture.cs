namespace Alis.Reactive.Plan;

internal sealed class PlanCulture
{
    internal PlanCulture(string collation)
    {
        Collation = collation;
    }

    public string Collation { get; }
}
