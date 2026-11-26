import type { LiveCheck } from "../InputsAndIds/index";
import type { Owner } from "../OwnershipAndState/index";
import {
  assertNever, type CheckedBody, type FnExpression, type FormCheck, type FormPlace, type InputEntry, type PathStep, type PlanDocument, type Registration,
  type RegistrationStep, type RuleGroup, type SentInput,
} from "../Schema/index";
import { called, Stop, value } from "../ValuesAndSources/index";

type Apply = (fn: FnExpression, args: readonly unknown[]) => unknown;
type TypedText = (input: SentInput) => string | null;
type Fill = "form" | "json";

export interface Failure {
  readonly name: string;
  readonly message: string;
}

export interface InputsTable {
  typedText(input: SentInput): string | null;
  holds(entry: InputEntry): boolean;
  summaryOf(containerId: string): string;
  liveChecksOf(inputId: string): readonly LiveCheck[];
}

export interface Places {
  showMessage(entry: InputEntry, message: string): void;
  clearMessage(entry: InputEntry): void;
  showSummary(summaryId: string, messages: readonly string[]): void;
  notice(): unknown;
}

const noticeKind = "error";
const notAProblem = "The body ShowValidationErrors shows is no validation problem, {\"errors\": {\"<binding name>\": [\"message\"]}}.";

interface Carried {
  readonly registration: Registration;
  readonly apply: Apply;
}

interface Reached {
  readonly value: unknown;
  readonly path: readonly PathStep[];
}

export class PageForms {
  readonly #table: InputsTable;
  readonly #places: Places;
  readonly #carried = new Map<string, Carried[]>();

  constructor(table: InputsTable, places: Places) {
    this.#table = table;
    this.#places = places;
  }

  join(plan: PlanDocument, owner: Owner, apply: Apply): void {
    for (const locator of Object.values(plan.objects)) {
      if (locator.kind !== "plugin" || locator.name !== "alis.form" || locator.data.kind !== "form") continue;
      const carried: Carried = { registration: locator.data.registration, apply };
      const key = locator.key;
      this.#carried.set(key, [...this.#carried.get(key) ?? [], carried]);
      owner.registers(() => { this.#drop(key, carried); });
    }
  }

  validate(check: FormCheck): readonly Failure[] {
    const [carried] = this.#carried.get(check.registration) ?? [];
    if (carried === undefined) throw new Stop("No live plan carries the rules of '" + check.registration + "'.");
    const failures = new Run(carried, check.body, check.inputs, input => this.#table.typedText(input)).failures();
    this.#show(failures, check.inputs, check.place);
    return failures;
  }

  clear(entry: InputEntry): void {
    if (entry.binding.message === "") return;
    this.#places.clearMessage(entry);
  }

  checkLive(entry: InputEntry): void {
    if (entry.binding.message === "") return;
    const checks = this.#table.liveChecksOf(entry.id);
    if (checks.length === 0) return;
    const [first] = checks.flatMap(check => {
      const sent = check.inputs.find(input => input.entry.id === entry.id);
      return sent === undefined ? [] : this.#liveFailures(check).filter(failure => failure.name === sent.name);
    });
    this.#places.clearMessage(entry);
    if (first !== undefined) this.#places.showMessage(entry, first.message);
  }

  #liveFailures(check: LiveCheck): readonly Failure[] {
    const [carried] = this.#carried.get(check.registration) ?? [];
    if (carried === undefined) throw new Stop("No live plan carries the rules of '" + check.registration + "'.");
    return new Run(carried, check.body, check.inputs, input => this.#table.typedText(input)).failures();
  }

  showErrors(problem: unknown, inputs: readonly SentInput[], place: FormPlace): void {
    this.#show(failuresOf(problem), inputs, place);
  }

  #show(failures: readonly Failure[], inputs: readonly SentInput[], place: FormPlace): void {
    const atTheirInputs = inputs.filter(input => input.entry.binding.message !== "" && this.#table.holds(input.entry));
    for (const input of atTheirInputs) this.#places.clearMessage(input.entry);
    this.#summarize(failures.map(failure => failure.message), place);
    for (const input of atTheirInputs) {
      const failure = failures.find(failed => failed.name === input.name);
      if (failure !== undefined) this.#places.showMessage(input.entry, failure.message);
    }
  }

  #summarize(messages: readonly string[], place: FormPlace): void {
    const destination = place.destination;
    switch (destination.kind) {
      case "summary":
        this.#places.showSummary(this.#table.summaryOf(destination.containerId), messages);
        return;
      case "notice":
        if (messages.length > 0) called(value(this.#places.notice()), ["show"], "required", () => [messages.join(destination.separator), noticeKind], "value");
        return;
      default:
        assertNever(destination);
    }
  }

  #drop(key: string, carried: Carried): void {
    const left = (this.#carried.get(key) ?? []).filter(held => held !== carried);
    if (left.length === 0) this.#carried.delete(key);
    else this.#carried.set(key, left);
  }
}

class Run {
  readonly #registration: Registration;
  readonly #apply: Apply;
  readonly #body: CheckedBody;
  readonly #inputs: readonly SentInput[];
  readonly #typedText: TypedText;
  readonly #failures: Failure[] = [];
  readonly #refused: (readonly PathStep[])[] = [];

  constructor(carried: Carried, body: CheckedBody, inputs: readonly SentInput[], typedText: TypedText) {
    this.#registration = carried.registration;
    this.#apply = carried.apply;
    this.#body = body;
    this.#inputs = inputs;
    this.#typedText = typedText;
  }

  failures(): readonly Failure[] {
    for (const input of this.#inputs) this.#typed(input);
    this.#group(this.#registration.rules, { value: checkedBody(this.#registration, this.#body), path: [] });
    return this.#failures;
  }

  #typed(input: SentInput): void {
    const path = input.path;
    const scalar = this.#registration.scalars.find(bound => matches(bound.path, path));
    if (scalar === undefined) return;
    const text = this.#typedText(input);
    if (text === null) return;
    this.#failures.push({ name: input.name, message: scalar.before + text + scalar.after });
    this.#refused.push(path);
  }

  #group(group: RuleGroup, holder: Reached): void {
    for (const instance of reach(holder, group.path)) {
      if (group.path.length > 0 && this.#isRefused(instance.path)) continue;
      if (!this.#holds(group.when, instance.value)) continue;
      for (const field of group.fields) {
        for (const member of reach(instance, field.path)) {
          if (this.#isRefused(member.path)) continue;
          for (const test of field.tests) {
            if (!this.#holds(test.when, instance.value)) continue;
            if (!this.#holds(test.check, member.value, instance.value)) this.#failures.push({ name: this.#nameOf(member.path), message: test.message });
          }
        }
      }
      for (const inner of group.groups) this.#group(inner, instance);
    }
  }

  #holds(test: FnExpression, ...values: unknown[]): boolean {
    return this.#apply(test, values) === true;
  }

  #isRefused(path: readonly PathStep[]): boolean {
    return this.#refused.some(refused => same(refused, path));
  }

  #nameOf(path: readonly PathStep[]): string {
    return this.#inputs.find(input => same(input.path, path))?.name ?? "";
  }
}

function failuresOf(problem: unknown): readonly Failure[] {
  const errors: unknown = isHolder(problem) ? Reflect.get(problem, "errors") : undefined;
  if (!isHolder(errors)) throw new Stop(notAProblem);
  return Object.entries(errors).flatMap(([name, held]: [string, unknown]) => {
    if (!Array.isArray(held)) throw new Stop(notAProblem);
    const messages: readonly unknown[] = held;
    if (!messages.every((message): message is string => typeof message === "string")) throw new Stop(notAProblem);
    return messages.map(message => ({ name, message }));
  });
}

function checkedBody(registration: Registration, body: CheckedBody): unknown {
  switch (body.format) {
    case "none":
    case "text":
    case "file":
    case "urlEncoded":
    case "multipart":
      return bound(registration, structuredClone(registration.start), body.value, [], "form");
    case "json":
      return bound(registration, structuredClone(registration.start), body.value, [], "json");
    default:
      return assertNever(body.format);
  }
}

function bound(registration: Registration, start: unknown, sent: unknown, at: readonly PathStep[], fill: Fill): unknown {
  if (Array.isArray(sent)) return boundList(registration, start, sent, at, fill);
  if (isHolder(sent)) return boundObject(registration, start, sent, at, fill);
  return fill === "form" && sent === "" ? null : sent;
}

function boundObject(registration: Registration, start: unknown, sent: object, at: readonly PathStep[], fill: Fill): object {
  const declared = registration.objects.find(object => matches(object.path, at));
  const replaced = declared !== undefined && fill === "json" && declared.json === "replace";
  const base: unknown = !replaced && isHolder(start) ? start : declared === undefined ? {} : structuredClone(declared.start);
  const holder = isHolder(base) ? base : {};
  for (const [name, value] of Object.entries(sent)) {
    Object.defineProperty(holder, name, { value: bound(registration, Reflect.get(holder, name), value, [...at, name], fill), enumerable: true, writable: true, configurable: true });
  }
  return holder;
}

function boundList(registration: Registration, start: unknown, sent: readonly unknown[], at: readonly PathStep[], fill: Fill): unknown[] {
  const declared = registration.lists.find(list => matches(list.path, at));
  const kept: readonly unknown[] = declared !== undefined && fill === "json" && declared.json === "append" && Array.isArray(start) ? start : [];
  const items = sent.map((item, index) => bound(registration, declared === undefined ? undefined : structuredClone(declared.item), item, [...at, kept.length + index], fill));
  return [...kept, ...items];
}

function reach(holder: Reached, path: readonly RegistrationStep[]): Reached[] {
  const [step, ...past] = path;
  if (step === undefined) return [holder];
  if (typeof step === "object") {
    if (!Array.isArray(holder.value)) return [];
    const items: readonly unknown[] = holder.value;
    return items.flatMap((item, index) => reach({ value: item, path: [...holder.path, index] }, past));
  }
  const value: unknown = isHolder(holder.value) ? Reflect.get(holder.value, step) : undefined;
  return reach({ value, path: [...holder.path, step] }, past);
}

function matches(declared: readonly RegistrationStep[], path: readonly PathStep[]): boolean {
  return declared.length === path.length && declared.every((step, at) => typeof step === "object" ? typeof path[at] === "number" : step === path[at]);
}

function same(one: readonly PathStep[], other: readonly PathStep[]): boolean {
  return one.length === other.length && one.every((step, at) => step === other[at]);
}

function isHolder(value: unknown): value is object {
  return typeof value === "object" && value !== null && !Array.isArray(value) && !isAWholeValue(value);
}

function isAWholeValue(value: object): boolean {
  return value instanceof Date || value instanceof Blob;
}
