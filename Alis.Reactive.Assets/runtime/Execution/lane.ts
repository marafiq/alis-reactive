import type { Owner } from "../OwnershipAndState/index";
import { assertNever, type Policy, type Statement } from "../Schema/index";
import type { Context } from "./context";
import { Run } from "./run";

type Raised = Omit<Context, "owner">;

export class Lane {
  readonly #owner: Owner;
  readonly #policy: Policy;
  readonly #block: readonly Statement[];
  #inFlight: Run[] = [];
  #due: Raised[] = [];
  #disposed = false;

  constructor(policy: Policy, block: readonly Statement[], owner: Owner) {
    this.#owner = owner;
    this.#policy = policy;
    this.#block = block;
  }

  raise(context: Raised): void {
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

  #replaces(context: Raised): void {
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

  #begins(context: Raised): void {
    const run = Run.ofAHandler(this.#block, { ...context, owner: this.#owner });
    this.#inFlight.push(run);
    run.begin();
  }
}
