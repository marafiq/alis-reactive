import { useRouterState } from '@tanstack/react-router';

/**
 * Extracts planId from the current URL when the route is plan-scoped
 * (i.e., /plans/:planId/stories/:storyId or /plans/:planId/board).
 *
 * Returns null when the route is global (/stories/:storyId or /board).
 */
export function usePlanContext(): string | null {
  const pathname = useRouterState({ select: (s) => s.location.pathname });

  // Match /plans/:planId/stories/... or /plans/:planId/board
  const match = pathname.match(/^\/plans\/([^/]+)\/(stories|board)/);
  if (match) return decodeURIComponent(match[1]);

  return null;
}
