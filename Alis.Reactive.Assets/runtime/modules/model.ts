// alis.model (grammar.md 6.3): the inputs of a view, keyed by its planId. Plans that share a planId
// (a view and its partials, including partials loaded into slots) contribute their inputs under their
// owner, and a slot's unload removes exactly its own.
// M1 `values`: every mounted input of the composed plan, read through its value expression as MVC
//    binds it (a binding read, grammar.md 2.1), nested by its JSON body path. Unmounted inputs are
//    skipped.
//    `fill(record)` writes the same values into a record a request authored, leaf by leaf, so an
//    authored value beside them (Address.Verified next to Address.City) is kept.
// M2 `fields`: the same inputs, flat, keyed by MVC binding name; `arrays` names those holding arrays.
// M3 `valueOf(id)`: one input's value converted to its shape, for a plan that reads a field another
//    plan rendered; missing when no plan of the composition rendered it. `boundValueOf(id)` is the
//    same input as a binding read: what validation checks, so the browser judges what the server will.
// M4 Each input's `committed` event is re-raised on its element as alis:commit, once per element and
//    for as long as its owner lives. The plan that renders an input contributes it, so the widget
//    exists by then, whenever the form that validates it was wired.

import type { InputBinding, Shape } from "../types/index";
import type { Contribution, Module } from "../objects/modules";
import type { PlanWorld } from "../objects/world";
import { Environment } from "../grammar/environment";
import { evaluate } from "../grammar/evaluate";
import { bound, shaped } from "../grammar/shape";
import type { Owner } from "../objects/owner";
import { COMMIT_EVENT } from "../plans/mount-events";
import { scope } from "../diagnostics/trace";
import { toJavaScriptString } from "../shared/javascript-string";

const log = scope("model");

export interface ModelInput {
  readonly id: string;
  readonly binding: InputBinding;
  readonly world: PlanWorld;
}

export interface ModelObject {
  readonly values: Record<string, unknown>;
  /** Writes every mounted input into the record at its JSON body path; returns the record. */
  fill(record: Record<string, unknown>): Record<string, unknown>;
  readonly fields: Record<string, unknown>;
  readonly arrays: string[];
  /** Every input currently contributed, mounted or not. */
  inputs(): readonly ModelInput[];
  /** The input registered under an element id. */
  input(id: string): ModelInput | undefined;
  /** Whether the input's element is on the page. */
  isMounted(input: ModelInput): boolean;
  /** The value of the input registered under an element id, converted to its shape; missing when none is. */
  valueOf(id: string): unknown;
  /** The same input's value as MVC binds it; missing when none is. */
  boundValueOf(id: string): unknown;
}

const inputsByPlan = new Map<string, Map<string, ModelInput>>();

function inputsOf(planId: string): Map<string, ModelInput> {
  let inputs = inputsByPlan.get(planId);
  if (!inputs) {
    inputs = new Map();
    inputsByPlan.set(planId, inputs);
  }
  return inputs;
}

function currentValue(input: ModelInput): unknown {
  return evaluate(input.binding.value, { env: Environment.root(), world: input.world });
}

function readInput(input: ModelInput): unknown {
  return shaped(currentValue(input), input.binding.shape);
}

function bindInput(input: ModelInput): unknown {
  return bound(currentValue(input), input.binding.shape);
}

function isMounted(input: ModelInput): boolean {
  return input.world.element(input.id) !== null;
}

function mountedInput(planId: string, id: string): ModelInput | undefined {
  const input = inputsOf(planId).get(id);
  return input !== undefined && isMounted(input) ? input : undefined;
}

function describesArray(shape: Shape): boolean {
  if (shape.kind === "nullable") return describesArray(shape.inner);
  return shape.kind === "array";
}

function modelObject(planId: string): ModelObject {
  const mounted = (): ModelInput[] => [...inputsOf(planId).values()].filter(isMounted);
  const fill = (record: Record<string, unknown>): Record<string, unknown> => {
    for (const input of mounted()) assignAt(record, input.binding.body, bindInput(input));
    return record;
  };
  return {
    get values() {
      return fill({});
    },
    fill,
    get fields() {
      const fields: Record<string, unknown> = {};
      for (const input of mounted()) fields[input.binding.name] = bindInput(input);
      return fields;
    },
    get arrays() {
      return [...inputsOf(planId).values()]
        .filter(input => describesArray(input.binding.shape))
        .map(input => input.binding.name);
    },
    inputs: () => [...inputsOf(planId).values()],
    input: id => inputsOf(planId).get(id),
    isMounted,
    valueOf(id: string) {
      const input = mountedInput(planId, id);
      return input === undefined ? undefined : readInput(input);
    },
    boundValueOf(id: string) {
      const input = mountedInput(planId, id);
      return input === undefined ? undefined : bindInput(input);
    },
  };
}

// A JSON body path: an index segment (Lines[0]) needs an array, a name needs an object. A matching
// node already there keeps its other members, copied first so an object the request read from a
// component is never written into; anything else is replaced.
function assignAt(root: Record<string, unknown>, path: readonly (string | number)[], value: unknown): void {
  if (path.length === 0) return;
  let parent: Record<string | number, unknown> = root;
  for (let index = 0; index < path.length - 1; index++) {
    const segment = path[index]!;
    const nextIsIndex = typeof path[index + 1] === "number";
    const existing = parent[segment];
    if (nextIsIndex) parent[segment] = Array.isArray(existing) ? [...existing] : [];
    else parent[segment] = isPlainRecord(existing) ? { ...existing } : {};
    parent = parent[segment] as Record<string | number, unknown>;
  }
  parent[path[path.length - 1]!] = value;
}

function isPlainRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}

export const modelModule: Module = {
  object: planId => modelObject(planId),
  contribute({ key, data, owner, world }: Contribution) {
    if (data.kind !== "model") return;
    const inputs = inputsOf(key);
    const added: ModelInput[] = [];
    for (const [id, binding] of Object.entries(data.inputs)) {
      const input: ModelInput = { id, binding, world };
      inputs.set(id, input);
      added.push(input);
      announceCommits(input, owner);
    }
    owner.onDispose(() => {
      for (const input of added) {
        if (inputs.get(input.id) === input) inputs.delete(input.id);
      }
    });
  },
};

// Elements whose commit event is already re-raised: a view and its partial may both declare an input.
const announcing = new WeakSet<HTMLElement>();

function announceCommits(input: ModelInput, owner: Owner): void {
  const element = input.world.element(input.id);
  if (element === null || announcing.has(element)) return;

  const announce = (): void => {
    element.ownerDocument.dispatchEvent(new CustomEvent(COMMIT_EVENT, { bubbles: true }));
  };
  try {
    input.world.subscribe(input.id, input.binding.committed, announce, owner);
  } catch (error) {
    log.error("input.commit-unwired", { input: input.id, event: input.binding.committed, error: toJavaScriptString(error) });
    return;
  }
  announcing.add(element);
  owner.onDispose(() => announcing.delete(element));
}

export function modelFor(planId: string): ModelObject {
  return modelObject(planId);
}

export function resetModelForTests(): void {
  inputsByPlan.clear();
}
