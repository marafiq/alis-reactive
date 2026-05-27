using System.Collections.Generic;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    internal sealed class PipelineDraft<TModel> where TModel : class
    {
        private readonly List<ReactionGraph> _segments = new List<ReactionGraph>();
        private readonly List<ReactionGraph> _commands = new List<ReactionGraph>();
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

        internal ReactionGraph BuildReaction()
        {
            FlushSegment();
            return _segments.Count == 1
                ? _segments[0]
                : ReactionGraph.Sequence(_segments);
        }

        internal List<ReactionGraph> BuildReactions()
        {
            return new List<ReactionGraph> { BuildReaction() };
        }

        internal void AddCommand(ReactionGraph reaction)
        {
            FlushActiveAsyncBlock();
            _commands.Add(reaction);
        }

        private void FlushActiveAsyncBlock()
        {
            if (_http is not null)
            {
                FlushCommands();
                _segments.Add(ReactionGraph.Request(_http.BuildRequest()));
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
            _segments.Add(ReactionGraph.Branch(_branches));
            AddSequence(commands, start: _branchCommandIndex, count: commands.Count - _branchCommandIndex);

            _branches = null;
            _branchCommandIndex = 0;
        }

        private void FlushCommands()
        {
            if (_commands.Count == 0)
                return;

            _segments.Add(ReactionGraph.Sequence(ConsumeCommands()));
        }

        private List<ReactionGraph> ConsumeCommands()
        {
            var commands = new List<ReactionGraph>(_commands);
            _commands.Clear();
            return commands;
        }

        private void AddSequence(List<ReactionGraph> commands, int start, int count)
        {
            if (count > 0)
                _segments.Add(ReactionGraph.Sequence(commands.GetRange(start, count)));
        }
    }
}
