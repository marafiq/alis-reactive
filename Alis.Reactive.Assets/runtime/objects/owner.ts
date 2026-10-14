// An owner is the page or a slot (grammar.md 3.5). It holds disposers for what was done on its behalf
// (handler subscriptions, plugin data contributions, connection subscriptions) and knows nothing about
// what they are. Starting an owner runs the work waiting for its start: the document's `ready`.

import { scope } from "../diagnostics/trace";
import { toJavaScriptString } from "../shared/javascript-string";

const log = scope("owner");

export class Owner {
  private readonly disposers: (() => void)[] = [];
  private readonly startHooks: (() => void)[] = [];
  private started = false;
  private disposed = false;

  private constructor(readonly name: string, readonly isSlot: boolean) {}

  static page(): Owner {
    return new Owner("page", false);
  }

  static slot(id: string): Owner {
    return new Owner(`slot:${id}`, true);
  }

  get isDisposed(): boolean {
    return this.disposed;
  }

  onDispose(disposer: () => void): void {
    if (this.disposed) {
      disposer();
      return;
    }
    this.disposers.push(disposer);
  }

  whenStarted(hook: () => void): void {
    if (this.started) return;
    this.startHooks.push(hook);
  }

  start(): void {
    if (this.started) return;
    this.started = true;
    for (const hook of this.startHooks.splice(0)) {
      try {
        hook();
      } catch (error) {
        log.error("start.hook-failed", { error: toJavaScriptString(error), owner: this.name });
      }
    }
  }

  dispose(): void {
    if (this.disposed) return;
    this.disposed = true;
    for (const disposer of this.disposers.splice(0).reverse()) {
      try {
        disposer();
      } catch (error) {
        log.error("dispose.failed", { error: toJavaScriptString(error), owner: this.name });
      }
    }
  }
}
