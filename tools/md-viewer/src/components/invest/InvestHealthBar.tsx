import { cn } from '@/lib/utils';
import type { InvestHealth } from '@/lib/types';
import { INVEST_LETTERS } from './invest-types';
import type { CriterionVerdict } from './invest-types';
import { criterionVerdict, failedCriteriaSummary } from './invest-utils';

interface InvestHealthBarProps {
  investHealth: InvestHealth[];
  variant?: 'compact' | 'expanded';
}

const PILL_STYLES: Record<CriterionVerdict, string> = {
  pass: 'bg-emerald-500 text-white',
  fail: 'bg-red-500 text-white',
  contested: 'bg-amber-500 text-white',
  pending: 'bg-zinc-200 text-zinc-400',
};

export function InvestHealthBar({ investHealth, variant = 'compact' }: InvestHealthBarProps) {
  const hasData = investHealth.length > 0;

  const summary = hasData ? failedCriteriaSummary(investHealth) : '';

  return (
    <div className="flex flex-col gap-1">
      {/* 6 letter pills */}
      <div className="inline-flex gap-0.5">
        {INVEST_LETTERS.map(letter => {
          const health = investHealth.find(h => h.criterion === letter);
          const verdict = criterionVerdict(health);

          return (
            <span
              key={letter}
              title={hasData ? `${letter}: ${verdict}` : letter}
              className={cn(
                'w-5 h-5 rounded text-[9px] font-bold flex items-center justify-center',
                PILL_STYLES[verdict],
              )}
            >
              {letter}
            </span>
          );
        })}
      </div>

      {/* Failure summary text */}
      {summary && (
        <span className="text-[10px] text-muted-foreground leading-tight">
          {variant === 'compact' && summary.length > 40
            ? summary.slice(0, 37) + '...'
            : summary}
        </span>
      )}
    </div>
  );
}
