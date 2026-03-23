import { useMemo, useState } from 'react';
import { marked } from 'marked';
import hljs from 'highlight.js';
import { cn } from '@/lib/utils';
import { useStory, useReviews, useDispatchReview } from '@/hooks/queries';
import { useWS } from '@/App';
import { ReviewSection } from '@/components/reviews/ReviewSection';
import type { ParsedReview, Dependency } from '@/lib/types';
import {
  parseJson,
  investScores,
  INVEST_LABELS,
  ROLE_NAMES,
  verdictLabel,
  type AgentRole,
  type Story,
} from '@/lib/types';

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

// ── Size badge ──

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

// ── INVEST badges ──

function InvestBadges({ story }: { story: Story }) {
  const scores = investScores(story);
  return (
    <span className="inline-flex gap-0.5">
      {(Object.entries(scores) as [string, boolean][]).map(([letter, pass]) => (
        <span
          key={letter}
          title={INVEST_LABELS[letter]}
          className={cn(
            'inline-flex items-center justify-center w-5 h-5 rounded text-[10px] font-bold leading-none',
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

// ── Dependency status icon ──

function DepStatusIcon({ status }: { status?: string }) {
  if (status === 'done') {
    return (
      <svg width="14" height="14" viewBox="0 0 16 16" className="text-approve shrink-0">
        <circle cx="8" cy="8" r="7" fill="none" stroke="currentColor" strokeWidth="1.5" />
        <path d="M5 8l2 2 4-4" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
      </svg>
    );
  }
  if (status === 'in-progress') {
    return <span className="text-changes shrink-0 text-sm" title="In Progress">&#9203;</span>;
  }
  return (
    <svg width="14" height="14" viewBox="0 0 16 16" className="text-blocker shrink-0">
      <circle cx="8" cy="8" r="7" fill="none" stroke="currentColor" strokeWidth="1.5" />
      <path d="M5 5l6 6M11 5l-6 6" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
    </svg>
  );
}

// ── Agent progress row (during review dispatch) ──

const ALL_ROLES: AgentRole[] = ['architect', 'csharp', 'bdd', 'pm', 'ui', 'human-proxy'];

interface AgentProgressProps {
  progress: Record<string, { status: string; verdict?: string }>;
}

function AgentProgress({ progress }: AgentProgressProps) {
  return (
    <div className="space-y-2 mt-4">
      {ALL_ROLES.map((role) => {
        const entry = progress[role];
        const roleName = ROLE_NAMES[role];
        const status = entry?.status ?? 'waiting';
        const verdict = entry?.verdict;

        return (
          <div
            key={role}
            className="flex items-center gap-3 px-3 py-2 rounded-md bg-muted/50 border border-border"
          >
            {/* Status dot */}
            <span
              className={cn(
                'w-2 h-2 rounded-full shrink-0',
                status === 'done' && 'bg-approve',
                status === 'reviewing' && 'bg-changes animate-pulse',
                status === 'waiting' && 'bg-muted-foreground/30',
                status === 'error' && 'bg-blocker',
              )}
            />

            {/* Role name */}
            <span className="text-sm font-medium text-foreground flex-1">{roleName}</span>

            {/* Status label */}
            <span className="text-[10px] uppercase tracking-wider text-muted-foreground">
              {status}
            </span>

            {/* Verdict (if done) */}
            {verdict && (
              <span
                className={cn(
                  'px-1.5 py-0.5 rounded text-[9px] font-bold uppercase tracking-wider',
                  verdict === 'approve' && 'bg-approve-light text-approve',
                  verdict === 'object' && 'bg-blocker-light text-blocker',
                  verdict === 'approve-with-notes' && 'bg-changes-light text-changes',
                )}
              >
                {verdictLabel(verdict)}
              </span>
            )}
          </div>
        );
      })}
    </div>
  );
}

// ── StoryDetail ──

interface StoryDetailProps {
  storyId: string;
  onSelectStory?: (storyId: string) => void;
  onSelectConcept?: (name: string) => void;
  onOpenReview?: (review: ParsedReview) => void;
}

export function StoryDetail({
  storyId,
  onSelectStory,
  onSelectConcept,
  onOpenReview,
}: StoryDetailProps) {
  const { data: story, isLoading: storyLoading } = useStory(storyId);
  const { data: reviews = [] } = useReviews(storyId);
  const dispatchReview = useDispatchReview();
  const { agentProgress } = useWS();
  const [isDispatching, setIsDispatching] = useState(false);

  const concepts: string[] = useMemo(
    () => (story ? parseJson<string>(story.concepts) : []),
    [story],
  );

  const deps: Dependency[] = story?._dependencies ?? [];
  const hasReviews = reviews.length > 0;
  const isReviewing = isDispatching || Object.keys(agentProgress).length > 0;

  const renderedBody = useMemo(() => {
    if (!story?.body) return '';
    return marked.parse(story.body) as string;
  }, [story?.body]);

  function handleLaunchReview() {
    if (!story) return;
    setIsDispatching(true);
    dispatchReview.mutate(story.id, {
      onSettled: () => setIsDispatching(false),
    });
  }

  if (storyLoading) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="text-muted-foreground text-sm">Loading story...</div>
      </div>
    );
  }

  if (!story) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="text-muted-foreground text-sm">Story not found.</div>
      </div>
    );
  }

  const investAll = investScores(story);
  const allPass = Object.values(investAll).every(Boolean);

  return (
    <div className="max-w-4xl mx-auto px-8 py-8 space-y-8">
      {/* Title */}
      <h1 className="text-2xl font-semibold text-foreground">{story.title}</h1>

      {/* Meta row */}
      <div className="flex flex-wrap items-center gap-2.5">
        <StatusBadge status={story.status} />
        <SizeBadge size={story.size} />
        <InvestBadges story={story} />
        {allPass && (
          <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-approve-light text-approve text-[10px] font-bold uppercase tracking-wider">
            <svg width="10" height="10" viewBox="0 0 12 12" fill="none">
              <path d="M2 6l3 3 5-5" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
            </svg>
            INVEST Validated
          </span>
        )}
      </div>

      {/* Dependencies */}
      {deps.length > 0 && (
        <section>
          <h2 className="text-xs font-bold uppercase tracking-[0.12em] text-muted-foreground mb-2">
            Dependencies
          </h2>
          <div className="flex flex-wrap gap-2">
            {deps.map((dep) => (
              <button
                key={dep.id}
                onClick={() => onSelectStory?.(dep.blocked_by_id)}
                className={cn(
                  'inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs border',
                  'hover:bg-muted transition-colors cursor-pointer',
                  dep.blocked_by_status === 'done'
                    ? 'border-approve/30 text-approve'
                    : dep.blocked_by_status === 'in-progress'
                      ? 'border-changes/30 text-changes'
                      : 'border-blocker/30 text-blocker',
                )}
              >
                <DepStatusIcon status={dep.blocked_by_status} />
                <span className="font-medium">{dep.blocked_by_title || dep.blocked_by_id}</span>
              </button>
            ))}
          </div>
        </section>
      )}

      {/* Concepts */}
      {concepts.length > 0 && (
        <section>
          <h2 className="text-xs font-bold uppercase tracking-[0.12em] text-muted-foreground mb-2">
            Concepts
          </h2>
          <div className="flex flex-wrap gap-1.5">
            {concepts.map((name) => (
              <button
                key={name}
                onClick={() => onSelectConcept?.(name)}
                className={cn(
                  'px-2.5 py-1 rounded-full text-xs font-medium border border-primary/20 text-primary',
                  'hover:bg-primary/5 transition-colors cursor-pointer',
                )}
              >
                {name}
              </button>
            ))}
          </div>
        </section>
      )}

      {/* Story body (markdown) */}
      {story.body && (
        <section>
          <div
            className="prose max-w-none"
            dangerouslySetInnerHTML={{ __html: renderedBody }}
          />
        </section>
      )}

      {/* Reviews section */}
      {hasReviews && onOpenReview && (
        <section className="border-t border-border pt-6">
          <ReviewSection
            reviews={reviews}
            story={story}
            onSelectReview={onOpenReview}
          />
        </section>
      )}

      {/* Launch review (if no reviews yet) */}
      {!hasReviews && (
        <section className="border-t border-border pt-6">
          {!isReviewing ? (
            <button
              onClick={handleLaunchReview}
              disabled={dispatchReview.isPending}
              className={cn(
                'px-5 py-2.5 rounded-lg text-sm font-semibold',
                'bg-primary text-primary-foreground',
                'hover:bg-primary/90 disabled:opacity-50',
                'transition-colors',
              )}
            >
              {dispatchReview.isPending ? 'Dispatching...' : 'Launch INVEST Review'}
            </button>
          ) : (
            <div>
              <h2 className="text-xs font-bold uppercase tracking-[0.15em] text-muted-foreground mb-1">
                Review In Progress
              </h2>
              <p className="text-xs text-muted-foreground mb-3">
                Agents are reviewing this story against INVEST criteria.
              </p>
              <AgentProgress progress={agentProgress} />
            </div>
          )}
        </section>
      )}
    </div>
  );
}
