namespace Alis.Reactive.Conformance.World;

public static partial class MoveInChanges
{
    public static Seed ThePacketIsSealed() => new() { MoveIn = new MoveInChange { PacketSealed = true } };

    public static Seed ThePacketServiceIsUnavailable() => new() { MoveIn = new MoveInChange { PacketServiceUnavailable = true } };

    public static Seed TheConsentRevisionChanges() => new() { MoveIn = new MoveInChange { ConsentRevisionChanged = true } };

    public static Seed DocumentMetadataIsUnavailable() => new() { MoveIn = new MoveInChange { DocumentMetadataUnavailable = true } };
}

public sealed partial class MoveInChange
{
    public bool PacketSealed { get; set; }

    public bool PacketServiceUnavailable { get; set; }

    public bool ConsentRevisionChanged { get; set; }

    public bool DocumentMetadataUnavailable { get; set; }
}
