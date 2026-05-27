// event-fusion.ts — Syncfusion modelObserver callback subscription.
// SF components use modelObserver.on(event, fn) behind addEventListener.
// Handler receives a plain args object ({value, item, ...}), NOT a DOM Event.
// No AbortSignal support — cleanup via removeEventListener only.

interface SyncfusionEventSource {
  addEventListener(channel: string, handler: (args: unknown) => void): void;
  removeEventListener?(channel: string, handler: (args: unknown) => void): void;
}

/**
 * Subscribe to a Syncfusion semantic event on the SF component instance.
 * The handler receives the SF args object directly — no unwrapping needed.
 */
export function wire(
  root: unknown,
  channel: string,
  handler: (data: unknown) => void,
  opts?: AddEventListenerOptions,
): void {
  const source = root as SyncfusionEventSource;
  if (opts?.signal?.aborted === true) return;

  const listener = (args: unknown) => {
    handler(syncfusionEventPayload(args));
  };

  source.addEventListener(channel, listener);
  opts?.signal?.addEventListener(
    "abort",
    () => source.removeEventListener?.(channel, listener),
    { once: true },
  );
}

function syncfusionEventPayload(args: unknown): unknown {
  const argsWereProvided = args !== null && args !== undefined;
  if (argsWereProvided) return args;

  return {};
}
