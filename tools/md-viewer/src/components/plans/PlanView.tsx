import { useState, useMemo } from 'react';
import { marked } from 'marked';
import hljs from 'highlight.js';
import { cn } from '@/lib/utils';
import {
  usePlan,
  useStories,
  useUpdatePlan,
  useCreatePlan,
} from '@/hooks/queries';
import type { Plan, Story, Goal } from '@/lib/types';
import { parseJson, investScores, INVEST_LABELS } from '@/lib/types';

// ── Marked config (highlight.js via custom renderer) ──

const renderer = new marked.Renderer();
renderer.code = function ({ text, lang }: { text: string; lang?: string }) {
  const language = lang && hljs.getLanguage(lang) ? lang : undefined;
  const highlighted = language
    ? hljs.highlight(text, { language }).value
    : hljs.highlightAuto(text).value;
  return `<pre><code class="hljs${language ? ` language-${language}` : ''}">${highlighted}</code></pre>`;
};
marked.use({ renderer });

// ── INVEST badge (inline, reusable) ──

function InvestBadges({ story }: { story: Story }) {
  const scores = investScores(story);
  return (
    <span className="inline-flex gap-0.5">
      {(Object.entries(scores) as [string, boolean][]).map(([letter, pass]) => (
        <span
          key={letter}
          title={INVEST_LABELS[letter]}
          className={cn(
            'inline-flex items-center justify-center w-4 h-4 rounded text-[9px] font-bold leading-none',
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

// ── Size badge ──

const SIZE_COLORS: Record<string, string> = {
  S: 'bg-approve-light text-approve',
  M: 'bg-changes-light text-changes',
  L: 'bg-blocker-light text-blocker',
};

function SizeBadge({ size }: { size: string | null }) {
  if (!size) return <span className="text-muted-foreground text-xs">--</span>;
  return (
    <span
      className={cn(
        'inline-block px-1.5 py-0.5 rounded text-[10px] font-bold uppercase tracking-wider',
        SIZE_COLORS[size] ?? 'bg-muted text-muted-foreground',
      )}
    >
      {size}
    </span>
  );
}

// ── Status badge ──

const STATUS_COLORS: Record<string, string> = {
  draft: 'bg-muted text-muted-foreground',
  ready: 'bg-blue-100 text-blue-700',
  'in-progress': 'bg-changes-light text-changes',
  review: 'bg-conflict-light text-conflict',
  done: 'bg-approve-light text-approve',
};

function StatusBadge({ status }: { status: string }) {
  return (
    <span
      className={cn(
        'inline-block px-2 py-0.5 rounded-full text-[10px] font-semibold uppercase tracking-wider',
        STATUS_COLORS[status] ?? 'bg-muted text-muted-foreground',
      )}
    >
      {status}
    </span>
  );
}

// ── Plan Form ──

function PlanForm({ onClose }: { onClose: () => void }) {
  const createPlan = useCreatePlan();
  const [id, setId] = useState('');
  const [title, setTitle] = useState('');
  const [masterPrompt, setMasterPrompt] = useState('');

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!id.trim() || !title.trim()) return;
    createPlan.mutate(
      { id: id.trim(), title: title.trim(), masterPrompt: masterPrompt.trim() || undefined },
      { onSuccess: () => onClose() },
    );
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="border border-border rounded-lg p-4 bg-card space-y-3"
    >
      <h3 className="text-sm font-semibold text-foreground">New Plan</h3>

      <div className="grid grid-cols-2 gap-3">
        <div>
          <label className="block text-xs font-medium text-muted-foreground mb-1">
            Plan ID
          </label>
          <input
            type="text"
            value={id}
            onChange={(e) => setId(e.target.value)}
            placeholder="e.g. reactive-reader-v3"
            className="w-full px-3 py-1.5 text-sm rounded-md border border-input bg-background focus:outline-none focus:ring-2 focus:ring-ring"
          />
        </div>
        <div>
          <label className="block text-xs font-medium text-muted-foreground mb-1">
            Title
          </label>
          <input
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            placeholder="e.g. Reactive Reader v3"
            className="w-full px-3 py-1.5 text-sm rounded-md border border-input bg-background focus:outline-none focus:ring-2 focus:ring-ring"
          />
        </div>
      </div>

      <div>
        <label className="block text-xs font-medium text-muted-foreground mb-1">
          Master Prompt
        </label>
        <textarea
          value={masterPrompt}
          onChange={(e) => setMasterPrompt(e.target.value)}
          rows={3}
          placeholder="Describe the plan's mission..."
          className="w-full px-3 py-1.5 text-sm rounded-md border border-input bg-background focus:outline-none focus:ring-2 focus:ring-ring resize-none"
        />
      </div>

      <div className="flex items-center gap-2 justify-end">
        <button
          type="button"
          onClick={onClose}
          className="px-3 py-1.5 text-xs rounded-md border border-border text-muted-foreground hover:bg-muted transition-colors"
        >
          Cancel
        </button>
        <button
          type="submit"
          disabled={createPlan.isPending || !id.trim() || !title.trim()}
          className="px-3 py-1.5 text-xs rounded-md bg-primary text-primary-foreground hover:bg-primary/90 disabled:opacity-50 transition-colors"
        >
          {createPlan.isPending ? 'Creating...' : 'Create Plan'}
        </button>
      </div>
    </form>
  );
}

// ── PlanView ──

interface PlanViewProps {
  planId: string;
  onSelectStory: (storyId: string) => void;
}

export function PlanView({ planId, onSelectStory }: PlanViewProps) {
  const { data: plan, isLoading: planLoading } = usePlan(planId);
  const { data: stories = [] } = useStories(planId);
  const updatePlan = useUpdatePlan();
  const [showForm, setShowForm] = useState(false);

  const goals: Goal[] = useMemo(
    () => (plan ? parseJson<Goal>(plan.goals) : []),
    [plan],
  );

  const constraints: string[] = useMemo(
    () => (plan ? parseJson<string>(plan.constraints) : []),
    [plan],
  );

  const doneCount = stories.filter((s) => s.status === 'done').length;

  function toggleGoal(idx: number) {
    if (!plan) return;
    const next = goals.map((g, i) =>
      i === idx ? { ...g, done: !g.done } : g,
    );
    updatePlan.mutate({ id: plan.id, goals: JSON.stringify(next) });
  }

  if (planLoading) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="text-muted-foreground text-sm">Loading plan...</div>
      </div>
    );
  }

  if (!plan) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="text-muted-foreground text-sm">Plan not found.</div>
      </div>
    );
  }

  const progressPct = stories.length > 0 ? (doneCount / stories.length) * 100 : 0;

  return (
    <div className="max-w-4xl mx-auto px-8 py-8 space-y-8">
      {/* Header */}
      <div>
        <div className="flex items-start justify-between gap-4">
          <h1 className="text-2xl font-semibold text-foreground">{plan.title}</h1>
          <div className="flex items-center gap-2 shrink-0">
            <StatusBadge status={plan.status} />
            <button
              onClick={() => setShowForm(!showForm)}
              className="px-3 py-1 text-xs rounded-md border border-border text-muted-foreground hover:bg-muted transition-colors"
            >
              + New Plan
            </button>
          </div>
        </div>

        {/* Progress bar */}
        <div className="mt-4">
          <div className="flex items-center justify-between text-xs text-muted-foreground mb-1.5">
            <span>Progress</span>
            <span>
              {doneCount}/{stories.length} stories complete
            </span>
          </div>
          <div className="h-2 w-full rounded-full bg-muted overflow-hidden">
            <div
              className="h-full rounded-full bg-approve transition-all duration-500"
              style={{ width: `${progressPct}%` }}
            />
          </div>
        </div>
      </div>

      {/* Plan Form */}
      {showForm && <PlanForm onClose={() => setShowForm(false)} />}

      {/* Master Prompt */}
      {plan.master_prompt && (
        <div className="rounded-lg bg-[#1a1614] p-6 relative overflow-hidden">
          <div className="absolute top-3 left-5 text-4xl text-primary/30 font-serif select-none leading-none">
            &ldquo;
          </div>
          <div className="relative z-10">
            <div className="text-[10px] font-bold uppercase tracking-[0.15em] text-primary/70 mb-3">
              Master Prompt
            </div>
            <p className="text-[#e8e2da] text-sm leading-relaxed whitespace-pre-wrap">
              {plan.master_prompt}
            </p>
          </div>
        </div>
      )}

      {/* Goals */}
      {goals.length > 0 && (
        <section>
          <h2 className="text-xs font-bold uppercase tracking-[0.12em] text-muted-foreground mb-3">
            Goals
          </h2>
          <ul className="space-y-1.5">
            {goals.map((goal, idx) => (
              <li key={idx} className="flex items-start gap-2.5">
                <button
                  onClick={() => toggleGoal(idx)}
                  className={cn(
                    'mt-0.5 w-4 h-4 rounded border flex items-center justify-center shrink-0 transition-colors',
                    goal.done
                      ? 'bg-approve border-approve text-white'
                      : 'border-border hover:border-primary',
                  )}
                >
                  {goal.done && (
                    <svg width="10" height="8" viewBox="0 0 10 8" fill="none">
                      <path
                        d="M1 4L3.5 6.5L9 1"
                        stroke="currentColor"
                        strokeWidth="1.5"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                      />
                    </svg>
                  )}
                </button>
                <span
                  className={cn(
                    'text-sm leading-relaxed',
                    goal.done && 'line-through opacity-50',
                  )}
                >
                  {goal.text}
                </span>
              </li>
            ))}
          </ul>
        </section>
      )}

      {/* Constraints */}
      {constraints.length > 0 && (
        <section>
          <h2 className="text-xs font-bold uppercase tracking-[0.12em] text-muted-foreground mb-3">
            Constraints
          </h2>
          <ul className="space-y-1">
            {constraints.map((c, idx) => (
              <li key={idx} className="flex items-start gap-2 text-sm">
                <span className="text-primary font-bold shrink-0">&ndash;</span>
                <span>{c}</span>
              </li>
            ))}
          </ul>
        </section>
      )}

      {/* Architecture (D2 Placeholder) */}
      <section>
        <h2 className="text-xs font-bold uppercase tracking-[0.12em] text-muted-foreground mb-3">
          Architecture
        </h2>
        {plan.d2_diagram ? (
          <div
            className="prose max-w-none"
            dangerouslySetInnerHTML={{ __html: marked.parse(plan.d2_diagram) as string }}
          />
        ) : (
          <div className="border-2 border-dashed border-border rounded-lg p-8 bg-muted/50 text-center">
            <pre className="font-mono text-xs text-muted-foreground">
              {`┌─────────────────────────┐
│   D2 Architecture       │
│   Diagram Placeholder   │
│                         │
│   d2_diagram is null    │
└─────────────────────────┘`}
            </pre>
            <p className="mt-3 text-xs text-muted-foreground">
              Add a D2 diagram to the plan to render it here.
            </p>
          </div>
        )}
      </section>

      {/* Story Table */}
      <section>
        <h2 className="text-xs font-bold uppercase tracking-[0.12em] text-muted-foreground mb-3">
          Stories ({stories.length})
        </h2>

        {stories.length === 0 ? (
          <p className="text-sm text-muted-foreground">No stories yet.</p>
        ) : (
          <div className="border border-border rounded-lg overflow-hidden">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-muted/70">
                  <th className="text-left px-4 py-2.5 text-[10px] font-bold uppercase tracking-[0.1em] text-muted-foreground w-[100px]">
                    ID
                  </th>
                  <th className="text-left px-4 py-2.5 text-[10px] font-bold uppercase tracking-[0.1em] text-muted-foreground">
                    Title
                  </th>
                  <th className="text-center px-4 py-2.5 text-[10px] font-bold uppercase tracking-[0.1em] text-muted-foreground w-[60px]">
                    Size
                  </th>
                  <th className="text-center px-4 py-2.5 text-[10px] font-bold uppercase tracking-[0.1em] text-muted-foreground w-[100px]">
                    Status
                  </th>
                  <th className="text-center px-4 py-2.5 text-[10px] font-bold uppercase tracking-[0.1em] text-muted-foreground w-[100px]">
                    INVEST
                  </th>
                </tr>
              </thead>
              <tbody>
                {stories.map((story) => (
                  <tr
                    key={story.id}
                    onClick={() => onSelectStory(story.id)}
                    className="border-t border-border hover:bg-muted/40 cursor-pointer transition-colors"
                  >
                    <td className="px-4 py-2.5 font-mono text-xs text-muted-foreground">
                      {story.id.length > 10
                        ? story.id.slice(0, 10) + '...'
                        : story.id}
                    </td>
                    <td className="px-4 py-2.5 font-medium text-foreground">
                      {story.title}
                    </td>
                    <td className="px-4 py-2.5 text-center">
                      <SizeBadge size={story.size} />
                    </td>
                    <td className="px-4 py-2.5 text-center">
                      <StatusBadge status={story.status} />
                    </td>
                    <td className="px-4 py-2.5 text-center">
                      <InvestBadges story={story} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  );
}
