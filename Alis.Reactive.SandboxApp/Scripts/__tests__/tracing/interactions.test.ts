import { afterEach, beforeEach, describe, expect, it } from "vitest";
import {
  currentTraceparent,
  getCurrentRoot,
  getCurrentSpanId,
  getCurrentTraceId,
  resetForTests as resetInteractions,
  run,
  runWithRoot,
  setRootFromTraceparent,
} from "../../tracing/interactions";
import {
  boundTracer,
  configure,
  resetForTests as resetTrace,
  tracer,
} from "../../tracing/trace";
import type { TraceEvent, TraceSink } from "../../tracing/types";

class RecordingSink implements TraceSink {
  readonly events: TraceEvent[] = [];
  emit(event: TraceEvent): void {
    this.events.push(event);
  }
  flush(): void {}
}

let sink: RecordingSink;

beforeEach(() => {
  resetInteractions();
  resetTrace();
  sink = new RecordingSink();
  configure({ level: "trace", sink });
});

afterEach(() => {
  resetInteractions();
  resetTrace();
});

describe("run — sync return", () => {
  it("emits interaction.start then interaction.end and returns the value", () => {
    const result = run("document-event", { event: "click" }, () => 42);
    expect(result).toBe(42);
    const names = sink.events.map((e) => e.event);
    expect(names).toEqual(["interaction.start", "interaction.end"]);
    expect(sink.events[0].data).toMatchObject({ name: "document-event", event: "click" });
    expect(sink.events[1].data).toMatchObject({ name: "document-event" });
    expect(sink.events[1].data).toHaveProperty("ms");
  });

  it("clears current interaction after sync success", () => {
    run("test", {}, () => 1);
    expect(getCurrentTraceId()).toBeUndefined();
    expect(currentTraceparent()).toBeUndefined();
  });
});

describe("run — sync throw", () => {
  it("emits interaction.fail and rethrows", () => {
    const err = new Error("boom");
    expect(() => run("test", { foo: "bar" }, () => { throw err; })).toThrow("boom");
    const names = sink.events.map((e) => e.event);
    expect(names).toEqual(["interaction.start", "interaction.fail"]);
    expect(sink.events[1].level).toBe("error");
    expect(sink.events[1].error?.message).toBe("boom");
    expect(sink.events[1].data).toMatchObject({ name: "test", foo: "bar" });
  });

  it("clears current interaction after sync throw", () => {
    expect(() => run("test", {}, () => { throw new Error("x"); })).toThrow();
    expect(getCurrentTraceId()).toBeUndefined();
    expect(currentTraceparent()).toBeUndefined();
  });

  it("coerces non-Error throws to Error for structured carriage", () => {
    expect(() => run("test", {}, () => { throw "string-error"; })).toThrow("string-error");
    expect(sink.events[1].error?.message).toBe("string-error");
  });
});

describe("run — async resolve", () => {
  it("emits interaction.start immediately, interaction.end on resolve", async () => {
    const p = run("http", { url: "/api" }, async () => {
      await Promise.resolve();
      return "ok";
    });
    // start already emitted synchronously
    expect(sink.events.map((e) => e.event)).toEqual(["interaction.start"]);
    const result = await p;
    expect(result).toBe("ok");
    expect(sink.events.map((e) => e.event)).toEqual(["interaction.start", "interaction.end"]);
  });

  it("clears current interaction after async resolve", async () => {
    await run("test", {}, async () => 1);
    expect(getCurrentTraceId()).toBeUndefined();
  });
});

describe("run — async reject", () => {
  it("emits interaction.fail and rethrows the rejection", async () => {
    const err = new Error("network down");
    await expect(
      run("http", { url: "/api" }, async () => {
        await Promise.resolve();
        throw err;
      }),
    ).rejects.toThrow("network down");
    const names = sink.events.map((e) => e.event);
    expect(names).toEqual(["interaction.start", "interaction.fail"]);
    expect(sink.events[1].error?.message).toBe("network down");
  });

  it("clears current interaction after async reject", async () => {
    await expect(
      run("test", {}, async () => { throw new Error("x"); }),
    ).rejects.toThrow();
    expect(getCurrentTraceId()).toBeUndefined();
  });
});

describe("run — nested reuses outer trace", () => {
  it("inner run does not start a new trace-id", () => {
    let outerTraceId: string | undefined;
    let innerTraceId: string | undefined;
    run("outer", {}, () => {
      outerTraceId = getCurrentTraceId();
      run("inner", {}, () => {
        innerTraceId = getCurrentTraceId();
        return 1;
      });
    });
    expect(outerTraceId).toBeDefined();
    expect(innerTraceId).toBe(outerTraceId);
  });

  it("inner run restores outer context on inner fail", () => {
    let outerTraceId: string | undefined;
    run("outer", {}, () => {
      outerTraceId = getCurrentTraceId();
      try {
        run("inner", {}, () => { throw new Error("x"); });
      } catch {
        // swallowed — the outer context should still be active
      }
      expect(getCurrentTraceId()).toBe(outerTraceId);
    });
  });
});

describe("run — concurrent async interactions stay isolated", () => {
  it("a fresh entry-point firing while another is awaiting gets its own trace-id", async () => {
    // This is the regression test for the module-global `current` bug.
    // When click A is awaiting and click B fires synchronously between
    // microtasks, B must NOT inherit A's trace-id even though `current`
    // is still set to A's root.
    let aTraceIdAtStart: string | undefined;
    let bTraceIdAtStart: string | undefined;

    let resolveA!: () => void;
    const aGate = new Promise<void>((r) => { resolveA = r; });

    const promiseA = run("A", {}, async () => {
      aTraceIdAtStart = getCurrentTraceId();
      await aGate;
    });

    // Start B while A is still awaiting aGate.
    const promiseB = run("B", {}, async () => {
      bTraceIdAtStart = getCurrentTraceId();
    });

    await promiseB;
    resolveA();
    await promiseA;

    expect(aTraceIdAtStart).toMatch(/^[0-9a-f]{32}$/);
    expect(bTraceIdAtStart).toMatch(/^[0-9a-f]{32}$/);
    expect(aTraceIdAtStart).not.toBe(bTraceIdAtStart);
  });

  it("interaction.start and interaction.end events carry the right trace-id under concurrency", async () => {
    let resolveA!: () => void;
    const aGate = new Promise<void>((r) => { resolveA = r; });

    const promiseA = run("A", {}, async () => {
      await aGate;
    });
    const promiseB = run("B", {}, async () => {
      // sync body
    });

    await promiseB;
    resolveA();
    await promiseA;

    // sink should contain start/end pairs for both A and B with distinct trace-ids
    const aStart = sink.events.find((e) => e.event === "interaction.start" && e.data?.name === "A");
    const aEnd = sink.events.find((e) => e.event === "interaction.end" && e.data?.name === "A");
    const bStart = sink.events.find((e) => e.event === "interaction.start" && e.data?.name === "B");
    const bEnd = sink.events.find((e) => e.event === "interaction.end" && e.data?.name === "B");

    expect(aStart?.traceId).toBeDefined();
    expect(aEnd?.traceId).toBe(aStart?.traceId);
    expect(bStart?.traceId).toBeDefined();
    expect(bEnd?.traceId).toBe(bStart?.traceId);
    expect(aStart?.traceId).not.toBe(bStart?.traceId);
  });
});

describe("currentTraceparent", () => {
  it("returns undefined when no interaction is active", () => {
    expect(currentTraceparent()).toBeUndefined();
  });

  it("returns a valid W3C header when an interaction is active", () => {
    run("test", {}, () => {
      const tp = currentTraceparent();
      expect(tp).toMatch(/^00-[0-9a-f]{32}-[0-9a-f]{16}-[0-9a-f]{2}$/);
    });
  });

  it("holds trace-id constant across successive calls within one interaction", () => {
    run("test", {}, () => {
      const first = currentTraceparent()!;
      const second = currentTraceparent()!;
      const firstTraceId = first.split("-")[1];
      const secondTraceId = second.split("-")[1];
      expect(firstTraceId).toBe(secondTraceId);
    });
  });

  it("generates a fresh span-id on each call", () => {
    run("test", {}, () => {
      const first = currentTraceparent()!;
      const second = currentTraceparent()!;
      const firstSpanId = first.split("-")[2];
      const secondSpanId = second.split("-")[2];
      expect(firstSpanId).not.toBe(secondSpanId);
    });
  });

  it("preserves server flags when configured from traceparent", () => {
    setRootFromTraceparent("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-00");
    run("test", {}, () => {
      const tp = currentTraceparent()!;
      const [, traceId, , flags] = tp.split("-");
      expect(traceId).toBe("4bf92f3577b34da6a3ce929d0e0e4736");
      expect(flags).toBe("00");
    });
  });

  it("defaults to sampled flags when generating a new root", () => {
    run("test", {}, () => {
      const tp = currentTraceparent()!;
      const flags = tp.split("-")[3];
      expect(flags).toBe("01");
    });
  });
});

describe("getCurrentTraceId + getCurrentSpanId", () => {
  it("return undefined outside an interaction", () => {
    expect(getCurrentTraceId()).toBeUndefined();
    expect(getCurrentSpanId()).toBeUndefined();
  });

  it("return valid IDs inside an interaction", () => {
    run("test", {}, () => {
      expect(getCurrentTraceId()).toMatch(/^[0-9a-f]{32}$/);
      expect(getCurrentSpanId()).toMatch(/^[0-9a-f]{16}$/);
    });
  });
});

describe("setRootFromTraceparent", () => {
  it("clears root when passed undefined", () => {
    setRootFromTraceparent("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01");
    setRootFromTraceparent(undefined);
    expect(currentTraceparent()).toBeUndefined();
  });

  it("ignores malformed traceparent and leaves no default root", () => {
    setRootFromTraceparent("garbage");
    expect(currentTraceparent()).toBeUndefined();
  });

  it("server traceparent is consumed once — subsequent top-level runs mint fresh roots", () => {
    setRootFromTraceparent("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01");

    let firstTraceId: string | undefined;
    let secondTraceId: string | undefined;

    run("first", {}, () => {
      firstTraceId = getCurrentTraceId();
    });
    run("second", {}, () => {
      secondTraceId = getCurrentTraceId();
    });

    // First top-level run inherits the server traceparent.
    expect(firstTraceId).toBe("4bf92f3577b34da6a3ce929d0e0e4736");
    // Second top-level run gets its own freshly-minted trace-id —
    // configuredFromTraceparent was consumed exactly once.
    expect(secondTraceId).toBeDefined();
    expect(secondTraceId).not.toBe(firstTraceId);
    expect(secondTraceId).toMatch(/^[0-9a-f]{32}$/);
  });

  it("consumed traceparent does not leak into nested runs", () => {
    setRootFromTraceparent("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01");
    let outerTraceId: string | undefined;
    let nestedTraceId: string | undefined;

    run("outer", {}, () => {
      outerTraceId = getCurrentTraceId();
      run("inner", {}, () => {
        nestedTraceId = getCurrentTraceId();
      });
    });

    expect(outerTraceId).toBe("4bf92f3577b34da6a3ce929d0e0e4736");
    // Sync nesting reuses outer's trace-id, not a re-consumed traceparent.
    expect(nestedTraceId).toBe(outerTraceId);
  });
});

describe("boundTracer — async context preservation", () => {
  it("pinned tracer carries the captured root through emits even after the global current is overwritten", () => {
    let pinned: ReturnType<typeof boundTracer> | undefined;
    let aTraceId: string | undefined;

    run("A", {}, () => {
      aTraceId = getCurrentTraceId();
      pinned = boundTracer("test");
    });
    // After A's run, current is undefined.
    expect(getCurrentTraceId()).toBeUndefined();

    // Concurrent unrelated interaction overwrites current.
    run("B", {}, () => {
      // pinned tracer was captured under A — emits via pinned should still
      // carry A's trace-id even though current is now B's root.
      pinned!.info("post-A");
      const postAEvent = sink.events.find((e) => e.event === "post-A");
      expect(postAEvent?.traceId).toBe(aTraceId);
    });
  });

  it("pinned tracer survives concurrent overlapping interactions and out-of-order completion", async () => {
    let aRoot: ReturnType<typeof boundTracer> | undefined;
    let aTraceIdAtCapture: string | undefined;

    let resolveA!: () => void;
    const aGate = new Promise<void>((r) => { resolveA = r; });

    const promiseA = run("A", {}, async () => {
      aTraceIdAtCapture = getCurrentTraceId();
      aRoot = boundTracer("test");
      await aGate;
      // After the await, emit through the pinned tracer — even if a
      // concurrent interaction has touched current, this emit must
      // still carry A's trace-id.
      aRoot.info("post-await");
    });

    // Start B while A is awaiting; B completes first, displacing current.
    const promiseB = run("B", {}, () => {});
    await promiseB;
    resolveA();
    await promiseA;

    const postAwaitEvent = sink.events.find((e) => e.event === "post-await");
    expect(postAwaitEvent).toBeDefined();
    expect(postAwaitEvent?.traceId).toBe(aTraceIdAtCapture);
  });
});

describe("runWithRoot — sync re-entry helper", () => {
  it("temporarily installs a root and restores the previous current", () => {
    const fakeRoot = { traceId: "a".repeat(32), flags: "01" } as const;
    expect(getCurrentRoot()).toBeUndefined();
    runWithRoot(fakeRoot, () => {
      expect(getCurrentRoot()).toEqual(fakeRoot);
      tracer("test").info("inside");
    });
    expect(getCurrentRoot()).toBeUndefined();
    const event = sink.events.find((e) => e.event === "inside");
    expect(event?.traceId).toBe(fakeRoot.traceId);
  });

  it("is a no-op when root is undefined", () => {
    let observed: string | undefined = "before";
    runWithRoot(undefined, () => {
      observed = getCurrentTraceId();
    });
    expect(observed).toBeUndefined();
  });

  it("restores current even when fn throws", () => {
    const fakeRoot = { traceId: "b".repeat(32), flags: "01" } as const;
    expect(() =>
      runWithRoot(fakeRoot, () => {
        throw new Error("boom");
      }),
    ).toThrow("boom");
    expect(getCurrentRoot()).toBeUndefined();
  });
});
