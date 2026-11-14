import { assertNever, type Locator, type PlanDocument } from "../Schema/index";
import type { Owner } from "./owner";

const slotPlugin = "alis.slot";

interface SlotHold {
  readonly slot: string;
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
