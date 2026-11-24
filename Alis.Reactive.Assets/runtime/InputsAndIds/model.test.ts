import { describe, expect, test } from "vitest";
import { PageInputs } from "./index";

type Plan = Parameters<PageInputs["join"]>[0];
type Owner = Parameters<PageInputs["join"]>[1];
type Source = Parameters<Parameters<PageInputs["join"]>[3]>[0];
type Model = Extract<Plan["objects"][string], { kind: "plugin" }>["data"];
type Inputs = Extract<Model, { kind: "model" }>["inputs"];
type Containers = Extract<Model, { kind: "model" }>["containers"];

class Visit implements Owner {
  readonly drops: (() => void)[] = [];
  runs(): void { return undefined; }
  mounts(): void { return undefined; }
  listens(): void { return undefined; }
  holds(): void { return undefined; }
  registers(drop: () => void): void { this.drops.push(drop); }
  release(): void { for (const drop of this.drops.splice(0)) drop(); }
}

const intake: Containers = { intake: { model: "Admission", kind: "form", summary: "intake_summary", live: [] } };
type Converted = Inputs[string]["converted"];
type Blank = Inputs[string]["blank"];
const convertedFrom = (read: Source): Converted => ({ kind: "as", expr: read, shape: { kind: "nullable", inner: { kind: "any" } } });
const field = (name: string, path: Inputs[string]["path"], converted: Converted, blank: Blank = { kind: "lit", value: null }): Inputs[string] =>
  ({ name, path, converted, blank, committed: "change", invalidClass: "invalid", message: "" });
const literal = (value: number | string): Blank => ({ kind: "lit", value });
const fields: Inputs = { own: field("Nights", ["nights"], convertedFrom(literal(5))) };

function plan(inputs: Inputs = fields, containers: Containers = intake, planId = "Admission"): Plan {
  return {
    version: 7, planId,
    culture: { name: "en-US", collation: "en-US", decimal: ".", group: ",", integer: { group: false, exponent: false }, fraction: { group: true, exponent: true } },
    timeZone: { kind: "clock" }, trace: { kind: "off" }, vendors: {}, handlers: [],
    objects: { $model: { kind: "plugin", name: "alis.model", key: "", listen: { kind: "method", subscribe: "addEventListener", unsubscribe: "removeEventListener" }, data: { kind: "model", inputs, containers } } },
  };
}

function literalValue(source: Source): unknown {
  if (source.kind === "as") return literalValue(source.expr);
  if (source.kind !== "lit") throw new Error("This contribution reads a literal.");
  return source.value;
}

function page(containers = new Map([["intake", new Set(["own", "slot", "second"])]]), forms = new Map<string, () => FormData>()): PageInputs {
  return new PageInputs(id => {
    const inputs = containers.get(id);
    return inputs === undefined ? null : input => inputs.has(input);
  }, id => forms.get(id)?.() ?? null);
}

describe("when alis.model reads what a container sends", () => {
  test("ReadIsWhatTheEndpointBinds: the contributed value is read with its own plan's evaluator", () => {
    const inputs = page();
    const owner = new Visit();
    inputs.join(plan(), owner, "on the page", literalValue);
    inputs.join(plan({ slot: field("Room", ["room"], convertedFrom({ kind: "ref", name: "room" })) }, {}, "RoomCard"), owner, "in slot 'room'", () => "B-12");
    expect(inputs.sent("intake", "Admission")).toStrictEqual({ nights: 5, room: "B-12" });
  });

  test("ReadStopsWhenItsSendStops: a missing container element stops the read", () => {
    const inputs = page(new Map());
    inputs.join(plan(), new Visit(), "on the page", literalValue);
    expect(() => inputs.sent("intake", "Admission")).toThrow(/^No element has the id 'intake'\.$/);
  });

  test("ReadStopsWhenItsSendStops: the container must have a live rendering under the requested model", () => {
    const inputs = page();
    inputs.join(plan(), new Visit(), "on the page", literalValue);
    expect(() => inputs.sent("intake", "Other")).toThrow(/^Container 'intake' is rendered for model 'Admission', not 'Other'\.$/);
    expect(inputs.sent("intake", "Admission")).toStrictEqual({ nights: 5 });
  });

  test("ReadStopsWhenItsSendStops: a container no live plan renders stops even when its element exists", () => {
    const inputs = page();
    expect(() => inputs.sent("intake", "Admission")).toThrow(/^No live plan renders container 'intake' for model 'Admission'\.$/);
  });

  test("OneValueOnEveryHost: two scalar bindings stop before either input is evaluated", () => {
    const inputs = page();
    const evaluated: string[] = [];
    inputs.join(plan(), new Visit(), "on the page", () => { evaluated.push("own"); return 5; });
    inputs.join(plan({ slot: field("Nights", ["nights"], convertedFrom(literal(8))) }, {}), new Visit(), "in slot 'card'", () => { evaluated.push("slot"); return 8; });
    expect(() => inputs.sent("intake", "Admission")).toThrow(/^Inputs 'own' and 'slot' both bind 'Nights' inside container 'intake'\.$/);
    expect(evaluated).toStrictEqual([]);
  });

  test("ReadFindsWhatItsContainerSends: an outside input is neither read nor a duplicate", () => {
    const inputs = page();
    inputs.join(plan(), new Visit(), "on the page", literalValue);
    inputs.join(plan({ other: field("Nights", ["nights"], convertedFrom(literal(8))) }, {}), new Visit(), "on the page", () => { throw new Error("The outside input was read."); });
    expect(inputs.sent("intake", "Admission")).toStrictEqual({ nights: 5 });
  });

  test("indexed bindings build lists and dictionary keys without treating their distinct names as duplicates", () => {
    const inputs = page();
    inputs.join(plan({
      own: field("Doses[0].Amount", ["doses", 0, "amount"], convertedFrom(literal(2))),
      slot: field("Doses[1].Amount", ["doses", 1, "amount"], convertedFrom(literal(3))),
      second: field("Rounds[night].Name", ["rounds", "night", "name"], convertedFrom(literal("Ada"))),
    }), new Visit(), "on the page", literalValue);
    expect(inputs.sent("intake", "Admission")).toStrictEqual({ doses: [{ amount: 2 }, { amount: 3 }], rounds: { night: { name: "Ada" } } });
  });

  test("a path beginning at a list position builds the container's list body", () => {
    const inputs = page();
    inputs.join(plan({
      own: field("[0].Name", [0, "name"], convertedFrom(literal("Ada"))),
      slot: field("[1].Name", [1, "name"], convertedFrom(literal("Grace"))),
    }), new Visit(), "on the page", literalValue);
    expect(inputs.sent("intake", "Admission")).toStrictEqual([{ name: "Ada" }, { name: "Grace" }]);
  });

  test("a body key named like an inherited member remains its own data", () => {
    const inputs = page();
    inputs.join(plan({ own: field("Map[__proto__].Name", ["map", "__proto__", "name"], convertedFrom(literal("Ada"))) }), new Visit(), "on the page", literalValue);
    expect(inputs.sent("intake", "Admission")).toStrictEqual(JSON.parse('{"map":{"__proto__":{"name":"Ada"}}}'));
  });

  test("sent builds a fresh body and reads values in the call's step", () => {
    const containers = new Map([["intake", new Set(["own"])]]);
    const inputs = page(containers);
    let nights = 5;
    inputs.join(plan(), new Visit(), "on the page", () => nights);
    const before = inputs.sent("intake", "Admission");
    nights = 9;
    inputs.join(plan({ slot: field("Room", ["room"], convertedFrom(literal("B-12"))) }, {}), new Visit(), "in slot 'card'", literalValue);
    containers.set("intake", new Set(["own", "slot"]));
    expect(before).toStrictEqual({ nights: 5 });
    expect(inputs.sent("intake", "Admission")).toStrictEqual({ nights: 9, room: "B-12" });
  });

  test("ReadSeesTheSlotThatJoinedOrLeftBeforeIt: joining stops a duplicate and release removes it", () => {
    const inputs = page();
    inputs.join(plan(), new Visit(), "on the page", literalValue);
    const slot = new Visit();
    inputs.join(plan({ slot: field("Nights", ["nights"], convertedFrom(literal(8))) }, {}), slot, "in slot 'card'", literalValue);
    expect(() => inputs.sent("intake", "Admission")).toThrow(/^Inputs 'own' and 'slot' both bind 'Nights' inside container 'intake'\.$/);
    slot.release();
    expect(inputs.sent("intake", "Admission")).toStrictEqual({ nights: 5 });
  });

  test("AnUnsentMemberReadsWhatTheEndpointBinds: no binding leaves the body member absent for its data start", () => {
    const inputs = page();
    inputs.join(plan({}, intake), new Visit(), "on the page", literalValue);
    const slot = new Visit();
    inputs.join(plan({ slot: field("Nights", ["nights"], convertedFrom(literal(8))) }, {}), slot, "in slot 'card'", literalValue);
    expect(inputs.sent("intake", "Admission")).toStrictEqual({ nights: 8 });
    slot.release();
    expect(inputs.sent("intake", "Admission")).toStrictEqual({});
  });

  test("an owner's release drops its containers as well as its inputs", () => {
    const inputs = page();
    const owner = new Visit();
    inputs.join(plan(), owner, "on the page", literalValue);
    owner.release();
    expect(() => inputs.sent("intake", "Admission")).toThrow(/^No live plan renders container 'intake' for model 'Admission'\.$/);
    inputs.join(plan({}, intake), new Visit(), "on the page", literalValue);
    expect(inputs.sent("intake", "Admission")).toStrictEqual({});
  });

  test("releasing one rendering leaves another live rendering of the same container", () => {
    const inputs = page();
    const first = new Visit();
    inputs.join(plan({}, intake), first, "on the page", literalValue);
    inputs.join(plan({}, intake), new Visit(), "in slot 'card'", literalValue);
    first.release();
    expect(inputs.sent("intake", "Admission")).toStrictEqual({});
  });

  test("R14.6 refuses the whole contribution and names the input and both plan places", () => {
    const inputs = page();
    inputs.join(plan(), new Visit(), "on the page", literalValue);
    const refused = new Visit();
    expect(() => { inputs.join(plan({ second: field("Room", ["room"], convertedFrom(literal("B-12"))), ...fields }, {}), refused, "in slot 'card'", literalValue); })
      .toThrow(/^Input 'own' is already held by plan 'Admission' on the page; plan 'Admission' in slot 'card' did not start\.$/);
    refused.release();
    expect(inputs.sent("intake", "Admission")).toStrictEqual({ nights: 5 });
  });

  test("R14.6 permits an input id after the previous owner's release", () => {
    const inputs = page();
    const first = new Visit();
    inputs.join(plan(), first, "on the page", literalValue);
    first.release();
    inputs.join(plan({ own: field("Nights", ["nights"], convertedFrom(literal(9))) }), new Visit(), "in slot 'card'", literalValue);
    expect(inputs.sent("intake", "Admission")).toStrictEqual({ nights: 9 });
  });
});

describe("when a check reads what a container sends", () => {
  const typed = (text: string): Converted => ({ kind: "as", expr: literal(text), shape: { kind: "nullable", inner: { kind: "number", of: "integer", text: "view", overflow: "nothing" } } });
  const counted = (text: string): Inputs => ({ own: field("Count", ["count"], typed(text), literal(0)), outside: field("Name", ["name"], convertedFrom(literal("Ada"))) });
  const read = (expression: Source): unknown => {
    if (expression.kind === "lit") return expression.value;
    if (expression.kind !== "as" || expression.expr.kind !== "lit") throw new Error("This contribution reads a literal or a number typed into it.");
    const text = expression.expr.value;
    return typeof text === "string" && /^[0-9]+$/.test(text) ? Number(text) : null;
  };

  test("UnsentInputsUnread: the capture holds the container's inputs alone, each under its binding name", () => {
    const inputs = page(new Map([["intake", new Set(["own"])]]));
    inputs.join(plan(counted("12")), new Visit(), "on the page", read);
    const captured = inputs.captured("intake", "Admission");
    expect(captured.body).toStrictEqual({ count: 12 });
    expect(captured.inputs.map(input => [input.name, input.entry.id])).toStrictEqual([["Count", "own"]]);
  });

  test("NoTypedValueLost: a text the member's type cannot hold is read back for its check", () => {
    const inputs = page(new Map([["intake", new Set(["own"])]]));
    inputs.join(plan(counted("12a")), new Visit(), "on the page", read);
    const [input] = inputs.captured("intake", "Admission").inputs;
    if (input === undefined) throw new Error("The container sends one input.");
    expect(inputs.captured("intake", "Admission").body).toStrictEqual({ count: 0 });
    expect(inputs.typedText(input)).toBe("12a");
  });

  test("NoTypedValueLost: a text that converts, and a blank text, read no typed text", () => {
    for (const text of ["12", "", "   "]) {
      const inputs = page(new Map([["intake", new Set(["own"])]]));
      inputs.join(plan(counted(text)), new Visit(), "on the page", read);
      const [input] = inputs.captured("intake", "Admission").inputs;
      if (input === undefined) throw new Error("The container sends one input.");
      expect(inputs.typedText(input)).toBeNull();
    }
  });

  test("NoTypedValueLost: an input that left the page stops its check", () => {
    const inputs = page(new Map([["intake", new Set(["own"])]]));
    const owner = new Visit();
    inputs.join(plan(counted("12a")), owner, "on the page", read);
    const [input] = inputs.captured("intake", "Admission").inputs;
    if (input === undefined) throw new Error("The container sends one input.");
    owner.release();
    expect(() => inputs.typedText(input)).toThrow(/^Input 'own' left the page before its check read it\.$/);
  });
});

describe("when an input is checked live", () => {
  const live = (validator: string, body: string): Containers[string]["live"][number] => ({ validator, ruleSet: "default", body: literal(body) });
  const nights = field("Nights", ["nights"], convertedFrom(literal(5)));
  const room = field("Room", ["room"], convertedFrom(literal("B-12")));
  const held: Inputs = { own: nights, second: room };
  const sentOwn = { name: "Nights", entry: { id: "own", binding: nights } };
  const sentSecond = { name: "Room", entry: { id: "second", binding: room } };

  test("each container that holds the input gives its live check over its own inputs, and a container that does not hold it gives none", () => {
    const inputs = page(new Map([["intake", new Set(["own", "second"])], ["kin", new Set(["own"])], ["ward", new Set(["second"])], ["bare", new Set(["own"])]]));
    inputs.join(plan(held, {
      intake: { model: "Admission", kind: "form", summary: "intake_summary", live: [live("AdmissionRules", "the intake's body")] },
      kin: { model: "Admission", kind: "element", summary: "kin_summary", live: [live("KinRules", "the kin's body")] },
      ward: { model: "Admission", kind: "element", summary: "ward_summary", live: [live("WardRules", "the ward's body")] },
      bare: { model: "Admission", kind: "element", summary: "bare_summary", live: [] },
    }), new Visit(), "on the page", literalValue);
    expect(inputs.liveChecksOf("own")).toStrictEqual([
      { registration: "AdmissionRules:default", body: "the intake's body", inputs: [sentOwn, sentSecond] },
      { registration: "KinRules:default", body: "the kin's body", inputs: [sentOwn] },
    ]);
  });

  test("a container whose element the page lacks gives no live check, and a released one gives none", () => {
    const inputs = page(new Map([["kin", new Set(["own"])]]));
    const owner = new Visit();
    inputs.join(plan(held, {
      intake: { model: "Admission", kind: "form", summary: "intake_summary", live: [live("AdmissionRules", "the intake's body")] },
      kin: { model: "Admission", kind: "element", summary: "kin_summary", live: [live("KinRules", "the kin's body")] },
    }), owner, "on the page", literalValue);
    expect(inputs.liveChecksOf("own")).toStrictEqual([{ registration: "KinRules:default", body: "the kin's body", inputs: [sentOwn] }]);
    owner.release();
    expect(inputs.liveChecksOf("own")).toStrictEqual([]);
    expect(inputs.entries()).toStrictEqual([]);
  });
});

describe("when an answer shows where a request's failures go", () => {
  test("InlineOnlyAtTheSentInput: the table holds an entry sent until its input leaves, and a load under the same id is another entry", () => {
    const inputs = page();
    const slot = new Visit();
    inputs.join(plan(), new Visit(), "on the page", literalValue);
    inputs.join(plan({ slot: field("Room", ["room"], convertedFrom(literal("B-12"))) }, {}, "RoomCard"), slot, "in slot 'room'", literalValue);
    const [own, room] = inputs.captured("intake", "Admission").inputs;
    if (own === undefined || room === undefined) throw new Error("The container sends two inputs.");
    expect([inputs.holds(own.entry), inputs.holds(room.entry)]).toStrictEqual([true, true]);
    slot.release();
    expect(inputs.holds(room.entry)).toBe(false);
    inputs.join(plan({ slot: field("Room", ["room"], convertedFrom(literal("B-12"))) }, {}, "RoomCard"), new Visit(), "in slot 'room'", literalValue);
    expect([inputs.holds(own.entry), inputs.holds(room.entry)]).toStrictEqual([true, false]);
  });

  test("FailureReachesSummary: a container's summary is the element its live rendering names", () => {
    const inputs = page();
    const owner = new Visit();
    inputs.join(plan({}, intake), owner, "on the page", literalValue);
    expect(inputs.summaryOf("intake")).toBe("intake_summary");
    owner.release();
    expect(() => inputs.summaryOf("intake")).toThrow(/^No live plan renders container 'intake', whose summary shows the request's failures\.$/);
  });
});

describe("when a request captures what it sends", () => {
  test("RecordsAMissingContainer: a container whose element the page lacks is recorded missing, not stopped at the read", () => {
    const inputs = page(new Map());
    inputs.join(plan(), new Visit(), "on the page", literalValue);
    expect(inputs.sends("intake", "Admission")).toStrictEqual({ kind: "missing", id: "intake" });
    expect(inputs.submits("intake", "Admission")).toStrictEqual({ kind: "missing", id: "intake" });
  });

  test("ContainerOfItsOwnModel: a container rendered for another model stops the request where it is read", () => {
    const inputs = page();
    inputs.join(plan(), new Visit(), "on the page", literalValue);
    expect(() => inputs.sends("intake", "Visit")).toThrow(/^Container 'intake' is rendered for model 'Admission', not 'Visit'\.$/);
    expect(inputs.sends("intake", "Admission")).toMatchObject({ kind: "captured", id: "intake", body: { nights: 5 } });
  });

  test("SubmissionSetAtTheRead: a native form's submission set is built when the request reads it, beside its framework inputs' body", () => {
    let code = "A7";
    const inputs = page(undefined, new Map([["intake", () => {
      const submitted = new FormData();
      submitted.append("Code", code);
      return submitted;
    }]]));
    inputs.join(plan(), new Visit(), "on the page", literalValue);
    const captured = inputs.submits("intake", "Admission");
    if (captured.kind !== "captured") throw new Error("The form is on the page.");
    code = "B9";
    const submission = captured.body as { entries: FormData; members: object };
    expect([...submission.entries.entries()]).toStrictEqual([["Code", "A7"]]);
    expect(submission.members).toStrictEqual({ nights: 5 });
  });

  test("RecordsAMissingInput: an input the page lacks is recorded missing and its value reads nothing", () => {
    const inputs = page();
    inputs.join(plan(), new Visit(), "on the page", literalValue);
    expect(inputs.input("gone", "Ward")).toStrictEqual({ kind: "missing", id: "gone", name: "Ward" });
    expect(inputs.valueOf("gone")).toBeUndefined();
    expect(inputs.input("own", "Stay")).toStrictEqual({ kind: "captured", input: { name: "Stay", entry: { id: "own", binding: field("Nights", ["nights"], convertedFrom(literal(5))) } } });
    expect(inputs.valueOf("own")).toBe(5);
  });

  test("CheckedIsWhatIsSent: the checked body holds the set members, with the container's inputs over them", () => {
    const inputs = page();
    inputs.join(plan(), new Visit(), "on the page", literalValue);
    const container = inputs.sends("intake", "Admission");
    const checked = inputs.checked({ container, individual: [] }, null,
      [{ path: ["nights"], value: { kind: "held", value: 2 } }, { path: ["ward"], value: { kind: "held", value: "North" } }, { path: ["room"], value: { kind: "nothing" } }]);
    expect(checked?.body).toStrictEqual({ nights: 5, ward: "North" });
    expect(checked?.inputs.map(input => input.entry.id)).toStrictEqual(["own"]);
    expect(inputs.checked({ container: { kind: "none" }, individual: [] }, { ward: "East" }, [])).toStrictEqual({ body: { ward: "East" }, inputs: [] });
  });

  test("CheckedWaitsForTheGuard: a missing container or input leaves the check to the guard that stops the request", () => {
    const inputs = page();
    expect(inputs.checked({ container: { kind: "missing", id: "intake" }, individual: [] }, null, [])).toBeNull();
    expect(inputs.checked({ container: { kind: "none" }, individual: [{ kind: "missing", id: "gone", name: "Ward" }] }, null, [])).toBeNull();
  });
});
