import { Link } from '@tanstack/react-router';
import { useConcepts } from '@/hooks/queries';
import type { Concept } from '@/lib/types';
import { cn } from '@/lib/utils';
import { BookOpen, Loader2, AlertCircle } from 'lucide-react';
import { Card } from '@/components/ui/card';

function ConceptCard({ concept }: { concept: Concept }) {
  return (
    <Link
      to="/knowledge/$concept"
      params={{ concept: concept.name }}
      className="group block focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
    >
      <Card className="h-full border-l-[3px] border-l-primary transition-all duration-150 hover:-translate-y-0.5 hover:shadow-md">
        <div className="px-4 py-3.5 flex flex-col gap-1.5">
          <span className="text-sm font-semibold text-foreground group-hover:text-primary transition-colors">
            {concept.name}
          </span>
          <span className="text-xs text-muted-foreground">
            {concept.link_count} {concept.link_count === 1 ? 'reference' : 'references'}
          </span>
        </div>
      </Card>
    </Link>
  );
}

export function KnowledgeHome() {
  const { data: concepts, isLoading, error } = useConcepts();

  return (
    <div className="mx-auto max-w-5xl px-8 py-8">
      {/* Header */}
      <div className="mb-8 flex items-start gap-4">
        <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-primary/10">
          <BookOpen className="h-5 w-5 text-primary" />
        </div>
        <div>
          <h1 className="text-2xl font-semibold tracking-tight text-foreground">
            Connected Knowledge
          </h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Concepts spanning plans, stories, reviews, and files
          </p>
        </div>
      </div>

      {/* Loading state */}
      {isLoading && (
        <div className="flex items-center justify-center py-20">
          <Loader2 className="h-5 w-5 animate-spin text-primary" />
          <span className="ml-2 text-sm text-muted-foreground">Loading concepts...</span>
        </div>
      )}

      {/* Error state */}
      {error && (
        <Card className="border-destructive/30 bg-destructive/5">
          <div className="flex items-center gap-3 px-4 py-3">
            <AlertCircle className="h-4 w-4 shrink-0 text-destructive" />
            <p className="text-sm text-destructive">
              Failed to load concepts: {error.message}
            </p>
          </div>
        </Card>
      )}

      {/* Empty state */}
      {!isLoading && !error && concepts?.length === 0 && (
        <div className="flex flex-col items-center justify-center py-20 text-center">
          <BookOpen className="h-10 w-10 text-muted-foreground/30" />
          <p className="mt-3 text-sm font-medium text-muted-foreground">No concepts yet</p>
          <p className="mt-1 text-xs text-muted-foreground/60">
            Concepts are extracted from stories, plans, and reviews.
          </p>
        </div>
      )}

      {/* Concept grid */}
      {concepts && concepts.length > 0 && (
        <div
          className="grid gap-3"
          style={{ gridTemplateColumns: 'repeat(auto-fill, minmax(220px, 1fr))' }}
        >
          {concepts.map((concept) => (
            <ConceptCard key={concept.id} concept={concept} />
          ))}
        </div>
      )}
    </div>
  );
}
