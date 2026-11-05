namespace Alis.Reactive.Framework.App.Validation;

public sealed class WardTransfer
{
    public string? Resident { get; set; } = "Ada";

    public string? Ward { get; set; } = "West";

    public string? ApprovedBy { get; set; }

    public string? EscortedBy { get; set; } = "Sam";
}

public static class BedBook
{
    public static async Task<bool> IsFullAsync(string? ward, CancellationToken cancellation)
    {
        await Task.Yield();
        return ward == "East";
    }
}

public sealed class TransferRules : ReactiveValidator<WardTransfer>
{
    public TransferRules()
    {
        RuleFor(x => x.Resident).NotEmpty().WithMessage("Name the resident.");
        RuleFor(x => x.Ward).NotEmpty().WithMessage("Name the ward.");
        WhenAsync((transfer, cancellation) => BedBook.IsFullAsync(transfer.Ward, cancellation), () =>
            RuleFor(x => x.ApprovedBy).NotEmpty().WithMessage("A full ward takes a transfer with an approver's name."));
        UnlessAsync((transfer, cancellation) => BedBook.IsFullAsync(transfer.Ward, cancellation), () =>
            RuleFor(x => x.EscortedBy).NotEmpty().WithMessage("A ward with room takes a transfer with an escort's name."));
    }
}
