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
  heardOn: () => untraced,
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

function raisedWith(bound: Readonly<Record<string, unknown>>): Raised {
  const scope = new Map<string, Held>();
  for (const [name, value] of Object.entries(bound)) scope.set(name, { kind: "value", value });
  return { plan: ward, scope, signal: new AbortController().signal, trace: untraced };
}

function invoke(answer: unknown, ...args: unknown[]): unknown {
  if (typeof answer !== "function") throw new Error("The pump has no answer.");
  return Reflect.apply(answer, undefined, args);
}

function installed(body: Source, bound: Readonly<Record<string, unknown>> = {}, name = "answer1"): unknown {
  const pump = { answer: undefined };
  new Lane("overlap", [{
    kind: "set", path: ["answer"], members: "required", content: false,
    target: { kind: "ref", name: "pump" },
    value: { kind: "answer", arguments: name, body },
  }], aRoundsOwner).raise(raisedWith({ ...bound, pump }));
  return pump.answer;
}

class Pump {
  answer: unknown;
  readonly labels: unknown[] = ["Primed"];
  rate = 4;

  draw(...args: unknown[]): unknown {
    const label = invoke(this.answer, ...args);
    this.labels.push(label);
    return label;
  }

  set reading(row: unknown) {
    this.draw(row);
  }

  measure(): number {
    this.rate += 1;
    return this.rate;
  }

  order(key: unknown): number[] {
    return [{ rate: 8 }, { rate: 2 }].map((reading) => Number(invoke(key, reading))).sort((first, second) => first - second);
  }
}

afterEach(() => {
  vi.restoreAllMocks();
});

describe("when a vendor calls an answer", () => {
  test("the function returns the value its body reads", () => {
    const answer = installed({ kind: "lit", value: 17 });

    expect(invoke(answer)).toBe(17);
  });

  test("the first argument is the object whose member the body reads", () => {
    const answer = installed({
      kind: "get", path: ["label"], members: "required",
      target: { kind: "get", path: [0], members: "required", target: { kind: "ref", name: "answer1" } },
    });

    expect(invoke(answer, { label: "North" })).toBe("North");
    expect(invoke(answer, { label: "East" })).toBe("East");
  });

  test("the plan names the complete list of arguments and the value accessor reads position one", () => {
    const answer = installed({
      kind: "get", path: [1, "label"], members: "required", target: { kind: "ref", name: "answer7" },
    }, {}, "answer7");

    expect(invoke(answer, "dose", { label: "Insulin" })).toBe("Insulin");
  });

  test("the answer keeps the enclosing handlers arguments visible", () => {
    const answer = installed({
      kind: "op", op: "eq", args: [
        { kind: "get", path: [0], members: "required", target: { kind: "ref", name: "arguments" } },
        { kind: "get", path: [0], members: "required", target: { kind: "ref", name: "answer1" } },
      ],
    }, { arguments: [10] });

    expect(invoke(answer, 10)).toBe(true);
    expect(invoke(answer, 4)).toBe(false);
  });

  test("X17.20: a page property is read anew on every call", () => {
    const page = { rate: 4 };
    const answer = installed({
      kind: "get", path: ["rate"], members: "required", target: { kind: "ref", name: "page" },
    }, { page });

    expect(invoke(answer)).toBe(4);
    page.rate = 9;
    expect(invoke(answer)).toBe(9);
  });

  test("X17.20: a method result is called anew on every call", () => {
    const page = new Pump();
    const answer = installed({
      kind: "call", path: ["measure"], members: "required", target: { kind: "ref", name: "page" }, args: [],
    }, { page });

    expect(page.rate).toBe(4);
    expect(invoke(answer)).toBe(5);
    expect(invoke(answer)).toBe(6);
  });

  test("a result read twice is called once in each answer", () => {
    const page = new Pump();
    const answer = installed({
      kind: "call", path: [], members: "required",
      target: {
        kind: "fn", params: ["result1"],
        body: { kind: "op", op: "eq", args: [{ kind: "ref", name: "result1" }, { kind: "ref", name: "result1" }] },
      },
      args: [{ kind: "call", path: ["measure"], members: "required", target: { kind: "ref", name: "page" }, args: [] }],
    }, { page });

    expect(invoke(answer)).toBe(true);
    expect(invoke(answer)).toBe(true);
    expect(page.rate).toBe(6);
  });

  test("nested answers preserve each calls own arguments", () => {
    const outer = installed({
      kind: "answer", arguments: "answer2",
      body: {
        kind: "op", op: "eq", args: [
          { kind: "get", path: [0, "rate"], members: "required", target: { kind: "ref", name: "answer1" } },
          { kind: "get", path: [0, "rate"], members: "required", target: { kind: "ref", name: "answer2" } },
        ],
      },
    });
    const first = invoke(outer, { rate: 2 });
    const second = invoke(outer, { rate: 7 });

    expect(invoke(first, { rate: 2 })).toBe(true);
    expect(invoke(second, { rate: 2 })).toBe(false);
    expect(invoke(first, { rate: 7 })).toBe(false);
  });

  test("X17.20: an answer passed as a method argument supplies its sort keys", () => {
    const pump = new Pump();
    const result = { rates: [] };
    new Lane("overlap", [{
      kind: "set", path: ["rates"], members: "required", content: false, target: { kind: "ref", name: "result" },
      value: {
        kind: "call", path: ["order"], members: "required", target: { kind: "ref", name: "pump" },
        args: [{
          kind: "answer", arguments: "answer1",
          body: { kind: "get", path: [0, "rate"], members: "required", target: { kind: "ref", name: "answer1" } },
        }],
      },
    }], aRoundsOwner).raise(raisedWith({ pump, result }));

    expect(result.rates).toStrictEqual([2, 8]);
  });

  test("an optional member that reads nothing answers null", () => {
    const answer = installed({
      kind: "get", path: ["label"], members: "optionalMember",
      target: { kind: "get", path: [0], members: "required", target: { kind: "ref", name: "answer1" } },
    });

    expect(invoke(answer, { label: "North" })).toBe("North");
    expect(invoke(answer, {})).toBeNull();
  });

  test("a required argument that the vendor omits throws the stop to its call", () => {
    const answer = installed({
      kind: "get", path: [1], members: "required", target: { kind: "ref", name: "answer1" },
    });

    expect(() => invoke(answer, "dose")).toThrow(expect.objectContaining({
      name: "Stop", message: "The member '1' is not on the object that should hold it.",
    }));
  });

  test("X17.22: a stop in the vendors own turn reaches it and leaves what it already drew", () => {
    const pump = new Pump();
    pump.answer = installed({
      kind: "get", path: [0, "label"], members: "required", target: { kind: "ref", name: "answer1" },
    });
    pump.draw({ label: "Running" });

    expect(() => pump.draw({})).toThrow(expect.objectContaining({
      name: "Stop", message: "The member 'label' is not on the object that should hold it.",
    }));
    expect(pump.labels).toStrictEqual(["Primed", "Running"]);
  });

  test("X17.21: a stop while a write makes the vendor ask ends that pipeline before its next step", () => {
    vi.spyOn(console, "error").mockImplementation(() => undefined);
    const pump = new Pump();
    pump.answer = installed({
      kind: "get", path: [0, "label"], members: "required", target: { kind: "ref", name: "answer1" },
    });
    const after = { status: "Waiting" };
    new Lane("overlap", [
      { kind: "set", path: ["reading"], members: "required", content: false, target: { kind: "ref", name: "pump" }, value: { kind: "ref", name: "good" } },
      { kind: "set", path: ["reading"], members: "required", content: false, target: { kind: "ref", name: "pump" }, value: { kind: "ref", name: "missing" } },
      { kind: "set", path: ["status"], members: "required", content: false, target: { kind: "ref", name: "after" }, value: { kind: "lit", value: "Finished" } },
    ], aRoundsOwner).raise(raisedWith({ pump, after, good: { label: "Running" }, missing: {} }));

    expect(pump.labels).toStrictEqual(["Primed", "Running"]);
    expect(after.status).toBe("Waiting");
  });

  test("a step after a vendor answer runs when the answer succeeds", () => {
    const pump = new Pump();
    pump.answer = installed({
      kind: "get", path: [0, "label"], members: "required", target: { kind: "ref", name: "answer1" },
    });
    const after = { status: "Waiting" };
    new Lane("overlap", [
      { kind: "do", await: false, expr: {
        kind: "call", path: ["draw"], members: "required", target: { kind: "ref", name: "pump" },
        args: [{ kind: "ref", name: "good" }],
      } },
      { kind: "set", path: ["status"], members: "required", content: false, target: { kind: "ref", name: "after" }, value: { kind: "lit", value: "Finished" } },
    ], aRoundsOwner).raise(raisedWith({ pump, after, good: { label: "Running" } }));

    expect(pump.labels).toStrictEqual(["Primed", "Running"]);
    expect(after.status).toBe("Finished");
  });
});
