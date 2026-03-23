import { useMemo, useState } from 'react';
import { useNavigate } from '@tanstack/react-router';
import {
  DndContext,
  DragOverlay,
  PointerSensor,
  useSensor,
  useSensors,
  closestCenter,
  type DragStartEvent,
  type DragEndEvent,
  type DragOverEvent,
} from '@dnd-kit/core';
import {
  SortableContext,
  verticalListSortingStrategy,
  useSortable,
} from '@dnd-kit/sortable';
import { useDroppable } from '@dnd-kit/core';
import { CSS } from '@dnd-kit/utilities';
import { cn } from '@/lib/utils';
import { useStories, useUpdateStoryStatus } from '@/hooks/queries';
import { SizeBadge } from '@/components/ui/badges';
import { InvestHealthBar } from '@/components/invest/InvestHealthBar';
import { storyToInvestHealth } from '@/components/invest/invest-utils';
import { Card } from '@/components/ui/card';
import { Columns3 } from 'lucide-react';
import type { Story } from '@/lib/types';

// ── Column config ──

const COLUMNS = [
  { key: 'draft', label: 'Draft', dot: 'bg-muted-foreground/40' },
  { key: 'ready', label: 'Ready', dot: 'bg-blue-500' },
  { key: 'in-progress', label: 'In Progress', dot: 'bg-amber-500' },
  { key: 'review', label: 'Review', dot: 'bg-violet-500' },
  { key: 'done', label: 'Done', dot: 'bg-approve' },
] as const;

// ── Draggable story card ──

function SortableStoryCard({ story, onClick }: { story: Story; onClick: () => void }) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: story.id });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
  };

  return (
    <div
      ref={setNodeRef}
      style={style}
      {...attributes}
      {...listeners}
      className={cn(
        'touch-none',
        isDragging && 'opacity-30',
      )}
    >
      <StoryCardContent story={story} onClick={onClick} />
    </div>
  );
}

// ── Card content (shared between sortable + overlay) ──

function StoryCardContent({ story, onClick, isOverlay }: { story: Story; onClick?: () => void; isOverlay?: boolean }) {
  return (
    <button
      onClick={onClick}
      className={cn(
        'w-full text-left focus:outline-none rounded-xl',
        !isOverlay && 'focus:ring-2 focus:ring-ring',
      )}
      tabIndex={isOverlay ? -1 : 0}
    >
      <Card className={cn(
        'transition-all duration-150',
        isOverlay
          ? 'shadow-xl ring-2 ring-primary/40 rotate-[2deg] scale-105'
          : 'hover:-translate-y-0.5 hover:shadow-md hover:ring-primary/30 hover:ring-1',
      )}>
        <div className="px-3.5 py-3 space-y-2">
          <div className="font-medium text-sm text-foreground leading-snug">
            {story.title}
          </div>
          <div className="flex items-center justify-between gap-2">
            <span className="font-mono text-[11px] text-muted-foreground truncate">
              {story.id}
            </span>
            <div className="flex items-center gap-1.5 shrink-0">
              <SizeBadge size={story.size} />
              <InvestHealthBar investHealth={storyToInvestHealth(story)} variant="compact" />
            </div>
          </div>
        </div>
      </Card>
    </button>
  );
}

// ── Droppable column ──

function DroppableColumn({
  columnKey,
  label,
  dot,
  stories,
  onSelectStory,
  isOver,
}: {
  columnKey: string;
  label: string;
  dot: string;
  stories: Story[];
  onSelectStory: (storyId: string) => void;
  isOver: boolean;
}) {
  const { setNodeRef } = useDroppable({ id: columnKey });

  return (
    <div className="flex flex-col min-w-[220px] flex-1">
      {/* Column header */}
      <div className="flex items-center gap-2.5 mb-3 px-1">
        <span className={cn('w-2 h-2 rounded-full', dot)} />
        <h3 className="section-heading !mb-0">{label}</h3>
        <span className="text-[11px] text-muted-foreground/60 tabular-nums">
          {stories.length}
        </span>
      </div>

      {/* Drop zone */}
      <div
        ref={setNodeRef}
        className={cn(
          'flex-1 space-y-2.5 min-h-[120px] rounded-xl p-1.5 -m-1.5 transition-colors duration-200',
          isOver && 'bg-primary/5 ring-2 ring-primary/20 ring-dashed',
        )}
      >
        <SortableContext
          items={stories.map((s) => s.id)}
          strategy={verticalListSortingStrategy}
        >
          {stories.length === 0 && !isOver && (
            <div className="border border-dashed border-border rounded-xl p-4 flex items-center justify-center min-h-[80px]">
              <span className="text-xs text-muted-foreground/60">No stories</span>
            </div>
          )}
          {stories.map((story) => (
            <SortableStoryCard
              key={story.id}
              story={story}
              onClick={() => onSelectStory(story.id)}
            />
          ))}
        </SortableContext>
      </div>
    </div>
  );
}

// ── Board ──

export function Board() {
  const navigate = useNavigate();
  const { data: stories = [], isLoading } = useStories();
  const updateStatus = useUpdateStoryStatus();
  const [activeStory, setActiveStory] = useState<Story | null>(null);
  const [overColumn, setOverColumn] = useState<string | null>(null);

  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: { distance: 5 },
    }),
  );

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

  function findColumnForStory(storyId: string): string | null {
    for (const [col, items] of Object.entries(grouped)) {
      if (items.some((s) => s.id === storyId)) return col;
    }
    return null;
  }

  function handleDragStart(event: DragStartEvent) {
    const story = stories.find((s) => s.id === event.active.id);
    setActiveStory(story ?? null);
  }

  function handleDragOver(event: DragOverEvent) {
    const { over } = event;
    if (!over) {
      setOverColumn(null);
      return;
    }

    // over.id could be a column key or a story id
    const columnKeys = COLUMNS.map((c) => c.key as string);
    if (columnKeys.includes(over.id as string)) {
      setOverColumn(over.id as string);
    } else {
      // It's a story — find which column it belongs to
      setOverColumn(findColumnForStory(over.id as string));
    }
  }

  function handleDragEnd(event: DragEndEvent) {
    const { active, over } = event;
    setActiveStory(null);
    setOverColumn(null);

    if (!over) return;

    const storyId = active.id as string;
    const columnKeys = COLUMNS.map((c) => c.key as string);

    let targetColumn: string | null = null;
    if (columnKeys.includes(over.id as string)) {
      targetColumn = over.id as string;
    } else {
      targetColumn = findColumnForStory(over.id as string);
    }

    if (!targetColumn) return;

    const currentColumn = findColumnForStory(storyId);
    if (currentColumn === targetColumn) return;

    // Optimistically update and call API
    updateStatus.mutate({ id: storyId, status: targetColumn });
  }

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
          <Columns3 className="h-10 w-10 text-muted-foreground/30 mx-auto mb-3" />
          <div className="text-muted-foreground text-sm mb-1">No stories yet</div>
          <p className="text-xs text-muted-foreground/60">
            Create stories in a plan to see them on the board.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="h-full px-8 py-8 overflow-x-auto">
      {/* Page header */}
      <div className="flex items-center gap-3 mb-6">
        <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-primary/10">
          <Columns3 className="h-5 w-5 text-primary" />
        </div>
        <div>
          <h1 className="text-xl font-semibold text-foreground">Story Board</h1>
          <p className="text-sm text-muted-foreground">
            {stories.length} {stories.length === 1 ? 'story' : 'stories'} across {COLUMNS.length} stages
            <span className="text-muted-foreground/50"> &middot; drag to move</span>
          </p>
        </div>
      </div>

      <DndContext
        sensors={sensors}
        collisionDetection={closestCenter}
        onDragStart={handleDragStart}
        onDragOver={handleDragOver}
        onDragEnd={handleDragEnd}
      >
        <div className="flex gap-4 min-w-max">
          {COLUMNS.map((col) => (
            <DroppableColumn
              key={col.key}
              columnKey={col.key}
              label={col.label}
              dot={col.dot}
              stories={grouped[col.key]}
              onSelectStory={handleSelectStory}
              isOver={overColumn === col.key}
            />
          ))}
        </div>

        {/* Drag overlay — floating card that follows cursor */}
        <DragOverlay dropAnimation={null}>
          {activeStory && (
            <div className="w-[220px]">
              <StoryCardContent story={activeStory} isOverlay />
            </div>
          )}
        </DragOverlay>
      </DndContext>
    </div>
  );
}
