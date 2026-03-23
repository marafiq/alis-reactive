import { cn } from '@/lib/utils';
import type { EvidenceQuality } from './invest-types';

interface EvidenceQualityBadgeProps {
  quality: EvidenceQuality;
  variant?: 'row' | 'inline';
}

const DOT_COLORS: Record<EvidenceQuality, string> = {
  strong: 'bg-emerald-500',
  adequate: 'bg-amber-500',
  weak: 'bg-red-500',
  missing: 'bg-zinc-300',
};

const DOT_COUNTS: Record<EvidenceQuality, number> = {
  strong: 3,
  adequate: 2,
  weak: 1,
  missing: 0,
};

const LABELS: Record<EvidenceQuality, string> = {
  strong: 'Strong',
  adequate: 'Adequate',
  weak: 'Weak',
  missing: 'Missing',
};

export function EvidenceQualityBadge({ quality, variant = 'row' }: EvidenceQualityBadgeProps) {
  const count = DOT_COUNTS[quality];
  const color = DOT_COLORS[quality];
  const label = LABELS[quality];

  const dots = (
    <span className="inline-flex items-center gap-0.5">
      {[0, 1, 2].map(i => (
        <span
          key={i}
          className={cn(
            'w-1.5 h-1.5 rounded-full',
            i < count ? color : 'bg-zinc-200',
          )}
        />
      ))}
    </span>
  );

  if (variant === 'inline') {
    return (
      <span title={label} className="inline-flex items-center">
        {dots}
      </span>
    );
  }

  return (
    <span className="inline-flex items-center gap-1.5">
      {dots}
      <span className="text-xs text-muted-foreground">{label}</span>
    </span>
  );
}
