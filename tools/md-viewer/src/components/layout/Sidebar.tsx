import { cn } from '@/lib/utils';
import { parseJson, type Plan, type Story, type Concept, type Goal } from '@/lib/types';
import type { View } from '@/App';

// ── Nav items ──

const NAV_ITEMS: { view: View; icon: string; label: string }[] = [
  { view: 'plans', icon: '\u25C7', label: 'Plans' },
  { view: 'board', icon: '\u2592', label: 'Board' },
  { view: 'knowledge', icon: '\u25CE', label: 'Knowledge' },
  { view: 'files', icon: '\u2261', label: 'Files' },
];

const STATUS_COLORS: Record<string, string> = {
  draft:         'bg-muted-foreground/40',
  ready:         'bg-blue-400',
  'in-progress': 'bg-amber-400',
  review:        'bg-purple-400',
  done:          'bg-emerald-400',
};

const SIZE_STYLES: Record<string, string> = {
  S: 'bg-emerald-500/20 text-emerald-300',
  M: 'bg-amber-500/20 text-amber-300',
  L: 'bg-rose-500/20 text-rose-300',
};

// ── Props ──

interface SidebarProps {
  view: View;
  plans: Plan[];
  stories: Story[];
  concepts: Concept[];
  selectedId: string | null;
  searchQuery: string;
  onSwitchView: (view: View) => void;
  onSelect: (id: string) => void;
  onSearch: (query: string) => void;
}

// ── Component ──

export function Sidebar({
  view,
  plans,
  stories,
  concepts,
  selectedId,
  searchQuery,
  onSwitchView,
  onSelect,
  onSearch,
}: SidebarProps) {
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
        <div className="flex gap-0.5 bg-white/5 rounded-lg p-0.5">
          {NAV_ITEMS.map(({ view: v, icon, label }) => (
            <button
              key={v}
              onClick={() => onSwitchView(v)}
              className={cn(
                'flex-1 flex items-center justify-center gap-1.5 py-1.5 px-2 rounded-md text-[11px] font-medium transition-all duration-150',
                view === v
                  ? 'bg-sidebar-active text-white shadow-sm'
                  : 'text-sidebar-muted hover:text-sidebar-foreground hover:bg-white/5',
              )}
            >
              <span className="text-xs">{icon}</span>
              <span>{label}</span>
            </button>
          ))}
        </div>
      </nav>

      {/* ── Search ── */}
      <div className="px-3 mb-3">
        <div className="relative">
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => onSearch(e.target.value)}
            placeholder="Search..."
            className="w-full bg-white/5 border border-white/10 rounded-md px-3 py-1.5 text-[12px] text-sidebar-foreground placeholder:text-sidebar-muted/60 focus:outline-none focus:border-primary/50 focus:ring-1 focus:ring-primary/30 transition-colors"
          />
          {searchQuery && (
            <button
              onClick={() => onSearch('')}
              className="absolute right-2 top-1/2 -translate-y-1/2 text-sidebar-muted hover:text-sidebar-foreground text-xs"
            >
              \u2715
            </button>
          )}
        </div>
      </div>

      {/* ── Content ── */}
      <div className="flex-1 overflow-y-auto px-3 pb-3">
        {view === 'plans' && (
          <PlansList plans={plans} selectedId={selectedId} query={q} onSelect={onSelect} />
        )}
        {view === 'board' && (
          <StoriesList stories={stories} selectedId={selectedId} query={q} onSelect={onSelect} />
        )}
        {view === 'knowledge' && (
          <ConceptsList concepts={concepts} selectedId={selectedId} query={q} onSelect={onSelect} />
        )}
        {view === 'files' && (
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
  onSelect,
}: {
  plans: Plan[];
  selectedId: string | null;
  query: string;
  onSelect: (id: string) => void;
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
          <button
            key={plan.id}
            onClick={() => onSelect(plan.id)}
            className={cn(
              'w-full text-left px-3 py-2 rounded-lg transition-all duration-150 group',
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
                <span className="flex-shrink-0 text-[10px] font-medium px-1.5 py-0.5 rounded bg-white/10 text-sidebar-foreground tabular-nums">
                  {doneCount}/{goals.length}
                </span>
              )}
            </div>
          </button>
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
  onSelect,
}: {
  stories: Story[];
  selectedId: string | null;
  query: string;
  onSelect: (id: string) => void;
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
          <div className="flex items-center gap-2 px-2 mb-1">
            <span className={cn('w-2 h-2 rounded-full', STATUS_COLORS[status])} />
            <span className="text-[10px] font-semibold uppercase tracking-wider text-sidebar-muted">
              {status.replace('-', ' ')}
            </span>
            <span className="text-[10px] text-sidebar-muted/60">{items.length}</span>
          </div>
          {items.map((story) => {
            const isSelected = selectedId === story.id;
            return (
              <button
                key={story.id}
                onClick={() => onSelect(story.id)}
                className={cn(
                  'w-full text-left px-3 py-1.5 rounded-lg transition-all duration-150 group',
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
                    {story.title}
                  </span>
                  {story.size && (
                    <span className={cn(
                      'flex-shrink-0 text-[9px] font-bold px-1.5 py-0.5 rounded',
                      SIZE_STYLES[story.size] ?? 'bg-white/10 text-sidebar-foreground',
                    )}>
                      {story.size}
                    </span>
                  )}
                </div>
              </button>
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
  onSelect,
}: {
  concepts: Concept[];
  selectedId: string | null;
  query: string;
  onSelect: (id: string) => void;
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
          <button
            key={concept.id}
            onClick={() => onSelect(concept.name)}
            className={cn(
              'w-full text-left px-3 py-1.5 rounded-lg transition-all duration-150 group',
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
          </button>
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
