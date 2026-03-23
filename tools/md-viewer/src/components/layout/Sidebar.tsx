import { useState } from 'react';
import { Link, useRouterState } from '@tanstack/react-router';
import { LayoutDashboard, Columns3, Network, FolderOpen } from 'lucide-react';
import { cn } from '@/lib/utils';
import { usePlans, useStories, useConcepts } from '@/hooks/queries';
import { SizeBadge } from '@/components/ui/badges';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { parseJson, type Plan, type Story, type Concept, type Goal } from '@/lib/types';

// ── Nav items ──

type TabKey = 'plans' | 'board' | 'knowledge' | 'files';

const NAV_ITEMS: { key: TabKey; icon: React.ElementType; label: string; to: string }[] = [
  { key: 'plans', icon: LayoutDashboard, label: 'Plans', to: '/plans' },
  { key: 'board', icon: Columns3, label: 'Board', to: '/board' },
  { key: 'knowledge', icon: Network, label: 'Knowledge', to: '/knowledge' },
  { key: 'files', icon: FolderOpen, label: 'Files', to: '/plans' },
];

const STATUS_COLORS: Record<string, string> = {
  draft:         'bg-muted-foreground/40',
  ready:         'bg-blue-400',
  'in-progress': 'bg-amber-400',
  review:        'bg-purple-400',
  done:          'bg-emerald-400',
};

// ── Determine active tab from current route ──

function useActiveTab(): TabKey {
  const location = useRouterState({ select: (s) => s.location });
  const path = location.pathname;
  if (path.startsWith('/board')) return 'board';
  if (path.startsWith('/stories')) return 'board';
  if (path.startsWith('/knowledge')) return 'knowledge';
  return 'plans';
}

// ── Determine selected IDs from current route ──

function useSelectedIds() {
  const location = useRouterState({ select: (s) => s.location });
  const path = location.pathname;

  let selectedPlanId: string | null = null;
  let selectedStoryId: string | null = null;
  let selectedConceptName: string | null = null;

  const planMatch = path.match(/^\/plans\/(.+)$/);
  if (planMatch) selectedPlanId = decodeURIComponent(planMatch[1]);

  const storyMatch = path.match(/^\/stories\/(.+)$/);
  if (storyMatch) selectedStoryId = decodeURIComponent(storyMatch[1]);

  const conceptMatch = path.match(/^\/knowledge\/(.+)$/);
  if (conceptMatch) selectedConceptName = decodeURIComponent(conceptMatch[1]);

  return { selectedPlanId, selectedStoryId, selectedConceptName };
}

// ── Component ──

export function Sidebar() {
  const [searchQuery, setSearchQuery] = useState('');
  const activeTab = useActiveTab();
  const { selectedPlanId, selectedStoryId, selectedConceptName } = useSelectedIds();
  const { data: plans = [] } = usePlans();
  const { data: stories = [] } = useStories();
  const { data: concepts = [] } = useConcepts();

  const q = searchQuery.toLowerCase();

  return (
    <aside className="w-[260px] flex-shrink-0 bg-sidebar text-sidebar-foreground flex flex-col h-screen select-none">
      {/* ── Header ── */}
      <div className="px-5 pt-5 pb-3">
        <div className="flex items-center gap-3 mb-1">
          <div className="w-8 h-8 rounded-lg bg-primary flex items-center justify-center">
            <span className="text-sm font-bold text-primary-foreground tracking-tight">R</span>
          </div>
          <div>
            <div className="text-[13px] font-semibold text-white leading-tight">Reactive Reader</div>
            <div className="text-[11px] text-sidebar-muted leading-tight">Collaboration Hub</div>
          </div>
        </div>
      </div>

      {/* ── Nav Tabs ── */}
      <nav className="px-3 mb-2">
        <div className="flex gap-1 bg-white/5 rounded-lg p-0.5">
          {NAV_ITEMS.map(({ key, icon: Icon, label, to }) => (
            <Link
              key={key}
              to={to}
              className={cn(
                'flex-1 flex items-center justify-center gap-1.5 py-1.5 px-1.5 rounded-md text-[11px] font-medium transition-all duration-150',
                activeTab === key
                  ? 'bg-white/10 text-white shadow-sm'
                  : 'text-[#6b5f54] hover:text-sidebar-foreground hover:bg-white/5',
              )}
            >
              <Icon size={16} className="shrink-0" />
              <span className="truncate">{label}</span>
            </Link>
          ))}
        </div>
      </nav>

      {/* ── Search ── */}
      <div className="px-3 mb-3">
        <div className="relative">
          <Input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder="Search..."
            className="w-full bg-white/5 border-white/10 text-[12px] text-sidebar-foreground placeholder:text-sidebar-muted/60 h-7 focus-visible:border-primary/50 focus-visible:ring-primary/30"
          />
          {searchQuery && (
            <button
              onClick={() => setSearchQuery('')}
              className="absolute right-2 top-1/2 -translate-y-1/2 text-sidebar-muted hover:text-sidebar-foreground text-xs"
            >
              {'\u2715'}
            </button>
          )}
        </div>
      </div>

      {/* ── Content ── */}
      <div className="flex-1 overflow-y-auto px-3 pb-3">
        {activeTab === 'plans' && (
          <PlansList plans={plans} selectedId={selectedPlanId} query={q} />
        )}
        {activeTab === 'board' && (
          <StoriesList stories={stories} selectedId={selectedStoryId} query={q} />
        )}
        {activeTab === 'knowledge' && (
          <ConceptsList concepts={concepts} selectedId={selectedConceptName} query={q} />
        )}
        {activeTab === 'files' && (
          <div className="px-2 py-8 text-center">
            <p className="text-sidebar-muted text-[12px]">File browser coming soon.</p>
          </div>
        )}
      </div>
    </aside>
  );
}

// ── Plans List ──

function PlansList({
  plans,
  selectedId,
  query,
}: {
  plans: Plan[];
  selectedId: string | null;
  query: string;
}) {
  const filtered = plans.filter((p) => !query || p.title.toLowerCase().includes(query));

  return (
    <div className="space-y-0.5">
      <SectionLabel count={filtered.length}>Plans</SectionLabel>
      {filtered.length === 0 && <EmptyState text="No plans found" />}
      {filtered.map((plan) => {
        const goals = parseJson<Goal>(plan.goals);
        const doneCount = goals.filter((g) => g.done).length;
        const isSelected = selectedId === plan.id;

        return (
          <Link
            key={plan.id}
            to="/plans/$planId"
            params={{ planId: plan.id }}
            className={cn(
              'w-full text-left px-3 py-2 rounded-lg transition-all duration-150 group block',
              isSelected
                ? 'bg-sidebar-active text-white'
                : 'hover:bg-white/5 text-sidebar-foreground',
            )}
          >
            <div className="flex items-start justify-between gap-2">
              <div className="min-w-0 flex-1">
                <div className={cn(
                  'text-[12px] font-medium leading-snug truncate',
                  isSelected ? 'text-white' : 'text-sidebar-foreground group-hover:text-white',
                )}>
                  {plan.title}
                </div>
                <div className="text-[10px] text-sidebar-muted mt-0.5">
                  {plan.status === 'active' ? 'Active' : plan.status}
                </div>
              </div>
              {goals.length > 0 && (
                <Badge variant="secondary" className="flex-shrink-0 text-[10px] font-medium bg-white/10 text-sidebar-foreground border-0 h-auto py-0.5 px-1.5 tabular-nums">
                  {doneCount}/{goals.length}
                </Badge>
              )}
            </div>
          </Link>
        );
      })}
      <NewItemLink label="+ New Plan" />
    </div>
  );
}

// ── Stories List ──

function StoriesList({
  stories,
  selectedId,
  query,
}: {
  stories: Story[];
  selectedId: string | null;
  query: string;
}) {
  const filtered = stories.filter((s) => !query || s.title.toLowerCase().includes(query));

  // Group by status
  const groups: [string, Story[]][] = ['draft', 'ready', 'in-progress', 'review', 'done']
    .map((status) => [status, filtered.filter((s) => s.status === status)] as [string, Story[]])
    .filter(([, items]) => items.length > 0);

  return (
    <div className="space-y-3">
      {groups.length === 0 && <EmptyState text="No stories found" />}
      {groups.map(([status, items]) => (
        <div key={status} className="space-y-0.5">
          <div className="flex items-center gap-2 px-2 mt-2 mb-1.5">
            <span className={cn('w-2 h-2 rounded-full', STATUS_COLORS[status])} />
            <span className="text-[9px] font-bold uppercase tracking-wider text-sidebar-muted">
              {status.replace('-', ' ')}
            </span>
            <span className="text-[9px] text-sidebar-muted/60">{items.length}</span>
          </div>
          {items.map((story) => {
            const isSelected = selectedId === story.id;
            return (
              <Link
                key={story.id}
                to="/stories/$storyId"
                params={{ storyId: story.id }}
                className={cn(
                  'w-full text-left px-3 py-1.5 rounded-lg transition-all duration-150 group block',
                  isSelected
                    ? 'bg-sidebar-active text-white'
                    : 'hover:bg-white/5 text-sidebar-foreground',
                )}
              >
                <div className="flex items-start justify-between gap-2">
                  <span className={cn(
                    'text-[12px] leading-snug line-clamp-2',
                    isSelected ? 'text-white font-medium' : 'group-hover:text-white',
                  )}>
                    {story.title}
                  </span>
                  {story.size && (
                    <SizeBadge size={story.size} className="shrink-0 mt-0.5" />
                  )}
                </div>
              </Link>
            );
          })}
        </div>
      ))}
      <NewItemLink label="+ New Story" />
    </div>
  );
}

// ── Concepts List ──

function ConceptsList({
  concepts,
  selectedId,
  query,
}: {
  concepts: Concept[];
  selectedId: string | null;
  query: string;
}) {
  const filtered = concepts.filter((c) => !query || c.name.toLowerCase().includes(query));
  const sorted = [...filtered].sort((a, b) => b.link_count - a.link_count);

  return (
    <div className="space-y-0.5">
      <SectionLabel count={sorted.length}>Concepts</SectionLabel>
      {sorted.length === 0 && <EmptyState text="No concepts found" />}
      {sorted.map((concept) => {
        const isSelected = selectedId === concept.name;
        return (
          <Link
            key={concept.id}
            to="/knowledge/$concept"
            params={{ concept: concept.name }}
            className={cn(
              'w-full text-left px-3 py-1.5 rounded-lg transition-all duration-150 group block',
              isSelected
                ? 'bg-sidebar-active text-white'
                : 'hover:bg-white/5 text-sidebar-foreground',
            )}
          >
            <div className="flex items-center justify-between gap-2">
              <span className={cn(
                'text-[12px] leading-snug truncate',
                isSelected ? 'text-white font-medium' : 'group-hover:text-white',
              )}>
                {concept.name}
              </span>
              <span className="flex-shrink-0 text-[10px] text-sidebar-muted tabular-nums">
                {concept.link_count} link{concept.link_count !== 1 ? 's' : ''}
              </span>
            </div>
          </Link>
        );
      })}
    </div>
  );
}

// ── Shared Sub-Components ──

function SectionLabel({ children, count }: { children: string; count: number }) {
  return (
    <div className="flex items-center justify-between px-2 mb-1.5">
      <span className="text-[10px] font-semibold uppercase tracking-wider text-sidebar-muted">
        {children}
      </span>
      <span className="text-[10px] text-sidebar-muted/60 tabular-nums">{count}</span>
    </div>
  );
}

function EmptyState({ text }: { text: string }) {
  return (
    <div className="px-2 py-4 text-center">
      <p className="text-[11px] text-sidebar-muted">{text}</p>
    </div>
  );
}

function NewItemLink({ label }: { label: string }) {
  return (
    <button className="w-full text-left px-3 py-1.5 rounded-lg text-[11px] text-sidebar-muted hover:text-primary hover:bg-white/5 transition-colors mt-1">
      {label}
    </button>
  );
}
