import { useMemo } from 'react';
import { cn } from '@/lib/utils';
import { useStories } from '@/hooks/queries';
import type { Story } from '@/lib/types';
import { investScores, INVEST_LABELS } from '@/lib/types';

// ── Column config ──

const COLUMNS = [
  { key: 'draft', label: 'Draft', color: 'text-muted-foreground' },
  { key: 'ready', label: 'Ready', color: 'text-blue-600' },
  { key: 'in-progress', label: 'In Progress', color: 'text-changes' },
  { key: 'review', label: 'Review', color: 'text-conflict' },
  { key: 'done', label: 'Done', color: 'text-approve' },
] as const;

// ── INVEST badges (tiny) ──

function InvestBadgesTiny({ story }: { story: Story }) {
  const scores = investScores(story);
  return (
    <span className="inline-flex gap-px">
      {(Object.entries(scores) as [string, boolean][]).map(([letter, pass]) => (
        <span
          key={letter}
          title={INVEST_LABELS[letter]}
          className={cn(
            'inline-flex items-center justify-center w-3.5 h-3.5 rounded-sm text-[8px] font-bold leading-none',
            pass
              ? 'bg-approve-light text-approve'
              : 'bg-muted text-muted-foreground',
          )}
        >
          {letter}
        </span>
      ))}
    </span>
  );
}

// ── Size badge (compact) ──

const SIZE_COLORS: Record<string, string> = {
  S: 'bg-approve-light text-approve',
  M: 'bg-changes-light text-changes',
  L: 'bg-blocker-light text-blocker',
};

function SizeBadge({ size }: { size: string | null }) {
  if (!size) return null;
  return (
    <span
      className={cn(
        'inline-block px-1 py-px rounded text-[9px] font-bold uppercase tracking-wider',
        SIZE_COLORS[size] ?? 'bg-muted text-muted-foreground',
      )}
    >
      {size}
    </span>
  );
}

// ── Story card ──

interface StoryCardProps {
  story: Story;
  onClick: () => void;
}

function StoryCard({ story, onClick }: StoryCardProps) {
  return (
    <button
      onClick={onClick}
      className={cn(
        'w-full text-left bg-card border border-border rounded-lg p-3 space-y-2',
        'hover:shadow-md hover:-translate-y-0.5 transition-all duration-150',
        'cursor-pointer focus:outline-none focus:ring-2 focus:ring-ring',
      )}
    >
      <div className="font-medium text-sm text-foreground leading-snug">
        {story.title}
      </div>
      <div className="flex items-center justify-between gap-2">
        <span className="font-mono text-[10px] text-muted-foreground truncate">
          {story.id}
        </span>
        <div className="flex items-center gap-1.5 shrink-0">
          <SizeBadge size={story.size} />
          <InvestBadgesTiny story={story} />
        </div>
      </div>
    </button>
  );
}

// ── Kanban column ──

interface ColumnProps {
  label: string;
  color: string;
  stories: Story[];
  onSelectStory: (storyId: string) => void;
}

function Column({ label, color, stories, onSelectStory }: ColumnProps) {
  return (
    <div className="flex flex-col min-w-[220px] flex-1">
      {/* Column header */}
      <div className="flex items-center gap-2 mb-3 px-1">
        <h3
          className={cn(
            'text-[10px] font-bold uppercase tracking-[0.15em]',
            color,
          )}
        >
          {label}
        </h3>
        <span className="inline-flex items-center justify-center min-w-[18px] h-[18px] rounded-full bg-muted text-muted-foreground text-[10px] font-semibold px-1">
          {stories.length}
        </span>
      </div>

      {/* Cards */}
      <div className="flex-1 space-y-2 min-h-[120px]">
        {stories.length === 0 && (
          <div className="border border-dashed border-border rounded-lg p-4 flex items-center justify-center">
            <span className="text-[11px] text-muted-foreground">No stories</span>
          </div>
        )}
        {stories.map((story) => (
          <StoryCard
            key={story.id}
            story={story}
            onClick={() => onSelectStory(story.id)}
          />
        ))}
      </div>
    </div>
  );
}

// ── Board ──

interface BoardProps {
  planId?: string;
  onSelectStory: (storyId: string) => void;
}

export function Board({ planId, onSelectStory }: BoardProps) {
  const { data: stories = [], isLoading } = useStories(planId);

  const grouped = useMemo(() => {
    const map: Record<string, Story[]> = {
      draft: [],
      ready: [],
      'in-progress': [],
      review: [],
      done: [],
    };
    for (const s of stories) {
      const bucket = map[s.status];
      if (bucket) bucket.push(s);
      else map.draft.push(s);
    }
    return map;
  }, [stories]);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="text-muted-foreground text-sm">Loading board...</div>
      </div>
    );
  }

  if (stories.length === 0) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="text-center">
          <div className="text-muted-foreground text-sm mb-1">No stories yet</div>
          <p className="text-xs text-muted-foreground">
            Create stories in a plan to see them on the board.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="h-full px-6 py-6 overflow-x-auto">
      <div className="flex gap-4 min-w-max">
        {COLUMNS.map((col) => (
          <Column
            key={col.key}
            label={col.label}
            color={col.color}
            stories={grouped[col.key]}
            onSelectStory={onSelectStory}
          />
        ))}
      </div>
    </div>
  );
}
