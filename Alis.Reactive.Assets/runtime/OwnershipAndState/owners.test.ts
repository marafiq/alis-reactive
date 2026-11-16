import { describe, expect, test, vi } from "vitest";
import type { AnnouncesUnmount } from "./index";

async function aPage() {
  vi.resetModules();
  return import("./index");
}

function leaves(left: string[], what: string): () => void {
  return () => {
    left.push(what);
  };
}

function announcesTo(heard: string[]): AnnouncesUnmount {
  return (root) => {
    heard.push("the unmount of '" + root.id + "' announced");
  };
}

const unheard: AnnouncesUnmount = () => undefined;

const body = { id: "", parentElement: null };
const card = { id: "card", parentElement: body };
const bedList = { id: "bed-list", parentElement: card };
const bedNotes = { id: "bed-notes", parentElement: bedList };
const eastWard = { id: "east-ward", parentElement: body };
const westWard = { id: "west-ward", parentElement: body };
const panel = { id: "panel", parentElement: body };

const aScriptOfThePage = { id: "", parentElement: body };
const aScriptInTheCard = { id: "", parentElement: card };
const aSecondScriptInTheCard = { id: "", parentElement: card };
const aScriptInTheBedList = { id: "", parentElement: bedList };
const aScriptInThePanel = { id: "", parentElement: panel };

describe("when a plan's owner is found", () => {
  test("R14.6 names the page or the innermost slot holding the plan's script", async () => {
    const { ownerAround, placeOf } = await aPage();
    const named = new Set(["card", "bed-list"]);
    expect(placeOf(ownerAround(aScriptOfThePage, named, unheard))).toBe("on the page");
    expect(placeOf(ownerAround(aScriptInTheCard, named, unheard))).toBe("in slot 'card'");
    expect(placeOf(ownerAround(aScriptInTheBedList, named, unheard))).toBe("in slot 'bed-list'");
  });

  test("a plan the server rendered inside a slot leaves at the slot's first release, and the page's plan stays", async () => {
    const { ownerAround, releaseSlot } = await aPage();
    const named = new Set(["card"]);
    const left: string[] = [];
    ownerAround(aScriptOfThePage, named, unheard).listens(leaves(left, "the page's listener"));
    ownerAround(aScriptInTheCard, named, unheard).listens(leaves(left, "the card's listener"));

    releaseSlot(card);

    expect(left).toStrictEqual(["the card's listener"]);
  });

  test("a plan belongs to the innermost slot around its script", async () => {
    const { ownerAround, releaseSlot } = await aPage();
    const named = new Set(["card", "bed-list"]);
    const left: string[] = [];
    ownerAround(aScriptInTheCard, named, unheard).listens(leaves(left, "the card's listener"));
    ownerAround(aScriptInTheBedList, named, unheard).listens(leaves(left, "the bed list's listener"));

    releaseSlot(bedList);

    expect(left).toStrictEqual(["the bed list's listener"]);
  });

  test("an element no plan names is no slot, whatever id it has", async () => {
    const { ownerAround, releaseSlot } = await aPage();
    const named = new Set(["card"]);
    const left: string[] = [];
    ownerAround(aScriptInThePanel, named, unheard).listens(leaves(left, "the panel's listener"));
    ownerAround(aScriptInTheCard, named, unheard).listens(leaves(left, "the card's listener"));

    releaseSlot(panel);
    releaseSlot(card);

    expect(left).toStrictEqual(["the card's listener"]);
  });

  test("two plans the server rendered in one slot leave together", async () => {
    const { ownerAround, releaseSlot } = await aPage();
    const named = new Set(["card"]);
    const left: string[] = [];
    ownerAround(aScriptInTheCard, named, unheard).listens(leaves(left, "the first plan's listener"));
    ownerAround(aSecondScriptInTheCard, named, unheard).listens(leaves(left, "the second plan's listener"));

    releaseSlot(card);

    expect(left).toStrictEqual(["the first plan's listener", "the second plan's listener"]);
  });

  test("a plan a load brings belongs to the slot it was loaded into, though the plan names no slot", async () => {
    const { holdSlots, ownerAround, page, releaseSlot, slotOwner } = await aPage();
    const left: string[] = [];
    holdSlots(["card"], page);
    slotOwner(card, unheard);
    ownerAround(aScriptInTheCard, new Set(), unheard).listens(leaves(left, "the loaded plan's listener"));

    releaseSlot(card);

    expect(left).toStrictEqual(["the loaded plan's listener"]);
  });

  test("D12.3: the owner a plan's script finds in a slot announces the unmount of that slot's content, innermost first, and the page announces none", async () => {
    const { ownerAround, releaseSlot } = await aPage();
    const named = new Set(["card", "bed-list"]);
    const heard: string[] = [];
    ownerAround(aScriptOfThePage, named, announcesTo(heard));
    ownerAround(aScriptInTheCard, named, announcesTo(heard));
    ownerAround(aScriptInTheBedList, named, announcesTo(heard));

    releaseSlot(card);

    expect(heard).toStrictEqual(["the unmount of 'bed-list' announced", "the unmount of 'card' announced"]);
  });
});

describe("when a slot is released", () => {
  test("a slot inside its content leaves with it", async () => {
    const { releaseSlot, slotOwner } = await aPage();
    const left: string[] = [];
    slotOwner(card, unheard);
    slotOwner(bedList, unheard).listens(leaves(left, "the bed list's listener"));

    releaseSlot(card);

    expect(left).toStrictEqual(["the bed list's listener"]);
  });

  test("the owners inside its content leave innermost first, then its own", async () => {
    const { releaseSlot, slotOwner } = await aPage();
    const left: string[] = [];
    slotOwner(card, unheard).listens(leaves(left, "the card's listener"));
    slotOwner(bedList, unheard).listens(leaves(left, "the bed list's listener"));
    slotOwner(bedNotes, unheard).listens(leaves(left, "the bed notes' listener"));

    releaseSlot(card);

    expect(left).toStrictEqual(["the bed notes' listener", "the bed list's listener", "the card's listener"]);
  });

  test("the owners its release ends are named innermost first, then its own, and the naming releases none of them", async () => {
    const { ownersEndedBy, placeOf, releaseSlot, slotOwner } = await aPage();
    const left: string[] = [];
    slotOwner(card, unheard).listens(leaves(left, "the card's listener"));
    slotOwner(bedList, unheard).listens(leaves(left, "the bed list's listener"));
    slotOwner(bedNotes, unheard).listens(leaves(left, "the bed notes' listener"));
    slotOwner(eastWard, unheard).listens(leaves(left, "the east ward's listener"));

    const ending = ownersEndedBy(card).map(placeOf);
    const leftWhenNamed = [...left];
    releaseSlot(card);

    expect([ending, leftWhenNamed, left]).toStrictEqual([
      ["in slot 'bed-notes'", "in slot 'bed-list'", "in slot 'card'"],
      [],
      ["the bed notes' listener", "the bed list's listener", "the card's listener"],
    ]);
  });

  test("the slot beside it keeps the hold it has on the hub both hold", async () => {
    const { releaseSlot, slotOwner } = await aPage();
    const letGo: string[] = [];
    slotOwner(eastWard, unheard).holds(leaves(letGo, "the east ward let the hub go"));
    slotOwner(westWard, unheard).holds(leaves(letGo, "the west ward let the hub go"));

    releaseSlot(eastWard);

    expect(letGo).toStrictEqual(["the east ward let the hub go"]);
  });

  test("D12.4: its owner ends its runs, announces the unmount of its content, removes its listeners, releases its holds and drops its registrations, in that order", async () => {
    const { releaseSlot, slotOwner } = await aPage();
    const done: string[] = [];
    const owner = slotOwner(card, announcesTo(done));
    owner.registers(leaves(done, "its registration dropped"));
    owner.holds(leaves(done, "its hold released"));
    owner.listens(leaves(done, "its listener removed"));
    owner.runs(leaves(done, "its run ended"));

    releaseSlot(card);

    expect(done).toStrictEqual(["its run ended", "the unmount of 'card' announced", "its listener removed", "its hold released", "its registration dropped"]);
  });

  test("its owner announces the unmount once a release, however often it was asked for", async () => {
    const { releaseSlot, slotOwner } = await aPage();
    const heard: string[] = [];
    slotOwner(card, announcesTo(heard));
    slotOwner(card, announcesTo(heard));

    releaseSlot(card);
    slotOwner(card, announcesTo(heard));
    releaseSlot(card);

    expect(heard).toStrictEqual(["the unmount of 'card' announced", "the unmount of 'card' announced"]);
  });

  test("every owner is released past a removal that throws", async () => {
    const { releaseSlot, slotOwner } = await aPage();
    const done: string[] = [];
    slotOwner(card, unheard).listens(leaves(done, "the card's listener removed"));
    const inner = slotOwner(bedList, unheard);
    inner.listens(() => {
      throw new Error("The bell's unsubscribe failed.");
    });
    inner.listens(leaves(done, "the bed list's second listener removed"));
    inner.registers(leaves(done, "the bed list's registration dropped"));

    releaseSlot(card);

    expect(done).toStrictEqual(["the bed list's second listener removed", "the bed list's registration dropped", "the card's listener removed"]);
  });

  test("what a removal threw is kept with its slot and its step", async () => {
    const { releaseSlot, slotOwner } = await aPage();
    const failed = new Error("The bell's unsubscribe failed.");
    const closed = new Error("The hub was closed twice.");
    slotOwner(card, unheard).holds(() => {
      throw closed;
    });
    slotOwner(bedList, unheard).listens(() => {
      throw failed;
    });

    expect(releaseSlot(card)).toStrictEqual([
      { slot: "bed-list", step: "listeners", error: failed },
      { slot: "card", step: "holds", error: closed },
    ]);
  });

  test("a clean release keeps no error", async () => {
    const { releaseSlot, slotOwner } = await aPage();
    const done: string[] = [];
    slotOwner(card, unheard).listens(leaves(done, "the card's listener removed"));

    expect(releaseSlot(card)).toStrictEqual([]);
  });

  test("a slot loaded three times holds the registration of its last load alone", async () => {
    const { page, releaseSlot, slotOwner } = await aPage();
    const rules = ["the page's rules", "the first card's rules"];
    page.registers(leaves(rules, "the page's rules dropped"));
    slotOwner(card, unheard).registers(() => {
      rules.splice(rules.indexOf("the first card's rules"), 1);
    });

    releaseSlot(card);
    rules.push("the second card's rules");
    slotOwner(card, unheard).registers(() => {
      rules.splice(rules.indexOf("the second card's rules"), 1);
    });
    releaseSlot(card);
    rules.push("the third card's rules");
    slotOwner(card, unheard).registers(() => {
      rules.splice(rules.indexOf("the third card's rules"), 1);
    });

    expect(rules).toStrictEqual(["the page's rules", "the third card's rules"]);
  });

  test("its rules leave with it and the page's stay", async () => {
    const { page, releaseSlot, slotOwner } = await aPage();
    const rules = ["the page's rules", "the card's rules"];
    page.registers(() => {
      rules.splice(rules.indexOf("the page's rules"), 1);
    });
    slotOwner(card, unheard).registers(() => {
      rules.splice(rules.indexOf("the card's rules"), 1);
    });

    releaseSlot(card);

    expect(rules).toStrictEqual(["the page's rules"]);
  });
});
