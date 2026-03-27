export type CriterionVerdict = 'pass' | 'fail' | 'contested' | 'pending';
export type EvidenceQuality = 'strong' | 'adequate' | 'weak' | 'missing';

export const CRITERION_DISPLAY: Record<string, { name: string; description: string }> = {
  I: { name: 'Independent', description: 'Can be developed and delivered independently' },
  N: { name: 'Negotiable', description: 'Details can be negotiated, not a rigid contract' },
  V: { name: 'Valuable', description: 'Delivers value to a stakeholder' },
  E: { name: 'Estimable', description: 'Can be estimated with reasonable confidence' },
  S: { name: 'Small', description: 'Small enough to complete in one iteration' },
  T: { name: 'Testable', description: 'Has clear criteria for verification' },
};

export const INVEST_LETTERS = ['I', 'N', 'V', 'E', 'S', 'T'] as const;
