using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Narrow interface for emitting reactions into a pipeline.
    /// Used by vendor-specific extensions (Fusion, Native) and ComponentRef.
    /// </summary>
    public interface IReactionEmitter
    {
        /// <summary>Adds a reaction step to the current pipeline.</summary>
        void AddStep(ReactionGraph step);
        /// <summary>Plan build context used for component registration.</summary>
        PlanBuildContext BuildContext { get; }
    }
}
