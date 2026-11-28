namespace Alis.Reactive.Conformance.App.Care;

public sealed record HelenAllergy(string Name, int Code);

public static class HelensAllergies
{
    public static IReadOnlyList<HelenAllergy> Offered() => [new HelenAllergy("Penicillin", 7), new HelenAllergy("Latex", 9), new HelenAllergy("Peanut", 12)];
}
