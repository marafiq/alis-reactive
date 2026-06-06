using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Pipeline append handle passed to component event-args helper methods.
    /// </summary>
    /// <remarks>
    /// Component packages use this to append event-argument mutations, such as
    /// canceling a vendor event or feeding server-filtered data back into a popup,
    /// to the current Reactive Plan pipeline.
    /// </remarks>
    public interface IReactionEmitter
    {
        /// <summary>Appends a low-level reaction emitted by a component event helper.</summary>
        /// <param name="step">Reaction graph node appended to the owning Reactive Plan pipeline.</param>
        internal void AddStep(ReactionGraph step);
        /// <summary>Plan build context used for component registration.</summary>
        internal PlanBuildContext BuildContext { get; }
    }
}
