import { useConcepts } from '@/hooks/queries';
import type { Concept } from '@/lib/types';
import { cn } from '@/lib/utils';
import { BookOpen, Loader2, AlertCircle } from 'lucide-react';

interface KnowledgeHomeProps {
  onSelectConcept: (name: string) => void;
}

const ENTITY_BADGE_STYLES: Record<string, string> = {
  plan: 'bg-purple-100 text-purple-800',
  story: 'bg-violet-100 text-violet-800',
  review: 'bg-amber-100 text-amber-800',
  file: 'bg-gray-100 text-gray-700',
};

function EntityBadge({ type }: { type: string }) {
  return (
    <span
      className={cn(
        'inline-flex items-center rounded-full px-2 py-0.5 text-[11px] font-semibold uppercase tracking-wide',
        ENTITY_BADGE_STYLES[type] ?? 'bg-gray-100 text-gray-600',
      )}
    >
      {type}
    </span>
  );
}

function ConceptCard({
  concept,
  onClick,
}: {
  concept: Concept;
  onClick: () => void;
}) {
  // Derive entity types from the concept name context — the API returns link_count
  // but not per-type counts. We show the badge set based on available links when
  // the user drills in. For the card grid, we show the aggregate count.
  return (
    <button
      type="button"
      onClick={onClick}
      className={cn(
        'group flex flex-col gap-3 rounded-lg border-l-4 border-[#7A2E3B] bg-white p-4',
        'text-left shadow-sm transition-all duration-200',
        'hover:-translate-y-0.5 hover:shadow-md',
        'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[#7A2E3B]',
      )}
    >
      <span className="text-sm font-bold text-gray-900 group-hover:text-[#7A2E3B]">
        {concept.name}
      </span>

      <span className="text-xs text-gray-500">
        {concept.link_count} {concept.link_count === 1 ? 'reference' : 'references'}
      </span>
    </button>
  );
}

export function KnowledgeHome({ onSelectConcept }: KnowledgeHomeProps) {
  const { data: concepts, isLoading, error } = useConcepts();

  return (
    <div className="mx-auto max-w-5xl px-6 py-8">
      {/* Header */}
      <div className="mb-8 flex items-start gap-4">
        <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-lg bg-[#7A2E3B]/10">
          <BookOpen className="h-5 w-5 text-[#7A2E3B]" />
        </div>
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-gray-900">
            Connected Knowledge
          </h1>
          <p className="mt-1 text-sm text-gray-500">
            Concepts spanning plans, stories, reviews, and files
          </p>
        </div>
      </div>

      {/* Loading state */}
      {isLoading && (
        <div className="flex items-center justify-center py-20">
          <Loader2 className="h-6 w-6 animate-spin text-[#7A2E3B]" />
          <span className="ml-2 text-sm text-gray-500">Loading concepts...</span>
        </div>
      )}

      {/* Error state */}
      {error && (
        <div className="flex items-center gap-3 rounded-lg border border-red-200 bg-red-50 px-4 py-3">
          <AlertCircle className="h-5 w-5 shrink-0 text-red-600" />
          <p className="text-sm text-red-700">
            Failed to load concepts: {error.message}
          </p>
        </div>
      )}

      {/* Empty state */}
      {!isLoading && !error && concepts?.length === 0 && (
        <div className="flex flex-col items-center justify-center py-20 text-center">
          <BookOpen className="h-10 w-10 text-gray-300" />
          <p className="mt-3 text-sm font-medium text-gray-500">No concepts yet</p>
          <p className="mt-1 text-xs text-gray-400">
            Concepts are extracted from stories, plans, and reviews.
          </p>
        </div>
      )}

      {/* Concept grid */}
      {concepts && concepts.length > 0 && (
        <div
          className="grid gap-4"
          style={{ gridTemplateColumns: 'repeat(auto-fill, minmax(220px, 1fr))' }}
        >
          {concepts.map((concept) => (
            <ConceptCard
              key={concept.id}
              concept={concept}
              onClick={() => onSelectConcept(concept.name)}
            />
          ))}
        </div>
      )}
    </div>
  );
}
