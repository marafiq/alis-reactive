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
  NativeEventSource.from(root).subscribe(channel, event => {
    handler(NativeEventPayload.from(event));
  }, opts);
}

class NativeEventSource {
  private constructor(private readonly target: EventTarget) {}

  static from(root: unknown): NativeEventSource {
    return new NativeEventSource(root as EventTarget);
  }

  subscribe(
    channel: string,
    handler: (event: Event) => void,
    opts: AddEventListenerOptions | undefined,
  ): void {
    this.target.addEventListener(channel, handler, opts);
  }
}

class NativeEventPayload {
  static from(event: Event): unknown {
    if (event instanceof CustomEvent) return CustomEventPayload.from(event);

    return DomEventPayload.from(event);
  }
}

class CustomEventPayload {
  static from(event: CustomEvent): unknown {
    const detailWasProvided = event.detail !== null && event.detail !== undefined;
    if (detailWasProvided) return event.detail;

    return {};
  }
}

class DomEventPayload {
  static from(event: Event): unknown {
    if (event.currentTarget !== null) return event.currentTarget;
    if (event.target !== null) return event.target;

    return event;
  }
}
