namespace Alis.Reactive.Framework.App.Validation;

/// <summary>A transfer of a resident to another ward.</summary>
public sealed class WardTransfer
{
    public string? Resident { get; set; } = "Ada";

    public string? Ward { get; set; } = "West";

    public string? ApprovedBy { get; set; }
}

/// <summary>The bed book the rules ask while they run: East is full tonight.</summary>
public static class BedBook
{
    public static async Task<bool> IsFullAsync(string? ward, CancellationToken cancellation)
    {
        await Task.Yield();
        return ward == "East";
    }
}

/// <summary>
/// The transfer's rules: the resident and the ward always; an approver only when the bed book, asked while
/// the rules run, says the ward is full. A condition that waits is the server's.
/// </summary>
public sealed class TransferRules : ReactiveValidator<WardTransfer>
{
    public TransferRules()
    {
        RuleFor(x => x.Resident).NotEmpty().WithMessage("Name the resident.");
        RuleFor(x => x.Ward).NotEmpty().WithMessage("Name the ward.");
        WhenAsync((transfer, cancellation) => BedBook.IsFullAsync(transfer.Ward, cancellation), () =>
            RuleFor(x => x.ApprovedBy).NotEmpty().WithMessage("A full ward takes a transfer with an approver's name."));
    }
}
