namespace Alis.Reactive.Builders.Conditions
{
    public sealed class TypedComponentSource<TProp> : TypedSource<TProp>
    {
        private readonly string _componentId;
        private readonly string _vendor;
        private readonly string _readExpr;

        public TypedComponentSource(string componentId, string vendor, string readExpr)
        {
            _componentId = componentId;
            _vendor = vendor;
            _readExpr = readExpr;
        }

        public override BindSource ToBindSource()
            => new ComponentSource(_componentId, _vendor, _readExpr);
    }
}
