// alis.model (grammar.md 6.3): the inputs of a view, keyed by its planId. Plans that share a planId
// (a view and its partials, including partials loaded into slots) contribute their inputs under their
// owner, and a slot's unload removes exactly its own.
// M1 `values`: every mounted input of the composed plan, read through its value expression as a
//    binding read (the value MVC would bind), nested by its JSON body path. Unmounted inputs are skipped.
// M2 `fields`: the same inputs, flat, keyed by MVC binding name; `arrays` names those holding arrays.

import type { InputBinding, Shape } from "../types/index";
import type { Contribution, Module } from "../objects/modules";
import type { PlanWorld } from "../objects/world";
import { Environment } from "../grammar/environment";
import { evaluate } from "../grammar/evaluate";
import { bound } from "../grammar/shape";

export interface ModelInput {
  readonly id: string;
  readonly binding: InputBinding;
  readonly world: PlanWorld;
}

export interface ModelObject {
  readonly values: Record<string, unknown>;
  readonly fields: Record<string, unknown>;
  readonly arrays: string[];
  /** Every input currently contributed, mounted or not. */
  inputs(): readonly ModelInput[];
  /** The input registered under an element id. */
  input(id: string): ModelInput | undefined;
  /** Whether the input's element is on the page. */
  isMounted(input: ModelInput): boolean;
  /** The input's current value, as MVC would bind it. */
  read(input: ModelInput): unknown;
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

export function readInput(input: ModelInput): unknown {
  const env = Environment.root();
  env.bind(input.binding.value.params[0] ?? "self", input.world.object(input.id));
  return bound(evaluate(input.binding.value.body, { env, world: input.world }), input.binding.shape);
}

function isMounted(input: ModelInput): boolean {
  return input.world.element(input.id) !== null;
}

function describesArray(shape: Shape): boolean {
  if (shape.kind === "nullable") return describesArray(shape.inner);
  return shape.kind === "array";
}

function modelObject(planId: string): ModelObject {
  const mounted = (): ModelInput[] => [...inputsOf(planId).values()].filter(isMounted);
  return {
    get values() {
      const values: Record<string, unknown> = {};
      for (const input of mounted()) assignAt(values, input.binding.body, readInput(input));
      return values;
    },
    get fields() {
      const fields: Record<string, unknown> = {};
      for (const input of mounted()) fields[input.binding.name] = readInput(input);
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
    read: readInput,
  };
}

// A JSON body path: an index segment (Lines[0]) needs an array, a name needs an object. A matching
// node already there is reused; anything else is replaced.
function assignAt(root: Record<string, unknown>, path: readonly (string | number)[], value: unknown): void {
  if (path.length === 0) return;
  let parent: Record<string | number, unknown> = root;
  for (let index = 0; index < path.length - 1; index++) {
    const segment = path[index]!;
    const nextIsIndex = typeof path[index + 1] === "number";
    const existing = parent[segment];
    const reusable = nextIsIndex
      ? Array.isArray(existing)
      : typeof existing === "object" && existing !== null && !Array.isArray(existing);
    if (!reusable) parent[segment] = nextIsIndex ? [] : {};
    parent = parent[segment] as Record<string | number, unknown>;
  }
  parent[path[path.length - 1]!] = value;
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
    }
    owner.onDispose(() => {
      for (const input of added) {
        if (inputs.get(input.id) === input) inputs.delete(input.id);
      }
    });
  },
};

export function modelFor(planId: string): ModelObject {
  return modelObject(planId);
}

export function resetModelForTests(): void {
  inputsByPlan.clear();
}
