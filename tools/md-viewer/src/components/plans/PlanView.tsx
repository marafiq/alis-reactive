import { useState, useMemo } from 'react';
import { useParams, Link } from '@tanstack/react-router';
import { marked } from 'marked';
import hljs from 'highlight.js';
import { cn } from '@/lib/utils';
import {
  usePlan,
  useStories,
  useUpdatePlan,
  useCreatePlan,
  useD2Render,
} from '@/hooks/queries';
import { SizeBadge, StatusBadge } from '@/components/ui/badges';
import { InvestHealthBar } from '@/components/invest/InvestHealthBar';
import { storyToInvestHealth } from '@/components/invest/invest-utils';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Card, CardContent } from '@/components/ui/card';
import { SectionHeading } from '@/components/ui/section-heading';
import type { Plan, Story, Goal } from '@/lib/types';
import { parseJson } from '@/lib/types';

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
    <Card>
      <CardContent>
        <form onSubmit={handleSubmit} className="space-y-3">
          <h3 className="text-sm font-semibold text-foreground">New Plan</h3>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-medium text-muted-foreground mb-1">
                Plan ID
              </label>
              <Input
                type="text"
                value={id}
                onChange={(e) => setId(e.target.value)}
                placeholder="e.g. reactive-reader-v3"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-muted-foreground mb-1">
                Title
              </label>
              <Input
                type="text"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                placeholder="e.g. Reactive Reader v3"
              />
            </div>
          </div>

          <div>
            <label className="block text-xs font-medium text-muted-foreground mb-1">
              Master Prompt
            </label>
            <Textarea
              value={masterPrompt}
              onChange={(e) => setMasterPrompt(e.target.value)}
              rows={3}
              placeholder="Describe the plan's mission..."
            />
          </div>

          <div className="flex items-center gap-2 justify-end">
            <Button type="button" variant="outline" size="sm" onClick={onClose}>
              Cancel
            </Button>
            <Button
              type="submit"
              size="sm"
              disabled={createPlan.isPending || !id.trim() || !title.trim()}
            >
              {createPlan.isPending ? 'Creating...' : 'Create Plan'}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}

// ── D2 Diagram (renders via server-side d2 CLI) ──

function D2Diagram({ source }: { source: string }) {
  const { data: svg, isLoading, isError } = useD2Render(source);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center rounded-lg border border-border bg-white p-12">
        <div className="flex items-center gap-2 text-muted-foreground text-sm">
          <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24" fill="none">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
          </svg>
          Rendering D2 diagram...
        </div>
      </div>
    );
  }

  if (isError || !svg) {
    return (
      <div className="bg-[#1a1614] text-[#e8e2da] rounded-lg p-6 font-mono text-sm leading-relaxed whitespace-pre-wrap">
        {source}
      </div>
    );
  }

  return (
    <div
      className="rounded-lg border border-border bg-white p-6 overflow-auto"
      dangerouslySetInnerHTML={{ __html: svg }}
    />
  );
}

// ── PlanView ──

export function PlanView() {
  const { planId } = useParams({ from: '/plans/$planId' });
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
            <Button variant="outline" size="sm" onClick={() => setShowForm(!showForm)}>
              + New Plan
            </Button>
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
          <SectionHeading count={goals.length}>Goals</SectionHeading>
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
          <SectionHeading count={constraints.length}>Constraints</SectionHeading>
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

      {/* Architecture (D2 Diagram) */}
      <section>
        <SectionHeading>Architecture</SectionHeading>
        {plan.d2_diagram ? (
          <D2Diagram source={plan.d2_diagram} />
        ) : (
          <div className="border-2 border-dashed border-border rounded-lg p-8 bg-muted/50 text-center">
            <pre className="font-mono text-xs text-muted-foreground">
              {`\u250C\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2510
\u2502   D2 Architecture       \u2502
\u2502   Diagram Placeholder   \u2502
\u2502                         \u2502
\u2502   d2_diagram is null    \u2502
\u2514\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2518`}
            </pre>
            <p className="mt-3 text-xs text-muted-foreground">
              Add a D2 diagram to the plan to render it here.
            </p>
          </div>
        )}
      </section>

      {/* Story Table */}
      <section>
        <SectionHeading count={stories.length}>Stories</SectionHeading>

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
                  <th className="text-center px-4 py-2.5 text-[10px] font-bold uppercase tracking-[0.1em] text-muted-foreground w-[120px]">
                    INVEST
                  </th>
                </tr>
              </thead>
              <tbody>
                {stories.map((story) => (
                  <tr
                    key={story.id}
                    className="border-t border-border hover:bg-muted/40 transition-colors"
                  >
                    <td className="px-4 py-2.5">
                      <Link
                        to="/stories/$storyId"
                        params={{ storyId: story.id }}
                        className="font-mono text-xs text-muted-foreground hover:text-primary transition-colors"
                      >
                        {story.id.length > 10
                          ? story.id.slice(0, 10) + '...'
                          : story.id}
                      </Link>
                    </td>
                    <td className="px-4 py-2.5">
                      <Link
                        to="/stories/$storyId"
                        params={{ storyId: story.id }}
                        className="font-medium text-foreground hover:text-primary transition-colors"
                      >
                        {story.title}
                      </Link>
                    </td>
                    <td className="px-4 py-2.5 text-center">
                      <SizeBadge size={story.size} />
                    </td>
                    <td className="px-4 py-2.5 text-center">
                      <StatusBadge status={story.status} />
                    </td>
                    <td className="px-4 py-2.5 text-center">
                      <InvestHealthBar investHealth={storyToInvestHealth(story)} variant="compact" />
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
