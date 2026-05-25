using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    internal abstract class RequestGather<TModel> where TModel : class
    {
        internal static RequestGather<TModel> None { get; } =
            new NoRequestGather<TModel>();

        internal static RequestGather<TModel> Configured(GatherBuilder<TModel> builder) =>
            new ConfiguredRequestGather<TModel>(builder);

        internal abstract RequestGatherPlan Resolve(RequestGatherContext context);
    }

    internal sealed class NoRequestGather<TModel> : RequestGather<TModel> where TModel : class
    {
        internal override RequestGatherPlan Resolve(RequestGatherContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return RequestGatherPlan.WithoutPayload(context.Url);
        }
    }

    internal sealed class ConfiguredRequestGather<TModel> : RequestGather<TModel> where TModel : class
    {
        private readonly GatherBuilder<TModel> _builder;

        internal ConfiguredRequestGather(GatherBuilder<TModel> builder)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        internal override RequestGatherPlan Resolve(RequestGatherContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var supplementalPayload = _builder.Draft.ToSupplementalPayload();
            return RequestGatherPlan.From(
                ResolveInput(context, supplementalPayload),
                ResolveParameters(context.Url));
        }

        private RequestInput ResolveInput(
            RequestGatherContext context,
            SupplementalGatherPayload supplementalPayload)
        {
            var fieldSelection = GatherPayloadFieldSelection.From(_builder.Draft, context.BuildContext);
            var gatherInputMustRemainExecutable = fieldSelection.RequiresGatherInput;
            if (gatherInputMustRemainExecutable)
            {
                return fieldSelection.ToInput(
                    context.Transport,
                    supplementalPayload.AsGatherInputFields);
            }

            var requestBodyComesOnlyFromSupplementalFields = supplementalPayload.HasFields;
            if (requestBodyComesOnlyFromSupplementalFields)
                return new ValueInput(
                    supplementalPayload.AsStandaloneRequestBody,
                    context.Transport);

            return RequestInput.None;
        }

        private RequestParameters ResolveParameters(RequestUrl url)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));

            return RequestParameters.From(
                _builder.Draft.HeadersForRequest(),
                _builder.Draft.RouteParametersFor(url));
        }
    }

    internal sealed class RequestGatherContext
    {
        private readonly PlanBuildContext _buildContext;
        private readonly RequestTransport _transport;
        private readonly RequestUrl _url;

        private RequestGatherContext(
            PlanBuildContext buildContext,
            RequestTransport transport,
            RequestUrl url)
        {
            _buildContext = buildContext ?? throw new ArgumentNullException(nameof(buildContext));
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
            _url = url ?? throw new ArgumentNullException(nameof(url));
        }

        internal PlanBuildContext BuildContext => _buildContext;
        internal RequestTransport Transport => _transport;
        internal RequestUrl Url => _url;

        internal static RequestGatherContext For(
            PlanBuildContext buildContext,
            RequestTransport transport,
            RequestUrl url) =>
            new RequestGatherContext(
                buildContext,
                transport,
                url);
    }

    internal sealed class RequestGatherPlan
    {
        private readonly RequestParameters _parameters;

        private RequestGatherPlan(
            RequestInput input,
            RequestParameters parameters)
        {
            Input = input ?? throw new ArgumentNullException(nameof(input));
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        internal RequestInput Input { get; }
        internal RequestParameters Parameters => _parameters;

        internal static RequestGatherPlan WithoutPayload(RequestUrl url)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));

            var routeParameters = RequestRouteTemplate
                .For(url)
                .Bind(new Dictionary<string, ValueProducer>());
            return new RequestGatherPlan(
                RequestInput.None,
                RequestParameters.From(
                    new Dictionary<string, ValueProducer>(),
                    routeParameters));
        }

        internal static RequestGatherPlan From(
            RequestInput input,
            RequestParameters parameters) =>
            new RequestGatherPlan(input, parameters);
    }

    internal sealed class GatherPayloadFieldSelection
    {
        private readonly List<GatherPayloadField> _declaredFields;
        private readonly List<GatherPayloadField> _registeredInputFields;
        private readonly GatherSelection _selection;

        private GatherPayloadFieldSelection(
            List<GatherPayloadField> declaredFields,
            List<GatherPayloadField> registeredInputFields,
            GatherSelection selection)
        {
            _declaredFields = declaredFields ?? throw new ArgumentNullException(nameof(declaredFields));
            _registeredInputFields = registeredInputFields ?? throw new ArgumentNullException(nameof(registeredInputFields));
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
        }

        internal bool HasFields => _declaredFields.Count > 0 || _registeredInputFields.Count > 0;

        internal bool RequiresGatherInput =>
            HasFields || _selection.MayExpandRegisteredInputsAtRuntime;

        internal static GatherPayloadFieldSelection From(
            GatherDraft draft,
            PlanBuildContext context)
        {
            if (draft == null) throw new ArgumentNullException(nameof(draft));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var declaredFields = new List<GatherPayloadField>(draft.DeclaredFields);
            var registeredInputFields = new List<GatherPayloadField>();
            var selection = draft.Selection;
            var claims = GatherPayloadClaims.From(
                declaredFields,
                draft.SupplementalPayloadPaths);
            selection.AddBuildTimeRegisteredInputFields(registeredInputFields, context, claims);

            return new GatherPayloadFieldSelection(
                declaredFields,
                registeredInputFields,
                selection);
        }

        internal GatherInput ToInput(
            RequestTransport transport,
            SupplementalGatherFields supplementalFields)
        {
            if (transport == null) throw new ArgumentNullException(nameof(transport));
            if (supplementalFields == null) throw new ArgumentNullException(nameof(supplementalFields));

            return GatherInput.From(
                _declaredFields,
                _registeredInputFields,
                transport,
                supplementalFields,
                _selection);
        }
    }

    internal sealed class SupplementalGatherPayload
    {
        private readonly IReadOnlyList<GatherPayloadField> _fields;

        private SupplementalGatherPayload(IReadOnlyList<GatherPayloadField> fields)
        {
            _fields = fields ?? throw new ArgumentNullException(nameof(fields));
        }

        internal static SupplementalGatherPayload Empty { get; } =
            new SupplementalGatherPayload(Array.Empty<GatherPayloadField>());

        internal bool HasFields => _fields.Count > 0;

        internal SupplementalGatherFields AsGatherInputFields
        {
            get
            {
                var hasNoSupplementalFields = !HasFields;
                if (hasNoSupplementalFields)
                    return SupplementalGatherFields.None;

                return SupplementalGatherFields.From(_fields);
            }
        }

        internal ObjectProducer AsStandaloneRequestBody
        {
            get
            {
                var hasNoSupplementalFields = !HasFields;
                if (hasNoSupplementalFields)
                    throw new InvalidOperationException(
                        "Cannot build a request body from empty supplemental gather payload.");
                return ValueProducer.Object(CopyFields());
            }
        }

        internal static SupplementalGatherPayload From(IReadOnlyList<GatherPayloadField> fields) =>
            new SupplementalGatherPayload(fields);

        private Dictionary<string, ValueProducer> CopyFields()
        {
            var copy = new Dictionary<string, ValueProducer>();
            foreach (var field in _fields)
                copy[field.PayloadPath] = field.Value;
            return copy;
        }
    }
}
