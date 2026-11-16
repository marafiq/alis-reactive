import {
  endRunsWith,
  ownerAround,
  ownersEndedBy,
  releaseSlot,
  slotOwner,
  slotsNamedBy,
  type AnnouncesUnmount,
  type Owner,
  type Placed,
  type ReleaseStep,
  type Thrown,
} from "../OwnershipAndState/index";
import { assertNever, type PlanDocument, type PlanRead } from "../Schema/index";
import { Stop } from "../ValuesAndSources/index";

export interface PlanScript {
  readonly read: PlanRead;
  readonly script: Placed;
}

export interface SlotPage<Element extends Placed, Content> {
  readonly elementById: (id: string) => Element | null;
  readonly parsed: (html: string) => Content;
  readonly plansIn: (content: Content) => readonly PlanScript[];
  readonly replaces: (container: Element, content: Content) => void;
  readonly empties: (container: Element) => void;
  readonly runsScripts: (container: Element) => void;
  readonly mounts: (root: Placed) => void;
  readonly unmounts: (root: Placed) => void;
  readonly starts: (plan: PlanDocument, owner: Owner) => void;
  readonly started: () => void;
  readonly cuts: (owners: readonly Owner[], returned: () => void) => boolean;
  readonly logs: (error: string) => void;
}

export interface Slot {
  load(html: unknown): void;
  unload(): void;
}

interface Loaded {
  readonly plan: PlanDocument;
  readonly script: Placed;
}

export class Slots<Element extends Placed, Content> {
  readonly #page: SlotPage<Element, Content>;
  readonly #announcesUnmount: AnnouncesUnmount;

  constructor(page: SlotPage<Element, Content>) {
    this.#page = page;
    this.#announcesUnmount = (root) => {
      page.unmounts(root);
    };
  }

  of(key: string): Slot {
    return {
      load: (html) => {
        this.#loads(key, html);
      },
      unload: () => {
        this.#unloads(key);
      },
    };
  }

  ownerAround(placed: Placed, named: ReadonlySet<string>): Owner {
    return ownerAround(placed, named, this.#announcesUnmount);
  }

  #loads(key: string, html: unknown): void {
    const container = this.#containerOf(key);
    if (typeof html !== "string") throw new Stop("The slot '" + key + "' was handed no HTML to load.");
    const content = this.#page.parsed(html);
    const loaded = this.#plansOf(key, content);
    this.#releases(key, container, () => {
      this.#page.replaces(container, content);
      this.#page.runsScripts(container);
      slotOwner(container, this.#announcesUnmount);
      this.#page.mounts(container);
      const named = new Set(loaded.flatMap(({ plan }) => slotsNamedBy(plan)));
      for (const { plan, script } of loaded) this.#page.starts(plan, this.ownerAround(script, named));
      this.#page.started();
    });
  }

  #unloads(key: string): void {
    const container = this.#containerOf(key);
    this.#releases(key, container, () => {
      this.#page.empties(container);
    });
  }

  #containerOf(key: string): Element {
    const container = this.#page.elementById(key);
    if (container === null) throw new Stop(noElement(key));
    return container;
  }

  #plansOf(key: string, content: Content): Loaded[] {
    return this.#page.plansIn(content).map(({ read, script }) => {
      switch (read.kind) {
        case "plan":
          return { plan: read.plan, script };
        case "refused":
          throw new Stop("The content loaded into the slot '" + key + "' holds a plan this page refuses: " + read.error);
        default:
          return assertNever(read);
      }
    });
  }

  #releases(key: string, container: Element, fills: () => void): void {
    const endedAtTheCut: Thrown[] = [];
    const waits = this.#page.cuts(ownersEndedBy(container), () => {
      this.#releasesAfterTheWait(key, container, endedAtTheCut, fills);
    });
    if (waits) {
      endedAtTheCut.push(...endRunsWith(container));
      return;
    }
    const thrown = this.#released(container);
    fills();
    if (thrown.length > 0) throw new Stop(releasedPast(key, thrown));
  }

  #releasesAfterTheWait(key: string, container: Element, endedAtTheCut: readonly Thrown[], fills: () => void): void {
    if (this.#page.elementById(key) !== container) {
      this.#page.logs(noElement(key));
      return;
    }
    const thrown = [...endedAtTheCut, ...this.#released(container)];
    fills();
    if (thrown.length > 0) this.#page.logs(releasedPast(key, thrown));
  }

  #released(container: Element): Thrown[] {
    slotOwner(container, this.#announcesUnmount);
    return releaseSlot(container);
  }
}

function noElement(key: string): string {
  return "No element has the id '" + key + "'.";
}

function releasedPast(key: string, thrown: readonly Thrown[]): string {
  return "Releasing the slot '" + key + "' threw, and every owner was released past it: " + thrown.map(described).join("; ");
}

function described(thrown: Thrown): string {
  return "the slot '" + thrown.slot + "' threw while " + doing(thrown.step) + " (" + messageOf(thrown.error) + ")";
}

function doing(step: ReleaseStep): string {
  switch (step) {
    case "runs":
      return "ending its runs";
    case "unmount":
      return "announcing the unmount of its content";
    case "listeners":
      return "removing its listeners";
    case "holds":
      return "releasing its holds";
    case "registrations":
      return "dropping its registrations";
    default:
      return assertNever(step);
  }
}

function messageOf(error: unknown): string {
  return error instanceof Error ? error.message : String(error);
}
