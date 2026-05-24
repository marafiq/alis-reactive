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
  SyncfusionEvents.from(root).subscribe(channel, args => {
    handler(SyncfusionEventPayload.from(args));
  }, ListenerLifetime.from(opts));
}

class SyncfusionEvents {
  private constructor(private readonly source: SyncfusionEventSource) {}

  static from(root: unknown): SyncfusionEvents {
    return new SyncfusionEvents(root as SyncfusionEventSource);
  }

  subscribe(channel: string, handler: (args: unknown) => void, lifetime: ListenerLifetime): void {
    if (lifetime.isAlreadyEnded()) return;

    this.source.addEventListener(channel, handler);
    lifetime.onEnded(() => this.source.removeEventListener?.(channel, handler));
  }
}

class ListenerLifetime {
  private constructor(private readonly signal: AbortSignal | undefined) {}

  static from(opts: AddEventListenerOptions | undefined): ListenerLifetime {
    return new ListenerLifetime(opts?.signal);
  }

  isAlreadyEnded(): boolean {
    return this.signal?.aborted === true;
  }

  onEnded(cleanup: () => void): void {
    if (this.signal === undefined) return;

    this.signal.addEventListener("abort", cleanup, { once: true });
  }
}

class SyncfusionEventPayload {
  static from(args: unknown): unknown {
    const argsWereProvided = args !== null && args !== undefined;
    if (argsWereProvided) return args;

    return {};
  }
}
