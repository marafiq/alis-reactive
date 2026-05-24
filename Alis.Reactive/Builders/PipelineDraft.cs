using System;
using System.Collections.Generic;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    internal sealed class PipelineDraft<TModel> where TModel : class
    {
        private readonly List<Reaction> _segments = new List<Reaction>();
        private readonly PipelineCommandBuffer _commands = new PipelineCommandBuffer();
        private readonly ConditionalBranchDraft _condition = new ConditionalBranchDraft();
        private ActivePipelineSegment<TModel> _active = SequentialPipelineSegment<TModel>.Instance;

        internal bool HasPendingCondition => _condition.HasBranches;
        internal bool HasCommands => !_commands.IsEmpty;

        internal HttpRequestBuilder<TModel> BeginHttp(PlanBuildContext context)
        {
            FlushActiveSegmentWhenNeeded();
            var builder = new HttpRequestBuilder<TModel>(context);
            _active = new HttpPipelineSegment<TModel>(builder);
            return builder;
        }

        internal ParallelBuilder<TModel> BeginParallel(PlanBuildContext context)
        {
            FlushActiveSegmentWhenNeeded();
            var builder = new ParallelBuilder<TModel>(context);
            _active = new ParallelPipelineSegment<TModel>(builder);
            return builder;
        }

        internal void BeginConditional()
        {
            FlushActiveSegmentWhenNeeded();
            _active = ConditionalPipelineSegment<TModel>.Instance;
        }

        internal void SetConditionalBranches(List<BranchCase> branches)
        {
            _condition.Capture(branches, _commands.Count);
        }

        internal void FlushSegment()
        {
            _active.FlushInto(this);
            FlushUnattachedConditionBranch();
            _active = SequentialPipelineSegment<TModel>.Instance;
        }

        internal Reaction BuildReaction()
        {
            var reactions = BuildReactions();
            if (reactions.Count > 1)
                throw new InvalidOperationException(
                    $"BuildReaction() requires exactly one reaction segment but found {reactions.Count}.");
            return reactions[0];
        }

        internal List<Reaction> BuildReactions()
        {
            if (_segments.Count == 0)
                return new List<Reaction> { _active.BuildSingle(this) };

            FlushSegment();
            return _segments;
        }

        internal void AppendSegment(Reaction reaction)
        {
            _segments.Add(reaction ?? throw new ArgumentNullException(nameof(reaction)));
        }

        internal void AddCommand(Reaction reaction)
        {
            _commands.Add(reaction);
        }

        internal Reaction BuildCommandSequence() => _commands.BuildSequence();

        internal List<Reaction> SnapshotCommands() => _commands.Snapshot();

        internal List<Reaction> ConsumeCommands()
        {
            return _commands.Consume();
        }

        internal void FlushCommandsAroundCondition()
        {
            _condition.FlushAround(_commands.Consume(), _segments);
        }

        internal Reaction BuildConditionalReaction()
        {
            return _condition.BuildSingle(_commands.Snapshot());
        }

        private void FlushActiveSegmentWhenNeeded()
        {
            if (!_active.IsSequential)
                FlushSegment();
        }

        private void FlushUnattachedConditionBranch()
        {
            if (!_condition.HasBranches)
                return;

            AppendSegment(_condition.BuildBranch());
            _condition.Clear();
        }
    }

    internal sealed class PipelineCommandBuffer
    {
        private readonly List<Reaction> _commands = new List<Reaction>();

        internal int Count => _commands.Count;
        internal bool IsEmpty => _commands.Count == 0;

        internal void Add(Reaction command)
        {
            _commands.Add(command ?? throw new ArgumentNullException(nameof(command)));
        }

        internal List<Reaction> Snapshot()
        {
            return new List<Reaction>(_commands);
        }

        internal List<Reaction> Consume()
        {
            var snapshot = Snapshot();
            _commands.Clear();
            return snapshot;
        }

        internal Reaction BuildSequence()
        {
            return Reaction.Sequence(Snapshot());
        }
    }

    internal sealed class ConditionalBranchDraft
    {
        private List<BranchCase> _branches = new List<BranchCase>();
        private PipelineCommandBoundary _commandBoundary = PipelineCommandBoundary.Start;

        internal bool HasBranches => _branches.Count > 0;

        internal void Capture(List<BranchCase> branches, int commandBoundary)
        {
            _branches = branches ?? throw new ArgumentNullException(nameof(branches));
            _commandBoundary = PipelineCommandBoundary.At(commandBoundary);
        }

        internal void Clear()
        {
            _branches = new List<BranchCase>();
            _commandBoundary = PipelineCommandBoundary.Start;
        }

        internal Reaction BuildBranch()
        {
            if (!HasBranches)
                throw new InvalidOperationException(
                    "Conditional branch has no executable cases. Call Then(...) after When(...) or Confirm(...).");

            return Reaction.Branch(_branches);
        }

        internal Reaction BuildSingle(List<Reaction> commands)
        {
            var branch = BuildBranch();
            if (commands.Count == 0)
                return branch;

            var split = _commandBoundary.Split(commands);
            var reactions = new List<Reaction>();
            split.AddBeforeTo(reactions);
            reactions.Add(branch);
            split.AddAfterTo(reactions);
            return Reaction.Sequence(reactions);
        }

        internal void FlushAround(List<Reaction> commands, List<Reaction> segments)
        {
            var split = _commandBoundary.Split(commands);
            split.AddBeforeTo(segments);

            if (HasBranches)
            {
                segments.Add(BuildBranch());
                Clear();
            }

            split.AddAfterTo(segments);
            _commandBoundary = PipelineCommandBoundary.Start;
        }
    }

    internal sealed class PipelineCommandBoundary
    {
        private readonly int _index;

        private PipelineCommandBoundary(int index)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            _index = index;
        }

        internal static PipelineCommandBoundary Start { get; } =
            new PipelineCommandBoundary(0);

        internal static PipelineCommandBoundary At(int index) =>
            new PipelineCommandBoundary(index);

        internal BranchCommandSplit Split(List<Reaction> commands)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            if (_index > commands.Count)
                throw new InvalidOperationException(
                    $"Pipeline branch boundary {_index} is outside command count {commands.Count}.");

            return BranchCommandSplit.At(commands, _index);
        }
    }

    internal sealed class BranchCommandSplit
    {
        private readonly List<Reaction> _before;
        private readonly List<Reaction> _after;

        private BranchCommandSplit(List<Reaction> before, List<Reaction> after)
        {
            _before = before ?? throw new ArgumentNullException(nameof(before));
            _after = after ?? throw new ArgumentNullException(nameof(after));
        }

        internal static BranchCommandSplit At(List<Reaction> commands, int branchIndex)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            if (branchIndex < 0 || branchIndex > commands.Count)
                throw new ArgumentOutOfRangeException(nameof(branchIndex));

            return new BranchCommandSplit(
                commands.GetRange(0, branchIndex),
                commands.GetRange(branchIndex, commands.Count - branchIndex));
        }

        internal void AddBeforeTo(List<Reaction> target)
        {
            AddSequenceWhenPresent(_before, target);
        }

        internal void AddAfterTo(List<Reaction> target)
        {
            AddSequenceWhenPresent(_after, target);
        }

        private static void AddSequenceWhenPresent(List<Reaction> commands, List<Reaction> target)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (commands.Count == 0)
                return;

            target.Add(Reaction.Sequence(commands));
        }
    }

    internal abstract class ActivePipelineSegment<TModel> where TModel : class
    {
        internal virtual bool IsSequential => false;

        internal abstract Reaction BuildSingle(PipelineDraft<TModel> draft);

        internal abstract void FlushInto(PipelineDraft<TModel> draft);
    }

    internal sealed class SequentialPipelineSegment<TModel> : ActivePipelineSegment<TModel>
        where TModel : class
    {
        internal static SequentialPipelineSegment<TModel> Instance { get; } =
            new SequentialPipelineSegment<TModel>();

        private SequentialPipelineSegment()
        {
        }

        internal override bool IsSequential => true;

        internal override Reaction BuildSingle(PipelineDraft<TModel> draft)
        {
            return draft.BuildCommandSequence();
        }

        internal override void FlushInto(PipelineDraft<TModel> draft)
        {
            draft.FlushCommandsAroundCondition();
        }
    }

    internal sealed class ConditionalPipelineSegment<TModel> : ActivePipelineSegment<TModel>
        where TModel : class
    {
        internal static ConditionalPipelineSegment<TModel> Instance { get; } =
            new ConditionalPipelineSegment<TModel>();

        private ConditionalPipelineSegment()
        {
        }

        internal override Reaction BuildSingle(PipelineDraft<TModel> draft)
        {
            return draft.BuildConditionalReaction();
        }

        internal override void FlushInto(PipelineDraft<TModel> draft)
        {
            draft.FlushCommandsAroundCondition();
        }
    }

    internal sealed class HttpPipelineSegment<TModel> : ActivePipelineSegment<TModel>
        where TModel : class
    {
        private readonly HttpRequestBuilder<TModel> _builder;

        internal HttpPipelineSegment(HttpRequestBuilder<TModel> builder)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        internal override Reaction BuildSingle(PipelineDraft<TModel> draft)
        {
            var request = _builder.BuildRequest();
            var requestReaction = Reaction.Request(request);
            var hasNoPendingCommands = !draft.HasCommands;
            if (hasNoPendingCommands)
                return requestReaction;

            var reactions = draft.SnapshotCommands();
            reactions.Add(requestReaction);
            return Reaction.Sequence(reactions);
        }

        internal override void FlushInto(PipelineDraft<TModel> draft)
        {
            var request = _builder.BuildRequest();
            var commands = draft.ConsumeCommands();
            var requestHasPreFetchCommands = commands.Count > 0;
            if (requestHasPreFetchCommands)
                request = request.WithBefore(commands);

            draft.AppendSegment(Reaction.Request(request));
        }
    }

    internal sealed class ParallelPipelineSegment<TModel> : ActivePipelineSegment<TModel>
        where TModel : class
    {
        private readonly ParallelBuilder<TModel> _builder;

        internal ParallelPipelineSegment(ParallelBuilder<TModel> builder)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        internal override Reaction BuildSingle(PipelineDraft<TModel> draft)
        {
            return _builder.BuildReaction(draft.SnapshotCommands());
        }

        internal override void FlushInto(PipelineDraft<TModel> draft)
        {
            draft.AppendSegment(_builder.BuildReaction(draft.ConsumeCommands()));
        }
    }
}
