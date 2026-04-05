using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// A typed source that reads the current value of a component in the browser.
    /// Returned by each component's Value() extension method.
    /// </summary>
    public sealed class TypedComponentSource<TProp> : TypedSource<TProp>
    {
        private readonly string _componentId;
        private readonly string _vendor;
        private readonly string _valueMember;

        internal TypedComponentSource(string componentId, string vendor, string valueMember)
        {
            _componentId = componentId;
            _vendor = vendor;
            _valueMember = valueMember;
        }

        public override ValueProducer ToValueProducer() =>
            ValueProducer.Read(PlanModel.ComponentSource.Of(_componentId), _valueMember, shape: Shape);

        public override PlanModel.ComponentSource ToComponentSource() =>
            PlanModel.ComponentSource.Of(_componentId);

        public override string ReadMember => _valueMember;
    }
}
