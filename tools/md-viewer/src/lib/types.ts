export interface Plan {
  id: string;
  title: string;
  master_prompt: string;
  goals: string; // JSON array
  constraints: string; // JSON array
  d2_diagram: string | null;
  status: 'active' | 'completed' | 'archived';
  created_at: string;
  updated_at: string;
}

export interface Goal {
  text: string;
  done: boolean;
}

export interface Story {
  id: string;
  plan_id: string;
  title: string;
  file_path: string | null;
  size: 'S' | 'M' | 'L' | null;
  status: 'draft' | 'ready' | 'in-progress' | 'review' | 'done';
  invest_independent: string | null;
  invest_negotiable: string | null;
  invest_valuable: string | null;
  invest_estimable: string | null;
  invest_small: string | null;
  invest_testable: string | null;
  invest_validated: number;
  sort_order: number;
  body: string;
  concepts: string; // JSON array
  created_at: string;
  updated_at: string;
  _dependencies?: Dependency[];
  _blocks?: Dependency[];
}

export interface Dependency {
  id: string;
  story_id: string;
  blocked_by_id: string;
  blocked_by_title?: string;
  blocked_by_status?: string;
  story_title?: string;
  story_status?: string;
  reason: string;
}

export type AgentRole = 'architect' | 'csharp' | 'bdd' | 'pm' | 'ui' | 'human-proxy';
export type Verdict = 'approve' | 'object' | 'approve-with-notes';
export type Confidence = 'high' | 'medium' | 'low';

export interface Review {
  id: string;
  story_id: string;
  agent_role: AgentRole;
  round: number;
  verdict: Verdict;
  confidence: Confidence;
  review_json: string; // JSON
  created_at: string;
}

export interface ReviewData {
  roleName: string;
  executive: string;
  findings: Finding[];
  artifacts: Artifact[];
  investScores?: Record<string, { pass: boolean; reasoning: string }>;
}

export interface Finding {
  severity: 'blocker' | 'concern' | 'observation';
  title: string;
  text: string;
  evidence: string;
  recommendation: string;
}

export interface Artifact {
  kind: string;
  label: string;
  content: string;
}

export interface ParsedReview extends Review, ReviewData {}

export interface Concept {
  id: string;
  name: string;
  link_count: number;
  created_at: string;
}

export interface ConceptLink {
  concept_id: string;
  entity_type: 'plan' | 'story' | 'review' | 'file';
  entity_id: string;
  title: string;
  source: string;
}

export interface HumanVerdict {
  id: string;
  story_id: string;
  verdict: 'approve' | 'approve-with-conditions' | 'request-changes' | 'reject' | 'defer';
  notes: string | null;
  created_at: string;
}

export interface InvestValidation {
  valid: boolean;
  errors: string[];
  scores: Record<string, boolean>;
  investCount: number;
}

// Helpers
export function parseJson<T>(val: string | T[] | null | undefined): T[] {
  if (!val) return [];
  if (Array.isArray(val)) return val;
  try { return JSON.parse(val as string); } catch { return []; }
}

export function parseReview(r: Review): ParsedReview {
  const data: ReviewData = typeof r.review_json === 'string'
    ? JSON.parse(r.review_json)
    : (r.review_json as unknown as ReviewData) ?? { executive: '', findings: [], artifacts: [] };
  return { ...r, ...data };
}

export const ROLE_NAMES: Record<AgentRole, string> = {
  architect: 'Architect',
  csharp: 'C# Expert',
  bdd: 'BDD Tester',
  pm: 'PM/Collaborator',
  ui: 'UI Expert',
  'human-proxy': 'Human Proxy (Adnan)',
};

export const INVEST_LABELS: Record<string, string> = {
  I: 'Independent', N: 'Negotiable', V: 'Valuable',
  E: 'Estimable', S: 'Small', T: 'Testable',
};

export function investScores(story: Story) {
  return {
    I: !!story.invest_independent,
    N: !!story.invest_negotiable,
    V: !!story.invest_valuable,
    E: !!story.invest_estimable,
    S: !!story.invest_small,
    T: !!story.invest_testable,
  };
}

export function confidenceLevel(c: Confidence | number): number {
  if (typeof c === 'number') return Math.min(5, Math.max(1, c));
  return c === 'high' ? 5 : c === 'medium' ? 3 : 1;
}

export function verdictLabel(v: Verdict | string): string {
  if (v === 'approve') return 'Approve';
  if (v === 'object') return 'Object';
  if (v === 'approve-with-notes') return 'Notes';
  return v;
}
