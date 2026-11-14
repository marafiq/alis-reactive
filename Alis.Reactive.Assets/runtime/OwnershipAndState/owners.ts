import { Holdings, type Owner, type ReleaseStep } from "./owner";
import { heldSlots, holdSlots } from "./slot-holds";

export interface Placed {
  readonly id: string;
  readonly parentElement: Placed | null;
}

export interface Thrown {
  readonly slot: string;
  readonly step: ReleaseStep;
  readonly error: unknown;
}

interface LiveSlot {
  readonly container: Placed;
  readonly owner: Holdings;
}

const liveSlots = new Map<string, LiveSlot>();

export const page: Owner = new Holdings();

export function slotOwner(container: Placed): Owner {
  const live = liveSlots.get(container.id);
  if (live !== undefined) return live.owner;
  const owner = new Holdings();
  liveSlots.set(container.id, { container, owner });
  holdSlots([container.id], owner);
  return owner;
}

export function ownerAround(placed: Placed, named: ReadonlySet<string>): Owner {
  const held = heldSlots();
  const innermost = elementsAround(placed).find((around) => named.has(around.id) || held.has(around.id));
  return innermost === undefined ? page : slotOwner(innermost);
}

export function releaseSlot(container: Placed): Thrown[] {
  const leaving = [...insideInnermostFirst(container), ...liveAt(container)];
  for (const slot of leaving) liveSlots.delete(slot.container.id);
  return leaving.flatMap((slot) => slot.owner.release().map((thrown) => ({ slot: slot.container.id, ...thrown })));
}

function insideInnermostFirst(container: Placed): LiveSlot[] {
  return [...liveSlots.values()]
    .map((slot) => ({ slot, around: elementsAround(slot.container) }))
    .filter(({ around }) => around.includes(container))
    .sort((one, other) => other.around.length - one.around.length)
    .map(({ slot }) => slot);
}

function liveAt(container: Placed): LiveSlot[] {
  const live = liveSlots.get(container.id);
  return live === undefined ? [] : [live];
}

function elementsAround(placed: Placed): Placed[] {
  const around: Placed[] = [];
  for (let element = placed.parentElement; element !== null; element = element.parentElement) around.push(element);
  return around;
}
