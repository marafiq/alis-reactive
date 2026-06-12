using System;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Authoring-time payload typing contract. Records which payload type a
    /// typed registration was authored against so a channel registered twice
    /// can be checked for agreement (<see cref="SameAs"/>). Never serialized:
    /// the runtime resolves authored payload paths directly against the
    /// payload object and has no use for the type name.
    /// </summary>
    internal abstract class PayloadContract
    {
        private protected PayloadContract() { }

        internal static PayloadContract Untyped { get; } = new UntypedPayloadContract();

        internal static PayloadContract Named(string value) => new NamedPayloadContract(value);

        internal static PayloadContract ForPayload(Type payloadType)
        {
            if (payloadType == null) throw new ArgumentNullException(nameof(payloadType));
            return Named(payloadType.FullName
                ?? throw new ArgumentException("Payload type must have a full name.", nameof(payloadType)));
        }

        internal abstract string DisplayName { get; }

        internal abstract bool SameAs(PayloadContract other);
    }

    internal sealed class UntypedPayloadContract : PayloadContract
    {
        internal override string DisplayName => "<untyped>";

        internal override bool SameAs(PayloadContract other) =>
            other is UntypedPayloadContract;
    }

    internal sealed class NamedPayloadContract : PayloadContract
    {
        private readonly PlanString _name;

        internal NamedPayloadContract(string value)
        {
            _name = PayloadTypeName.Of(value);
        }

        internal override string DisplayName => _name.Value;

        internal override bool SameAs(PayloadContract other) =>
            other is NamedPayloadContract named && named._name.Equals(_name);
    }

    internal sealed class PayloadTypeName : PlanString
    {
        private PayloadTypeName(string value) : base(value, nameof(value)) { }

        internal static PayloadTypeName Of(string value) => new PayloadTypeName(value);
    }
}
