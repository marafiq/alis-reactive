import type { Owner } from "../OwnershipAndState/index";
import {
  assertNever, notBuilt, type AsExpression, type Container, type ContainerCapture, type Expression, type InputBinding, type InputCapture, type ModelPluginData, type OpExpression, type PathStep,
  type PlanDocument, type SentInput, type SentInputs, type ValueRead,
} from "../Schema/index";
import { Stop } from "../ValuesAndSources/index";

type ReadValue = (expression: Expression) => unknown;
type Inside = (containerId: string) => ((inputId: string) => boolean) | null;
type SubmissionOf = (formId: string) => FormData | null;

export class Submission {
  readonly entries: FormData;
  readonly members: object;

  constructor(entries: FormData, members: object) {
    this.entries = entries;
    this.members = members;
  }
}

export interface CheckedEntry {
  readonly path: readonly PathStep[];
  readonly value: ValueRead;
}

export interface CheckedBody {
  readonly body: object;
  readonly inputs: readonly SentInput[];
}

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
  readonly #submissionOf: SubmissionOf;
  readonly #inputs = new Map<string, Input>();
  readonly #containers = new Map<string, Map<Contribution, Container>>();

  constructor(inside: Inside, submissionOf: SubmissionOf) {
    this.#inside = inside;
    this.#submissionOf = submissionOf;
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

  sends(containerId: string, modelKey: string): ContainerCapture {
    if (this.#inside(containerId) === null) return { kind: "missing", id: containerId };
    return this.captured(containerId, modelKey);
  }

  submits(formId: string, modelKey: string): ContainerCapture {
    const submitted = this.#submissionOf(formId);
    if (submitted === null) return { kind: "missing", id: formId };
    const captured = this.captured(formId, modelKey);
    return { ...captured, body: new Submission(submitted, captured.body) };
  }

  input(inputId: string, name: string): InputCapture {
    const input = this.#inputs.get(inputId);
    if (input === undefined) return { kind: "missing", id: inputId, name };
    return { kind: "captured", input: { name, entry: { id: inputId, binding: input.binding } } };
  }

  valueOf(inputId: string): unknown {
    const input = this.#inputs.get(inputId);
    return input === undefined ? undefined : input.contribution.read(input.binding.value);
  }

  checked(inputs: SentInputs, sentValue: unknown, entries: readonly CheckedEntry[]): CheckedBody | null {
    const individual: SentInput[] = [];
    for (const capture of inputs.individual) {
      switch (capture.kind) {
        case "missing":
          return null;
        case "captured":
          individual.push(capture.input);
          break;
        default:
          assertNever(capture);
      }
    }
    const body = typeof sentValue === "object" && sentValue !== null ? structuredClone(sentValue) : {};
    for (const entry of entries) if (entry.value.kind === "held") write(body, entry.path, entry.value.value);
    const container = inputs.container;
    switch (container.kind) {
      case "none":
        return { body, inputs: individual };
      case "missing":
        return null;
      case "captured":
        merge(body, container.body instanceof Submission ? container.body.members : container.body);
        return { body, inputs: [...container.inputs, ...individual] };
      default:
        return assertNever(container);
    }
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

function merge(holder: object, over: unknown): void {
  if (typeof over !== "object" || over === null) return;
  for (const [step, value] of Object.entries(over)) {
    const current: unknown = Reflect.get(holder, step);
    const nests = typeof value === "object" && value !== null && !Array.isArray(value) && typeof current === "object" && current !== null && !Array.isArray(current);
    if (nests) {
      merge(current, value);
      continue;
    }
    Object.defineProperty(holder, step, { value, enumerable: true, writable: true, configurable: true });
  }
}
