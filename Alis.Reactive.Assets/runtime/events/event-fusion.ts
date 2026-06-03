// Syncfusion semantic events provide args objects, not DOM Events; abort cleanup
// must go through Syncfusion removeEventListener when the component exposes it.

interface SyncfusionEventSource {
  addEventListener(channel: string, handler: (args: unknown) => void): void;
  removeEventListener?(channel: string, handler: (args: unknown) => void): void;
}

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
