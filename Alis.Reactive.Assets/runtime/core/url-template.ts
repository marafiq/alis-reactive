// url-template.ts — URL template parameter resolution.
// Pure function: resolves {param} placeholders using resolved route values.

const ROUTE_PARAM_RE = /\{(\w+)\}/g;

/** Resolve {param} placeholders in a URL template using resolved route values. */
export function resolveRouteParams(
  urlTemplate: string,
  routeParams: Record<string, string>,
): string {
  return urlTemplate.replace(ROUTE_PARAM_RE, (_match, paramName: string) =>
    encodeURIComponent(routeParams[paramName]!));
}
