import { afterEach, describe, expect, test, vi } from "vitest";
import { Lane } from "./index";

const aRoundsOwner: ConstructorParameters<typeof Lane>[2] = {
  runs: () => undefined,
  mounts: () => undefined,
  listens: () => undefined,
  holds: () => undefined,
  registers: () => undefined,
};

type Block = ConstructorParameters<typeof Lane>[1];
type Step = Block[number];
type Source = Extract<Step, { kind: "do" }>["expr"];
type Raised = Parameters<Lane["raise"]>[0];
type Held = Raised["scope"] extends ReadonlyMap<string, infer Read> ? Read : never;

const untraced: Raised["trace"] = {
  handler: () => untraced,
  run: () => untraced,
  span: () => untraced,
  send: () => ({ attempt: () => ({ headers: {}, answered: () => undefined, end: () => undefined }), end: () => undefined }),
  event: () => undefined,
  end: () => undefined,
};

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
}

function raisedWith(bound: Readonly<Record<string, unknown>>): Raised {
  const scope = new Map<string, Held>();
  for (const [name, value] of Object.entries(bound)) scope.set(name, { kind: "value", value });
  return { plan: ward, scope, signal: new AbortController().signal, trace: untraced };
}

const pagerNumber: Source = { kind: "get", path: ["pager", "number"], members: "required", target: { kind: "ref", name: "station" } };
const pagerExtension: Source = { kind: "get", path: ["pager", "extension"], members: "required", target: { kind: "ref", name: "station" } };
const binds = (name: string, binding: "line" | "read", expr: Source): Step => ({ kind: "let", name, await: false, binds: binding, expr });
const renumbers: Step = { kind: "set", path: ["pager", "number"], members: "required", content: false, target: { kind: "ref", name: "station" }, value: { kind: "lit", value: "7" } };
const charts = (value: Source): Step => ({ kind: "set", path: ["line"], members: "required", content: false, target: { kind: "ref", name: "chart" }, value });

function charted(...block: Step[]): unknown[] {
  const chart = new Chart();
  new Lane("overlap", block, aRoundsOwner).raise(raisedWith({ chart, station: { pager: { number: "4" } } }));
  return chart.lines;
}

afterEach(() => {
  vi.restoreAllMocks();
});

describe("when a let binds a value, its expression runs where its binding says", () => {
  test("a let bound at its line runs its expression there, so a later write does not change what its name reads", () => {
    expect(charted(binds("number", "line", pagerNumber), renumbers, charts({ kind: "ref", name: "number" }))).toStrictEqual(["4"]);
  });

  test("a let bound where its name is read runs its expression at the read, after the steps before it", () => {
    expect(charted(binds("number", "read", pagerNumber), renumbers, charts({ kind: "ref", name: "number" }))).toStrictEqual(["7"]);
  });

  test("a let bound where its name is read runs nothing when no step reads its name", () => {
    expect(charted(binds("extension", "read", pagerExtension), charts({ kind: "lit", value: "after" }))).toStrictEqual(["after"]);
  });

  test("a let bound at its line runs its expression though no step reads its name", () => {
    vi.spyOn(console, "error").mockImplementation(() => undefined);

    expect(charted(binds("extension", "line", pagerExtension), charts({ kind: "lit", value: "after" }))).toStrictEqual([]);
  });
});
