namespace Alis.Reactive.Framework.App.Inputs;

public sealed class TagsPage
{
    public List<string?> Codes { get; set; } = new List<string?>();
}

public sealed class TagsEcho
{
    public string Summary { get; set; } = "";
}
