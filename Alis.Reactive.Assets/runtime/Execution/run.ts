import type { Owner } from "../OwnershipAndState/index";
import type { Statement } from "../Schema/index";
import type { Attributes, EventName, SpanOutcome, Traced } from "../Tracing/index";
import { Stop, value, type Read } from "../ValuesAndSources/index";
import { under, within, type Context } from "./context";
import { BlockFrame, type Frame, type Outcome } from "./frames";

export type Status = "idle" | "running" | "waiting" | "ended";

export type Cut = "replace" | "dispose";

type Block = readonly Statement[];

interface Halted {
  readonly kind: "halts";
  readonly reason: string;
}

interface Thrown {
  readonly kind: "throws";
  readonly error: unknown;
}

interface Recorded {
  readonly name: EventName;
  readonly attributes: Attributes;
}

const aPageMismatch: Recorded = { name: "alis.stop", attributes: {} };
const anError: Recorded = { name: "alis.error", attributes: {} };

function runsNothing(): void {
  return undefined;
}

function messageOf(error: unknown): string {
  return error instanceof Error ? error.message : String(error);
}

function rejected(error: unknown): string {
  return "An awaited step rejected, so its pipeline stops: " + messageOf(error);
}

function aStopStatement(reason: string): Recorded {
  return { name: "alis.stop", attributes: { "alis.reason": reason } };
}

function outcomeOf(cause: Cut): SpanOutcome {
  return cause === "replace" ? "replaced" : "disposed";
}

const inTheirStep: Run[] = [];

export function cutsRunsInTheirStep(owners: readonly Owner[], returned: () => void): boolean {
  const ending = inTheirStep.filter((run) => owners.includes(run.owner));
  const [outermost] = ending;
  if (outermost === undefined) return false;
  for (const run of ending) run.cut("dispose");
  outermost.afterItReturns(returned);
  return true;
}

export class Run {
  readonly #block: Block;
  readonly #context: Context;
  readonly #trace: Traced;
  readonly #ownSignals: readonly AbortController[];
  readonly #layerSignals: AbortController[] = [];
  readonly #frames: Frame[] = [];
  readonly #afterItReturns: (() => void)[] = [];
  readonly #afterItEnds: (() => void)[] = [];
  #status: Status = "idle";
  #cut: Cut | "none";
  #cutsInItsStep: Cut[] = [];
  #branches: Run[] = [];
  #unwindsAtItsJoin = false;
  #waited = 0;
  #outcome: SpanOutcome = "completed";

  private constructor(block: Block, context: Context, trace: Traced, ownSignals: readonly AbortController[], cut: Cut | "none") {
    this.#block = block;
    this.#context = within(context, trace);
    this.#trace = trace;
    this.#ownSignals = ownSignals;
    this.#cut = cut;
  }

  static ofAHandler(block: Block, context: Context): Run {
    const own = new AbortController();
    return new Run(block, under(context, own.signal), context.trace.run(), [own], "none");
  }

  get status(): Status {
    return this.#status;
  }

  get owner(): Owner {
    return this.#context.owner;
  }

  begin(): void {
    this.#frames.push(BlockFrame.of(this.#block, this.#context));
    this.#drive();
  }

  cut(cause: Cut): void {
    if (this.#status === "ended" || this.#cut === "dispose" || this.#cut === cause) return;
    this.#cut = cause;
    if (this.#status === "running") {
      this.#cutsInItsStep = [cause];
      return;
    }
    this.#lands(cause);
    if (this.#isRunning()) this.#drive();
  }

  afterItReturns(returned: () => void): void {
    this.#afterItReturns.push(returned);
  }

  afterItEnds(ended: () => void): void {
    this.#afterItEnds.push(ended);
  }

  ofALayer(): AbortSignal {
    const layer = new AbortController();
    if (this.#cut === "dispose") layer.abort();
    this.#layerSignals.push(layer);
    return layer.signal;
  }

  #drive(): void {
    this.#status = "running";
    inTheirStep.push(this);
    try {
      while (this.#isRunning()) {
        this.#steps();
        for (const cause of this.#cutsInItsStep.splice(0)) this.#lands(cause);
      }
    } finally {
      inTheirStep.pop();
    }
    for (const returned of this.#afterItReturns.splice(0)) returned();
  }

  #isRunning(): boolean {
    return this.#status === "running";
  }

  #steps(): void {
    const frame = this.#frames.at(-1);
    if (frame === undefined) {
      this.#ends();
      return;
    }
    const outcome = this.#outcomeOf(frame);
    switch (outcome.kind) {
      case "completed":
        return;
      case "opens":
        this.#frames.push(outcome.frame);
        return;
      case "forks":
        this.#forks(outcome.blocks, outcome.context);
        return;
      case "waits":
        this.#waits(outcome.on, outcome.settled);
        return;
      case "stops":
        this.#stops(outcome.reason, aStopStatement(outcome.reason));
        return;
      case "halts":
        this.#stops(outcome.reason, aPageMismatch);
        return;
      case "throws":
        this.#stops(messageOf(outcome.error), anError);
        return;
      case "ends":
        this.#frames.pop();
        if (frame.kind === "request" && frame.endsAbruptly) this.#unwinds();
        return;
    }
  }

  #outcomeOf(frame: Frame): Outcome | Halted | Thrown {
    try {
      return frame.kind === "request" ? frame.step(this) : frame.step();
    } catch (thrown) {
      return thrown instanceof Stop ? { kind: "halts", reason: thrown.reason } : { kind: "throws", error: thrown };
    }
  }

  #forks(blocks: readonly Block[], context: Context): void {
    const branches: Run[] = [];
    for (const block of blocks) {
      const branch = new Run(block, context, context.trace.span("alis.all", {}), [], this.#cut === "dispose" ? "dispose" : "none");
      branches.push(branch);
      branch.begin();
    }
    this.#branches = branches.filter(isInFlight);
    if (this.#branches.length === 0) return;
    this.#waits(Promise.all(this.#branches.map((branch) => branch.#whenItEnds())), runsNothing);
  }

  #whenItEnds(): Promise<void> {
    return new Promise((resolve) => {
      this.#afterItEnds.push(resolve);
    });
  }

  #waits(on: Promise<unknown>, settled: (read: Read) => void): void {
    if (this.#cut === "dispose") {
      on.then(runsNothing, runsNothing);
      this.#unwinds();
      return;
    }
    this.#waited += 1;
    const wait = this.#waited;
    this.#status = "waiting";
    on.then(
      (held) => {
        this.#settles(wait, () => {
          settled(value(held));
        });
      },
      (error: unknown) => {
        this.#settles(wait, () => {
          this.#stops(rejected(error), anError);
        });
      },
    );
  }

  #settles(wait: number, goesOn: () => void): void {
    if (wait !== this.#waited) return;
    this.#branches = [];
    if (this.#unwindsAtItsJoin) {
      this.#unwindsAtItsJoin = false;
      this.#unwinds();
    } else goesOn();
    this.#drive();
  }

  #stops(reason: string, recorded: Recorded): void {
    console.error(reason);
    this.#trace.event(recorded.name, recorded.attributes);
    if (this.#outcome === "completed") this.#outcome = "stopped";
    this.#unwinds();
  }

  #unwinds(): void {
    for (let frame = this.#frames.at(-1); frame !== undefined; frame = this.#frames.at(-1)) {
      if (frame.kind === "request") {
        frame.unwinds();
        return;
      }
      this.#frames.pop();
    }
  }

  #lands(cause: Cut): void {
    if (this.#outcome === "completed") this.#outcome = outcomeOf(cause);
    for (const own of this.#ownSignals) own.abort();
    if (cause === "dispose") for (const layer of this.#layerSignals) layer.abort();
    const outermostFinally = this.#frames.findIndex((frame) => frame.kind === "block" && frame.isAFinally);
    const goesOnInItsFinally = outermostFinally >= 0 && (cause === "replace" || this.#isRunning());
    if (goesOnInItsFinally) {
      for (const frame of this.#frames.slice(0, outermostFinally)) if (frame.kind === "request") frame.marksAbrupt();
      return;
    }
    for (const branch of this.#branches) branch.cut(cause);
    if (this.#branches.some(isInFlight)) {
      this.#unwindsAtItsJoin = true;
      return;
    }
    this.#waited += 1;
    this.#branches = [];
    this.#unwindsAtItsJoin = false;
    this.#unwinds();
    this.#status = "running";
  }

  #ends(): void {
    this.#status = "ended";
    this.#trace.end(this.#outcome);
    for (const ended of this.#afterItEnds.splice(0)) ended();
  }
}

function isInFlight(run: Run): boolean {
  return run.status !== "ended";
}
