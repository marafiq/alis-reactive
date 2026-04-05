using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Narrow interface for emitting reactions into a pipeline.
    /// Used by vendor-specific extensions (Fusion, Native) and ComponentRef.
    /// </summary>
    public interface IReactionEmitter
    {
        void AddStep(Reaction step);
        PlanBuildContext BuildContext { get; }
    }
}
