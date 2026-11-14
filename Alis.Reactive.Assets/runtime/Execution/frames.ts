import { notBuilt, type RequestStatement, type Statement } from "../Schema/index";
import type { Read } from "../ValuesAndSources/index";
import { bind, under, type Context } from "./context";
import { evaluate } from "./expressions";
import { execute } from "./statements";

export type Outcome =
  | { readonly kind: "completed" }
  | { readonly kind: "opens"; readonly frame: Frame }
  | { readonly kind: "forks"; readonly blocks: readonly (readonly Statement[])[]; readonly context: Context }
  | { readonly kind: "waits"; readonly on: Promise<unknown>; readonly settled: (read: Read) => void }
  | { readonly kind: "stops"; readonly reason: string }
  | { readonly kind: "ends" };

export type Frame = BlockFrame | RequestFrame;

export interface LayerSignals {
  ofALayer(): AbortSignal;
}

type RequestState = "check" | "loading" | "guard" | "finally" | "after";

const completed: Outcome = { kind: "completed" };
const ends: Outcome = { kind: "ends" };

function completesNothing(): void {
  return undefined;
}

export class BlockFrame {
  readonly kind = "block";
  readonly isAFinally: boolean;
  readonly #statements: readonly Statement[];
  readonly #completes: () => void;
  #context: Context;
  #next = 0;

  constructor(statements: readonly Statement[], context: Context, isAFinally: boolean, completes: () => void) {
    this.#statements = statements;
    this.#context = context;
    this.isAFinally = isAFinally;
    this.#completes = completes;
  }

  static of(statements: readonly Statement[], context: Context): BlockFrame {
    return new BlockFrame(statements, context, false, completesNothing);
  }

  step(): Outcome {
    const statement = this.#statements[this.#next];
    if (statement === undefined) {
      this.#completes();
      return ends;
    }
    this.#next += 1;
    const step = execute(statement, this.#context);
    switch (step.kind) {
      case "completed":
        this.#context = step.context;
        return completed;
      case "block":
        return { kind: "opens", frame: BlockFrame.of(step.statements, this.#context) };
      case "blocks":
        return { kind: "forks", blocks: step.blocks, context: this.#context };
      case "request":
        return { kind: "opens", frame: new RequestFrame(step.statement, this.#context) };
      case "wait":
        return {
          kind: "waits",
          on: step.on,
          settled: (read) => {
            this.#context = step.settled(read);
          },
        };
      case "stop":
        return { kind: "stops", reason: step.reason };
    }
  }
}

export class RequestFrame {
  readonly kind = "request";
  readonly #statement: RequestStatement;
  #context: Context;
  #state: RequestState = "check";
  #loaded = 0;
  #owed = 0;
  #abrupt = false;

  constructor(statement: RequestStatement, context: Context) {
    this.#statement = statement;
    this.#context = context;
  }

  get endsAbruptly(): boolean {
    return this.#abrupt;
  }

  step(signals: LayerSignals): Outcome {
    switch (this.#state) {
      case "check":
        this.#context = bind(this.#context, this.#statement.argument, evaluate(this.#statement.send, this.#context));
        this.#state = "loading";
        return { kind: "opens", frame: BlockFrame.of(this.#statement.check, this.#context) };
      case "loading": {
        const layer = this.#statement.layers[this.#loaded];
        if (layer === undefined) {
          this.#state = "guard";
          return completed;
        }
        return {
          kind: "opens",
          frame: new BlockFrame(layer.whileLoading, this.#context, false, () => {
            this.#loaded += 1;
          }),
        };
      }
      case "guard":
        throw notBuilt("a request past its WhileLoading: its guard, its send, its routes and its Chained");
      case "finally": {
        const layer = this.#statement.layers[this.#owed - 1];
        if (layer === undefined) {
          this.#state = "after";
          return completed;
        }
        this.#owed -= 1;
        return { kind: "opens", frame: new BlockFrame(layer.finally, under(this.#context, signals.ofALayer()), true, completesNothing) };
      }
      case "after":
        return ends;
    }
  }

  marksAbrupt(): void {
    this.#abrupt = true;
  }

  unwinds(): void {
    this.#abrupt = true;
    if (this.#state === "finally" || this.#state === "after") return;
    this.#owed = this.#loaded;
    this.#state = "finally";
  }
}
