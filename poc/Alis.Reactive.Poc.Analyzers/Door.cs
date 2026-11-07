using Microsoft.CodeAnalysis;

namespace Alis.Reactive.Poc.Analyzers;

internal enum DoorShape
{
    Value,
    Call,
    Itself,
    Event,
}

internal sealed class Door
{
    private Door(string name, DoorShape shape, bool reads, bool writes, bool awaits, bool checksNull, string other)
    {
        Name = name;
        Shape = shape;
        Reads = reads;
        Writes = writes;
        Awaits = awaits;
        ChecksNull = checksNull;
        Other = other;
    }

    public string Name { get; }

    public DoorShape Shape { get; }

    public bool Reads { get; }

    public bool Writes { get; }

    public bool Awaits { get; }

    public bool ChecksNull { get; }

    public string Other { get; }

    public static Door? Of(IMethodSymbol method, Framework framework)
    {
        if (!SymbolEqualityComparer.Default.Equals(method.ContainingType?.OriginalDefinition, framework.JsObject)) return null;
        return method.Name switch
        {
            "ReadOnly" => new Door("ReadOnly", DoorShape.Value, reads: true, writes: false, awaits: false, checksNull: true, ""),
            "Property" => new Door("Property", DoorShape.Value, reads: true, writes: true, awaits: false, checksNull: true, ""),
            "WriteOnly" => new Door("WriteOnly", DoorShape.Value, reads: false, writes: true, awaits: false, checksNull: true, ""),
            "LiveObject" => new Door("LiveObject", DoorShape.Value, reads: true, writes: false, awaits: false, checksNull: false, ""),
            "LiveObjects" => new Door("LiveObjects", DoorShape.Value, reads: true, writes: false, awaits: false, checksNull: false, ""),
            "Callback" => new Door("Callback", DoorShape.Value, reads: false, writes: true, awaits: false, checksNull: false, ""),
            "Method" => new Door("Method", DoorShape.Call, reads: false, writes: false, awaits: false, checksNull: method.IsGenericMethod, "MethodAsync"),
            "MethodAsync" => new Door("MethodAsync", DoorShape.Call, reads: false, writes: false, awaits: true, checksNull: method.IsGenericMethod, "Method"),
            "ReturnedObject" => new Door("ReturnedObject", DoorShape.Call, reads: false, writes: false, awaits: false, checksNull: false, "MethodAsync<TResult>, which reads what the promise resolves to as data; no door awaits a live object"),
            "ReturnedObjects" => new Door("ReturnedObjects", DoorShape.Call, reads: false, writes: false, awaits: false, checksNull: false, "MethodAsync<TResult>, which reads what the promise resolves to as data; no door awaits a live object"),
            "CallItself" => new Door("CallItself", DoorShape.Itself, reads: false, writes: false, awaits: false, checksNull: false, ""),
            "Event" => new Door("Event", DoorShape.Event, reads: false, writes: false, awaits: false, checksNull: false, ""),
            _ => null,
        };
    }
}
