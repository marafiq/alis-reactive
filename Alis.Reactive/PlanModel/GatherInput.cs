using System.Collections.Generic;
using System.Linq;

namespace Alis.Reactive.PlanModel
{
    internal sealed class GatherInput : RequestInput
    {
        public string Kind => "gather";
        public IReadOnlyList<RequestPayloadAssignment> Fields { get; }
        public string Transport => RequestTransport.Value;
        public GatherSelection Selection { get; }

        private RequestTransport RequestTransport { get; }

        private GatherInput(
            IReadOnlyList<RequestPayloadAssignment> fields,
            RequestTransport transport,
            GatherSelection selection)
        {
            Fields = fields;
            RequestTransport = transport;
            Selection = selection;
        }

        internal static GatherInput From(
            IEnumerable<RequestPayloadAssignment> fields,
            RequestTransport transport,
            GatherSelection selection) =>
            new GatherInput(
                fields.ToList(),
                transport,
                selection);
    }

    internal abstract class GatherSelection
    {
        private GatherSelection() { }

        internal static GatherSelection ExplicitFields { get; } = new ExplicitGatherSelection();
        internal static GatherSelection AllRegisteredInputs { get; } = new AllRegisteredInputsGatherSelection();

        public abstract string Kind { get; }
        internal abstract bool SelectsRegisteredInputs { get; }

        private sealed class ExplicitGatherSelection : GatherSelection
        {
            public override string Kind => "explicit";
            internal override bool SelectsRegisteredInputs => false;
        }

        private sealed class AllRegisteredInputsGatherSelection : GatherSelection
        {
            public override string Kind => "all-registered-inputs";
            internal override bool SelectsRegisteredInputs => true;
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
