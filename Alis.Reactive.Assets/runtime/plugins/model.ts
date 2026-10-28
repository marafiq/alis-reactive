// alis.model (grammar.md 6.3): the inputs of a view, keyed by its planId. Plans that share a planId
// (a view and its partials, including partials loaded into slots) contribute their inputs under their
// owner, and a slot's unload removes exactly its own.
// M1 The values: every mounted input of the composed plan, read through its value expression as MVC
//    binds it (a binding read, grammar.md 2.1). Unmounted inputs are skipped.
// M2 `fields`: the values of M1, flat, keyed by MVC binding name; `arrays` names those holding arrays.
// M3 `fill(record)` writes the values of M1 into a record a request authored, at each input's JSON
//    body path, leaf by leaf, so an authored value beside them (Address.Verified next to Address.City)
//    is kept.
// M4 Each input's `committed` event is re-raised on its element as alis:commit, once per element and
//    for as long as its owner lives. The plan that renders an input contributes it, so the widget
//    exists by then, whenever the form that validates it was wired.
// M5 `boundValueOf(id)`: one input's value as a binding read, for a rule or a gather that reads a
//    field any plan of the composition rendered: what validation checks, so the browser judges what
//    the server will; missing when no plan rendered it.

import type { InputBinding, ModelPluginData } from "../types/index";
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

export interface ModelObject {
  /** Writes every mounted input into the record at its JSON body path; returns the record. */
  fill(record: Record<string, unknown>): Record<string, unknown>;
  readonly fields: Record<string, unknown>;
  readonly arrays: string[];
  /** The value of the input registered under an element id, as MVC binds it; missing when none is. */
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
  return evaluate(input.binding.value, { env: Environment.root(), objects: input.objects });
}

function bindInput(input: ModelInput): unknown {
  return bound(currentValue(input), input.binding.shape);
}

function isMounted(input: ModelInput): boolean {
  return input.objects.element(input.id) !== null;
}

function mountedInput(planId: string, id: string): ModelInput | undefined {
  const input = inputsOf(planId).get(id);
  return input !== undefined && isMounted(input) ? input : undefined;
}

function modelObject(planId: string): ModelObject {
  const mounted = (): ModelInput[] => [...inputsOf(planId).values()].filter(isMounted);
  return {
    fill(record: Record<string, unknown>) {
      for (const input of mounted()) assignAt(record, input.binding.body, bindInput(input));
      return record;
    },
    get fields() {
      const fields: Record<string, unknown> = {};
      for (const input of mounted()) fields[input.binding.name] = bindInput(input);
      return fields;
    },
    get arrays() {
      return [...inputsOf(planId).values()]
        .filter(input => input.binding.list)
        .map(input => input.binding.name);
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

export const modelPlugin: Plugin = {
  object: planId => modelObject(planId),
  // The plan gives alis.model no data but its own: the inputs it rendered.
  contribute({ key, data, owner, objects }: Contribution) {
    const inputs = inputsOf(key);
    const added: ModelInput[] = [];
    for (const [id, binding] of Object.entries((data as ModelPluginData).inputs)) {
      const input: ModelInput = { id, binding, objects };
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
  const element = input.objects.element(input.id);
  if (element === null || announcing.has(element)) return;

  const announce = (): void => {
    element.dispatchEvent(new CustomEvent(COMMIT_EVENT, { bubbles: true }));
  };
  // An input whose component is not constructed yet cannot announce its commits; the form then hears
  // none for it, and the trace says so.
  try {
    input.objects.subscribe(input.id, [], input.binding.committed, announce, owner);
  } catch (error) {
    log.error("input.commit-unwired", { input: input.id, event: input.binding.committed, error: toJavaScriptString(error) });
    return;
  }
  announcing.add(element);
  owner.onDispose(() => announcing.delete(element));
}
