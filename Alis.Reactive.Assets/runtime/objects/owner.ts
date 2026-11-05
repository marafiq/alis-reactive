import { logger } from "../diagnostics/trace";
import { toJavaScriptString } from "../shared/javascript-string";

const log = logger("owner");

export class Owner {
  private readonly disposers: (() => void)[] = [];
  private readonly startHooks: (() => void)[] = [];

  private constructor(readonly name: string, readonly isSlot: boolean) {}

  static page(): Owner {
    return new Owner("page", false);
  }

  static slot(id: string): Owner {
    return new Owner(`slot:${id}`, true);
  }

  onDispose(disposer: () => void): void {
    this.disposers.push(disposer);
  }

  whenStarted(hook: () => void): void {
    this.startHooks.push(hook);
  }

  start(): void {
    for (const hook of this.startHooks.splice(0)) hook();
  }

  dispose(): void {
    for (const disposer of this.disposers.splice(0).reverse()) {
      try {
        disposer();
      } catch (error) {
        log.error("dispose.failed", { error: toJavaScriptString(error), owner: this.name });
      }
    }
  }
}
