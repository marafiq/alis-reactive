import { assertNever, type Busy, type BusyTarget } from "../Schema/index";
import { called, got, Stop, value } from "../ValuesAndSources/index";
import { held } from "./wire";

type BusyElement = Pick<Element, "id" | "setAttribute" | "removeAttribute">;
type ElementFor = (id: string) => BusyElement | null;
type ComponentElement = (component: unknown) => BusyElement | null;
type CapturedTarget =
  | { readonly kind: "page" }
  | { readonly kind: "element"; readonly id: string; readonly element: BusyElement }
  | { readonly kind: "missing"; readonly reason: string };
interface Holding {
  count: number;
  readonly target: readonly string[];
  readonly elements: Set<BusyElement>;
  readonly contract: unknown;
}
const pageTarget = Symbol("page busy");
const counts = new Map<string | symbol, Holding>();

export function captureBusy(busy: Busy, elementFor: ElementFor = id => document.getElementById(id), componentElement: ComponentElement = asElement): CapturedBusy {
  switch (busy.kind) {
    case "none":
      return new CapturedBusy([]);
    case "page":
      return new CapturedBusy([{ kind: "page" }]);
    case "targets":
      return new CapturedBusy(busy.values.map(target => capture(target, elementFor, componentElement)));
    default:
      return assertNever(busy);
  }
}

function asElement(component: unknown): BusyElement | null {
  return component instanceof Element ? component : null;
}

function capture(target: BusyTarget, elementFor: ElementFor, componentElement: ComponentElement): CapturedTarget {
  switch (target.kind) {
    case "missing":
      return missing(target.id);
    case "element": {
      const element = elementFor(target.id);
      return element === null ? missing(target.id) : { kind: "element", id: target.id, element };
    }
    case "component": {
      const element = componentElement(held(target.value));
      if (element === null) return { kind: "missing", reason: "The request's busy component holds no element." };
      if (element.id === "") return { kind: "missing", reason: "The request's busy component's element has no id." };
      return { kind: "element", id: element.id, element };
    }
    default:
      return assertNever(target);
  }
}

function missing(id: string): CapturedTarget {
  return { kind: "missing", reason: "The request's busy target '" + id + "' is missing." };
}

export class CapturedBusy {
  readonly #targets: readonly CapturedTarget[];

  constructor(targets: readonly CapturedTarget[]) {
    this.#targets = targets;
  }

  hold(contract: unknown): () => void {
    const targets = new Map<string | symbol, { readonly target: readonly string[]; readonly elements: Set<BusyElement> }>();
    for (const target of this.#targets) {
      switch (target.kind) {
        case "missing":
          throw new Stop(target.reason);
        case "page":
          targets.set(pageTarget, { target: [], elements: new Set() });
          break;
        case "element": {
          const existing = targets.get(target.id);
          if (existing === undefined) targets.set(target.id, { target: [target.id], elements: new Set([target.element]) });
          else existing.elements.add(target.element);
          break;
        }
        default:
          assertNever(target);
      }
    }
    if (targets.size > 0) {
      for (const name of ["show", "hide"]) {
        const method = got(value(contract), [name], "required");
        if (method.kind === "nothing" || typeof method.value !== "function") throw new Stop("The busy contract's '" + name + "' is no method.");
      }
    }
    const held = new Map<string | symbol, Holding>();
    try {
      for (const [key, target] of targets) {
        const existing = counts.get(key);
        const holding = existing ?? { count: 0, target: target.target, elements: new Set<BusyElement>(), contract };
        holding.count += 1;
        counts.set(key, holding);
        held.set(key, holding);
        for (const element of target.elements) {
          holding.elements.add(element);
          element.setAttribute("aria-busy", "true");
        }
        if (holding.count === 1) invoke(holding, "show");
      }
    } catch (error) {
      release(held);
      throw error;
    }
    return () => {
      const errors = release(held);
      if (errors.length > 0) throw errors[0];
    };
  }
}

function invoke(holding: Holding, method: string): void {
  called(value(holding.contract), [method], "required", () => holding.target, "value");
}

function release(held: Map<string | symbol, Holding>): unknown[] {
  const errors: unknown[] = [];
  for (const [key, holding] of held) {
    held.delete(key);
    holding.count -= 1;
    if (holding.count > 0) continue;
    counts.delete(key);
    for (const element of holding.elements) {
      try { element.removeAttribute("aria-busy"); } catch (error) { errors.push(error); }
    }
    try { invoke(holding, "hide"); } catch (error) { errors.push(error); }
  }
  return errors;
}
