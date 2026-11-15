import type { Owner } from "../OwnershipAndState/index";
import { assertNever, notBuilt, type AsExpression, type Container, type Expression, type InputBinding, type ModelPluginData, type OpExpression, type PathStep, type PlanDocument, type SentInput } from "../Schema/index";
import { Stop } from "../ValuesAndSources/index";

type ReadValue = (expression: Expression) => unknown;
type Inside = (containerId: string) => ((inputId: string) => boolean) | null;

interface Contribution {
  readonly planId: string;
  readonly place: string;
  readonly data: ModelPluginData;
  readonly read: ReadValue;
}

interface Input {
  readonly binding: InputBinding;
  readonly contribution: Contribution;
}

export interface CapturedContainer {
  readonly kind: "captured";
  readonly id: string;
  readonly body: object;
  readonly inputs: readonly SentInput[];
}

export class PageInputs {
  readonly #inside: Inside;
  readonly #inputs = new Map<string, Input>();
  readonly #containers = new Map<string, Map<Contribution, Container>>();

  constructor(inside: Inside) {
    this.#inside = inside;
  }

  join(plan: PlanDocument, owner: Owner, place: string, read: ReadValue): void {
    const data = Object.values(plan.objects).flatMap(locator =>
      locator.kind === "plugin" && locator.name === "alis.model" && locator.data.kind === "model" ? [locator.data] : []);
    for (const model of data) {
      for (const id of Object.keys(model.inputs)) {
        const first = this.#inputs.get(id);
        if (first !== undefined) {
          throw new Stop("Input '" + id + "' is already held by plan '" + first.contribution.planId + "' " + first.contribution.place +
            "; plan '" + plan.planId + "' " + place + " did not start.");
        }
      }
    }
    for (const model of data) {
      const contribution: Contribution = { planId: plan.planId, place, data: model, read };
      for (const [id, binding] of Object.entries(model.inputs)) this.#inputs.set(id, { binding, contribution });
      for (const [id, container] of Object.entries(model.containers)) {
        let renderings = this.#containers.get(id);
        if (renderings === undefined) {
          renderings = new Map();
          this.#containers.set(id, renderings);
        }
        renderings.set(contribution, container);
      }
      owner.registers(() => { this.#drop(contribution); });
    }
  }

  sent(containerId: string, modelKey: string): object {
    return this.captured(containerId, modelKey).body;
  }

  captured(containerId: string, modelKey: string): CapturedContainer {
    const inside = this.#inside(containerId);
    if (inside === null) throw new Stop("No element has the id '" + containerId + "'.");
    const renderings = this.#containers.get(containerId);
    if (renderings === undefined) throw new Stop("No live plan renders container '" + containerId + "' for model '" + modelKey + "'.");
    const models = [...new Set([...renderings.values()].map(container => container.model))];
    if (!models.includes(modelKey)) throw new Stop("Container '" + containerId + "' is rendered for model '" + models.join("', '") + "', not '" + modelKey + "'.");
    const inputs = [...this.#inputs].filter(([id]) => inside(id));
    const names = new Map<string, string>();
    for (const [id, input] of inputs) {
      const first = names.get(input.binding.name);
      if (first !== undefined) throw new Stop("Inputs '" + first + "' and '" + id + "' both bind '" + input.binding.name + "' inside container '" + containerId + "'.");
      names.set(input.binding.name, id);
    }
    const [first] = inputs;
    const body = first !== undefined && typeof first[1].binding.path[0] === "number" ? [] : {};
    for (const [, input] of inputs) write(body, input.binding.path, input.contribution.read(input.binding.value));
    return { kind: "captured", id: containerId, body, inputs: inputs.map(([id, input]) => ({ name: input.binding.name, entry: { id, binding: input.binding } })) };
  }

  typedText(sent: SentInput): string | null {
    const input = this.#inputs.get(sent.entry.id);
    if (input === undefined) throw new Stop("Input '" + sent.entry.id + "' left the page before its check read it.");
    const converted = convertedRead(input.binding.value);
    const typed = input.contribution.read(converted.expr);
    if (typeof typed !== "string" || typed.trim() === "") return null;
    return input.contribution.read(converted) === null ? typed : null;
  }

  #drop(contribution: Contribution): void {
    for (const id of Object.keys(contribution.data.inputs)) this.#inputs.delete(id);
    for (const id of Object.keys(contribution.data.containers)) {
      const renderings = this.#containers.get(id);
      renderings?.delete(contribution);
      if (renderings?.size === 0) this.#containers.delete(id);
    }
  }
}

const notCoalesced = "a typed text read through a binding whose value is not coalesce(as(read, shape), blank): the contract types InputBinding.value as any expression, though C# writes that form (N3.3)";

function convertedRead(value: Expression): AsExpression {
  switch (value.kind) {
    case "op":
      return coalescedRead(value);
    case "lit":
    case "obj":
    case "ref":
    case "get":
    case "data":
    case "as":
    case "call":
    case "fn":
    case "answer":
    case "record":
    case "list":
    case "signal":
      throw notBuilt(notCoalesced);
    default:
      return assertNever(value);
  }
}

function coalescedRead(value: OpExpression): AsExpression {
  const [converted] = value.args;
  if (value.op !== "coalesce" || converted === undefined) throw notBuilt(notCoalesced);
  switch (converted.kind) {
    case "as":
      return converted;
    case "lit":
    case "obj":
    case "ref":
    case "get":
    case "data":
    case "call":
    case "fn":
    case "answer":
    case "record":
    case "list":
    case "op":
    case "signal":
      throw notBuilt(notCoalesced);
    default:
      return assertNever(converted);
  }
}

function write(holder: object, path: readonly PathStep[], value: unknown): void {
  const [step, ...past] = path;
  if (step === undefined) return;
  if (past.length === 0) {
    Object.defineProperty(holder, step, { value, enumerable: true, writable: true, configurable: true });
    return;
  }
  const child = Object.hasOwn(holder, step) ? Reflect.get(holder, step) as object : typeof past[0] === "number" ? [] : {};
  Object.defineProperty(holder, step, { value: child, enumerable: true, writable: true, configurable: true });
  write(child, past, value);
}
