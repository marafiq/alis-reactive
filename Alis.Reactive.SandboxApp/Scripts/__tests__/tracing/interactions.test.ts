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

  it("preserves server flags when a page-ready inherits from configuredFromTraceparent", () => {
    setRootFromTraceparent("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-00");
    run("page-ready", {}, () => {
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

  it("page-ready is the only trigger kind that inherits configuredFromTraceparent", () => {
    setRootFromTraceparent("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01");

    let pageReadyTraceId: string | undefined;
    let clickTraceId: string | undefined;

    // Only `page-ready` inherits.
    run("page-ready", {}, () => {
      pageReadyTraceId = getCurrentTraceId();
    });
    // `document-event` is a user interaction → fresh root, clears config.
    run("document-event", {}, () => {
      clickTraceId = getCurrentTraceId();
    });

    expect(pageReadyTraceId).toBe("4bf92f3577b34da6a3ce929d0e0e4736");
    expect(clickTraceId).toBeDefined();
    expect(clickTraceId).not.toBe(pageReadyTraceId);
    expect(clickTraceId).toMatch(/^[0-9a-f]{32}$/);
  });

  it("a nested run inside an outer page-ready reuses the inherited server trace", () => {
    setRootFromTraceparent("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01");
    let outerTraceId: string | undefined;
    let nestedTraceId: string | undefined;

    run("page-ready", {}, () => {
      outerTraceId = getCurrentTraceId();
      run("inner", {}, () => {
        nestedTraceId = getCurrentTraceId();
      });
    });

    expect(outerTraceId).toBe("4bf92f3577b34da6a3ce929d0e0e4736");
    // Sync nesting reuses outer's root regardless of inner name.
    expect(nestedTraceId).toBe(outerTraceId);
  });
});

describe("run — boot phase server traceparent inheritance (Codex round 3 finding 1)", () => {
  // boot.ts wires every DomReady behavior as its own top-level `run()`
  // call. When multiple page-ready behaviors fire during the initial
  // boot burst, they must all share the server-issued trace so the full
  // page-load startup correlates to a single distributed trace.
  // Regression: an earlier one-shot-consume rule cleared
  // configuredFromTraceparent on the first non-nested run, which broke
  // correlation for every page-ready beyond the first.

  const SERVER_TRACE_ID = "4bf92f3577b34da6a3ce929d0e0e4736";
  const SERVER_TRACEPARENT = `00-${SERVER_TRACE_ID}-00f067aa0ba902b7-01`;

  it("multiple page-ready behaviors in the same boot burst share the server trace-id", () => {
    setRootFromTraceparent(SERVER_TRACEPARENT);

    const ids: Array<string | undefined> = [];
    // Simulate boot.ts wireBehaviors calling runInteraction once per page-ready.
    run("page-ready", { planId: "plan-1" }, () => {
      ids.push(getCurrentTraceId());
    });
    run("page-ready", { planId: "plan-1" }, () => {
      ids.push(getCurrentTraceId());
    });
    run("page-ready", { planId: "plan-2" }, () => {
      ids.push(getCurrentTraceId());
    });

    expect(ids).toEqual([SERVER_TRACE_ID, SERVER_TRACE_ID, SERVER_TRACE_ID]);
  });

  it("the first non-page-ready top-level run ends the boot phase and clears the configured root", () => {
    setRootFromTraceparent(SERVER_TRACEPARENT);

    let firstPageReady: string | undefined;
    let firstClick: string | undefined;
    let secondPageReady: string | undefined;
    let secondClick: string | undefined;

    run("page-ready", {}, () => {
      firstPageReady = getCurrentTraceId();
    });
    run("document-event", {}, () => {
      firstClick = getCurrentTraceId();
    });
    run("page-ready", {}, () => {
      secondPageReady = getCurrentTraceId();
    });
    run("document-event", {}, () => {
      secondClick = getCurrentTraceId();
    });

    // Page-ready during the boot burst inherits the server trace.
    expect(firstPageReady).toBe(SERVER_TRACE_ID);
    // First click ends the boot phase: fresh root, not the server trace.
    expect(firstClick).toBeDefined();
    expect(firstClick).not.toBe(SERVER_TRACE_ID);
    // After boot ends, configuredFromTraceparent is cleared. A later
    // page-ready (e.g. from a mergePlan partial) does NOT inherit a
    // stale page-load trace.
    expect(secondPageReady).toBeDefined();
    expect(secondPageReady).not.toBe(SERVER_TRACE_ID);
    expect(secondPageReady).not.toBe(firstClick);
    // A later click also gets its own fresh root.
    expect(secondClick).toBeDefined();
    expect(secondClick).not.toBe(firstClick);
    expect(secondClick).not.toBe(secondPageReady);
    expect(secondClick).not.toBe(SERVER_TRACE_ID);
  });

  it("sse/signalr/action-link do NOT inherit the server traceparent even before any page-ready fires", () => {
    setRootFromTraceparent(SERVER_TRACEPARENT);

    let sseId: string | undefined;
    let signalrId: string | undefined;
    let actionLinkId: string | undefined;

    // A page could theoretically receive SSE before its page-ready fires
    // (async race). These are server-initiated events with their own
    // distributed traces — they must NOT collapse into the page-load trace.
    run("server-push", {}, () => {
      sseId = getCurrentTraceId();
    });
    run("signalr", {}, () => {
      signalrId = getCurrentTraceId();
    });
    run("action-link", {}, () => {
      actionLinkId = getCurrentTraceId();
    });

    expect(sseId).not.toBe(SERVER_TRACE_ID);
    expect(signalrId).not.toBe(SERVER_TRACE_ID);
    expect(actionLinkId).not.toBe(SERVER_TRACE_ID);
    expect(new Set([sseId, signalrId, actionLinkId]).size).toBe(3);
  });

  it("inheritance survives across component-event fired synchronously inside a page-ready", () => {
    // A page-ready reaction whose body synchronously dispatches a custom
    // event (simulated by directly calling nested `run`) — the nested
    // run is inside the sync stack, so the depth counter already makes
    // it a sync-nested call that reuses current. This verifies the
    // depth-counter fix and the page-ready inheritance fix compose.
    setRootFromTraceparent(SERVER_TRACEPARENT);

    let pageReadyId: string | undefined;
    let nestedId: string | undefined;

    run("page-ready", {}, () => {
      pageReadyId = getCurrentTraceId();
      run("component-event", {}, () => {
        nestedId = getCurrentTraceId();
      });
    });

    expect(pageReadyId).toBe(SERVER_TRACE_ID);
    expect(nestedId).toBe(SERVER_TRACE_ID);
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

  it("brackets depth so a sync nested run inside reuses the captured root", () => {
    // Regression for Codex adversarial round 2 finding #1:
    //   run() sees depth === 0 after an async step resumes and
    //   runWithRoot is called from executeSequence's continuation.
    //   If runWithRoot doesn't increment depth, any run() fired inside
    //   it (e.g. a dispatch reaction's document-event listener) mints
    //   a fresh root and splits the logical user action into two traces.
    // Fix: runWithRoot brackets depth; any nested run inside sees
    //   depth > 0, isNested=true, and reuses the captured root.
    const captured = { traceId: "c".repeat(32), flags: "01" } as const;
    let nestedTraceId: string | undefined;
    runWithRoot(captured, () => {
      run("nested-after-runWithRoot", {}, () => {
        nestedTraceId = getCurrentTraceId();
      });
    });
    expect(nestedTraceId).toBe(captured.traceId);
  });

  it("sync dispatch chain inside runWithRoot re-entry preserves the originating trace-id", async () => {
    // Simulates the Codex-described path:
    //   run(outer) -> async fn -> await -> runWithRoot(root, sync body)
    //   -> sync body triggers run(nested) (as a DOM dispatch listener would)
    // Expected: nested inherits outer's trace-id under the same logical
    // interaction. Before the depth-bracket fix, nested would mint a
    // fresh root.
    let outerTraceId: string | undefined;
    let nestedTraceId: string | undefined;

    await run("outer", {}, async () => {
      outerTraceId = getCurrentTraceId();
      await Promise.resolve();
      // After the await, executeSequence's continuation would call
      // runWithRoot(root, ...) for each remaining step. Simulate by
      // reconstructing the root from the captured trace-id.
      runWithRoot({ traceId: outerTraceId!, flags: "01" }, () => {
        run("nested-dispatch", {}, () => {
          nestedTraceId = getCurrentTraceId();
        });
      });
    });

    expect(outerTraceId).toBeDefined();
    expect(nestedTraceId).toBe(outerTraceId);
  });

  it("depth is fully restored after runWithRoot exits (no leak)", () => {
    const root1 = { traceId: "d".repeat(32), flags: "01" } as const;
    const root2 = { traceId: "e".repeat(32), flags: "01" } as const;

    // Before: depth=0, no nesting.
    let freshBefore: string | undefined;
    run("before", {}, () => {
      freshBefore = getCurrentTraceId();
    });

    // runWithRoot brackets depth internally...
    runWithRoot(root1, () => {
      // ... but exits cleanly.
    });

    // After: a fresh run should still get a fresh root (depth back to 0,
    // so isNested=false, so we don't leak root1 into unrelated interactions).
    let freshAfter: string | undefined;
    run("after", {}, () => {
      freshAfter = getCurrentTraceId();
    });

    expect(freshBefore).toBeDefined();
    expect(freshAfter).toBeDefined();
    expect(freshAfter).not.toBe(root1.traceId);
    expect(freshBefore).not.toBe(freshAfter);
  });
});

describe("sink failure containment", () => {
  it("a sink that throws does not propagate the exception out of tracer.info", () => {
    const badSink: TraceSink = {
      emit: () => {
        throw new Error("sink down");
      },
      flush: () => {},
    };
    // Silence the fallback console.error so it doesn't pollute the test
    // output. vitest's default jsdom console survives this mocking.
    const original = console.error;
    console.error = () => {};
    try {
      configure({ level: "trace", sink: badSink });
      expect(() => tracer("test").info("event", { ok: true })).not.toThrow();
      expect(() => tracer("test").error("e", {}, new Error("x"))).not.toThrow();
      expect(() => tracer("test").warn("w")).not.toThrow();
      expect(() => tracer("test").debug("d")).not.toThrow();
      expect(() => tracer("test").trace("t")).not.toThrow();
    } finally {
      console.error = original;
    }
  });

  it("sink failure does not abort the caller's work", () => {
    const badSink: TraceSink = {
      emit: () => {
        throw new TypeError("cannot serialize");
      },
      flush: () => {},
    };
    const original = console.error;
    console.error = () => {};
    try {
      configure({ level: "trace", sink: badSink });
      let reachedAfterEmit = false;
      run("test", {}, () => {
        tracer("test").info("event");
        reachedAfterEmit = true;
      });
      expect(reachedAfterEmit).toBe(true);
    } finally {
      console.error = original;
    }
  });
});
