import { assertNever, type Locator, type PlanDocument } from "../Schema/index";
import type { Owner } from "./owner";

const slotPlugin = "alis.slot";

const documentPositionFollowing = 4;

interface SlotHold {
  readonly slot: string;
}

interface InTheDocument {
  readonly parentElement: InTheDocument | null;
  compareDocumentPosition(other: InTheDocument): number;
}

type ElementOf = (id: string) => InTheDocument | null;

interface HeldAt {
  readonly slot: string;
  readonly container: InTheDocument;
}

const slotHolds: SlotHold[] = [];

export function slotsNamedBy(plan: PlanDocument): string[] {
  return Object.values(plan.objects).flatMap(slotsNamedAt);
}

export function holdSlots(slots: readonly string[], owner: Owner): void {
  for (const slot of slots) {
    const hold: SlotHold = { slot };
    slotHolds.push(hold);
    owner.holds(() => {
      slotHolds.splice(slotHolds.indexOf(hold), 1);
    });
  }
}

export function heldSlots(): ReadonlySet<string> {
  return new Set(slotHolds.map((hold) => hold.slot));
}

export function heldSlotsAtOrIn(element: object, elementOf: ElementOf): string[] {
  return [...heldSlots()]
    .flatMap((slot) => heldAt(element, slot, elementOf))
    .sort((one, other) => ((one.container.compareDocumentPosition(other.container) & documentPositionFollowing) === documentPositionFollowing ? -1 : 1))
    .map((held) => held.slot);
}

function heldAt(element: object, slot: string, elementOf: ElementOf): HeldAt[] {
  const container = elementOf(slot);
  if (container === null) return [];
  for (let around: InTheDocument | null = container; around !== null; around = around.parentElement) {
    if (around === element) return [{ slot, container }];
  }
  return [];
}

function slotsNamedAt(locator: Locator): string[] {
  switch (locator.kind) {
    case "plugin":
      return locator.name === slotPlugin ? [locator.key] : [];
    case "element":
      return [];
    default:
      return assertNever(locator);
  }
}
