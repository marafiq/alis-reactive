import { useMemo } from 'react';
import { useNavigate } from '@tanstack/react-router';
import { cn } from '@/lib/utils';
import { useStories } from '@/hooks/queries';
import { InvestBadges, SizeBadge } from '@/components/ui/badges';
import { Card, CardContent } from '@/components/ui/card';
import type { Story } from '@/lib/types';

// ── Column config ──

const COLUMNS = [
  { key: 'draft', label: 'Draft', countColor: 'bg-gray-200 text-gray-600' },
  { key: 'ready', label: 'Ready', countColor: 'bg-blue-100 text-blue-700' },
  { key: 'in-progress', label: 'In Progress', countColor: 'bg-amber-100 text-amber-700' },
  { key: 'review', label: 'Review', countColor: 'bg-violet-100 text-violet-700' },
  { key: 'done', label: 'Done', countColor: 'bg-emerald-100 text-emerald-700' },
] as const;

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
        'w-full text-left bg-card shadow-sm border border-border rounded-lg p-3 space-y-2',
        'hover:shadow-md hover:-translate-y-0.5 hover:border-primary/30 transition-all duration-150',
        'cursor-pointer focus:outline-none focus:ring-2 focus:ring-ring',
      )}
    >
      <div className="font-semibold text-sm text-foreground leading-snug">
        {story.title}
      </div>
      <div className="flex items-center justify-between gap-2">
        <span className="font-mono text-xs text-muted-foreground truncate">
          {story.id}
        </span>
        <div className="flex items-center gap-1.5 shrink-0">
          <SizeBadge size={story.size} />
          <InvestBadges story={story} size="sm" />
        </div>
      </div>
    </button>
  );
}

// ── Kanban column ──

interface ColumnProps {
  label: string;
  countColor: string;
  stories: Story[];
  onSelectStory: (storyId: string) => void;
}

function Column({ label, countColor, stories, onSelectStory }: ColumnProps) {
  return (
    <div className="flex flex-col min-w-[200px] flex-1">
      {/* Column header */}
      <div className="flex items-center gap-2 mb-3 px-1">
        <h3 className="text-[10px] font-bold uppercase tracking-[0.15em] text-muted-foreground">
          {label}
        </h3>
        <span className={cn(
          'inline-flex items-center justify-center min-w-[18px] h-[18px] rounded-full text-[10px] font-semibold px-1',
          countColor,
        )}>
          {stories.length}
        </span>
      </div>

      {/* Cards */}
      <div className="flex-1 space-y-2 min-h-[120px]">
        {stories.length === 0 && (
          <div className="border border-dashed border-border rounded-lg p-4 flex items-center justify-center min-h-[80px]">
            <span className="text-sm text-muted-foreground">No stories</span>
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

export function Board() {
  const navigate = useNavigate();
  const { data: stories = [], isLoading } = useStories();

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

  function handleSelectStory(storyId: string) {
    navigate({ to: '/stories/$storyId', params: { storyId } });
  }

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
            countColor={col.countColor}
            stories={grouped[col.key]}
            onSelectStory={handleSelectStory}
          />
        ))}
      </div>
    </div>
  );
}
