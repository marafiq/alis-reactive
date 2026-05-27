using System.Collections.Generic;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    internal sealed class PipelineDraft<TModel> where TModel : class
    {
        private readonly List<Reaction> _segments = new List<Reaction>();
        private readonly List<Reaction> _commands = new List<Reaction>();
        private List<BranchCase>? _branches;
        private int _branchCommandIndex;
        private HttpRequestBuilder<TModel>? _http;
        private ParallelBuilder<TModel>? _parallel;

        internal bool HasPendingCondition => _branches is not null;

        internal HttpRequestBuilder<TModel> BeginHttp(PlanBuildContext context)
        {
            FlushActiveAsyncBlock();
            FlushBranchBlock();
            var builder = new HttpRequestBuilder<TModel>(context);
            _http = builder;
            return builder;
        }

        internal ParallelBuilder<TModel> BeginParallel(PlanBuildContext context)
        {
            FlushActiveAsyncBlock();
            FlushBranchBlock();
            var builder = new ParallelBuilder<TModel>(context);
            _parallel = builder;
            return builder;
        }

        internal void BeginConditional()
        {
            FlushActiveAsyncBlock();
            FlushBranchBlock();
        }

        internal void SetConditionalBranches(List<BranchCase> branches)
        {
            _branches = branches;
            _branchCommandIndex = _commands.Count;
        }

        internal void FlushSegment()
        {
            FlushActiveAsyncBlock();
            FlushBranchBlock();
            FlushCommands();
        }

        internal Reaction BuildReaction()
        {
            FlushSegment();
            return _segments.Count == 1
                ? _segments[0]
                : Reaction.Sequence(_segments);
        }

        internal List<Reaction> BuildReactions()
        {
            return new List<Reaction> { BuildReaction() };
        }

        internal void AddCommand(Reaction reaction)
        {
            FlushActiveAsyncBlock();
            _commands.Add(reaction);
        }

        private void FlushActiveAsyncBlock()
        {
            if (_http is not null)
            {
                FlushCommands();
                _segments.Add(Reaction.Request(_http.BuildRequest()));
                _http = null;
                return;
            }

            if (_parallel is not null)
            {
                FlushCommands();
                _segments.Add(_parallel.BuildReaction());
                _parallel = null;
            }
        }

        private void FlushBranchBlock()
        {
            if (_branches is null)
                return;

            var commands = ConsumeCommands();
            AddSequence(commands, start: 0, count: _branchCommandIndex);
            _segments.Add(Reaction.Branch(_branches));
            AddSequence(commands, start: _branchCommandIndex, count: commands.Count - _branchCommandIndex);

            _branches = null;
            _branchCommandIndex = 0;
        }

        private void FlushCommands()
        {
            if (_commands.Count == 0)
                return;

            _segments.Add(Reaction.Sequence(ConsumeCommands()));
        }

        private List<Reaction> ConsumeCommands()
        {
            var commands = new List<Reaction>(_commands);
            _commands.Clear();
            return commands;
        }

        private void AddSequence(List<Reaction> commands, int start, int count)
        {
            if (count > 0)
                _segments.Add(Reaction.Sequence(commands.GetRange(start, count)));
        }
    }
}
