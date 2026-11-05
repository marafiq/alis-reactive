using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel.Reactions;
using Alis.Reactive.PlanModel.Writing;

namespace Alis.Reactive.PlanModel.Requests
{
    internal sealed class ParallelStep : Step
    {
        private readonly List<RequestStep> _requests = new List<RequestStep>();
        private Block _settled = new Block();
        private bool _settledAuthored;

        internal void Add(RequestStep request) => _requests.Add(request ?? throw new ArgumentNullException(nameof(request)));

        internal void OnAllSettled(Block settled)
        {
            if (_settledAuthored)
                throw new InvalidOperationException("Parallel requests have one OnAllSettled; write every step of it in that one callback.");
            _settledAuthored = true;
            _settled = settled ?? throw new ArgumentNullException(nameof(settled));
        }

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
