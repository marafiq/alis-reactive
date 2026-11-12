using System;

namespace Alis.Reactive.PlanContractGenerator;

public sealed class GrammarRefusedException : Exception
{
    public GrammarRefusedException(string message)
        : base(message)
    {
    }
}
