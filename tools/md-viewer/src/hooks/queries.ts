import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { Plan, Story, Review, Concept, ConceptLink, HumanVerdict, InvestValidation } from '@/lib/types';

// ── API helper ──
async function api<T>(path: string, opts?: RequestInit & { body?: unknown }): Promise<T> {
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
