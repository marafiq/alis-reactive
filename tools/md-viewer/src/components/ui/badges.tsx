import { cn } from '@/lib/utils';

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
