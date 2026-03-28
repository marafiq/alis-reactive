using Alis.Reactive.Descriptors.Commands;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Narrow interface for emitting commands into a pipeline.
    /// Used by vendor-specific extensions (Fusion, Native) and ComponentRef
    /// instead of the full PipelineBuilder surface.
    /// </summary>
    public interface ICommandEmitter
    {
        /// <summary>Appends a command to the current pipeline sequence.</summary>
        /// <param name="command">The command descriptor to add.</param>
        void AddCommand(Command command);
    }
}
