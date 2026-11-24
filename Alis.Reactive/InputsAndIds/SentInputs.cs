using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.InputsAndIds;

internal sealed class SentInputs
{
    internal SentInputs(ContainerCapture container, IReadOnlyList<InputCapture> individual)
    {
        Container = container;
        Individual = individual;
    }

    public ContainerCapture Container { get; }

    public IReadOnlyList<InputCapture> Individual { get; }
}

internal sealed class SentInput
{
    internal SentInput(string name, InputEntry entry, InputSent sent)
    {
        Name = name;
        Entry = entry;
        Sent = sent;
    }

    public string Name { get; }

    public InputEntry Entry { get; }

    public InputSent Sent { get; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(Kind))]
[JsonDerivedType(typeof(FieldInputSent))]
[JsonDerivedType(typeof(EnumInputSent))]
[JsonDerivedType(typeof(FilesInputSent))]
internal abstract class InputSent
{
    private protected InputSent()
    {
    }

    public abstract string Kind { get; }
}

internal sealed class FieldInputSent : InputSent
{
    internal FieldInputSent(WireForm form, WhenNothing whenNothing, WhenBlank whenBlank)
    {
        Form = form;
        WhenNothing = whenNothing;
        WhenBlank = whenBlank;
    }

    public override string Kind => "field";

    public WireForm Form { get; }

    public WhenNothing WhenNothing { get; }

    public WhenBlank WhenBlank { get; }
}

internal sealed class EnumInputSent : InputSent
{
    internal EnumInputSent(WireForm form, JsonForm json, WhenNothing whenNothing, WhenBlank whenBlank)
    {
        Form = form;
        Json = json;
        WhenNothing = whenNothing;
        WhenBlank = whenBlank;
    }

    public override string Kind => "enum";

    public WireForm Form { get; }

    public JsonForm Json { get; }

    public WhenNothing WhenNothing { get; }

    public WhenBlank WhenBlank { get; }
}

internal sealed class FilesInputSent : InputSent
{
    internal FilesInputSent()
    {
    }

    public override string Kind => "files";
}

internal sealed class InputEntry
{
    internal InputEntry(string id, InputBinding binding)
    {
        Id = id;
        Binding = binding;
    }

    public string Id { get; }

    public InputBinding Binding { get; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(Kind))]
[JsonDerivedType(typeof(NoneContainerCapture))]
[JsonDerivedType(typeof(MissingContainerCapture))]
[JsonDerivedType(typeof(CapturedContainerCapture))]
internal abstract class ContainerCapture
{
    private protected ContainerCapture()
    {
    }

    public abstract string Kind { get; }
}

internal sealed class NoneContainerCapture : ContainerCapture
{
    internal NoneContainerCapture()
    {
    }

    public override string Kind => "none";
}

internal sealed class MissingContainerCapture : ContainerCapture
{
    internal MissingContainerCapture(string id)
    {
        Id = id;
    }

    public override string Kind => "missing";

    public string Id { get; }
}

internal sealed class CapturedContainerCapture : ContainerCapture
{
    internal CapturedContainerCapture(string id, EvaluatedValue body, IReadOnlyList<SentInput> inputs)
    {
        Id = id;
        Body = body;
        Inputs = inputs;
    }

    public override string Kind => "captured";

    public string Id { get; }

    public EvaluatedValue Body { get; }

    public IReadOnlyList<SentInput> Inputs { get; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(Kind))]
[JsonDerivedType(typeof(MissingInputCapture))]
[JsonDerivedType(typeof(CapturedInputCapture))]
[JsonDerivedType(typeof(LeftOutInputCapture))]
internal abstract class InputCapture
{
    private protected InputCapture()
    {
    }

    public abstract string Kind { get; }
}

internal sealed class MissingInputCapture : InputCapture
{
    internal MissingInputCapture(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public override string Kind => "missing";

    public string Id { get; }

    public string Name { get; }
}

internal sealed class CapturedInputCapture : InputCapture
{
    internal CapturedInputCapture(SentInput input)
    {
        Input = input;
    }

    public override string Kind => "captured";

    public SentInput Input { get; }
}

internal sealed class LeftOutInputCapture : InputCapture
{
    internal LeftOutInputCapture()
    {
    }

    public override string Kind => "leftOut";
}
