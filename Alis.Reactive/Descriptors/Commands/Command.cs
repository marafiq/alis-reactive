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
    [JsonDerivedType(typeof(CallVoidMutation), "call-void")]
    [JsonDerivedType(typeof(CallValMutation), "call-val")]
    [JsonDerivedType(typeof(CallArgsMutation), "call-args")]
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

    public sealed class CallVoidMutation : Mutation
    {
        public string Method { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Chain { get; }

        public CallVoidMutation(string method, string? chain = null)
        {
            Method = method;
            Chain = chain;
        }
    }

    public sealed class CallValMutation : Mutation
    {
        public string Method { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Chain { get; }

        public CallValMutation(string method, string? chain = null)
        {
            Method = method;
            Chain = chain;
        }
    }

    public sealed class CallArgsMutation : Mutation
    {
        public string Method { get; }
        public object[] Args { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Chain { get; }

        public CallArgsMutation(string method, object[] args, string? chain = null)
        {
            Method = method;
            Args = args;
            Chain = chain;
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
