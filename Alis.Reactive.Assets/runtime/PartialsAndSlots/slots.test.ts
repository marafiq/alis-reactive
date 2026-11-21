import { describe, expect, test, vi } from "vitest";
import type { PlanScript, SlotPage } from "./index";

async function theSlots() {
  vi.resetModules();
  return import("./index");
}

class Box {
  readonly id: string;
  parentElement: Box | null;

  constructor(id: string, parentElement: Box | null) {
    this.id = id;
    this.parentElement = parentElement;
  }
}

interface Content {
  readonly name: string;
  readonly nodes: readonly Box[];
  readonly plans: readonly PlanScript[];
  readonly scripts: readonly string[];
}

type Page = SlotPage<Box, Content>;
type Plan = Parameters<Page["starts"]>[0];
type Owner = Parameters<Page["starts"]>[1];
type Root = Parameters<Page["mounts"]>[0];

const eventTargets = { kind: "method", subscribe: "addEventListener", unsubscribe: "removeEventListener" } as const;
const none = { kind: "none" } as const;

function aPlan(planId: string, slots: readonly string[]): Plan {
  return {
    version: 7,
    planId,
    culture: {
      name: "en-US",
      collation: "en-US",
      decimal: ".",
      group: ",",
      integer: { group: false, exponent: false },
      fraction: { group: true, exponent: true },
    },
    timeZone: { kind: "clock" },
    trace: { kind: "off" },
    vendors: {},
    objects: Object.fromEntries(slots.map((slot) => ["$slot:" + slot, { kind: "plugin", name: "alis.slot", key: slot, listen: eventTargets, data: none }])),
    handlers: [],
  };
}

function itsScript(plan: Plan, script: Box): PlanScript {
  return { read: { kind: "plan", plan }, script };
}

class Hall implements Page {
  readonly chart: string[] = [];
  readonly body = new Box("", null);
  readonly stuck = new Set<string>();
  aRunIsInItsStep = false;
  readonly #boxes: Box[] = [];
  readonly #contents = new Map<string, Content>();
  readonly #inside = new Map<Box, Content>();
  readonly #waiting: (() => void)[] = [];

  box(id: string, parentElement: Box | null): Box {
    const box = new Box(id, parentElement);
    this.#boxes.push(box);
    return box;
  }

  writes(content: Content): string {
    this.#contents.set(content.name, content);
    return content.name;
  }

  returns(): void {
    this.chart.push("the run in its step returned");
    for (const returned of this.#waiting.splice(0)) returned();
  }

  readonly elementById = (id: string): Box | null => this.#boxes.find((box) => box.id === id && this.#isOnThePage(box)) ?? null;

  readonly parsed = (html: string): Content => {
    const content = this.#contents.get(html);
    if (content === undefined) throw new Error("The hall writes no content named " + html + ".");
    return content;
  };

  readonly plansIn = (content: Content): readonly PlanScript[] => content.plans;

  readonly replaces = (container: Box, content: Content): void => {
    this.#leaves(container);
    for (const node of content.nodes) node.parentElement = container;
    this.#inside.set(container, content);
    this.chart.push("'" + container.id + "' shows " + content.name);
  };

  readonly empties = (container: Box): void => {
    this.#leaves(container);
    this.#inside.delete(container);
    this.chart.push("'" + container.id + "' emptied");
  };

  readonly runsScripts = (container: Box): void => {
    for (const script of this.#inside.get(container)?.scripts ?? []) this.chart.push(script + " ran");
  };

  readonly mounts = (root: Root): void => {
    this.chart.push("the mount of '" + root.id + "' announced");
  };

  readonly unmounts = (root: Root): void => {
    this.chart.push("the unmount of '" + root.id + "' announced");
  };

  readonly starts = (plan: Plan, owner: Owner): void => {
    this.chart.push(plan.planId + " started");
    owner.runs(() => {
      this.chart.push(plan.planId + "'s runs ended");
    });
    owner.listens(() => {
      if (this.stuck.has(plan.planId)) throw new Error("The radio is stuck.");
      this.chart.push(plan.planId + "'s listeners removed");
    });
  };

  readonly started = (): void => {
    this.chart.push("the on loads ran");
  };

  readonly cuts = (_owners: readonly Owner[], returned: () => void): boolean => {
    if (!this.aRunIsInItsStep) return false;
    this.#waiting.push(returned);
    this.chart.push("the release waits for the run in its step");
    return true;
  };

  readonly logs = (error: string): void => {
    this.chart.push("logged: " + error);
  };

  #leaves(container: Box): void {
    for (const node of this.#inside.get(container)?.nodes ?? []) node.parentElement = null;
  }

  #isOnThePage(box: Box): boolean {
    for (let at: Box | null = box; at !== null; at = at.parentElement) if (at === this.body) return true;
    return false;
  }
}

function aBoard(hall: Hall, name: string): string {
  const noteSlot = hall.box("note-slot", null);
  const script = hall.box("", null);
  return hall.writes({ name, nodes: [noteSlot, script], plans: [itsScript(aPlan(name, ["note-slot"]), script)], scripts: ["the board's stamp"] });
}

function aNote(hall: Hall, name: string): string {
  const script = hall.box("", null);
  return hall.writes({ name, nodes: [script], plans: [itsScript(aPlan(name, []), script)], scripts: [] });
}

describe("when a slot loads (D12.4, M9.D1, M1.D1)", () => {
  test("D12.3: a slot no plan gave an owner announces the unmount of its content, then shows the content, runs its scripts, announces its mount and starts its plans, and the on loads run last", async () => {
    const { Slots } = await theSlots();
    const hall = new Hall();
    hall.box("board-slot", hall.body);
    const slots = new Slots(hall);

    slots.of("board-slot").load(aBoard(hall, "the board"));

    expect(hall.chart).toStrictEqual([
      "the unmount of 'board-slot' announced",
      "'board-slot' shows the board",
      "the board's stamp ran",
      "the mount of 'board-slot' announced",
      "the board started",
      "the on loads ran",
    ]);
  });

  test("it releases every owner inside its content, innermost first, then its own, each in D12.4's order, before its content is replaced", async () => {
    const { Slots } = await theSlots();
    const hall = new Hall();
    hall.box("board-slot", hall.body);
    const slots = new Slots(hall);
    slots.of("board-slot").load(aBoard(hall, "the board"));
    slots.of("note-slot").load(aNote(hall, "the note"));
    hall.chart.splice(0);

    slots.of("board-slot").load(aBoard(hall, "the second board"));

    expect(hall.chart).toStrictEqual([
      "the note's runs ended",
      "the unmount of 'note-slot' announced",
      "the note's listeners removed",
      "the board's runs ended",
      "the unmount of 'board-slot' announced",
      "the board's listeners removed",
      "'board-slot' shows the second board",
      "the board's stamp ran",
      "the mount of 'board-slot' announced",
      "the second board started",
      "the on loads ran",
    ]);
  });

  test("M9.X3: a plan the content holds inside another slot's container belongs to that slot, whose own unload releases it alone, and the on loads run after every plan of the load started", async () => {
    const { Slots } = await theSlots();
    const hall = new Hall();
    hall.box("board-slot", hall.body);
    const noteSlot = hall.box("note-slot", null);
    const boardScript = hall.box("", null);
    const noteScript = hall.box("", noteSlot);
    const board = hall.writes({
      name: "the board",
      nodes: [boardScript, noteSlot],
      plans: [itsScript(aPlan("the board", ["note-slot"]), boardScript), itsScript(aPlan("the note", []), noteScript)],
      scripts: [],
    });
    const slots = new Slots(hall);
    slots.of("board-slot").load(board);
    const loaded = hall.chart.splice(0);

    slots.of("note-slot").unload();

    expect([loaded, hall.chart]).toStrictEqual([
      [
        "the unmount of 'board-slot' announced",
        "'board-slot' shows the board",
        "the mount of 'board-slot' announced",
        "the board started",
        "the note started",
        "the on loads ran",
      ],
      ["the note's runs ended", "the unmount of 'note-slot' announced", "the note's listeners removed", "'note-slot' emptied"],
    ]);
  });

  test("X12.8: the owner a plan the server rendered in a slot finds at boot announces its unmount through the page at the slot's first load", async () => {
    const { Slots } = await theSlots();
    const hall = new Hall();
    const steps = hall.box("move-in-step", hall.body);
    const slots = new Slots(hall);
    hall.starts(aPlan("the resident step", []), slots.ownerAround(hall.box("", steps), new Set(["move-in-step"])));
    hall.chart.splice(0);

    slots.of("move-in-step").load(aNote(hall, "the room step"));

    expect(hall.chart).toStrictEqual([
      "the resident step's runs ended",
      "the unmount of 'move-in-step' announced",
      "the resident step's listeners removed",
      "'move-in-step' shows the room step",
      "the mount of 'move-in-step' announced",
      "the room step started",
      "the on loads ran",
    ]);
  });

  test("M9.D2: content that holds a plan the page refuses stops the step with one error naming the slot, before anything is released or replaced", async () => {
    const { Slots } = await theSlots();
    const hall = new Hall();
    hall.box("board-slot", hall.body);
    const slots = new Slots(hall);
    slots.of("board-slot").load(aBoard(hall, "the board"));
    hall.chart.splice(0);
    const script = hall.box("", null);
    const torn = hall.writes({
      name: "the torn board",
      nodes: [script],
      plans: [{ read: { kind: "refused", error: "This plan's text is not JSON." }, script }],
      scripts: ["the torn board's stamp"],
    });

    expect(() => {
      slots.of("board-slot").load(torn);
    }).toThrow("The content loaded into the slot 'board-slot' holds a plan this page refuses: This plan's text is not JSON.");
    expect(hall.chart).toStrictEqual([]);
  });

  test("a reply whose body reads nothing stops the step with one error naming the slot, before anything is released", async () => {
    const { Slots } = await theSlots();
    const hall = new Hall();
    hall.box("board-slot", hall.body);
    const slots = new Slots(hall);
    slots.of("board-slot").load(aBoard(hall, "the board"));
    hall.chart.splice(0);

    expect(() => {
      slots.of("board-slot").load(null);
    }).toThrow("The slot 'board-slot' was handed no HTML to load.");
    expect(hall.chart).toStrictEqual([]);
  });

  test("M9.X5: a load or an unload of a slot whose element left with the slot around it stops its step and runs nothing", async () => {
    const { Slots } = await theSlots();
    const hall = new Hall();
    hall.box("board-slot", hall.body);
    const slots = new Slots(hall);
    slots.of("board-slot").load(aBoard(hall, "the board"));
    slots.of("board-slot").unload();
    hall.chart.splice(0);
    const note = aNote(hall, "the note");

    expect(() => {
      slots.of("note-slot").load(note);
    }).toThrow("No element has the id 'note-slot'.");
    expect(() => {
      slots.of("note-slot").unload();
    }).toThrow("No element has the id 'note-slot'.");
    expect(hall.chart).toStrictEqual([]);
  });
});

describe("when a slot unloads (D12.4, D12.9)", () => {
  test("it releases its owner and empties its content, starts nothing, and a slot nothing loaded into announces its unmount and empties as well", async () => {
    const { Slots } = await theSlots();
    const hall = new Hall();
    hall.box("board-slot", hall.body);
    const slots = new Slots(hall);
    slots.of("board-slot").load(aBoard(hall, "the board"));
    hall.chart.splice(0);

    slots.of("board-slot").unload();
    slots.of("board-slot").unload();

    expect(hall.chart).toStrictEqual([
      "the board's runs ended",
      "the unmount of 'board-slot' announced",
      "the board's listeners removed",
      "'board-slot' emptied",
      "the unmount of 'board-slot' announced",
      "'board-slot' emptied",
    ]);
  });
});

describe("when a release meets a throw (D12.4, X12.15)", () => {
  test("every owner is released past it, the content is replaced and its plans start, and then the step stops with one error naming each slot and its step", async () => {
    const { Slots } = await theSlots();
    const hall = new Hall();
    hall.box("card-slot", hall.body);
    const slots = new Slots(hall);
    slots.of("card-slot").load(aBoard(hall, "Ada's card"));
    slots.of("note-slot").load(aNote(hall, "Ada's notes"));
    hall.stuck.add("Ada's card");
    hall.stuck.add("Ada's notes");
    hall.chart.splice(0);
    const bensCard = aNote(hall, "Ben's card");

    expect(() => {
      slots.of("card-slot").load(bensCard);
    }).toThrow(
      "Releasing the slot 'card-slot' threw, and every owner was released past it: the slot 'note-slot' threw while removing its listeners (The radio is stuck.); the slot 'card-slot' threw while removing its listeners (The radio is stuck.)",
    );
    expect(hall.chart).toStrictEqual([
      "Ada's notes's runs ended",
      "the unmount of 'note-slot' announced",
      "Ada's card's runs ended",
      "the unmount of 'card-slot' announced",
      "'card-slot' shows Ben's card",
      "the mount of 'card-slot' announced",
      "Ben's card started",
      "the on loads ran",
    ]);
  });
});

describe("when a release meets a run of its owners in its own step (D12.4, D11.11, Lanes.tla's Dispose)", () => {
  test("their runs end at the cut, inside the step, and the release, the new content and its plans wait for the run's return", async () => {
    const { Slots } = await theSlots();
    const hall = new Hall();
    hall.box("board-slot", hall.body);
    const slots = new Slots(hall);
    slots.of("board-slot").load(aBoard(hall, "the board"));
    slots.of("note-slot").load(aNote(hall, "the note"));
    hall.chart.splice(0);
    hall.aRunIsInItsStep = true;

    slots.of("board-slot").load(aBoard(hall, "the second board"));
    hall.returns();

    expect(hall.chart).toStrictEqual([
      "the release waits for the run in its step",
      "the note's runs ended",
      "the board's runs ended",
      "the run in its step returned",
      "the unmount of 'note-slot' announced",
      "the note's listeners removed",
      "the unmount of 'board-slot' announced",
      "the board's listeners removed",
      "'board-slot' shows the second board",
      "the board's stamp ran",
      "the mount of 'board-slot' announced",
      "the second board started",
      "the on loads ran",
    ]);
  });

  test("a throw the release meets after the run returned is logged as one error, since the run that asked has ended", async () => {
    const { Slots } = await theSlots();
    const hall = new Hall();
    hall.box("card-slot", hall.body);
    const slots = new Slots(hall);
    slots.of("card-slot").load(aNote(hall, "the card"));
    hall.stuck.add("the card");
    hall.chart.splice(0);
    hall.aRunIsInItsStep = true;

    slots.of("card-slot").unload();
    hall.returns();

    expect(hall.chart).toStrictEqual([
      "the release waits for the run in its step",
      "the card's runs ended",
      "the run in its step returned",
      "the unmount of 'card-slot' announced",
      "'card-slot' emptied",
      "logged: Releasing the slot 'card-slot' threw, and every owner was released past it: the slot 'card-slot' threw while removing its listeners (The radio is stuck.)",
    ]);
  });

  test("a slot whose element left with the slot around it while the release waited runs nothing more and logs that no element has its id", async () => {
    const { Slots } = await theSlots();
    const hall = new Hall();
    hall.box("board-slot", hall.body);
    const slots = new Slots(hall);
    slots.of("board-slot").load(aBoard(hall, "the board"));
    slots.of("note-slot").load(aNote(hall, "the note"));
    hall.chart.splice(0);
    hall.aRunIsInItsStep = true;
    slots.of("note-slot").load(aNote(hall, "the second note"));
    hall.aRunIsInItsStep = false;

    slots.of("board-slot").unload();
    hall.returns();

    expect(hall.chart).toStrictEqual([
      "the release waits for the run in its step",
      "the note's runs ended",
      "the unmount of 'note-slot' announced",
      "the note's listeners removed",
      "the board's runs ended",
      "the unmount of 'board-slot' announced",
      "the board's listeners removed",
      "'board-slot' emptied",
      "the run in its step returned",
      "logged: No element has the id 'note-slot'.",
    ]);
  });
});
