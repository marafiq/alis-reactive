class CareSession extends EventTarget {
  override addEventListener(type: "expired" | "flushFailed", callback: ((event: Event) => void) | null, options?: boolean | AddEventListenerOptions): void;
  override addEventListener(type: string, callback: EventListenerOrEventListenerObject | null, options?: boolean | AddEventListenerOptions): void;
  override addEventListener(type: string, callback: EventListenerOrEventListenerObject | null, options?: boolean | AddEventListenerOptions): void {
    super.addEventListener(type, callback, options);
  }

  async renew(): Promise<void> {
    const response = await fetch("/world/session/renew", { method: "POST" });
    if (response.status === 401) this.dispatchEvent(new Event("expired"));
    if (!response.ok) throw new Error("Session renewal refused with " + String(response.status));
  }

  async flush(): Promise<void> {
    const response = await fetch("/world/session/flush", { method: "POST" });
    if (!response.ok) {
      this.dispatchEvent(new Event("flushFailed"));
      throw new Error("Session flush refused with " + String(response.status));
    }
  }
}

function provide(name: string, instance: unknown): void {
  const queue: unknown = Reflect.get(window, "__alisPlugins");
  if (Array.isArray(queue)) {
    queue.push({ name, instance });
    return;
  }
  Reflect.set(window, "__alisPlugins", [{ name, instance }]);
}

provide("session", new CareSession());
