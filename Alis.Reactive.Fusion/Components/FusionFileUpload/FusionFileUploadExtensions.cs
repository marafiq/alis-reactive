using Alis.Reactive.Builders.Conditions;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Mutation extensions for FusionFileUpload (Value only — no SetValue).
    /// Files are set by user interaction only, not programmatically.
    /// </summary>
    public static class FusionFileUploadExtensions
    {
        private static readonly FusionFileUpload Component = new FusionFileUpload();

        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionFileUpload, TModel> self)
            where TModel : class
            => new TypedComponentSource<string>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
