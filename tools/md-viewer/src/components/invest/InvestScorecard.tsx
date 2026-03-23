import type { Story, InvestAssessment, InvestHealth } from '@/lib/types';
import { Card } from '@/components/ui/card';
import { SectionHeading } from '@/components/ui/section-heading';
import { INVEST_LETTERS } from './invest-types';
import type { EvidenceQuality } from './invest-types';
import { criterionVerdict, authorStatementForLetter } from './invest-utils';
import { InvestCriterionRow } from './InvestCriterionRow';

interface InvestScorecardProps {
  story: Story;
  investAssessments: InvestAssessment[];
  investHealth: InvestHealth[];
}

export function InvestScorecard({ story, investAssessments, investHealth }: InvestScorecardProps) {
  const passCount = investHealth.filter(h => h.verdict === 'pass').length;
  const totalCount = INVEST_LETTERS.length;

  return (
    <div>
      <SectionHeading>INVEST Scorecard</SectionHeading>

      {/* Aggregate summary */}
      <div className="mb-3 flex items-center gap-2">
        <span className="text-sm font-medium text-foreground">
          {passCount}/{totalCount} criteria passing
        </span>
        {passCount === totalCount && (
          <span className="text-xs text-emerald-600 font-medium">All clear</span>
        )}
        {passCount < totalCount && investHealth.length > 0 && (
          <span className="text-xs text-amber-600 font-medium">
            {totalCount - passCount} need{totalCount - passCount === 1 ? 's' : ''} attention
          </span>
        )}
      </div>

      <Card>
        <div className="divide-y">
          {INVEST_LETTERS.map(letter => {
            const health = investHealth.find(h => h.criterion === letter);
            const verdict = criterionVerdict(health);
            const authorStatement = authorStatementForLetter(story, letter);

            // Get agent feedback for this criterion
            const feedback = investAssessments
              .filter(a => a.criterion === letter)
              .map(a => ({
                agentName: a.agent_template_id ?? 'Agent',
                pass: a.pass === 1,
                reasoning: a.reasoning,
                evidenceQuality: a.evidence_quality as EvidenceQuality,
              }));

            return (
              <InvestCriterionRow
                key={letter}
                letter={letter}
                verdict={verdict}
                authorStatement={authorStatement}
                agentFeedback={feedback}
              />
            );
          })}
        </div>
      </Card>
    </div>
  );
}
