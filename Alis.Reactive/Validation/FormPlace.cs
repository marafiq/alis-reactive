using System.Text.Json.Serialization;

namespace Alis.Reactive.Validation;

internal sealed class FormPlace
{
    internal FormPlace(FormDestination destination)
    {
        Destination = destination;
    }

    public FormDestination Destination { get; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(Kind))]
[JsonDerivedType(typeof(SummaryFormDestination))]
[JsonDerivedType(typeof(NoticeFormDestination))]
internal abstract class FormDestination
{
    private protected FormDestination()
    {
    }

    public abstract string Kind { get; }
}

internal sealed class SummaryFormDestination : FormDestination
{
    internal SummaryFormDestination(string containerId)
    {
        ContainerId = containerId;
    }

    public override string Kind => "summary";

    public string ContainerId { get; }
}

internal sealed class NoticeFormDestination : FormDestination
{
    internal NoticeFormDestination(string separator)
    {
        Separator = separator;
    }

    public override string Kind => "notice";

    public string Separator { get; }
}
