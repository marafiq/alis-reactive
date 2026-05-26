using System.Collections.Generic;
using System.Linq;

namespace Alis.Reactive.PlanModel
{
    internal sealed class GatherInput : RequestInput
    {
        public string Kind => "gather";
        public IReadOnlyList<RequestPayloadAssignment> DeclaredFields { get; }
        public IReadOnlyList<RequestPayloadAssignment> RegisteredInputFields { get; }
        public string Transport => RequestTransport.Value;
        public IReadOnlyList<RequestPayloadAssignment> SupplementalFields { get; }
        public GatherSelection Selection { get; }

        private RequestTransport RequestTransport { get; }

        private GatherInput(
            IReadOnlyList<RequestPayloadAssignment> declaredFields,
            IReadOnlyList<RequestPayloadAssignment> registeredInputFields,
            RequestTransport transport,
            IReadOnlyList<RequestPayloadAssignment> supplementalFields,
            GatherSelection selection)
        {
            DeclaredFields = declaredFields;
            RegisteredInputFields = registeredInputFields;
            RequestTransport = transport;
            SupplementalFields = supplementalFields;
            Selection = selection;
        }

        internal static GatherInput From(
            IEnumerable<RequestPayloadAssignment> declaredFields,
            IEnumerable<RequestPayloadAssignment> registeredInputFields,
            RequestTransport transport,
            IEnumerable<RequestPayloadAssignment> supplementalFields,
            GatherSelection selection) =>
            new GatherInput(
                declaredFields.ToList(),
                registeredInputFields.ToList(),
                transport,
                supplementalFields.ToList(),
                selection);
    }

    internal abstract class GatherSelection
    {
        private GatherSelection() { }

        internal static GatherSelection ExplicitFields { get; } = new ExplicitGatherSelection();
        internal static GatherSelection AllRegisteredInputs { get; } = new AllRegisteredInputsGatherSelection();

        public abstract string Kind { get; }
        internal abstract bool MayExpandRegisteredInputsAtRuntime { get; }

        internal void AddBuildTimeRegisteredInputFields(
            List<RequestPayloadAssignment> registeredInputFields,
            PlanBuildContext context) =>
            AddBuildTimeRegisteredInputFieldsCore(registeredInputFields, context);

        private protected abstract void AddBuildTimeRegisteredInputFieldsCore(
            List<RequestPayloadAssignment> registeredInputFields,
            PlanBuildContext context);

        private sealed class ExplicitGatherSelection : GatherSelection
        {
            public override string Kind => "explicit";
            internal override bool MayExpandRegisteredInputsAtRuntime => false;

            private protected override void AddBuildTimeRegisteredInputFieldsCore(
                List<RequestPayloadAssignment> registeredInputFields,
                PlanBuildContext context)
            {
            }
        }

        private sealed class AllRegisteredInputsGatherSelection : GatherSelection
        {
            public override string Kind => "all-registered-inputs";
            internal override bool MayExpandRegisteredInputsAtRuntime => true;

            private protected override void AddBuildTimeRegisteredInputFieldsCore(
                List<RequestPayloadAssignment> registeredInputFields,
                PlanBuildContext context)
            {
                foreach (var registration in context.GetRegisteredComponents())
                    registeredInputFields.Add(AssignmentFrom(registration.Value));
            }

            private static RequestPayloadAssignment AssignmentFrom(Alis.Reactive.ComponentRegistration component)
            {
                var componentValue = ValueProducer.Read(
                    ComponentSource.Of(component.ComponentId),
                    component.ValueMember,
                    shape: component.Shape);

                return RequestPayloadAssignment.Of(component.RegisteredBindingPath, componentValue);
            }
        }
    }

    internal sealed class RequestPayloadAssignment
    {
        public RequestPayloadTarget Target { get; }
        public ValueProducer Source { get; }

        private RequestPayloadAssignment(RequestPayloadTarget target, ValueProducer source)
        {
            Target = target;
            Source = source;
        }

        internal static RequestPayloadAssignment Of(string payloadPath, ValueProducer source)
            => Of(BindingPath.Of(payloadPath), source);

        internal static RequestPayloadAssignment Of(BindingPath payloadPath, ValueProducer source)
            => new RequestPayloadAssignment(RequestPayloadTarget.For(payloadPath), source);
    }

    public sealed class RequestPayloadTarget
    {
        private readonly BindingPath _path;

        private RequestPayloadTarget(BindingPath path)
        {
            _path = path;
        }

        public string Name => _path.Value;
        public Path Path => _path.Path;

        internal static RequestPayloadTarget For(string path) =>
            For(BindingPath.Of(path));

        internal static RequestPayloadTarget For(BindingPath path) =>
            new RequestPayloadTarget(path);
    }
}
