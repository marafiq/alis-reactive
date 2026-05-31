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
  const target = root as EventTarget;
  target.addEventListener(channel, event => {
    handler(nativeEventPayload(event));
  }, opts);
}

function nativeEventPayload(event: Event): unknown {
  if (event instanceof CustomEvent) return customEventPayload(event);

  return domEventPayload(event);
}

function customEventPayload(event: CustomEvent): unknown {
  const detailWasProvided = event.detail !== null && event.detail !== undefined;
  if (detailWasProvided) return event.detail;

  return {};
}

function domEventPayload(event: Event): unknown {
  if (event.currentTarget !== null) return event.currentTarget;
  if (event.target !== null) return event.target;

  return event;
}
