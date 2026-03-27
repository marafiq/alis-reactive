import type { InvestHealth, Story } from '@/lib/types';
import { investScores } from '@/lib/types';
import type { CriterionVerdict, EvidenceQuality } from './invest-types';

export function criterionVerdict(health: InvestHealth | undefined): CriterionVerdict {
  if (!health) return 'pending';
  return health.verdict as CriterionVerdict;
}

export function failedCriteriaSummary(healthData: InvestHealth[]): string {
  const failed = healthData
    .filter(h => h.verdict === 'fail' || h.verdict === 'contested')
    .map(h => {
      const labels: Record<string, string> = {
        I: 'Independent', N: 'Negotiable', V: 'Valuable',
        E: 'Estimable', S: 'Small', T: 'Testable',
      };
      const prefix = h.verdict === 'contested' ? '?' : '!';
      return `${prefix} ${labels[h.criterion] || h.criterion}`;
    });
  if (failed.length === 0) return '';
  if (failed.length <= 2) return failed.join(' \u00b7 ');
  return `${failed.slice(0, 2).join(' \u00b7 ')} +${failed.length - 2} more`;
}

/** Map INVEST letter to the corresponding story field value */
export function authorStatementForLetter(
  story: { invest_independent: string | null; invest_negotiable: string | null; invest_valuable: string | null; invest_estimable: string | null; invest_small: string | null; invest_testable: string | null },
  letter: string,
): string | null {
  const map: Record<string, string | null> = {
    I: story.invest_independent,
    N: story.invest_negotiable,
    V: story.invest_valuable,
    E: story.invest_estimable,
    S: story.invest_small,
    T: story.invest_testable,
  };
  return map[letter] ?? null;
}

/**
 * Convert author-side investScores (from story fields) into InvestHealth[]
 * for use in InvestHealthBar when agent-side data isn't available.
 */
export function storyToInvestHealth(story: Story): InvestHealth[] {
  const scores = investScores(story);
  return (Object.entries(scores) as [string, boolean][]).map(([criterion, pass]) => ({
    criterion,
    pass_count: pass ? 1 : 0,
    total_agents: 1,
    verdict: pass ? 'pass' as const : 'pending' as const,
  }));
}
