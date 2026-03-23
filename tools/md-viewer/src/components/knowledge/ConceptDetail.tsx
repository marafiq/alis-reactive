import { useConceptLinks } from '@/hooks/queries';
import type { ConceptLink } from '@/lib/types';
import { cn } from '@/lib/utils';
import { ArrowLeft, Loader2, AlertCircle, Link2 } from 'lucide-react';

interface ConceptDetailProps {
  conceptName: string;
  onBack?: () => void;
  onNavigate: (type: ConceptLink['entity_type'], id: string) => void;
}

const TYPE_BADGE_STYLES: Record<ConceptLink['entity_type'], string> = {
  plan: 'bg-purple-100 text-purple-800',
  story: 'bg-violet-100 text-violet-800',
  review: 'bg-amber-100 text-amber-800',
  file: 'bg-gray-100 text-gray-700',
};

function TypeBadge({ type }: { type: ConceptLink['entity_type'] }) {
  return (
    <span
      className={cn(
        'inline-flex items-center rounded-full px-2.5 py-0.5 text-[11px] font-semibold uppercase tracking-wide',
        TYPE_BADGE_STYLES[type],
      )}
    >
      {type}
    </span>
  );
}

function TimelineCard({
  link,
  onClick,
  isLast,
}: {
  link: ConceptLink;
  onClick: () => void;
  isLast: boolean;
}) {
  return (
    <div className="relative flex gap-4">
      {/* Timeline line + dot */}
      <div className="flex flex-col items-center">
        <div className="h-3 w-3 shrink-0 rounded-full border-2 border-[#7A2E3B] bg-white" />
        {!isLast && <div className="w-px grow bg-gray-200" />}
      </div>

      {/* Card */}
      <button
        type="button"
        onClick={onClick}
        className={cn(
          'mb-4 flex w-full flex-col gap-2 rounded-lg border border-gray-200 bg-white p-4',
          'text-left transition-all duration-150',
          'hover:border-[#7A2E3B]/30 hover:shadow-sm',
          'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[#7A2E3B]',
        )}
      >
        <div className="flex items-center gap-2">
          <TypeBadge type={link.entity_type} />
          {link.source && (
            <span className="text-[11px] text-gray-400">{link.source}</span>
          )}
        </div>

        <span className="text-sm font-bold text-gray-900">
          {link.title || 'Untitled'}
        </span>

        <span className="font-mono text-xs text-gray-400">
          {link.entity_id}
        </span>
      </button>
    </div>
  );
}

export function ConceptDetail({
  conceptName,
  onBack,
  onNavigate,
}: ConceptDetailProps) {
  const { data: links, isLoading, error } = useConceptLinks(conceptName);

  const referenceCount = links?.length ?? 0;

  return (
    <div className="mx-auto max-w-3xl px-6 py-8">
      {/* Header */}
      <div className="mb-8">
        {onBack && (
          <button
            type="button"
            onClick={onBack}
            className={cn(
              'mb-4 inline-flex items-center gap-1.5 text-sm text-gray-500',
              'transition-colors hover:text-[#7A2E3B]',
              'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[#7A2E3B]',
            )}
          >
            <ArrowLeft className="h-4 w-4" />
            All concepts
          </button>
        )}

        <div className="flex items-start gap-4">
          <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-lg bg-[#7A2E3B]/10">
            <Link2 className="h-5 w-5 text-[#7A2E3B]" />
          </div>
          <div>
            <h1 className="text-2xl font-bold tracking-tight text-gray-900">
              {conceptName}
            </h1>
            <p className="mt-1 text-sm text-gray-500">
              {isLoading
                ? 'Loading references...'
                : `${referenceCount} ${referenceCount === 1 ? 'reference' : 'references'}`}
            </p>
          </div>
        </div>
      </div>

      {/* Loading state */}
      {isLoading && (
        <div className="flex items-center justify-center py-16">
          <Loader2 className="h-6 w-6 animate-spin text-[#7A2E3B]" />
          <span className="ml-2 text-sm text-gray-500">Loading links...</span>
        </div>
      )}

      {/* Error state */}
      {error && (
        <div className="flex items-center gap-3 rounded-lg border border-red-200 bg-red-50 px-4 py-3">
          <AlertCircle className="h-5 w-5 shrink-0 text-red-600" />
          <p className="text-sm text-red-700">
            Failed to load concept links: {error.message}
          </p>
        </div>
      )}

      {/* Empty state */}
      {!isLoading && !error && referenceCount === 0 && (
        <div className="flex flex-col items-center justify-center py-16 text-center">
          <Link2 className="h-10 w-10 text-gray-300" />
          <p className="mt-3 text-sm font-medium text-gray-500">
            No references found
          </p>
          <p className="mt-1 text-xs text-gray-400">
            This concept has no linked entities yet.
          </p>
        </div>
      )}

      {/* Timeline */}
      {links && links.length > 0 && (
        <div className="pl-1">
          {links.map((link, index) => (
            <TimelineCard
              key={`${link.entity_type}-${link.entity_id}`}
              link={link}
              onClick={() => onNavigate(link.entity_type, link.entity_id)}
              isLast={index === links.length - 1}
            />
          ))}
        </div>
      )}
    </div>
  );
}
