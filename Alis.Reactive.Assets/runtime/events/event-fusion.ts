// Syncfusion semantic events provide args objects, not DOM Events; abort cleanup
// must go through Syncfusion removeEventListener when the component exposes it.

interface SyncfusionEventSource {
  readonly isDestroyed: boolean;
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
    () => stopListening(source, channel, listener),
    { once: true },
  );
}

// A host such as Schedule may destroy a component before its slot unloads. Syncfusion drops a
// component's listeners when it is destroyed, so a destroyed component has none left to remove.
function stopListening(source: SyncfusionEventSource, channel: string, listener: (args: unknown) => void): void {
  const listenersDroppedOnDestroy = source.isDestroyed;
  if (listenersDroppedOnDestroy) return;

  source.removeEventListener?.(channel, listener);
}

function syncfusionEventPayload(args: unknown): unknown {
  const argsWereProvided = args !== null && args !== undefined;
  if (argsWereProvided) return args;

  return {};
}
