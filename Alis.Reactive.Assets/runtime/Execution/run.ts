import type { Statement } from "../Schema/index";
import { Stop, value, type Read } from "../ValuesAndSources/index";
import { under, type Context } from "./context";
import { BlockFrame, type Frame, type Outcome } from "./frames";

export type Status = "idle" | "running" | "waiting" | "ended";

export type Cut = "replace" | "dispose";

type Block = readonly Statement[];

function runsNothing(): void {
  return undefined;
}

function rejected(error: unknown): string {
  return "An awaited step rejected, so its pipeline stops: " + (error instanceof Error ? error.message : String(error));
}

export class Run {
  readonly #block: Block;
  readonly #context: Context;
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

  private constructor(block: Block, context: Context, ownSignals: readonly AbortController[], cut: Cut | "none") {
    this.#block = block;
    this.#context = context;
    this.#ownSignals = ownSignals;
    this.#cut = cut;
  }

  static ofAHandler(block: Block, context: Context): Run {
    const own = new AbortController();
    return new Run(block, under(context, own.signal), [own], "none");
  }

  get status(): Status {
    return this.#status;
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

  ofALayer(): AbortSignal {
    const layer = new AbortController();
    if (this.#cut === "dispose") layer.abort();
    this.#layerSignals.push(layer);
    return layer.signal;
  }

  #drive(): void {
    this.#status = "running";
    while (this.#isRunning()) {
      this.#steps();
      for (const cause of this.#cutsInItsStep.splice(0)) this.#lands(cause);
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
        this.#stops(outcome.reason);
        return;
      case "ends":
        this.#frames.pop();
        if (frame.kind === "request" && frame.endsAbruptly) this.#unwinds();
        return;
    }
  }

  #outcomeOf(frame: Frame): Outcome {
    try {
      return frame.kind === "request" ? frame.step(this) : frame.step();
    } catch (stopped) {
      if (!(stopped instanceof Stop)) throw stopped;
      return { kind: "stops", reason: stopped.reason };
    }
  }

  #forks(blocks: readonly Block[], context: Context): void {
    const branches: Run[] = [];
    for (const block of blocks) {
      const branch = new Run(block, context, [], this.#cut === "dispose" ? "dispose" : "none");
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
          this.#stops(rejected(error));
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

  #stops(reason: string): void {
    console.error(reason);
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
    for (const ended of this.#afterItEnds.splice(0)) ended();
  }
}

function isInFlight(run: Run): boolean {
  return run.status !== "ended";
}
