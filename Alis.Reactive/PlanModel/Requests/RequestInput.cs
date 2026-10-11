
namespace Alis.Reactive.PlanModel
{
    /// <summary>Wire base for request input strategies authored through gather builders.</summary>
    internal abstract class RequestInput
    {
        private protected RequestInput() { }

        internal static RequestInput None { get; } = new NoRequestInput();
    }

    /// <summary>Represents a request with no authored input.</summary>
    internal sealed class NoRequestInput : RequestInput
    {
    }

}
