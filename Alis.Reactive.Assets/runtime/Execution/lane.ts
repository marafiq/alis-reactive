import { assertNever, type Policy, type Statement } from "../Schema/index";
import type { Context } from "./context";
import { Run } from "./run";

export class Lane {
  readonly #policy: Policy;
  readonly #block: readonly Statement[];
  #inFlight: Run[] = [];
  #due: Context[] = [];
  #disposed = false;

  constructor(policy: Policy, block: readonly Statement[]) {
    this.#policy = policy;
    this.#block = block;
  }

  raise(context: Context): void {
    if (this.#disposed) return;
    this.#inFlight = this.#inFlight.filter((run) => run.status !== "ended");
    switch (this.#policy) {
      case "overlap":
        this.#begins(context);
        return;
      case "skip":
        if (this.#inFlight.length === 0) this.#begins(context);
        return;
      case "replace":
        this.#replaces(context);
        return;
      default:
        assertNever(this.#policy);
    }
  }

  dispose(): void {
    this.#disposed = true;
    this.#due = [];
    for (const run of this.#inFlight) run.cut("dispose");
  }

  #replaces(context: Context): void {
    for (const run of this.#inFlight) run.cut("replace");
    const [inItsOwnStep] = this.#inFlight.filter((run) => run.status === "running");
    if (inItsOwnStep === undefined) {
      this.#begins(context);
      return;
    }
    if (this.#due.length === 0) {
      inItsOwnStep.afterItReturns(() => {
        this.#beginsWhatIsDue();
      });
    }
    this.#due = [context];
  }

  #beginsWhatIsDue(): void {
    for (const context of this.#due.splice(0)) this.#begins(context);
  }

  #begins(context: Context): void {
    const run = Run.ofAHandler(this.#block, context);
    this.#inFlight.push(run);
    run.begin();
  }
}
