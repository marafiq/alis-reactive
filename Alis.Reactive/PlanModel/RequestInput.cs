using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    /// <summary>Base class for request body strategies. Not constructed in application code.</summary>
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<RequestInput>))]
    public abstract class RequestInput
    {
        private protected RequestInput() { }

        internal static RequestInput None { get; } = new NoRequestInput();
    }

    /// <summary>Represents a request with no body or gathered input.</summary>
    public sealed class NoRequestInput : RequestInput
    {
        /// <summary>Gets the kind. Always <c>"none"</c>.</summary>
        public string Kind => "none";
    }

    [JsonConverter(typeof(GatherInputJsonConverter))]
    internal sealed class GatherInput : RequestInput
    {
        private readonly GatherPayloadFieldList _declaredFields;
        private readonly GatherPayloadFieldList _registeredInputFields;
        private readonly RequestTransport _transport;
        private readonly SupplementalGatherFields _supplementalFields;
        private readonly GatherSelection _selection;

        public string Kind => "gather";
        public IReadOnlyList<GatherPayloadField> DeclaredFields => _declaredFields.ForJson;
        public IReadOnlyList<GatherPayloadField> RegisteredInputFields => _registeredInputFields.ForJson;
        public string Transport => _transport.Value;
        public SupplementalGatherFields SupplementalFields => _supplementalFields;
        public GatherSelection Selection => _selection;

        private GatherInput(
            GatherPayloadFieldList declaredFields,
            GatherPayloadFieldList registeredInputFields,
            RequestTransport transport,
            SupplementalGatherFields supplementalFields,
            GatherSelection selection)
        {
            _declaredFields = declaredFields ?? throw new System.ArgumentNullException(nameof(declaredFields));
            _registeredInputFields = registeredInputFields ?? throw new System.ArgumentNullException(nameof(registeredInputFields));
            _transport = transport ?? throw new System.ArgumentNullException(nameof(transport));
            _supplementalFields = supplementalFields ?? throw new System.ArgumentNullException(nameof(supplementalFields));
            _selection = selection ?? throw new System.ArgumentNullException(nameof(selection));
        }

        internal static GatherInput From(
            IEnumerable<GatherPayloadField> declaredFields,
            IEnumerable<GatherPayloadField> registeredInputFields,
            RequestTransport transport,
            SupplementalGatherFields supplementalFields,
            GatherSelection selection) =>
            new GatherInput(
                GatherPayloadFieldList.From(declaredFields),
                GatherPayloadFieldList.From(registeredInputFields),
                transport,
                supplementalFields,
                selection);
    }

    internal sealed class GatherInputJsonConverter : JsonConverter<GatherInput>
    {
        public override void Write(Utf8JsonWriter writer, GatherInput value, JsonSerializerOptions options)
        {
            if (value == null) throw new System.ArgumentNullException(nameof(value));

            writer.WriteStartObject();
            writer.WriteString("kind", value.Kind);
            WriteProperty(writer, options, "declaredFields", value.DeclaredFields);
            WriteProperty(writer, options, "registeredInputFields", value.RegisteredInputFields);
            writer.WriteString("transport", value.Transport);
            WriteProperty(writer, options, "supplementalFields", value.SupplementalFields);
            WriteProperty(writer, options, "selection", value.Selection);
            writer.WriteEndObject();
        }

        public override GatherInput Read(
            ref Utf8JsonReader reader,
            System.Type typeToConvert,
            JsonSerializerOptions options) =>
            throw new System.NotSupportedException("Plan types are write-only.");

        private static void WriteProperty<T>(
            Utf8JsonWriter writer,
            JsonSerializerOptions options,
            string name,
            T value)
        {
            writer.WritePropertyName(name);
            JsonSerializer.Serialize(writer, value, options);
        }
    }

    internal sealed class GatherPayloadFieldList
    {
        private readonly IReadOnlyList<GatherPayloadField> _fields;

        private GatherPayloadFieldList(IReadOnlyList<GatherPayloadField> fields)
        {
            _fields = fields;
        }

        internal IReadOnlyList<GatherPayloadField> ForJson => _fields;

        internal static GatherPayloadFieldList From(IEnumerable<GatherPayloadField> fields)
        {
            if (fields == null) throw new System.ArgumentNullException(nameof(fields));

            var snapshot = new List<GatherPayloadField>();
            foreach (var field in fields)
            {
                if (field == null)
                    throw new System.ArgumentException("Gather payload field must not be null.", nameof(fields));

                snapshot.Add(field);
            }

            return new GatherPayloadFieldList(snapshot);
        }
    }

    [JsonConverter(typeof(SupplementalGatherFieldsJsonConverter))]
    internal abstract class SupplementalGatherFields
    {
        private SupplementalGatherFields() { }

        internal static SupplementalGatherFields None { get; } = new NoSupplementalGatherFields();

        public abstract string Kind { get; }
        internal abstract void WritePayload(Utf8JsonWriter writer, JsonSerializerOptions options);

        internal static SupplementalGatherFields From(IEnumerable<GatherPayloadField> fields)
        {
            if (fields == null) throw new System.ArgumentNullException(nameof(fields));

            var fieldList = GatherPayloadFieldList.From(fields);
            var hasNoFields = fieldList.ForJson.Count == 0;
            if (hasNoFields) return None;

            return new DeclaredSupplementalGatherFields(fieldList);
        }

        private sealed class NoSupplementalGatherFields : SupplementalGatherFields
        {
            public override string Kind => "none";

            internal override void WritePayload(Utf8JsonWriter writer, JsonSerializerOptions options)
            {
            }
        }

        private sealed class DeclaredSupplementalGatherFields : SupplementalGatherFields
        {
            private readonly GatherPayloadFieldList _fields;

            internal DeclaredSupplementalGatherFields(GatherPayloadFieldList fields)
            {
                _fields = fields ?? throw new System.ArgumentNullException(nameof(fields));
            }

            public override string Kind => "declared";

            internal override void WritePayload(Utf8JsonWriter writer, JsonSerializerOptions options) =>
                SupplementalGatherFieldsJsonConverter.WriteProperty(writer, options, "fields", _fields.ForJson);
        }
    }

    internal sealed class SupplementalGatherFieldsJsonConverter : JsonConverter<SupplementalGatherFields>
    {
        public override void Write(Utf8JsonWriter writer, SupplementalGatherFields value, JsonSerializerOptions options)
        {
            if (value == null) throw new System.ArgumentNullException(nameof(value));

            writer.WriteStartObject();
            writer.WriteString("kind", value.Kind);
            value.WritePayload(writer, options);
            writer.WriteEndObject();
        }

        public override SupplementalGatherFields Read(
            ref Utf8JsonReader reader,
            System.Type typeToConvert,
            JsonSerializerOptions options) =>
            throw new System.NotSupportedException("Plan types are write-only.");

        internal static void WriteProperty<T>(
            Utf8JsonWriter writer,
            JsonSerializerOptions options,
            string name,
            T value)
        {
            writer.WritePropertyName(name);
            JsonSerializer.Serialize(writer, value, options);
        }
    }

    internal abstract class GatherSelection
    {
        private GatherSelection() { }

        internal static GatherSelection ExplicitFields { get; } = new ExplicitGatherSelection();

        internal static GatherSelection AllRegisteredInputs { get; } = new AllRegisteredInputsGatherSelection();

        public abstract string Kind { get; }

        internal abstract bool MayExpandRegisteredInputsAtRuntime { get; }

        internal void AddBuildTimeRegisteredInputFields(
            List<GatherPayloadField> registeredInputFields,
            PlanBuildContext context,
            GatherPayloadClaims claims)
        {
            if (registeredInputFields == null)
                throw new System.ArgumentNullException(nameof(registeredInputFields));
            if (context == null) throw new System.ArgumentNullException(nameof(context));
            if (claims == null) throw new System.ArgumentNullException(nameof(claims));

            AddBuildTimeRegisteredInputFieldsCore(registeredInputFields, context, claims);
        }

        private protected abstract void AddBuildTimeRegisteredInputFieldsCore(
            List<GatherPayloadField> registeredInputFields,
            PlanBuildContext context,
            GatherPayloadClaims claims);

        private sealed class ExplicitGatherSelection : GatherSelection
        {
            public override string Kind => "explicit";

            internal override bool MayExpandRegisteredInputsAtRuntime => false;

            private protected override void AddBuildTimeRegisteredInputFieldsCore(
                List<GatherPayloadField> registeredInputFields,
                PlanBuildContext context,
                GatherPayloadClaims claims)
            {
            }
        }

        private sealed class AllRegisteredInputsGatherSelection : GatherSelection
        {
            public override string Kind => "all-registered-inputs";

            internal override bool MayExpandRegisteredInputsAtRuntime => true;

            private protected override void AddBuildTimeRegisteredInputFieldsCore(
                List<GatherPayloadField> registeredInputFields,
                PlanBuildContext context,
                GatherPayloadClaims claims)
            {
                var buildTimeFields = BuildTimeRegisteredInputGatherFields.From(registeredInputFields, claims);

                foreach (var registration in context.GetRegisteredComponents())
                    buildTimeFields.AddRegisteredInput(registration);
            }
        }
    }

    internal sealed class GatherPayloadClaims
    {
        private readonly GatherPayloadSlots _payloadSlots;
        private readonly SelectedGatherComponentReads _componentReads;

        private GatherPayloadClaims(
            GatherPayloadSlots payloadSlots,
            SelectedGatherComponentReads componentReads)
        {
            _payloadSlots = payloadSlots ?? throw new System.ArgumentNullException(nameof(payloadSlots));
            _componentReads = componentReads ?? throw new System.ArgumentNullException(nameof(componentReads));
        }

        internal static GatherPayloadClaims From(
            IEnumerable<GatherPayloadField> fields,
            IEnumerable<string> supplementalPayloadPaths)
        {
            if (fields == null) throw new System.ArgumentNullException(nameof(fields));
            if (supplementalPayloadPaths == null)
                throw new System.ArgumentNullException(nameof(supplementalPayloadPaths));

            var fieldSnapshot = new List<GatherPayloadField>(fields);
            var payloadSlots = GatherPayloadSlots.From(fieldSnapshot);
            foreach (var payloadPath in supplementalPayloadPaths)
            {
                if (payloadPath == null)
                    throw new System.ArgumentException(
                        "Supplemental gather payload path must not be null.",
                        nameof(supplementalPayloadPaths));

                payloadSlots.ClaimDeclared(payloadPath);
            }

            return new GatherPayloadClaims(
                payloadSlots,
                SelectedGatherComponentReads.From(fieldSnapshot));
        }

        internal bool TryReserve(Alis.Reactive.ComponentRegistration registration)
        {
            if (registration == null) throw new System.ArgumentNullException(nameof(registration));

            var componentReadWasAlreadySelected = _componentReads.Contains(registration);
            if (componentReadWasAlreadySelected)
                return false;

            return _payloadSlots.TryClaim(registration.BindingPath);
        }
    }

    internal sealed class GatherPayloadSlots
    {
        private readonly List<ClaimedGatherPayloadPath> _claimedPaths;

        private GatherPayloadSlots(List<ClaimedGatherPayloadPath> claimedPaths)
        {
            _claimedPaths = claimedPaths ?? throw new System.ArgumentNullException(nameof(claimedPaths));
        }

        internal static GatherPayloadSlots From(IEnumerable<GatherPayloadField> fields)
        {
            if (fields == null) throw new System.ArgumentNullException(nameof(fields));

            var payloadSlots = new GatherPayloadSlots(new List<ClaimedGatherPayloadPath>());
            foreach (var field in fields)
                payloadSlots.ClaimDeclared(field.PayloadPath);

            return payloadSlots;
        }

        internal void ClaimDeclared(string payloadPath)
        {
            if (payloadPath == null) throw new System.ArgumentNullException(nameof(payloadPath));
            var incoming = ClaimedGatherPayloadPath.From(payloadPath);
            var conflict = FindOverlap(incoming);
            if (conflict != null)
                throw new System.InvalidOperationException(
                    $"Gather payload path '{incoming.Text}' conflicts with already declared payload path '{conflict.Text}'. " +
                    "Use either the parent path or its child paths, not both.");

            _claimedPaths.Add(incoming);
        }

        internal bool TryClaim(string payloadPath)
        {
            if (payloadPath == null) throw new System.ArgumentNullException(nameof(payloadPath));

            var incoming = ClaimedGatherPayloadPath.From(payloadPath);
            var payloadPathAlreadyClaimed = FindOverlap(incoming) != null;
            if (payloadPathAlreadyClaimed)
                return false;

            _claimedPaths.Add(incoming);
            return true;
        }

        private ClaimedGatherPayloadPath? FindOverlap(ClaimedGatherPayloadPath incoming)
        {
            foreach (var claimedPath in _claimedPaths)
            {
                var payloadPathAlreadyClaimed = claimedPath.Overlaps(incoming);
                if (payloadPathAlreadyClaimed)
                    return claimedPath;
            }

            return null;
        }
    }

    internal sealed class ClaimedGatherPayloadPath
    {
        private readonly Path _path;

        private ClaimedGatherPayloadPath(string text, Path path)
        {
            Text = text ?? throw new System.ArgumentNullException(nameof(text));
            _path = path ?? throw new System.ArgumentNullException(nameof(path));
        }

        internal string Text { get; }

        internal static ClaimedGatherPayloadPath From(string payloadPath) =>
            new ClaimedGatherPayloadPath(payloadPath, Path.Parse(payloadPath));

        internal bool Overlaps(ClaimedGatherPayloadPath other)
        {
            if (other == null) throw new System.ArgumentNullException(nameof(other));
            return _path.Overlaps(other._path);
        }
    }

    internal sealed class BuildTimeRegisteredInputGatherFields
    {
        private readonly List<GatherPayloadField> _registeredInputFields;
        private readonly GatherPayloadClaims _claims;

        private BuildTimeRegisteredInputGatherFields(
            List<GatherPayloadField> registeredInputFields,
            GatherPayloadClaims claims)
        {
            _registeredInputFields = registeredInputFields
                ?? throw new System.ArgumentNullException(nameof(registeredInputFields));
            _claims = claims ?? throw new System.ArgumentNullException(nameof(claims));
        }

        internal static BuildTimeRegisteredInputGatherFields From(
            List<GatherPayloadField> registeredInputFields,
            GatherPayloadClaims claims) =>
            new BuildTimeRegisteredInputGatherFields(registeredInputFields, claims);

        internal void AddRegisteredInput(
            KeyValuePair<string, Alis.Reactive.ComponentRegistration> registration)
        {
            var payloadSlotWasReserved = _claims.TryReserve(registration.Value);
            if (!payloadSlotWasReserved)
                return;

            _registeredInputFields.Add(FieldFrom(registration));
        }

        private static GatherPayloadField FieldFrom(
            KeyValuePair<string, Alis.Reactive.ComponentRegistration> registration)
        {
            var component = registration.Value;
            var componentValue = ValueProducer.Read(
                ComponentSource.Of(component.ComponentId),
                component.ValueMember,
                shape: component.Shape);

            return GatherPayloadField.Of(registration.Key, componentValue);
        }
    }

    internal sealed class SelectedGatherComponentReads
    {
        private readonly HashSet<string> _componentKeys;

        private SelectedGatherComponentReads(HashSet<string> componentKeys)
        {
            _componentKeys = componentKeys ?? throw new System.ArgumentNullException(nameof(componentKeys));
        }

        internal static SelectedGatherComponentReads From(IEnumerable<GatherPayloadField> fields)
        {
            if (fields == null) throw new System.ArgumentNullException(nameof(fields));

            var componentKeys = new HashSet<string>(System.StringComparer.Ordinal);
            foreach (var field in fields)
            {
                GatherPayloadFieldComponentRead
                    .From(field)
                    .RecordIn(componentKeys);
            }

            return new SelectedGatherComponentReads(componentKeys);
        }

        internal bool Contains(Alis.Reactive.ComponentRegistration registration)
        {
            if (registration == null) throw new System.ArgumentNullException(nameof(registration));
            return _componentKeys.Contains(registration.ComponentId);
        }
    }

    internal abstract class GatherPayloadFieldComponentRead
    {
        private GatherPayloadFieldComponentRead() { }

        internal static GatherPayloadFieldComponentRead From(GatherPayloadField field)
        {
            if (field == null) throw new System.ArgumentNullException(nameof(field));

            return From(field.Value);
        }

        internal abstract void RecordIn(HashSet<string> componentKeys);

        private static GatherPayloadFieldComponentRead From(ValueProducer value)
        {
            if (!(value is ReadProducer read))
                return NoComponentRead.Instance;

            if (!(read.From is ComponentSource componentSource))
                return NoComponentRead.Instance;

            return new ComponentRead(
                Alis.Reactive.PlanModel.ComponentKey.Of(componentSource.Component));
        }

        private sealed class NoComponentRead : GatherPayloadFieldComponentRead
        {
            internal static NoComponentRead Instance { get; } = new NoComponentRead();

            internal override void RecordIn(HashSet<string> componentKeys)
            {
            }
        }

        private sealed class ComponentRead : GatherPayloadFieldComponentRead
        {
            private readonly ComponentKey _componentKey;

            internal ComponentRead(ComponentKey componentKey)
            {
                _componentKey = componentKey ?? throw new System.ArgumentNullException(nameof(componentKey));
            }

            internal override void RecordIn(HashSet<string> componentKeys)
            {
                if (componentKeys == null) throw new System.ArgumentNullException(nameof(componentKeys));
                componentKeys.Add(_componentKey.Value);
            }
        }
    }

    /// <summary>Sends a single evaluated value as the request body.</summary>
    public sealed class ValueInput : RequestInput
    {
        private readonly RequestTransport _transport;

        /// <summary>Gets the kind. Always <c>"value"</c>.</summary>
        public string Kind => "value";
        /// <summary>Gets the value expression to send as the body.</summary>
        public ObjectProducer Value { get; }
        /// <summary>Gets the transport format (json or form).</summary>
        public string Transport => _transport.Value;

        internal ValueInput(ObjectProducer value, RequestTransport transport)
        {
            Value = value ?? throw new System.ArgumentNullException(nameof(value));
            _transport = transport ?? throw new System.ArgumentNullException(nameof(transport));
        }
    }

    /// <summary>Maps an HTTP payload path to a value expression evaluated at request time.</summary>
    internal sealed class GatherPayloadField
    {
        private readonly BindingPath _payloadPath;

        /// <summary>HTTP payload path (from model binding path or explicit override).</summary>
        public string PayloadPath => _payloadPath.Value;
        /// <summary>How to read the value. Carries source, member, and shape.</summary>
        public ValueProducer Value { get; }

        internal GatherPayloadField(string payloadPath, ValueProducer value)
        {
            _payloadPath = BindingPath.Of(payloadPath);
            Value = value ?? throw new System.ArgumentNullException(nameof(value));
        }

        internal static GatherPayloadField Of(string payloadPath, ValueProducer value)
            => new GatherPayloadField(payloadPath, value);
    }
}
