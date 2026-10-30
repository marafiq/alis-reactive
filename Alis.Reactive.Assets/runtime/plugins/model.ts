// alis.model (grammar.md 6.3): one table of the page's inputs, keyed by element id. Every plan contributes
// the inputs it rendered under its owner, whichever plan or request rendered them, and an owner's release
// (a slot's unload) removes exactly its own entries. The plan id keys nothing.
// M1 The values: the mounted inputs inside a container (`plugin("alis.model", containerId)`), each read
//    through its value expression as MVC binds it (a binding read, grammar.md 2.1). An input not on the
//    page, or outside the container, is not sent; a container not on the page fails the request.
// M2 `fields`: the values of M1, flat, keyed by MVC binding name; `arrays` names those holding arrays.
// M3 `fill(record)` writes the values of M1 into a record a request authored, at each input's JSON
//    body path, leaf by leaf, so an authored value beside them (Address.Verified next to Address.City)
//    is kept.
// M4 Each input's `committed` event is re-raised on its element as alis:commit, once per element and
//    for as long as its owner lives. The plan that renders an input contributes it, so the widget
//    exists by then.
// M5 `boundValueOf(id)` on the page's table (`plugin("alis.model", "")`): the value of the mounted input
//    with that id, whichever plan rendered it, as a binding read, for a bound read or a named gather;
//    missing when none is on the page.
// M6 An id names one element. Two live entries for one id (two slots that each loaded one partial, which
//    no single request can render) both read the element the DOM finds by that id, the first in document
//    order; each owner's release removes only its own entry, so the other slot's input is read once the
//    first leaves.
// M7 `ids`: the element id of each mounted input inside a container, keyed by binding name, so the form
//    places a server message beside the input that binds the name the server reports, and clears every
//    message inside it, whichever plan rendered the input.
// M8 Names. A binding-name pattern names the mounted inputs whose binding name it spells, each `[*]` standing
//    for an index: Contacts[*].Name names Contacts[0].Name and Contacts[12].Name, Tags[*] each tag. `namedInputs`
//    lists every one on the page, with the index each `[*]` stood for, whichever plan rendered it (the form's
//    fields, F1). `valueOf(pattern, indexes)` on a container reads, as a binding read, the input that binds the
//    pattern at those indexes, the first `[*]` the first index: the one inside the container, else the one
//    elsewhere on the page (a member the request sends beside the form), the one contributed last of several;
//    missing when none is on the page, as the server sees a member no input posts.

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

export interface PageModel {
  /** The value of the mounted input with this element id, as MVC binds it; missing when none is on the page. */
  boundValueOf(id: string): unknown;
}

export interface ContainedInputs {
  /** Writes every mounted input inside the container into the record at its JSON body path; returns the record. */
  fill(record: Record<string, unknown>): Record<string, unknown>;
  readonly fields: Record<string, unknown>;
  readonly arrays: string[];
  /** The element id of every mounted input inside the container, keyed by MVC binding name (M7). */
  readonly ids: Record<string, string>;
  /** The value, as MVC binds it, of the input that binds the pattern at these indexes: inside the container, else on the page (M8). */
  valueOf(pattern: string, indexes: readonly number[]): unknown;
}

/** A mounted input a binding-name pattern names (M8). */
export interface NamedInput {
  readonly id: string;
  readonly element: HTMLElement;
  /** Its binding name, the pattern's `[*]` filled in. */
  readonly name: string;
  /** What each `[*]` of the pattern stood for, left to right. */
  readonly indexes: readonly number[];
  /** Its value as MVC binds it, read when asked. */
  readonly value: unknown;
}

// Every live entry for an id, in the order contributed (M6).
const inputsById = new Map<string, ModelInput[]>();

function currentValue(input: ModelInput): unknown {
  return evaluate(input.binding.value, { env: Environment.root(), objects: input.objects });
}

function bindInput(input: ModelInput): unknown {
  return bound(currentValue(input), input.binding.shape);
}

// Every entry of an id reads the one element the DOM finds by it, so the first entry speaks for the id.
function mountedInput(id: string): ModelInput | undefined {
  const input = inputsById.get(id)?.[0];
  return input !== undefined && input.objects.element(id) !== null ? input : undefined;
}

const pageModel: PageModel = {
  boundValueOf(id: string) {
    const input = mountedInput(id);
    return input === undefined ? undefined : bindInput(input);
  },
};

/** Whether the element is a mounted input of the page's table: the element the DOM finds by its id (M6). */
export function isMountedInput(element: HTMLElement): boolean {
  if (element.id === "") return false;
  return mountedInput(element.id)?.objects.element(element.id) === element;
}

/** Every mounted input on the page whose binding name the pattern names, whichever plan rendered it (M8). */
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
    valueOf(pattern: string, indexes: readonly number[]) {
      const holder = container();
      const at = namedInputs(pattern).filter(input => input.indexes.every((index, position) => index === indexes[position]));
      const held = at.filter(input => holder.contains(input.element));
      const read = held.length > 0 ? held[held.length - 1] : at[at.length - 1];
      return read?.value;
    },
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

function join(input: ModelInput): void {
  const entries = inputsById.get(input.id);
  if (entries === undefined) {
    inputsById.set(input.id, [input]);
    return;
  }
  // The server refuses an id rendered twice in one request; two requests can still bring it twice (M6).
  log.warn("input.duplicate-id", { input: input.id });
  entries.push(input);
}

function leave(input: ModelInput): void {
  const remaining = inputsById.get(input.id)!.filter(entry => entry !== input);
  if (remaining.length === 0) inputsById.delete(input.id);
  else inputsById.set(input.id, remaining);
}

export const modelPlugin: Plugin = {
  // The page's table is the singleton; any other key is the container whose inputs a request sends.
  object: key => (key === "" ? pageModel : containedInputs(key)),
  // The plan gives alis.model no data but its own: the inputs it rendered.
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

// Elements whose commit event is already re-raised: two entries may hold one element (M6).
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
