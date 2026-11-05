namespace Alis.Reactive.Framework.App.Validation;

public sealed class RoomMove
{
    public string? Resident { get; set; } = "Ada";

    public string? Room { get; set; } = "E-14";

    public string? Bed { get; set; }

    public RoomMoveEscort Escort { get; set; } = new();

    public List<string?> Boxes { get; set; } = new() { "Box-1", "Box-2" };
}

public sealed class RoomMoveEscort
{
    public string? Badge { get; set; } = "V-1";
}

public enum RoomMoveDependents
{
    None,
    AfterMustAsync,
    AfterCustomAsync,
    UnderWhenAsync,
}

public static class RoomBook
{
    public static async Task<bool> IsTakenAsync(string? room, CancellationToken cancellation)
    {
        await Task.Yield();
        cancellation.ThrowIfCancellationRequested();
        return room == "E-12";
    }

    public static async Task<bool> IsRevokedAsync(string? badge, CancellationToken cancellation)
    {
        await Task.Yield();
        return badge == "V-0";
    }

    public static async Task<bool> HasRoomForAsync(string? box, CancellationToken cancellation)
    {
        await Task.Yield();
        return box != "Box-9";
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
        RuleFor(x => x.Escort).SetValidator(new RoomMoveEscortRules());
        RuleForEach(x => x.Boxes).CustomAsync(async (box, context, cancellation) =>
        {
            if (!await RoomBook.HasRoomForAsync(box, cancellation))
                context.AddFailure($"The store has no room for {box}.");
        });
    }
}

public sealed class RoomMoveEscortRules : ReactiveValidator<RoomMoveEscort>
{
    public RoomMoveEscortRules()
    {
        RuleFor(x => x.Badge).CustomAsync(async (badge, context, cancellation) =>
        {
            if (await RoomBook.IsRevokedAsync(badge, cancellation))
                context.AddFailure($"Badge {badge} is revoked.");
        });
    }
}

public sealed class EscortedRoomMoveRules : ReactiveValidator<RoomMove>
{
    public EscortedRoomMoveRules()
    {
        RuleFor(x => x.Escort).SetValidator(new RoomMoveEscortRules());
    }
}

public sealed class RoomMoveAfterMustAsyncRules : ReactiveValidator<RoomMove>
{
    public RoomMoveAfterMustAsyncRules()
    {
        RuleFor(x => x.Room).MustAsync(async (room, cancellation) => !await RoomBook.IsTakenAsync(room, cancellation)).WithMessage("That room is taken.")
            .DependentRules(() => RuleFor(x => x.Bed).NotEmpty());
    }
}

public sealed class RoomMoveAfterCustomAsyncRules : ReactiveValidator<RoomMove>
{
    public RoomMoveAfterCustomAsyncRules()
    {
        RuleFor(x => x.Room).CustomAsync(async (room, context, cancellation) =>
        {
            if (await RoomBook.IsTakenAsync(room, cancellation))
                context.AddFailure($"Room {room} is taken.");
        }).DependentRules(() => RuleFor(x => x.Bed).NotEmpty());
    }
}

public sealed class RoomMoveUnderWhenAsyncRules : ReactiveValidator<RoomMove>
{
    public RoomMoveUnderWhenAsyncRules()
    {
        WhenAsync((move, cancellation) => RoomBook.IsSharedAsync(move.Room, cancellation), () =>
            RuleFor(x => x.Room).NotEmpty().DependentRules(() => RuleFor(x => x.Bed).NotEmpty()));
    }
}
