using System.Collections.Generic;
using System.Linq;

namespace Alis.Reactive.PlanModel
{
    internal sealed class GatherInput : RequestInput
    {
        public string Kind => "gather";
        public IReadOnlyList<RequestPayloadAssignment> PayloadAssignments { get; }
        public string BodyFormat => RequestBodyFormat.Value;
        public GatherSourceSelection SourceSelection { get; }

        private RequestBodyFormat RequestBodyFormat { get; }

        private GatherInput(
            IReadOnlyList<RequestPayloadAssignment> payloadAssignments,
            RequestBodyFormat bodyFormat,
            GatherSourceSelection sourceSelection)
        {
            PayloadAssignments = payloadAssignments;
            RequestBodyFormat = bodyFormat;
            SourceSelection = sourceSelection;
        }

        internal static GatherInput From(
            IEnumerable<RequestPayloadAssignment> payloadAssignments,
            RequestBodyFormat bodyFormat,
            GatherSourceSelection sourceSelection) =>
            new GatherInput(
                payloadAssignments.ToList(),
                bodyFormat,
                sourceSelection);
    }

    internal abstract class GatherSourceSelection
    {
        private GatherSourceSelection() { }

        internal static GatherSourceSelection ExplicitPayloadAssignments { get; } = new ExplicitGatherSourceSelection();
        internal static GatherSourceSelection AllRegisteredInputs { get; } = new AllRegisteredInputsGatherSourceSelection();

        public abstract string Kind { get; }
        internal abstract bool SelectsRegisteredInputs { get; }

        private sealed class ExplicitGatherSourceSelection : GatherSourceSelection
        {
            public override string Kind => "explicit";
            internal override bool SelectsRegisteredInputs => false;
        }

        private sealed class AllRegisteredInputsGatherSourceSelection : GatherSourceSelection
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
