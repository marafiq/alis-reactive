import { useMemo, useState, type FormEvent } from 'react';
import { useParams, useRouterState, Link } from '@tanstack/react-router';
import { marked } from 'marked';
import hljs from 'highlight.js';
import { ChevronRight, Pencil } from 'lucide-react';
import { cn } from '@/lib/utils';
import { useStory, usePlan, useReviews, useDispatchReview, useComments, useCreateComment, useDecisions, useCreateDecision, useUpdateStory, useCreateVerdict, usePlanAgents, useInvestSummary, useInvestAssessments } from '@/hooks/queries';
import { useWS } from '@/App';
import { usePlanContext } from '@/hooks/usePlanContext';
import { useReviewPanel } from '@/components/layout/reviewPanelContext';
import { ReviewSection } from '@/components/reviews/ReviewSection';
import { SizeBadge, StatusBadge } from '@/components/ui/badges';
import { InvestScorecard } from '@/components/invest/InvestScorecard';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Card } from '@/components/ui/card';
import { SectionHeading } from '@/components/ui/section-heading';
import { VerdictBar } from '@/components/layout/VerdictBar';
import type { ParsedReview, Dependency, PlanAgent } from '@/lib/types';
import {
  parseJson,
  parseReview,
  verdictLabel,
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

function AgentProgress({ progress, agents }: { progress: Record<string, { status: string; verdict?: string }>; agents: PlanAgent[] }) {
  return (
    <div className="space-y-2 mt-4">
      {agents.map((agent) => {
        const entry = progress[agent.agent_template_id];
        const status = entry?.status ?? 'waiting';
        const verdict = entry?.verdict;

        return (
          <Card key={agent.agent_template_id}>
            <div className="px-4 py-2.5 flex items-center gap-3">
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

              {/* Agent name */}
              <span className="text-sm font-medium text-foreground flex-1">{agent.display_name}</span>

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
          </Card>
        );
      })}
    </div>
  );
}

// ── StoryDetail ──

export function StoryDetail() {
  // Extract storyId from the URL path (works for both /stories/$storyId and /plans/$planId/stories/$storyId)
  const pathname = useRouterState({ select: (s) => s.location.pathname });
  const storyId = useMemo(() => {
    const match = pathname.match(/\/stories\/([^/]+)/);
    return match ? decodeURIComponent(match[1]) : null;
  }, [pathname]);

  // Plan context: available when route is /plans/$planId/stories/$storyId
  const planId = usePlanContext();
  const { data: planData } = usePlan(planId);

  const { setReviewPanelData } = useReviewPanel();
  const { data: story, isLoading: storyLoading } = useStory(storyId);
  const { data: rawReviews = [] } = useReviews(storyId);
  const { data: comments = [] } = useComments(storyId);
  const { data: decisions = [] } = useDecisions(storyId);
  const { data: planAgents = [] } = usePlanAgents(story?.plan_id ?? null);
  const { data: investHealth = [] } = useInvestSummary(storyId);
  const { data: investAssessments = [] } = useInvestAssessments(storyId);
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

  return (
    <>
      <div className="max-w-4xl mx-auto px-8 py-8 space-y-8">
        {/* Breadcrumb (when plan-scoped) */}
        {planId && planData && (
          <nav className="flex items-center gap-1.5 text-sm text-muted-foreground">
            <Link
              to="/plans/$planId"
              params={{ planId }}
              className="hover:text-primary transition-colors font-medium"
            >
              {planData.title}
            </Link>
            <ChevronRight size={14} className="text-muted-foreground/50" />
            <span className="text-foreground font-medium truncate">
              {story?.title ?? 'Story'}
            </span>
          </nav>
        )}

        {/* Title + Edit button */}
        {isEditing ? (
          <Card>
            <form onSubmit={handleSaveEdit} className="px-5 py-5 space-y-5">
              <div>
                <label className="block text-xs font-medium text-muted-foreground mb-1.5">Title</label>
                <Input
                  type="text"
                  value={editTitle}
                  onChange={(e) => setEditTitle(e.target.value)}
                  className="text-lg font-semibold"
                />
              </div>

              <div>
                <label className="block text-xs font-medium text-muted-foreground mb-1.5">Body (Markdown)</label>
                <Textarea
                  value={editBody}
                  onChange={(e) => setEditBody(e.target.value)}
                  rows={12}
                  className="font-mono"
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs font-medium text-muted-foreground mb-1.5">Size</label>
                  <select
                    value={editSize}
                    onChange={(e) => setEditSize(e.target.value)}
                    className="w-full px-3 py-2 text-sm rounded-lg border border-input bg-background focus:outline-none focus:ring-2 focus:ring-ring"
                  >
                    <option value="">--</option>
                    <option value="S">S</option>
                    <option value="M">M</option>
                    <option value="L">L</option>
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-medium text-muted-foreground mb-1.5">Concepts (comma-separated)</label>
                  <Input
                    type="text"
                    value={editConcepts}
                    onChange={(e) => setEditConcepts(e.target.value)}
                    placeholder="concept1, concept2"
                  />
                </div>
              </div>

              <div>
                <SectionHeading>INVEST Criteria</SectionHeading>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-xs font-medium text-muted-foreground mb-1.5">Independent</label>
                    <Textarea value={editIndependent} onChange={(e) => setEditIndependent(e.target.value)} rows={2} />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-muted-foreground mb-1.5">Negotiable</label>
                    <Textarea value={editNegotiable} onChange={(e) => setEditNegotiable(e.target.value)} rows={2} />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-muted-foreground mb-1.5">Valuable</label>
                    <Textarea value={editValuable} onChange={(e) => setEditValuable(e.target.value)} rows={2} />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-muted-foreground mb-1.5">Estimable</label>
                    <Textarea value={editEstimable} onChange={(e) => setEditEstimable(e.target.value)} rows={2} />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-muted-foreground mb-1.5">Small</label>
                    <Textarea value={editSmall} onChange={(e) => setEditSmall(e.target.value)} rows={2} />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-muted-foreground mb-1.5">Testable</label>
                    <Textarea value={editTestable} onChange={(e) => setEditTestable(e.target.value)} rows={2} />
                  </div>
                </div>
              </div>

              <div className="flex items-center gap-2 justify-end pt-2">
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
          </Card>
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
            </div>
          </>
        )}

        {/* Dependencies */}
        {deps.length > 0 && (
          <section>
            <SectionHeading count={deps.length}>Dependencies</SectionHeading>
            <div className="flex flex-wrap gap-2">
              {deps.map((dep) => (
                <Link
                  key={dep.id}
                  to={planId ? '/plans/$planId/stories/$storyId' : '/stories/$storyId'}
                  params={planId ? { planId, storyId: dep.blocked_by_id } : { storyId: dep.blocked_by_id }}
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
            <SectionHeading count={concepts.length}>Concepts</SectionHeading>
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

        {/* INVEST Scorecard */}
        <section>
          <InvestScorecard
            story={story}
            investAssessments={investAssessments}
            investHealth={investHealth}
          />
        </section>

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
                <SectionHeading>Review In Progress</SectionHeading>
                <p className="text-xs text-muted-foreground mb-3">
                  Agents are reviewing this story against INVEST criteria.
                </p>
                <AgentProgress progress={agentProgress} agents={planAgents} />
              </div>
            )}
          </section>
        )}

        {/* Comments section */}
        <section className="border-t border-border pt-6">
          <SectionHeading count={comments.length}>Comments</SectionHeading>

          {/* Existing comments */}
          {comments.length > 0 && (
            <div className="space-y-2.5 mb-5">
              {comments.map((comment) => (
                <Card key={comment.id}>
                  <div className="px-4 py-3">
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
                      <span className="text-[10px] text-muted-foreground">
                        {new Date(comment.created_at).toLocaleString()}
                      </span>
                    </div>
                    <p className="text-sm text-foreground whitespace-pre-wrap">{comment.body}</p>
                  </div>
                </Card>
              ))}
            </div>
          )}

          {comments.length === 0 && (
            <p className="text-xs text-muted-foreground mb-4">No comments yet.</p>
          )}

          {/* Add comment form */}
          <Card>
            <form onSubmit={handlePostComment} className="px-4 py-3 space-y-2.5">
              <Textarea
                value={commentBody}
                onChange={(e) => setCommentBody(e.target.value)}
                placeholder="Add a comment..."
                rows={3}
                className="border-0 shadow-none p-0 focus-visible:ring-0 resize-none"
              />
              <div className="flex justify-end border-t border-border pt-2.5">
                <Button
                  type="submit"
                  size="sm"
                  disabled={!commentBody.trim() || createComment.isPending}
                >
                  {createComment.isPending ? 'Posting...' : 'Post Comment'}
                </Button>
              </div>
            </form>
          </Card>
        </section>

        {/* Decisions section (only for done stories) */}
        {story.status === 'done' && (
          <section className="border-t border-border pt-6">
            <SectionHeading count={decisions.length}>Key Decisions</SectionHeading>

            {decisions.length > 0 && (
              <div className="space-y-2.5 mb-5">
                {decisions.map((decision) => {
                  let keyItems: string[] = [];
                  try {
                    keyItems = typeof decision.key_decisions === 'string'
                      ? JSON.parse(decision.key_decisions)
                      : (decision.key_decisions as unknown as string[]) ?? [];
                  } catch { keyItems = []; }

                  return (
                    <Card key={decision.id}>
                      <div className="px-4 py-3">
                        <span className="text-[10px] text-muted-foreground">
                          {new Date(decision.created_at).toLocaleString()}
                        </span>
                        <p className="text-sm font-medium text-foreground mt-1">{decision.summary}</p>
                        {keyItems.length > 0 && (
                          <ul className="list-disc list-inside space-y-0.5 mt-2">
                            {keyItems.map((item, idx) => (
                              <li key={idx} className="text-sm text-foreground/80">{item}</li>
                            ))}
                          </ul>
                        )}
                      </div>
                    </Card>
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
              <Card>
                <form onSubmit={handleSaveDecision} className="px-4 py-4 space-y-3">
                  <div>
                    <label className="block text-xs font-medium text-muted-foreground mb-1.5">Summary</label>
                    <Textarea
                      value={decisionSummary}
                      onChange={(e) => setDecisionSummary(e.target.value)}
                      rows={2}
                      placeholder="What was decided?"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-muted-foreground mb-1.5">Key Decisions (one per line)</label>
                    <Textarea
                      value={decisionItems}
                      onChange={(e) => setDecisionItems(e.target.value)}
                      rows={4}
                      placeholder={"Decision 1\nDecision 2\nDecision 3"}
                    />
                  </div>
                  <div className="flex items-center gap-2 justify-end pt-1">
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
              </Card>
            )}
          </section>
        )}
      </div>

      {/* Verdict Bar (bottom bar when story has reviews) */}
      {story && parsedReviews.length > 0 && (
        <VerdictBar
          reviews={parsedReviews}
          story={story}
          totalAgents={planAgents.length || parsedReviews.length}
          investHealth={investHealth}
          onVerdict={(verdict) => {
            createVerdict.mutate({ storyId: story.id, verdict });
            if (verdict === 'approve' && parsedReviews.length > 0) {
              const findingTitles = parsedReviews.flatMap((r) =>
                r.findings.map((f) => `[${r.agent_display_name || r.agent_template_id}] ${f.title}`),
              );
              createDecision.mutate({
                storyId: story.id,
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
