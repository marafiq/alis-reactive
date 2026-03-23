import { useParams, useNavigate, Link } from '@tanstack/react-router';
import { useConceptLinks } from '@/hooks/queries';
import type { ConceptLink } from '@/lib/types';
import { cn } from '@/lib/utils';
import { ArrowLeft, Loader2, AlertCircle, Link2 } from 'lucide-react';
import { Card } from '@/components/ui/card';

const TYPE_BADGE_STYLES: Record<ConceptLink['entity_type'], string> = {
  plan: 'bg-purple-100 text-purple-800',
  story: 'bg-violet-100 text-violet-800',
  review: 'bg-amber-100 text-amber-800',
  file: 'bg-muted text-muted-foreground',
};

function TypeBadge({ type }: { type: ConceptLink['entity_type'] }) {
  return (
    <span
      className={cn(
        'inline-flex items-center rounded-full px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wide',
        TYPE_BADGE_STYLES[type],
      )}
    >
      {type}
    </span>
  );
}

function TimelineCard({
  link,
  isLast,
}: {
  link: ConceptLink;
  isLast: boolean;
}) {
  const navigate = useNavigate();

  function handleClick() {
    if (link.entity_type === 'plan') {
      navigate({ to: '/plans/$planId', params: { planId: link.entity_id } });
    } else if (link.entity_type === 'story') {
      navigate({ to: '/stories/$storyId', params: { storyId: link.entity_id } });
    }
  }

  return (
    <div className="relative flex gap-4">
      {/* Timeline line + dot */}
      <div className="flex flex-col items-center">
        <div className="h-3 w-3 shrink-0 rounded-full border-2 border-primary bg-card" />
        {!isLast && <div className="w-px grow bg-border" />}
      </div>

      {/* Card */}
      <button
        type="button"
        onClick={handleClick}
        className="mb-4 w-full text-left focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
      >
        <Card className="transition-all duration-150 hover:border-primary/30 hover:shadow-sm">
          <div className="px-4 py-3.5 flex flex-col gap-2">
            <div className="flex items-center gap-2">
              <TypeBadge type={link.entity_type} />
              {link.source && (
                <span className="text-[10px] text-muted-foreground/60">{link.source}</span>
              )}
            </div>

            <span className="text-sm font-semibold text-foreground">
              {link.title || 'Untitled'}
            </span>

            <span className="font-mono text-xs text-muted-foreground">
              {link.entity_id}
            </span>
          </div>
        </Card>
      </button>
    </div>
  );
}

export function ConceptDetail() {
  const { concept: conceptName } = useParams({ from: '/knowledge/$concept' });
  const { data: links, isLoading, error } = useConceptLinks(conceptName);

  const referenceCount = links?.length ?? 0;

  return (
    <div className="mx-auto max-w-3xl px-8 py-8">
      {/* Header */}
      <div className="mb-8">
        <Link
          to="/knowledge"
          className="mb-4 inline-flex items-center gap-1.5 text-sm text-muted-foreground transition-colors hover:text-primary focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
        >
          <ArrowLeft className="h-4 w-4" />
          All concepts
        </Link>

        <div className="flex items-start gap-4">
          <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-primary/10">
            <Link2 className="h-5 w-5 text-primary" />
          </div>
          <div>
            <h1 className="text-2xl font-semibold tracking-tight text-foreground">
              {conceptName}
            </h1>
            <p className="mt-1 text-sm text-muted-foreground">
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
          <Loader2 className="h-5 w-5 animate-spin text-primary" />
          <span className="ml-2 text-sm text-muted-foreground">Loading links...</span>
        </div>
      )}

      {/* Error state */}
      {error && (
        <Card className="border-destructive/30 bg-destructive/5">
          <div className="flex items-center gap-3 px-4 py-3">
            <AlertCircle className="h-4 w-4 shrink-0 text-destructive" />
            <p className="text-sm text-destructive">
              Failed to load concept links: {error.message}
            </p>
          </div>
        </Card>
      )}

      {/* Empty state */}
      {!isLoading && !error && referenceCount === 0 && (
        <div className="flex flex-col items-center justify-center py-16 text-center">
          <Link2 className="h-10 w-10 text-muted-foreground/30" />
          <p className="mt-3 text-sm font-medium text-muted-foreground">
            No references found
          </p>
          <p className="mt-1 text-xs text-muted-foreground/60">
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
              isLast={index === links.length - 1}
            />
          ))}
        </div>
      )}
    </div>
  );
}
