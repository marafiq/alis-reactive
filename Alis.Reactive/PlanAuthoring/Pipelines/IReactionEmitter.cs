using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>The pipeline a plugin call appends its step to.</summary>
    internal interface IReactionEmitter
    {
        /// <summary>Appends a reaction to the owning pipeline.</summary>
        void AddStep(ReactionGraph step);

        /// <summary>The plan the pipeline belongs to.</summary>
        PlanBuildContext BuildContext { get; }
    }
}
