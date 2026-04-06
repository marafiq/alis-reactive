// event-native.ts — DOM EventTarget event subscription.
// Native components are DOM elements. addEventListener follows the W3C spec:
// supports AbortSignal cleanup, receives Event/CustomEvent objects.

/**
 * Subscribe to a DOM event on a native component (the DOM element itself).
 * Extracts event data from CustomEvent.detail or falls back to event target.
 */
export function wire(
  root: unknown,
  channel: string,
  handler: (data: unknown) => void,
  opts?: AddEventListenerOptions,
): void {
  (root as EventTarget).addEventListener(channel, (e: Event) => {
    const data = e instanceof CustomEvent
      ? (e.detail ?? {})
      : ((e as any).currentTarget ?? (e as any).target ?? e);
    handler(data);
  }, opts);
}
