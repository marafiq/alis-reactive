import { cn } from '@/lib/utils';
import { investScores, INVEST_LABELS, type Story } from '@/lib/types';

// ── INVEST Badges ──

const INVEST_PASS = 'bg-emerald-100 text-emerald-700 border border-emerald-200';
const INVEST_FAIL = 'bg-red-100 text-red-600 border border-red-200';
const INVEST_MISSING = 'bg-gray-100 text-gray-400 border border-gray-200';

interface InvestBadgesProps {
  story: Story;
  /** 'sm' = 14x14 for board cards, 'md' = 18x18 for tables & detail */
  size?: 'sm' | 'md';
}

export function InvestBadges({ story, size = 'md' }: InvestBadgesProps) {
  const scores = investScores(story);
  const dim = size === 'sm' ? 'w-3.5 h-3.5 text-[8px]' : 'w-[18px] h-[18px] text-[10px]';
  return (
    <span className="inline-flex gap-0.5">
      {(Object.entries(scores) as [string, boolean | undefined][]).map(([letter, pass]) => (
        <span
          key={letter}
          title={INVEST_LABELS[letter]}
          className={cn(
            'inline-flex items-center justify-center rounded-sm font-bold leading-none',
            dim,
            pass === true
              ? INVEST_PASS
              : pass === false
                ? INVEST_FAIL
                : INVEST_MISSING,
          )}
        >
          {letter}
        </span>
      ))}
    </span>
  );
}

// ── Size Badge ──

const SIZE_STYLES: Record<string, string> = {
  S: 'bg-emerald-500 text-white',
  M: 'bg-amber-500 text-white',
  L: 'bg-red-500 text-white',
};

interface SizeBadgeProps {
  size: string | null;
  className?: string;
}

export function SizeBadge({ size, className }: SizeBadgeProps) {
  if (!size) return null;
  return (
    <span
      className={cn(
        'inline-block px-1.5 py-0.5 rounded text-[10px] font-bold uppercase tracking-wider',
        SIZE_STYLES[size] ?? 'bg-gray-200 text-gray-600',
        className,
      )}
    >
      {size}
    </span>
  );
}

// ── Status Badge ──

const STATUS_STYLES: Record<string, string> = {
  draft: 'bg-gray-200 text-gray-600',
  ready: 'bg-blue-100 text-blue-700',
  'in-progress': 'bg-amber-100 text-amber-700',
  review: 'bg-violet-100 text-violet-700',
  done: 'bg-emerald-100 text-emerald-700',
  active: 'bg-blue-100 text-blue-700',
  completed: 'bg-emerald-100 text-emerald-700',
  archived: 'bg-gray-200 text-gray-600',
};

interface StatusBadgeProps {
  status: string;
}

export function StatusBadge({ status }: StatusBadgeProps) {
  return (
    <span
      className={cn(
        'inline-block px-2 py-0.5 rounded-full text-[10px] font-semibold uppercase tracking-wider',
        STATUS_STYLES[status] ?? 'bg-gray-200 text-gray-600',
      )}
    >
      {status.replace('-', ' ')}
    </span>
  );
}

// ── Verdict Badge ──

const VERDICT_STYLES: Record<string, string> = {
  approve: 'bg-emerald-100 text-emerald-700 border border-emerald-200',
  object: 'bg-red-100 text-red-600 border border-red-200',
  'approve-with-notes': 'bg-amber-100 text-amber-700 border border-amber-200',
};

interface VerdictBadgeProps {
  verdict: string;
  label: string;
}

export function VerdictBadge({ verdict, label }: VerdictBadgeProps) {
  return (
    <span
      className={cn(
        'shrink-0 px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-wider',
        VERDICT_STYLES[verdict] ?? 'bg-gray-200 text-gray-600',
      )}
    >
      {label}
    </span>
  );
}
