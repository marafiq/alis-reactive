import { useMemo, useState, type FormEvent } from 'react';
import { useParams, Link } from '@tanstack/react-router';
import { marked } from 'marked';
import hljs from 'highlight.js';
import { Pencil } from 'lucide-react';
import { cn } from '@/lib/utils';
import { useStory, useReviews, useDispatchReview, useComments, useCreateComment, useDecisions, useCreateDecision, useUpdateStory, useCreateVerdict } from '@/hooks/queries';
import { useWS } from '@/App';
import { useReviewPanel } from '@/components/layout/reviewPanelContext';
import { ReviewSection } from '@/components/reviews/ReviewSection';
import { InvestBadges, SizeBadge, StatusBadge } from '@/components/ui/badges';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { VerdictBar } from '@/components/layout/VerdictBar';
import type { ParsedReview, Dependency } from '@/lib/types';
import {
  parseJson,
  parseReview,
  investScores,
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
                  verdict === 'approve' && 'bg-emerald-100 text-emerald-700',
                  verdict === 'object' && 'bg-red-100 text-red-600',
                  verdict === 'approve-with-notes' && 'bg-amber-100 text-amber-700',
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

export function StoryDetail() {
  const { storyId } = useParams({ from: '/stories/$storyId' });
  const { setReviewPanelData } = useReviewPanel();
  const { data: story, isLoading: storyLoading } = useStory(storyId);
  const { data: rawReviews = [] } = useReviews(storyId);
  const { data: comments = [] } = useComments(storyId);
  const { data: decisions = [] } = useDecisions(storyId);
  const dispatchReview = useDispatchReview();
  const createComment = useCreateComment();
  const createDecision = useCreateDecision();
  const updateStory = useUpdateStory();
  const createVerdict = useCreateVerdict();
  const { agentProgress } = useWS();
  const [isDispatching, setIsDispatching] = useState(false);
  const [commentBody, setCommentBody] = useState('');

  // Decision log form state
  const [showDecisionForm, setShowDecisionForm] = useState(false);
  const [decisionSummary, setDecisionSummary] = useState('');
  const [decisionItems, setDecisionItems] = useState('');

  // Story edit mode state
  const [isEditing, setIsEditing] = useState(false);
  const [editTitle, setEditTitle] = useState('');
  const [editBody, setEditBody] = useState('');
  const [editSize, setEditSize] = useState<string>('');
  const [editConcepts, setEditConcepts] = useState('');
  const [editIndependent, setEditIndependent] = useState('');
  const [editNegotiable, setEditNegotiable] = useState('');
  const [editValuable, setEditValuable] = useState('');
  const [editEstimable, setEditEstimable] = useState('');
  const [editSmall, setEditSmall] = useState('');
  const [editTestable, setEditTestable] = useState('');

  const concepts: string[] = useMemo(
    () => (story ? parseJson<string>(story.concepts) : []),
    [story],
  );

  const deps: Dependency[] = story?._dependencies ?? [];
  const parsedReviews = useMemo(() => rawReviews.map(parseReview), [rawReviews]);
  const hasReviews = rawReviews.length > 0;
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

  function handlePostComment(e: FormEvent) {
    e.preventDefault();
    const trimmed = commentBody.trim();
    if (!trimmed || !story) return;
    createComment.mutate(
      { storyId: story.id, body: trimmed, author: 'user' },
      { onSuccess: () => setCommentBody('') },
    );
  }

  function handleSaveDecision(e: FormEvent) {
    e.preventDefault();
    if (!story || !decisionSummary.trim()) return;
    const keyDecisions = decisionItems
      .split('\n')
      .map((line) => line.trim())
      .filter(Boolean);
    createDecision.mutate(
      { storyId: story.id, summary: decisionSummary.trim(), keyDecisions },
      {
        onSuccess: () => {
          setDecisionSummary('');
          setDecisionItems('');
          setShowDecisionForm(false);
        },
      },
    );
  }

  function startEditing() {
    if (!story) return;
    setEditTitle(story.title);
    setEditBody(story.body || '');
    setEditSize(story.size || '');
    setEditConcepts(parseJson<string>(story.concepts).join(', '));
    setEditIndependent(story.invest_independent || '');
    setEditNegotiable(story.invest_negotiable || '');
    setEditValuable(story.invest_valuable || '');
    setEditEstimable(story.invest_estimable || '');
    setEditSmall(story.invest_small || '');
    setEditTestable(story.invest_testable || '');
    setIsEditing(true);
  }

  function cancelEditing() {
    setIsEditing(false);
  }

  function handleSaveEdit(e: FormEvent) {
    e.preventDefault();
    if (!story) return;
    const conceptsArr = editConcepts
      .split(',')
      .map((c) => c.trim())
      .filter(Boolean);
    updateStory.mutate(
      {
        id: story.id,
        title: editTitle.trim(),
        body: editBody,
        size: editSize || null,
        concepts: conceptsArr,
        invest_independent: editIndependent || null,
        invest_negotiable: editNegotiable || null,
        invest_valuable: editValuable || null,
        invest_estimable: editEstimable || null,
        invest_small: editSmall || null,
        invest_testable: editTestable || null,
      },
      { onSuccess: () => setIsEditing(false) },
    );
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
    <>
      <div className="max-w-4xl mx-auto px-8 py-8 space-y-8">
        {/* Title + Edit button */}
        {isEditing ? (
          <form onSubmit={handleSaveEdit} className="space-y-6">
            <div>
              <label className="block text-xs font-medium text-muted-foreground mb-1">Title</label>
              <Input
                type="text"
                value={editTitle}
                onChange={(e) => setEditTitle(e.target.value)}
                className="text-lg font-semibold"
              />
            </div>

            <div>
              <label className="block text-xs font-medium text-muted-foreground mb-1">Body (Markdown)</label>
              <Textarea
                value={editBody}
                onChange={(e) => setEditBody(e.target.value)}
                rows={12}
                className="font-mono"
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-xs font-medium text-muted-foreground mb-1">Size</label>
                <select
                  value={editSize}
                  onChange={(e) => setEditSize(e.target.value)}
                  className="w-full px-3 py-2 text-sm rounded-md border border-input bg-background focus:outline-none focus:ring-2 focus:ring-ring"
                >
                  <option value="">--</option>
                  <option value="S">S</option>
                  <option value="M">M</option>
                  <option value="L">L</option>
                </select>
              </div>
              <div>
                <label className="block text-xs font-medium text-muted-foreground mb-1">Concepts (comma-separated)</label>
                <Input
                  type="text"
                  value={editConcepts}
                  onChange={(e) => setEditConcepts(e.target.value)}
                  placeholder="concept1, concept2"
                />
              </div>
            </div>

            <div>
              <h3 className="text-xs font-bold uppercase tracking-[0.12em] text-muted-foreground mb-3">INVEST Criteria</h3>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs font-medium text-muted-foreground mb-1">Independent</label>
                  <Textarea value={editIndependent} onChange={(e) => setEditIndependent(e.target.value)} rows={2} />
                </div>
                <div>
                  <label className="block text-xs font-medium text-muted-foreground mb-1">Negotiable</label>
                  <Textarea value={editNegotiable} onChange={(e) => setEditNegotiable(e.target.value)} rows={2} />
                </div>
                <div>
                  <label className="block text-xs font-medium text-muted-foreground mb-1">Valuable</label>
                  <Textarea value={editValuable} onChange={(e) => setEditValuable(e.target.value)} rows={2} />
                </div>
                <div>
                  <label className="block text-xs font-medium text-muted-foreground mb-1">Estimable</label>
                  <Textarea value={editEstimable} onChange={(e) => setEditEstimable(e.target.value)} rows={2} />
                </div>
                <div>
                  <label className="block text-xs font-medium text-muted-foreground mb-1">Small</label>
                  <Textarea value={editSmall} onChange={(e) => setEditSmall(e.target.value)} rows={2} />
                </div>
                <div>
                  <label className="block text-xs font-medium text-muted-foreground mb-1">Testable</label>
                  <Textarea value={editTestable} onChange={(e) => setEditTestable(e.target.value)} rows={2} />
                </div>
              </div>
            </div>

            <div className="flex items-center gap-2 justify-end">
              <Button type="button" variant="outline" size="sm" onClick={cancelEditing}>
                Cancel
              </Button>
              <Button
                type="submit"
                size="sm"
                disabled={updateStory.isPending || !editTitle.trim()}
              >
                {updateStory.isPending ? 'Saving...' : 'Save Changes'}
              </Button>
            </div>
          </form>
        ) : (
          <>
            <div className="flex items-start justify-between gap-4">
              <h1 className="text-2xl font-semibold text-foreground">{story.title}</h1>
              <Button variant="outline" size="sm" onClick={startEditing}>
                <Pencil size={12} />
                Edit
              </Button>
            </div>

            {/* Meta row */}
            <div className="flex flex-wrap items-center gap-2.5">
              <StatusBadge status={story.status} />
              <SizeBadge size={story.size} />
              <InvestBadges story={story} />
              {allPass && (
                <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-emerald-100 text-emerald-700 text-[10px] font-bold uppercase tracking-wider">
                  <svg width="10" height="10" viewBox="0 0 12 12" fill="none">
                    <path d="M2 6l3 3 5-5" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
                  </svg>
                  INVEST Validated
                </span>
              )}
            </div>
          </>
        )}

        {/* Dependencies */}
        {deps.length > 0 && (
          <section>
            <h2 className="text-xs font-bold uppercase tracking-[0.12em] text-muted-foreground mb-2">
              Dependencies
            </h2>
            <div className="flex flex-wrap gap-2">
              {deps.map((dep) => (
                <Link
                  key={dep.id}
                  to="/stories/$storyId"
                  params={{ storyId: dep.blocked_by_id }}
                  className={cn(
                    'inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs border',
                    'hover:bg-muted transition-colors',
                    dep.blocked_by_status === 'done'
                      ? 'border-approve/30 text-approve'
                      : dep.blocked_by_status === 'in-progress'
                        ? 'border-changes/30 text-changes'
                        : 'border-blocker/30 text-blocker',
                  )}
                >
                  <DepStatusIcon status={dep.blocked_by_status} />
                  <span className="font-medium">{dep.blocked_by_title || dep.blocked_by_id}</span>
                </Link>
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
                <Link
                  key={name}
                  to="/knowledge/$concept"
                  params={{ concept: name }}
                  className={cn(
                    'px-2.5 py-1 rounded-full text-xs font-medium border border-primary/20 text-primary',
                    'hover:bg-primary/5 transition-colors',
                  )}
                >
                  {name}
                </Link>
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
        {hasReviews && (
          <section className="border-t border-border pt-6">
            <ReviewSection
              reviews={rawReviews}
              story={story}
              onSelectReview={setReviewPanelData}
            />
          </section>
        )}

        {/* Launch review (if no reviews yet) */}
        {!hasReviews && (
          <section className="border-t border-border pt-6">
            {!isReviewing ? (
              <Button onClick={handleLaunchReview} disabled={dispatchReview.isPending}>
                {dispatchReview.isPending ? 'Dispatching...' : 'Launch INVEST Review'}
              </Button>
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

        {/* Comments section */}
        <section className="border-t border-border pt-6">
          <h2 className="text-xs font-bold uppercase tracking-[0.12em] text-muted-foreground mb-4">
            Comments
          </h2>

          {/* Existing comments */}
          {comments.length > 0 && (
            <div className="space-y-3 mb-5">
              {comments.map((comment) => (
                <div
                  key={comment.id}
                  className="rounded-lg border border-border bg-muted/30 px-4 py-3"
                >
                  <div className="flex items-center gap-2 mb-1.5">
                    <span
                      className={cn(
                        'inline-block px-1.5 py-0.5 rounded text-[9px] font-bold uppercase tracking-wider',
                        comment.author === 'user'
                          ? 'bg-primary/10 text-primary'
                          : 'bg-amber-100 text-amber-700',
                      )}
                    >
                      {comment.author === 'user' ? 'You' : 'Agent'}
                    </span>
                    <span className="text-[11px] text-muted-foreground">
                      {new Date(comment.created_at).toLocaleString()}
                    </span>
                  </div>
                  <p className="text-sm text-foreground whitespace-pre-wrap">{comment.body}</p>
                </div>
              ))}
            </div>
          )}

          {comments.length === 0 && (
            <p className="text-xs text-muted-foreground mb-4">No comments yet.</p>
          )}

          {/* Add comment form */}
          <form onSubmit={handlePostComment} className="space-y-2">
            <Textarea
              value={commentBody}
              onChange={(e) => setCommentBody(e.target.value)}
              placeholder="Add a comment..."
              rows={3}
            />
            <div className="flex justify-end">
              <Button
                type="submit"
                size="sm"
                disabled={!commentBody.trim() || createComment.isPending}
              >
                {createComment.isPending ? 'Posting...' : 'Post Comment'}
              </Button>
            </div>
          </form>
        </section>

        {/* Decisions section (only for done stories) */}
        {story.status === 'done' && (
          <section className="border-t border-border pt-6">
            <h2 className="text-xs font-bold uppercase tracking-[0.12em] text-muted-foreground mb-4">
              Key Decisions
            </h2>

            {decisions.length > 0 && (
              <div className="space-y-4 mb-5">
                {decisions.map((decision) => {
                  let keyItems: string[] = [];
                  try {
                    keyItems = typeof decision.key_decisions === 'string'
                      ? JSON.parse(decision.key_decisions)
                      : (decision.key_decisions as unknown as string[]) ?? [];
                  } catch { keyItems = []; }

                  return (
                    <div
                      key={decision.id}
                      className="rounded-lg border border-border bg-muted/30 px-4 py-3"
                    >
                      <div className="flex items-center gap-2 mb-2">
                        <span className="text-[11px] text-muted-foreground">
                          {new Date(decision.created_at).toLocaleString()}
                        </span>
                      </div>
                      <p className="text-sm font-medium text-foreground mb-2">{decision.summary}</p>
                      {keyItems.length > 0 && (
                        <ul className="list-disc list-inside space-y-1">
                          {keyItems.map((item, idx) => (
                            <li key={idx} className="text-sm text-foreground/80">{item}</li>
                          ))}
                        </ul>
                      )}
                    </div>
                  );
                })}
              </div>
            )}

            {decisions.length === 0 && !showDecisionForm && (
              <div className="mb-4">
                <p className="text-xs text-muted-foreground mb-3">No decisions recorded yet.</p>
                <Button size="sm" onClick={() => setShowDecisionForm(true)}>
                  Record Decisions
                </Button>
              </div>
            )}

            {decisions.length > 0 && !showDecisionForm && (
              <Button variant="outline" size="sm" onClick={() => setShowDecisionForm(true)}>
                + Add Decision
              </Button>
            )}

            {showDecisionForm && (
              <form onSubmit={handleSaveDecision} className="space-y-3 border border-border rounded-lg p-4 bg-card">
                <div>
                  <label className="block text-xs font-medium text-muted-foreground mb-1">Summary</label>
                  <Textarea
                    value={decisionSummary}
                    onChange={(e) => setDecisionSummary(e.target.value)}
                    rows={2}
                    placeholder="What was decided?"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-muted-foreground mb-1">Key Decisions (one per line)</label>
                  <Textarea
                    value={decisionItems}
                    onChange={(e) => setDecisionItems(e.target.value)}
                    rows={4}
                    placeholder={"Decision 1\nDecision 2\nDecision 3"}
                  />
                </div>
                <div className="flex items-center gap-2 justify-end">
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={() => { setShowDecisionForm(false); setDecisionSummary(''); setDecisionItems(''); }}
                  >
                    Cancel
                  </Button>
                  <Button
                    type="submit"
                    size="sm"
                    disabled={!decisionSummary.trim() || createDecision.isPending}
                  >
                    {createDecision.isPending ? 'Saving...' : 'Save Decision'}
                  </Button>
                </div>
              </form>
            )}
          </section>
        )}
      </div>

      {/* Verdict Bar (bottom bar when story has reviews) */}
      {story && parsedReviews.length > 0 && (
        <VerdictBar
          reviews={parsedReviews}
          story={story}
          onVerdict={(verdict) => {
            createVerdict.mutate({ storyId, verdict });
            if (verdict === 'approve' && parsedReviews.length > 0) {
              const findingTitles = parsedReviews.flatMap((r) =>
                r.findings.map((f) => `[${r.agent_role}] ${f.title}`),
              );
              createDecision.mutate({
                storyId,
                summary: 'Story approved',
                keyDecisions: findingTitles.length > 0 ? findingTitles : ['Approved with no findings'],
              });
            }
          }}
        />
      )}
    </>
  );
}
