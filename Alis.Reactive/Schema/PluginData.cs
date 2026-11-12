using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Schema;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(NonePluginData), "none")]
[JsonDerivedType(typeof(HubPluginData), "hub")]
[JsonDerivedType(typeof(StreamPluginData), "stream")]
[JsonDerivedType(typeof(ModelPluginData), "model")]
[JsonDerivedType(typeof(FormPluginData), "form")]
internal abstract class PluginData
{
    private protected PluginData()
    {
    }
}

internal sealed class NonePluginData : PluginData
{
    internal static readonly NonePluginData Instance = new();

    private NonePluginData()
    {
    }
}

internal sealed class HubPluginData : PluginData
{
    internal HubPluginData(bool offer)
    {
        Offer = offer;
    }

    public bool Offer { get; }
}

internal sealed class StreamPluginData : PluginData
{
    internal StreamPluginData(bool offer)
    {
        Offer = offer;
    }

    public bool Offer { get; }
}

internal sealed class ModelPluginData : PluginData
{
    internal ModelPluginData(IReadOnlyDictionary<string, InputBinding> inputs, IReadOnlyDictionary<string, Container> containers)
    {
        Inputs = inputs;
        Containers = containers;
    }

    public IReadOnlyDictionary<string, InputBinding> Inputs { get; }

    public IReadOnlyDictionary<string, Container> Containers { get; }
}

internal sealed class FormPluginData : PluginData
{
    internal FormPluginData(Registration registration)
    {
        Registration = registration;
    }

    public Registration Registration { get; }
}

internal sealed class InputBinding
{
    internal InputBinding(string name, IReadOnlyList<PathStep> path, Expression value, string committed, string invalidClass, string message)
    {
        Name = name;
        Path = path;
        Value = value;
        Committed = committed;
        InvalidClass = invalidClass;
        Message = message;
    }

    public string Name { get; }

    public IReadOnlyList<PathStep> Path { get; }

    public Expression Value { get; }

    public string Committed { get; }

    public string InvalidClass { get; }

    public string Message { get; }
}

internal sealed class Container
{
    internal Container(string model, ContainerKind kind, string summary, IReadOnlyList<Live> live)
    {
        Model = model;
        Kind = kind;
        Summary = summary;
        Live = live;
    }

    public string Model { get; }

    public ContainerKind Kind { get; }

    public string Summary { get; }

    public IReadOnlyList<Live> Live { get; }
}

[JsonConverter(typeof(ChoiceConverter<ContainerKind>))]
internal sealed class ContainerKind : IChoice
{
    internal static readonly ContainerKind Form = new("form");
    internal static readonly ContainerKind Element = new("element");

    private ContainerKind(string text)
    {
        Text = text;
    }

    public string Text { get; }
}

internal sealed class Live
{
    internal Live(string validator, string ruleSet, Expression body)
    {
        Validator = validator;
        RuleSet = ruleSet;
        Body = body;
    }

    public string Validator { get; }

    public string RuleSet { get; }

    public Expression Body { get; }
}
