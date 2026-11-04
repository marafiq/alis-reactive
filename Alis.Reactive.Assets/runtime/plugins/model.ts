import type { BodyStep, InputBinding, ModelPluginData } from "../types/index";
import type { Contribution, Plugin } from "../objects/plugins";
import type { PlanObjects } from "../objects/plan-objects";
import { Environment } from "../grammar/environment";
import { evaluate } from "../grammar/evaluate";
import { bound } from "../grammar/shape";
import { isRecord } from "../shared/record";
import type { Owner } from "../objects/owner";
import { COMMIT_EVENT } from "../plans/mount-events";
import { logger } from "../diagnostics/trace";
import { toJavaScriptString } from "../shared/javascript-string";

const log = logger("model");

interface ModelInput {
  readonly id: string;
  readonly binding: InputBinding;
  readonly objects: PlanObjects;
}

export interface BoundValues {
  valueOf(pattern: string, indexes: readonly number[], inputId?: string): unknown;
  indexesOf(itemPattern: string, ownerIndexes: readonly number[]): (readonly number[])[];
}

export interface ContainedInputs extends BoundValues {
  fill(record: Record<string, unknown>): Record<string, unknown>;
  readonly fields: Record<string, unknown>;
  readonly arrays: string[];
  readonly ids: Record<string, string>;
}

export interface NamedInput {
  readonly id: string;
  readonly element: HTMLElement;
  readonly name: string;
  readonly indexes: readonly number[];
  readonly value: unknown;
}

const inputsById = new Map<string, ModelInput[]>();

function currentValue(input: ModelInput): unknown {
  return evaluate(input.binding.value, { env: Environment.root(), objects: input.objects });
}

function bindInput(input: ModelInput): unknown {
  return bound(currentValue(input), input.binding.shape);
}

function mountedInput(id: string): ModelInput | undefined {
  const input = inputsById.get(id)?.[0];
  return input !== undefined && input.objects.element(id) !== null ? input : undefined;
}

export function invalidClassOf(id: string): string {
  return mountedInput(id)!.binding.invalidClass;
}

export function boundNameOf(id: string): string {
  return mountedInput(id)!.binding.name;
}

export function isMountedInput(element: HTMLElement): boolean {
  if (element.id === "") return false;
  return mountedInput(element.id)?.objects.element(element.id) === element;
}

export function namedInputs(pattern: string): NamedInput[] {
  const names = new RegExp(`^${pattern.split("[*]").map(escapedForPattern).join("\\[(\\d+)\\]")}$`);
  const named: NamedInput[] = [];
  for (const id of inputsById.keys()) {
    const input = mountedInput(id);
    const match = input === undefined ? null : names.exec(input.binding.name);
    if (input === undefined || match === null) continue;
    named.push({
      id,
      element: input.objects.element(id)!,
      name: input.binding.name,
      indexes: match.slice(1).map(Number),
      get value() { return bindInput(input); },
    });
  }
  return named;
}

function escapedForPattern(text: string): string {
  return text.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

function boundValueIn(holder: Node, pattern: string, indexes: readonly number[], inputId?: string): unknown {
  const at = namedInputs(pattern)
    .filter(input => input.indexes.every((index, position) => index === indexes[position]))
    .filter(input => inputId === undefined || input.id === inputId);
  const held = at.filter(input => holder.contains(input.element));
  const read = held.length > 0 ? held[held.length - 1] : at[at.length - 1];
  return read?.value;
}

function itemIndexesIn(holder: Node, itemPattern: string, ownerIndexes: readonly number[]): (readonly number[])[] {
  const items = new Map<string, readonly number[]>();
  for (const input of namedInputs(itemPattern)) {
    const ownedHere = ownerIndexes.every((index, position) => input.indexes[position] === index);
    if (ownedHere && holder.contains(input.element)) items.set(input.indexes.join(","), input.indexes);
  }
  return [...items.values()];
}

const pageModel: BoundValues = {
  valueOf: (pattern, indexes, inputId) => boundValueIn(document, pattern, indexes, inputId),
  indexesOf: (itemPattern, ownerIndexes) => itemIndexesIn(document, itemPattern, ownerIndexes),
};

export function containedInputs(containerId: string): ContainedInputs {
  const container = (): HTMLElement => {
    const element = document.getElementById(containerId);
    if (element === null) throw new Error(`[alis] form #${containerId} is not on the page`);
    return element;
  };
  const inside = (): ModelInput[] => {
    const holder = container();
    const held: ModelInput[] = [];
    for (const id of inputsById.keys()) {
      const input = mountedInput(id);
      if (input !== undefined && holder.contains(input.objects.element(id))) held.push(input);
    }
    return held;
  };
  return {
    valueOf: (pattern, indexes) => boundValueIn(container(), pattern, indexes),
    indexesOf: (itemPattern, ownerIndexes) => itemIndexesIn(container(), itemPattern, ownerIndexes),
    fill(record: Record<string, unknown>) {
      for (const input of inside()) assignAt(record, input.binding.body, bindInput(input));
      return record;
    },
    get fields() {
      const fields: Record<string, unknown> = {};
      for (const input of inside()) fields[input.binding.name] = bindInput(input);
      return fields;
    },
    get arrays() {
      return inside().filter(input => input.binding.list).map(input => input.binding.name);
    },
    get ids() {
      const ids: Record<string, string> = {};
      for (const input of inside()) ids[input.binding.name] = input.id;
      return ids;
    },
  };
}

function assignAt(root: Record<string, unknown>, path: readonly BodyStep[], value: unknown): void {
  let parent: Record<string | number, unknown> = root;
  for (let index = 0; index < path.length - 1; index++) {
    const segment = path[index]!;
    const nextIsIndex = typeof path[index + 1] === "number";
    const existing = parent[segment];
    if (nextIsIndex) parent[segment] = Array.isArray(existing) ? [...existing] : [];
    else parent[segment] = isRecord(existing) ? { ...existing } : {};
    parent = parent[segment] as Record<string | number, unknown>;
  }
  parent[path[path.length - 1]!] = value;
}

function join(input: ModelInput): void {
  const entries = inputsById.get(input.id);
  if (entries === undefined) {
    inputsById.set(input.id, [input]);
    return;
  }
  log.warn("input.duplicate-id", { input: input.id });
  entries.push(input);
}

function leave(input: ModelInput): void {
  const remaining = inputsById.get(input.id)!.filter(entry => entry !== input);
  if (remaining.length === 0) inputsById.delete(input.id);
  else inputsById.set(input.id, remaining);
}

export const modelPlugin: Plugin = {
  object: key => (key === "" ? pageModel : containedInputs(key)),
  contribute({ data, owner, objects }: Contribution) {
    const added: ModelInput[] = [];
    for (const [id, binding] of Object.entries((data as ModelPluginData).inputs)) {
      const input: ModelInput = { id, binding, objects };
      join(input);
      added.push(input);
      announceCommits(input, owner);
    }
    owner.onDispose(() => {
      for (const input of added) leave(input);
    });
  },
};

const announcing = new WeakSet<HTMLElement>();

function announceCommits(input: ModelInput, owner: Owner): void {
  const element = input.objects.element(input.id);
  if (element === null || announcing.has(element)) return;

  const announce = (): void => {
    element.dispatchEvent(new CustomEvent(COMMIT_EVENT, { bubbles: true }));
  };
  try {
    input.objects.subscribe(input.id, [], input.binding.committed, announce, owner);
  } catch (error) {
    log.error("input.commit-unwired", { input: input.id, event: input.binding.committed, error: toJavaScriptString(error) });
    return;
  }
  announcing.add(element);
  owner.onDispose(() => announcing.delete(element));
}
