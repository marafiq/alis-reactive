using System.Text.Json.Serialization;

namespace Alis.Reactive.Schema;

[JsonConverter(typeof(ChoiceConverter<PathMembers>))]
internal sealed class PathMembers : IChoice
{
    internal static readonly PathMembers Required = new("required");
    internal static readonly PathMembers Optional = new("optional");
    internal static readonly PathMembers RequiredIfHeld = new("requiredIfHeld");
    internal static readonly PathMembers OptionalMember = new("optionalMember");

    private PathMembers(string text)
    {
        Text = text;
    }

    public string Text { get; }
}

[JsonConverter(typeof(ChoiceConverter<SetMembers>))]
internal sealed class SetMembers : IChoice
{
    internal static readonly SetMembers Required = new("required");
    internal static readonly SetMembers Optional = new("optional");

    private SetMembers(string text)
    {
        Text = text;
    }

    public string Text { get; }
}
