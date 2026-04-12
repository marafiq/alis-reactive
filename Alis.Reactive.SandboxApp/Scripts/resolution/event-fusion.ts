// event-fusion.ts — Syncfusion modelObserver callback subscription.
// SF components use modelObserver.on(event, fn) behind addEventListener.
// Handler receives a plain args object ({value, item, ...}), NOT a DOM Event.
// No AbortSignal support — cleanup via removeEventListener only.

/**
 * Subscribe to a Syncfusion semantic event on the SF component instance.
 * The handler receives the SF args object directly — no unwrapping needed.
 */
export function wire(
  root: unknown,
  channel: string,
  handler: (data: unknown) => void,
  _opts?: AddEventListenerOptions,
): void {
  (root as any).addEventListener(channel, (args: any) => handler(args ?? {}));
}
