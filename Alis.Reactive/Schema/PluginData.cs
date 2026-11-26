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
    internal HubPluginData(bool offer, IReadOnlyList<int> waits, string client)
    {
        Offer = offer;
        Waits = waits;
        Client = client;
    }

    public bool Offer { get; }

    public IReadOnlyList<int> Waits { get; }

    public string Client { get; }
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
    internal InputBinding(string name, IReadOnlyList<PathStep> path, AsExpression converted, LitExpression blank, Expression sent, string committed, string invalidClass, string message)
    {
        Name = name;
        Path = path;
        Converted = converted;
        Blank = blank;
        Sent = sent;
        Committed = committed;
        InvalidClass = invalidClass;
        Message = message;
    }

    public string Name { get; }

    public IReadOnlyList<PathStep> Path { get; }

    public AsExpression Converted { get; }

    public LitExpression Blank { get; }

    public Expression Sent { get; }

    public string Committed { get; }

    public string InvalidClass { get; }

    public string Message { get; }
}

internal sealed class Container
{
    internal Container(string model, FieldPrefix prefix, string summary, IReadOnlyList<Live> live)
    {
        Model = model;
        Prefix = prefix;
        Summary = summary;
        Live = live;
    }

    public string Model { get; }

    public FieldPrefix Prefix { get; }

    public string Summary { get; }

    public IReadOnlyList<Live> Live { get; }
}

internal sealed class FieldPrefix
{
    internal FieldPrefix(string name, IReadOnlyList<PathStep> path)
    {
        Name = name;
        Path = path;
    }

    public string Name { get; }

    public IReadOnlyList<PathStep> Path { get; }
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
