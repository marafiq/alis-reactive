using System.Text.Json.Serialization;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Guards;

namespace Alis.Reactive.Descriptors.Commands
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(DispatchCommand), "dispatch")]
    [JsonDerivedType(typeof(MutateElementCommand), "mutate-element")]
    [JsonDerivedType(typeof(ValidationErrorsCommand), "validation-errors")]
    [JsonDerivedType(typeof(IntoCommand), "into")]
    public abstract class Command
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guard? When { get; internal set; }
    }

    public sealed class DispatchCommand : Command
    {
        public string Event { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Payload { get; }

        public DispatchCommand(string @event, object? payload = null)
        {
            Event = @event;
            Payload = payload;
        }
    }

    // ── Mutation (discriminated by kind) ──

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(SetPropMutation), "set-prop")]
    [JsonDerivedType(typeof(CallMutation), "call")]
    public abstract class Mutation { }

    public sealed class SetPropMutation : Mutation
    {
        public string Prop { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Coerce { get; }

        public SetPropMutation(string prop, string? coerce = null)
        {
            Prop = prop;
            Coerce = coerce;
        }
    }

    // ── MethodArg (discriminated, per-arg resolution) ──

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(LiteralArg), "literal")]
    [JsonDerivedType(typeof(SourceArg), "source")]
    public abstract class MethodArg { }

    public sealed class LiteralArg : MethodArg
    {
        public object Value { get; }

        public LiteralArg(object value)
        {
            Value = value;
        }
    }

    public sealed class SourceArg : MethodArg
    {
        public BindSource Source { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Coerce { get; }

        public SourceArg(BindSource source, string? coerce = null)
        {
            Source = source;
            Coerce = coerce;
        }
    }

    public sealed class CallMutation : Mutation
    {
        public string Method { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Chain { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public MethodArg[]? Args { get; }

        public CallMutation(string method, string? chain = null, MethodArg[]? args = null)
        {
            Method = method;
            Chain = chain;
            Args = args;
        }
    }

    // ── MutateElementCommand ──

    public sealed class MutateElementCommand : Command
    {
        public string Target { get; }
        public Mutation Mutation { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Value { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public BindSource? Source { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Vendor { get; }

        public MutateElementCommand(
            string target,
            Mutation mutation,
            string? value = null,
            BindSource? source = null,
            string? vendor = null)
        {
            Target = target;
            Mutation = mutation;
            Value = value;
            Source = source;
            Vendor = vendor;
        }
    }

    public sealed class ValidationErrorsCommand : Command
    {
        public string FormId { get; }

        public ValidationErrorsCommand(string formId)
        {
            FormId = formId;
        }
    }

    /// <summary>
    /// Injects the HTTP response body as innerHTML of the target element.
    /// Used for loading partial views via GET requests.
    /// </summary>
    public sealed class IntoCommand : Command
    {
        public string Target { get; }

        public IntoCommand(string target)
        {
            Target = target;
        }
    }
}
