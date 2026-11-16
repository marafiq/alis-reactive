import { describe, expect, test, vi } from "vitest";

async function aPage() {
  vi.resetModules();
  return import("./index");
}

const body = { id: "", parentElement: null };
const card = { id: "card", parentElement: body };
const notes = { id: "notes", parentElement: body };
const unheard = (): void => undefined;

const eventTargets = { kind: "method", subscribe: "addEventListener", unsubscribe: "removeEventListener" } as const;
const none = { kind: "none" } as const;

describe("when a plan names its slots", () => {
  test("its slots are the keys of its alis.slot plugins, and no other object is one", async () => {
    const { slotsNamedBy } = await aPage();

    expect(
      slotsNamedBy({
        version: 7,
        planId: "Care.Residents.Board",
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
        objects: {
          "$document": { kind: "plugin", name: "alis.document", key: "", listen: eventTargets, data: none },
          "$plugin:bell": { kind: "plugin", name: "bell", key: "ward", listen: eventTargets, data: none },
          "$slot:card": { kind: "plugin", name: "alis.slot", key: "card", listen: eventTargets, data: none },
          "$slot:notes": { kind: "plugin", name: "alis.slot", key: "notes", listen: eventTargets, data: none },
          "card-title": { kind: "element", id: "card-title", vendor: "native" },
        },
        handlers: [],
      }),
    ).toStrictEqual(["card", "notes"]);
  });
});

describe("when slots are held", () => {
  test("a slot a card's plan names is held from the plan's start, though nothing is loaded into it", async () => {
    const { heldSlots, holdSlots, slotOwner } = await aPage();

    holdSlots(["notes"], slotOwner(card, unheard));

    expect(heldSlots()).toStrictEqual(new Set(["card", "notes"]));
  });

  test("an emptied slot stays held while the page's plan names it", async () => {
    const { heldSlots, holdSlots, page, releaseSlot, slotOwner } = await aPage();
    holdSlots(["card"], page);
    slotOwner(card, unheard);

    releaseSlot(card);

    expect(heldSlots()).toStrictEqual(new Set(["card"]));
  });

  test("notes a closed card loaded stay held", async () => {
    const { heldSlots, holdSlots, page, releaseSlot, slotOwner } = await aPage();
    holdSlots(["card"], page);
    holdSlots(["notes"], slotOwner(card, unheard));
    slotOwner(notes, unheard);

    releaseSlot(card);

    expect(heldSlots()).toStrictEqual(new Set(["card", "notes"]));
  });

  test("notes a closed card loaded are held no more once the notes slot is released", async () => {
    const { heldSlots, holdSlots, page, releaseSlot, slotOwner } = await aPage();
    holdSlots(["card"], page);
    holdSlots(["notes"], slotOwner(card, unheard));
    slotOwner(notes, unheard);

    releaseSlot(card);
    releaseSlot(notes);

    expect(heldSlots()).toStrictEqual(new Set(["card"]));
  });

  test("a slot only a closed card named, with nothing loaded into it, is held no more", async () => {
    const { heldSlots, holdSlots, page, releaseSlot, slotOwner } = await aPage();
    holdSlots(["card"], page);
    holdSlots(["notes"], slotOwner(card, unheard));

    releaseSlot(card);

    expect(heldSlots()).toStrictEqual(new Set(["card"]));
  });
});

interface Spot {
  readonly id: string;
  readonly parentElement: Spot | null;
  compareDocumentPosition(other: Spot): number;
}

const preceding = 2;
const following = 4;
const inDocumentOrder: Spot[] = [];

function placed(id: string, parentElement: Spot | null): Spot {
  const spot: Spot = {
    id,
    parentElement,
    compareDocumentPosition: (other) => (inDocumentOrder.indexOf(other) > inDocumentOrder.indexOf(spot) ? following : preceding),
  };
  inDocumentOrder.push(spot);
  return spot;
}

const theBody = placed("", null);
const noticeboard = placed("noticeboard", theBody);
const board = placed("board", theBody);
const boardTools = placed("board-tools", board);
const toolsHeading = placed("tools-heading", boardTools);
const rounds = placed("rounds", theBody);
const morning = placed("morning", rounds);
placed("morning-notes", morning);
placed("evening", rounds);
const deskCard = placed("desk-card", theBody);
const visitNote = placed("visit-note", theBody);

const byId = (id: string): Spot | null => inDocumentOrder.find((spot) => spot.id === id) ?? null;

describe("when a step writes the content of an element, the held slots at or inside it are read in document order (R14.7)", () => {
  test("AStepOverANamedSlotThatHoldsContentStops: a slot a live plan names, holding what a load put there, is held at its own element", async () => {
    const { heldSlotsAtOrIn, holdSlots, page, slotOwner } = await aPage();
    holdSlots(["desk-card"], page);
    slotOwner(deskCard, unheard);

    expect(heldSlotsAtOrIn(deskCard, byId)).toStrictEqual(["desk-card"]);
  });

  test("AStepOverANamedSlotThatHoldsNoneStops: a slot an open card's plan names, with nothing loaded into it, is held at its own element", async () => {
    const { heldSlotsAtOrIn, holdSlots, page, slotOwner } = await aPage();
    holdSlots(["desk-card"], page);
    holdSlots(["visit-note"], slotOwner(deskCard, unheard));

    expect(heldSlotsAtOrIn(visitNote, byId)).toStrictEqual(["visit-note"]);
  });

  test("AStepOverContentInASlotNoPlanNamesStops: what a closed card loaded into a slot outside it holds that slot at its element", async () => {
    const { heldSlotsAtOrIn, holdSlots, page, releaseSlot, slotOwner } = await aPage();
    holdSlots(["desk-card"], page);
    holdSlots(["visit-note"], slotOwner(deskCard, unheard));
    slotOwner(visitNote, unheard);

    releaseSlot(deskCard);

    expect(heldSlotsAtOrIn(visitNote, byId)).toStrictEqual(["visit-note"]);
  });

  test("OnlyAHeldSlotStopsAStep: once the plan that named a slot left and its content was released, nothing holds it at its element", async () => {
    const { heldSlotsAtOrIn, holdSlots, page, releaseSlot, slotOwner } = await aPage();
    holdSlots(["desk-card"], page);
    holdSlots(["visit-note"], slotOwner(deskCard, unheard));
    slotOwner(visitNote, unheard);

    releaseSlot(deskCard);
    releaseSlot(visitNote);

    expect(heldSlotsAtOrIn(visitNote, byId)).toStrictEqual([]);
  });

  test("AStepOverAnElementHoldingAHeldSlotStops: an element that holds a held slot names it, though the element is no slot", async () => {
    const { heldSlotsAtOrIn, holdSlots, page } = await aPage();
    holdSlots(["board-tools"], page);

    expect(heldSlotsAtOrIn(board, byId)).toStrictEqual(["board-tools"]);
  });

  test("an element that holds several held slots names them in document order, an outer slot before the slots inside it, whatever order they were held in", async () => {
    const { heldSlotsAtOrIn, holdSlots, page } = await aPage();
    holdSlots(["evening"], page);
    holdSlots(["morning-notes"], page);
    holdSlots(["morning"], page);

    expect(heldSlotsAtOrIn(rounds, byId)).toStrictEqual(["morning", "morning-notes", "evening"]);
  });

  test("OnlyAHeldSlotStopsAStep: an element beside a held slot, or inside its content, holds none, and a held slot the page lacks is at no element", async () => {
    const { heldSlotsAtOrIn, holdSlots, page } = await aPage();
    holdSlots(["board-tools", "gone-with-its-card"], page);

    expect([heldSlotsAtOrIn(noticeboard, byId), heldSlotsAtOrIn(toolsHeading, byId), heldSlotsAtOrIn(theBody, byId)]).toStrictEqual([
      [],
      [],
      ["board-tools"],
    ]);
  });
});
