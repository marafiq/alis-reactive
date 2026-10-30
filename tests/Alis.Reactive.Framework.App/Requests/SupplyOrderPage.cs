namespace Alis.Reactive.Framework.App.Requests;

/// <summary>
/// A supply order for the wings, posted as form data the way MVC binds a form: a line per item, the city it is
/// delivered to, and the wings it is for, ticked on a checklist.
/// </summary>
public sealed class SupplyOrderPage
{
    public List<SupplyLine> Lines { get; set; } = [];

    public SupplyAddress Address { get; set; } = new();

    public List<string> Wings { get; set; } = [];
}

public sealed class SupplyLine
{
    public string? Sku { get; set; }

    public int Quantity { get; set; }
}

public sealed class SupplyAddress
{
    public string? City { get; set; }
}
