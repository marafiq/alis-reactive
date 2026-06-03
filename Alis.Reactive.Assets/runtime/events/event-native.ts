// Native components are DOM EventTargets; payload extraction follows
// Event/CustomEvent semantics.
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
