import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { Plan, Story, Review, Concept, ConceptLink, HumanVerdict, InvestValidation, Comment, AgentTemplate, PlanAgent, InvestHealth, InvestAssessment } from '@/lib/types';

// ── API helper ──
async function api<T>(path: string, opts?: Omit<RequestInit, 'body'> & { body?: unknown }): Promise<T> {
  const res = await fetch(`/api${path}`, {
    headers: { 'Content-Type': 'application/json' },
    ...opts,
    body: opts?.body ? JSON.stringify(opts.body) : undefined,
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({ error: res.statusText }));
    throw new Error(err.error || res.statusText);
  }
  return res.json();
}

// ── Plans ──
export function usePlans() {
  return useQuery({ queryKey: ['plans'], queryFn: () => api<Plan[]>('/plans') });
}

export function usePlan(id: string | null) {
  return useQuery({
    queryKey: ['plan', id],
    queryFn: () => api<Plan>(`/plans/${id}`),
    enabled: !!id,
  });
}

export function useCreatePlan() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: { id: string; title: string; masterPrompt?: string; goals?: unknown[]; constraints?: string[] }) =>
      api<Plan>('/plans', { method: 'POST', body: data }),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['plans'] }),
  });
}

export function useUpdatePlan() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...fields }: { id: string } & Record<string, unknown>) =>
      api<Plan>(`/plans/${id}`, { method: 'PUT', body: fields }),
    onSuccess: (_, vars) => {
      qc.invalidateQueries({ queryKey: ['plans'] });
      qc.invalidateQueries({ queryKey: ['plan', vars.id] });
    },
  });
}

// ── Stories ──
export function useStories(planId?: string) {
  return useQuery({
    queryKey: ['stories', planId],
    queryFn: () => api<Story[]>(planId ? `/stories?plan_id=${planId}` : '/stories'),
  });
}

export function useStory(id: string | null) {
  return useQuery({
    queryKey: ['story', id],
    queryFn: () => api<Story>(`/stories/${id}`),
    enabled: !!id,
  });
}

export function useCreateStory() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: { id: string; planId: string; title: string; size?: string; body?: string; concepts?: string[] }) =>
      api<Story>('/stories', { method: 'POST', body: data }),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['stories'] }),
  });
}

export function useUpdateStory() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...fields }: { id: string } & Record<string, unknown>) =>
      api<Story>(`/stories/${id}`, { method: 'PUT', body: fields }),
    onSuccess: (_, vars) => {
      qc.invalidateQueries({ queryKey: ['stories'] });
      qc.invalidateQueries({ queryKey: ['story', vars.id] });
    },
  });
}

export function useUpdateStoryStatus() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, status }: { id: string; status: string }) =>
      api(`/stories/${id}/status`, { method: 'PUT', body: { status } }),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['stories'] }),
  });
}

// ── Reviews ──
export function useReviews(storyId: string | null) {
  return useQuery({
    queryKey: ['reviews', storyId],
    queryFn: () => api<Review[]>(`/reviews?story_id=${storyId}`),
    enabled: !!storyId,
  });
}

export function useDispatchReview() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (storyId: string) =>
      api(`/stories/${storyId}/review`, { method: 'POST' }),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['stories'] }),
  });
}

// ── INVEST ──
export function useInvestValidation(storyId: string | null) {
  return useQuery({
    queryKey: ['invest', storyId],
    queryFn: () => api<InvestValidation>(`/stories/${storyId}/invest`),
    enabled: !!storyId,
  });
}

// ── Human Verdicts ──
export function useCreateVerdict() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: { storyId: string; verdict: string; notes?: string }) =>
      api<{ id: string }>('/human-verdicts', { method: 'POST', body: data }),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['stories'] }),
  });
}

// ── Comments ──
export function useComments(storyId: string | null) {
  return useQuery({
    queryKey: ['comments', storyId],
    queryFn: () => api<Comment[]>(`/comments?storyId=${storyId}`),
    enabled: !!storyId,
  });
}

export function useCreateComment() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: { storyId: string; body: string; author: string }) =>
      api<Comment>('/comments', { method: 'POST', body: data }),
    onSuccess: (_, vars) => qc.invalidateQueries({ queryKey: ['comments', vars.storyId] }),
  });
}

// ── D2 Rendering ──
function hashString(str: string): string {
  let hash = 0;
  for (let i = 0; i < str.length; i++) {
    const ch = str.charCodeAt(i);
    hash = ((hash << 5) - hash) + ch;
    hash |= 0; // Convert to 32-bit int
  }
  return hash.toString(36);
}

export function useD2Render(source: string | null | undefined) {
  return useQuery({
    queryKey: ['d2-render', source ? hashString(source) : ''],
    queryFn: () => api<{ svg: string }>('/d2/render', { method: 'POST', body: { source } }).then(r => r.svg),
    enabled: !!source,
    staleTime: Infinity, // SVG won't change for same source
  });
}

// ── Decisions ──
export interface DecisionEntry {
  id: string;
  story_id: string;
  summary: string;
  key_decisions: string; // JSON array
  created_at: string;
}

export function useDecisions(storyId: string | null) {
  return useQuery({
    queryKey: ['decisions', storyId],
    queryFn: () => api<DecisionEntry[]>(`/decisions?story_id=${storyId}`),
    enabled: !!storyId,
  });
}

export function useCreateDecision() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: { storyId: string; summary: string; keyDecisions: string[] }) =>
      api<{ id: string }>('/decisions', { method: 'POST', body: data }),
    onSuccess: (_, vars) => qc.invalidateQueries({ queryKey: ['decisions', vars.storyId] }),
  });
}

// ── Agent Templates ──

export function useAgentTemplates() {
  return useQuery({ queryKey: ['agent-templates'], queryFn: () => api<AgentTemplate[]>('/agent-templates') });
}

export function usePlanAgents(planId: string | null) {
  return useQuery({
    queryKey: ['plan-agents', planId],
    queryFn: () => api<PlanAgent[]>(`/plans/${planId}/agents`),
    enabled: !!planId,
  });
}

export function useInvestSummary(storyId: string | null) {
  return useQuery({
    queryKey: ['invest-summary', storyId],
    queryFn: () => api<InvestHealth[]>(`/stories/${storyId}/invest-summary`),
    enabled: !!storyId,
  });
}

export function useInvestAssessments(storyId: string | null) {
  return useQuery({
    queryKey: ['invest-assessments', storyId],
    queryFn: () => api<InvestAssessment[]>(`/stories/${storyId}/invest-assessments`),
    enabled: !!storyId,
  });
}

export function useAssignAgent() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ planId, agentTemplateId }: { planId: string; agentTemplateId: string }) =>
      api(`/plans/${planId}/agents`, { method: 'POST', body: { agentTemplateId } }),
    onSuccess: (_, vars) => qc.invalidateQueries({ queryKey: ['plan-agents', vars.planId] }),
  });
}

export function useRemoveAgent() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ planId, agentId }: { planId: string; agentId: string }) =>
      api(`/plans/${planId}/agents/${agentId}`, { method: 'DELETE' }),
    onSuccess: (_, vars) => qc.invalidateQueries({ queryKey: ['plan-agents', vars.planId] }),
  });
}

// ── Concepts ──
export function useConcepts() {
  return useQuery({ queryKey: ['concepts'], queryFn: () => api<Concept[]>('/concepts') });
}

export function useConceptLinks(name: string | null) {
  return useQuery({
    queryKey: ['concept-links', name],
    queryFn: () => api<ConceptLink[]>(`/concepts/${encodeURIComponent(name!)}`),
    enabled: !!name,
  });
}
