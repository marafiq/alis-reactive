namespace Alis.Reactive.Framework.App.Validation;

public sealed class RoomMove
{
    public string? Resident { get; set; } = "Ada";

    public string? Room { get; set; } = "E-14";

    public string? Bed { get; set; }
}

public static class RoomBook
{
    public static async Task<bool> IsTakenAsync(string? room, CancellationToken cancellation)
    {
        await Task.Yield();
        return room == "E-12";
    }

    public static async Task<bool> IsSharedAsync(string? room, CancellationToken cancellation)
    {
        await Task.Yield();
        return room == "E-13";
    }
}

public sealed class RoomMoveRules : ReactiveValidator<RoomMove>
{
    public RoomMoveRules()
    {
        RuleFor(x => x.Room).NotEmpty().WithMessage("Name the room.").CustomAsync(async (room, context, cancellation) =>
        {
            if (await RoomBook.IsTakenAsync(room, cancellation))
                context.AddFailure("'{PropertyName}' {PropertyValue} is taken.");
            if (await RoomBook.IsSharedAsync(room, cancellation) && string.IsNullOrWhiteSpace(context.Model.Bed))
                context.AddFailure(x => x.Bed, "Room {PropertyValue} is shared; name the bed.");
        });
    }
}
