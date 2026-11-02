using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Grammar;

namespace Alis.Reactive.PlanModel
{
    /// <summary>Requests that start together; the steps after them run once every one has settled.</summary>
    internal sealed class ParallelStep : Step
    {
        private readonly List<RequestStep> _requests = new List<RequestStep>();
        private Block _settled = new Block();

        internal void Add(RequestStep request) => _requests.Add(request ?? throw new ArgumentNullException(nameof(request)));

        internal void OnAllSettled(Block settled) => _settled = settled ?? throw new ArgumentNullException(nameof(settled));

        internal override void WriteTo(List<Statement> block, Scope scope)
        {
            block.Add(G.All(_requests.Select(request =>
            {
                var branch = new List<Statement>();
                request.WriteTo(branch, scope.ForBlock());
                return (IReadOnlyList<Statement>)branch;
            }).ToList()));
            block.AddRange(_settled.Write(scope));
        }
    }
}
