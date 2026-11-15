import { afterEach, describe, expect, test, vi } from "vitest";
import { Lane } from "./index";

type Block = ConstructorParameters<typeof Lane>[1];
type Step = Block[number];
type Source = Extract<Step, { kind: "do" }>["expr"];
type Raised = Parameters<Lane["raise"]>[0];
type Held = Raised["scope"] extends ReadonlyMap<string, infer Read> ? Read : never;

const ward: Raised["plan"] = {
  version: 7,
  planId: "ward",
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
  objects: {},
  handlers: [],
};

class Chart {
  readonly lines: unknown[] = [];

  set line(written: unknown) {
    this.lines.push(written);
  }

  note(...said: unknown[]): undefined {
    this.lines.push(said);
    return undefined;
  }
}

const rounds = [
  { ward: "North", by: "Ada Hart" },
  { ward: "East", by: "Ben Okafor" },
  { ward: "North", by: "Ada Byron", bed: 3 },
];

const aRoundsOwner: ConstructorParameters<typeof Lane>[2] = {
  runs: () => undefined,
  mounts: () => undefined,
  listens: () => undefined,
  holds: () => undefined,
  registers: () => undefined,
};

function raisedWith(bound: Readonly<Record<string, unknown>>, plan: Raised["plan"] = ward): Raised {
  const scope = new Map<string, Held>();
  for (const [name, value] of Object.entries(bound)) scope.set(name, { kind: "value", value });
  return { plan, scope, signal: new AbortController().signal };
}

function charted(bound: Readonly<Record<string, unknown>>, read: Source, plan: Raised["plan"] = ward): unknown[] {
  const chart = new Chart();
  new Lane("overlap", [{ kind: "set", path: ["line"], members: "required", content: false, target: { kind: "ref", name: "chart" }, value: read }], aRoundsOwner).raise(raisedWith({ ...bound, chart }, plan));
  return chart.lines;
}

const ref = (name: string): Source => ({ kind: "ref", name });
const lit = (held: string | number | boolean | null): Source => ({ kind: "lit", value: held });
const member = (target: Source, name: string, members: "required" | "optional" = "required"): Source => ({ kind: "get", path: [name], members, target });
const listOf = (target: Source, name: string): Source => ({ kind: "get", path: [name], members: "optional", target });
const over = (list: Source, operation: string, param: string, body: Source): Source => ({
  kind: "call",
  path: [operation],
  members: "requiredIfHeld",
  target: list,
  args: [{ kind: "fn", params: [param], body }],
});
const isNorth = (item: string): Source => ({ kind: "op", op: "eq", args: [member(ref(item), "ward"), lit("North")] });
const sorted = (list: Source, compare: "compareNumbers" | "compareCulture", key: string, descending: boolean): Source => {
  const keys = [member(ref("left"), key), member(ref("right"), key)];
  return {
    kind: "call",
    path: ["toSorted"],
    members: "requiredIfHeld",
    target: list,
    args: [{ kind: "fn", params: ["left", "right"], body: { kind: "op", op: compare, args: descending ? keys.reverse() : keys } }],
  };
};
const doses = [
  { medicine: "aspirin", units: 5 },
  { medicine: "Digoxin", units: 1 },
  { medicine: "insulin", units: 5 },
  { medicine: "Bisoprolol", units: 2 },
];

afterEach(() => {
  vi.restoreAllMocks();
});

describe("when a list's operation runs a lambda, the list's own method calls the fn on each item", () => {
  test("a filter keeps the items its lambda holds true for, each item bound to the fn's parameter, in the list's order", () => {
    expect(charted({ rounds }, over(ref("rounds"), "filter", "round", isNorth("round")))).toStrictEqual([[rounds[0], rounds[2]]]);
  });

  test("a map builds a new list of what its lambda reads from each item, null where the lambda reads nothing", () => {
    expect(charted({ rounds }, over(ref("rounds"), "map", "round", member(ref("round"), "bed", "optional")))).toStrictEqual([[null, null, 3]]);
  });

  test("some holds when the lambda holds for an item, and every holds when it holds for each", () => {
    expect(charted({ rounds }, over(ref("rounds"), "some", "round", isNorth("round")))).toStrictEqual([true]);
    expect(charted({ rounds }, over(ref("rounds"), "every", "round", isNorth("round")))).toStrictEqual([false]);
  });

  test("Any is some over a lambda whose body is true: it holds over a list with an item and not over a list of none", () => {
    expect(charted({ rounds }, over(ref("rounds"), "some", "round", lit(true)))).toStrictEqual([true]);
    expect(charted({ rounds: [] }, over(ref("rounds"), "some", "round", lit(true)))).toStrictEqual([false]);
  });

  test("every over a list of no item holds, as .NET's All does", () => {
    expect(charted({ rounds: [] }, over(ref("rounds"), "every", "round", isNorth("round")))).toStrictEqual([true]);
  });

  test("a find reads the first item its lambda keeps, and nothing when no item matches", () => {
    expect(charted({ rounds }, over(ref("rounds"), "find", "round", isNorth("round")))).toStrictEqual([rounds[0]]);
    expect(charted({ rounds }, over(ref("rounds"), "find", "round", { kind: "op", op: "eq", args: [member(ref("round"), "ward"), lit("West")] }))).toStrictEqual([null]);
  });

  test("an inner lambda reads its own item and the item of the lambda around it, each by its own name", () => {
    const wards = [
      { lead: "Ada Hart", rounds: [{ by: "Ada Hart" }] },
      { lead: "Ben Okafor", rounds: [{ by: "Ada Hart" }] },
    ];
    const ledByItsLead = over(member(ref("ward"), "rounds"), "some", "round", { kind: "op", op: "eq", args: [member(ref("round"), "by"), member(ref("ward"), "lead")] });

    expect(charted({ wards }, over(ref("wards"), "filter", "ward", ledByItsLead))).toStrictEqual([[wards[0]]]);
  });

  test("MissingListReadsNothing: an operation over a list that reads nothing reads nothing, and so does its count", () => {
    expect(charted({ report: {} }, over(listOf(ref("report"), "alarms"), "some", "alarm", lit(true)))).toStrictEqual([null]);
    expect(charted({ report: {} }, { kind: "get", path: ["length"], members: "requiredIfHeld", target: listOf(ref("report"), "alarms") })).toStrictEqual([null]);
  });

  test("an operation over a value that lacks the list's method stops the pipeline, as a paged result lacks filter", () => {
    vi.spyOn(console, "error").mockImplementation(() => undefined);

    expect(charted({ rounds: { result: rounds, count: 3 } }, over(ref("rounds"), "filter", "round", isNorth("round")))).toStrictEqual([]);
  });

  test("a lambda that reads a required member an item lacks stops the pipeline", () => {
    vi.spyOn(console, "error").mockImplementation(() => undefined);

    expect(charted({ rounds }, over(ref("rounds"), "map", "round", member(ref("round"), "bed")))).toStrictEqual([]);
  });
});

describe("when a step calls a method, each argument is read when the line runs", () => {
  test("an argument that reads nothing passes null", () => {
    const chart = new Chart();
    const notes: Step = {
      kind: "do",
      await: false,
      expr: { kind: "call", path: ["note"], members: "required", target: ref("chart"), args: [lit("Ada Hart"), member(ref("round"), "bed", "optional")] },
    };
    new Lane("overlap", [notes], aRoundsOwner).raise(raisedWith({ chart, round: rounds[0] }));

    expect(chart.lines).toStrictEqual([["Ada Hart", null]]);
  });
});

describe("when a list's operation reads an operator over its items, the plan's operator runs in the evaluator", () => {
  test("OrderBy sorts by toSorted over a comparison of each item's key, the smallest first, equal keys in the list's order", () => {
    expect(charted({ doses }, sorted(ref("doses"), "compareNumbers", "units", false))).toStrictEqual([[doses[1], doses[3], doses[0], doses[2]]]);
  });

  test("OrderByDescending swaps the two keys, the greatest first, equal keys in the list's order", () => {
    expect(charted({ doses }, sorted(ref("doses"), "compareNumbers", "units", true))).toStrictEqual([[doses[0], doses[2], doses[3], doses[1]]]);
  });

  test("a text key compared by the culture orders under the plan's collation, a before B", () => {
    expect(charted({ doses }, sorted(ref("doses"), "compareCulture", "medicine", false))).toStrictEqual([[doses[0], doses[3], doses[1], doses[2]]]);
  });

  test("the culture's order is the plan's own collation, so Zeta comes before Örn under sv-SE", () => {
    const swedish: Raised["plan"] = { ...ward, culture: { ...ward.culture, name: "sv-SE", collation: "sv-SE" } };
    const carers = [{ name: "Örn" }, { name: "Zeta" }];

    expect(charted({ doses: carers }, sorted(ref("doses"), "compareCulture", "name", false), swedish)).toStrictEqual([[carers[1], carers[0]]]);
    expect(charted({ doses: carers }, sorted(ref("doses"), "compareCulture", "name", false))).toStrictEqual([[carers[0], carers[1]]]);
  });

  test("Sum adds the map of its term by its operator, exactly for decimals", () => {
    const sum = (op: "sumInt" | "sumDecimal", key: string): Source => ({ kind: "op", op, args: [over(ref("doses"), "map", "dose", member(ref("dose"), key))] });

    expect(charted({ doses }, sum("sumInt", "units"))).toStrictEqual([13]);
    expect(charted({ doses: [{ mg: 0.1 }, { mg: 0.2 }] }, sum("sumDecimal", "mg"))).toStrictEqual([0.3]);
  });

  test("a Select's field of a type that holds no null stops the pipeline where an item's field reads nothing", () => {
    vi.spyOn(console, "error").mockImplementation(() => undefined);
    const units = over(ref("doses"), "map", "dose", { kind: "op", op: "nonNull", args: [member(ref("dose"), "units", "optional")] });

    expect(charted({ doses }, units)).toStrictEqual([[5, 1, 5, 2]]);
    expect(charted({ doses: [...doses, { medicine: "warfarin" }] }, units)).toStrictEqual([]);
  });
});
