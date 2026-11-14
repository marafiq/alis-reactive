import { describe, expect, test, vi } from "vitest";

async function aPage() {
  vi.resetModules();
  return import("./index");
}

const body = { id: "", parentElement: null };
const card = { id: "card", parentElement: body };
const notes = { id: "notes", parentElement: body };

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

    holdSlots(["notes"], slotOwner(card));

    expect(heldSlots()).toStrictEqual(new Set(["card", "notes"]));
  });

  test("an emptied slot stays held while the page's plan names it", async () => {
    const { heldSlots, holdSlots, page, releaseSlot, slotOwner } = await aPage();
    holdSlots(["card"], page);
    slotOwner(card);

    releaseSlot(card);

    expect(heldSlots()).toStrictEqual(new Set(["card"]));
  });

  test("notes a closed card loaded stay held", async () => {
    const { heldSlots, holdSlots, page, releaseSlot, slotOwner } = await aPage();
    holdSlots(["card"], page);
    holdSlots(["notes"], slotOwner(card));
    slotOwner(notes);

    releaseSlot(card);

    expect(heldSlots()).toStrictEqual(new Set(["card", "notes"]));
  });

  test("notes a closed card loaded are held no more once the notes slot is released", async () => {
    const { heldSlots, holdSlots, page, releaseSlot, slotOwner } = await aPage();
    holdSlots(["card"], page);
    holdSlots(["notes"], slotOwner(card));
    slotOwner(notes);

    releaseSlot(card);
    releaseSlot(notes);

    expect(heldSlots()).toStrictEqual(new Set(["card"]));
  });

  test("a slot only a closed card named, with nothing loaded into it, is held no more", async () => {
    const { heldSlots, holdSlots, page, releaseSlot, slotOwner } = await aPage();
    holdSlots(["card"], page);
    holdSlots(["notes"], slotOwner(card));

    releaseSlot(card);

    expect(heldSlots()).toStrictEqual(new Set(["card"]));
  });
});
