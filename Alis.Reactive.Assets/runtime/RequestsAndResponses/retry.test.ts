import { afterEach, beforeEach, describe, expect, test, vi } from "vitest";
import { attempt, captureBusy, prepare } from "./index";

const untraced: Parameters<typeof attempt>[4] = {
  attempt: () => ({ headers: {}, answered: () => undefined, end: () => undefined }),
  end: () => undefined,
};
type Send = Parameters<typeof prepare>[0];
const culture = { name: "en-US", collation: "en-US", decimal: ".", group: ",", integer: { group: false, exponent: false }, fraction: { group: true, exponent: true } };
const send: Send = {
  method: "GET", url: { parts: [{ kind: "text", text: "/Residents" }], query: "" },
  query: { fields: [], inputs: { container: { kind: "none" }, individual: [] } }, body: { kind: "none" }, headers: [],
  antiforgery: { kind: "none" }, retry: { kind: "standard", attempts: 3, firstWaitMilliseconds: 2000 },
  busy: { kind: "none" }, contracts: [], respondedJson: false,
};
const routes = { success: { kind: "json", do: [] }, statuses: [], otherwise: { kind: "json", do: [] } } as const;
const response = (status: number, headers: Readonly<Record<string, string>> = {}) => new Response("{}", { status, headers: { "Content-Type": "application/json", ...headers } });
const prepared = (changes: Partial<Send> = {}) => prepare({ ...send, ...changes }, { kind: "clock" }, culture);
const run = (request = prepared(), signal = new AbortController().signal) => attempt(request, routes, signal, "https://care.example/", untraced).then(answer => answer.reply);
const fetches = () => {
  const fetch = vi.fn<typeof globalThis.fetch>();
  vi.stubGlobal("fetch", fetch);
  return fetch;
};

beforeEach(() => {
  vi.useFakeTimers();
  vi.spyOn(Math, "random").mockReturnValue(0.5);
});
afterEach(() => { vi.useRealTimers(); vi.unstubAllGlobals(); vi.restoreAllMocks(); });

const tracing = (calls: string[]): Parameters<typeof attempt>[4] => ({
  attempt: (toThePagesOrigin, method, template) => {
    const attempt = calls.filter(call => call.startsWith("attempt")).length + 1;
    calls.push("attempt " + String(attempt) + " " + (toThePagesOrigin ? "home " : "away ") + method + " " + template);
    return {
      headers: { traceparent: "span of attempt " + String(attempt) },
      answered: status => { calls.push("answered " + String(status)); },
      end: outcome => { calls.push("attempt ended " + outcome); },
    };
  },
  end: outcome => { calls.push("send " + outcome); },
});
const traceparents = (fetch: ReturnType<typeof fetches>) => fetch.mock.calls.map(call => new Headers(call[1]?.headers).get("traceparent"));

describe("when a send is traced, Tracing.tla's Attempt and Respond order its spans", () => {
  test("TracedAttempts: each attempt opens its span by method and template and sends its own headers, records its status as answered, and the send ends after its last", async () => {
    const calls: string[] = [];
    const fetch = fetches().mockResolvedValueOnce(response(503)).mockResolvedValueOnce(response(200));
    const pending = attempt(prepared(), routes, new AbortController().signal, "https://care.example/", tracing(calls));
    await vi.advanceTimersByTimeAsync(2000);
    expect((await pending).reply.status).toBe(200);
    expect(calls).toStrictEqual(["attempt 1 home GET /Residents", "answered 503", "attempt 2 home GET /Residents", "answered 200", "send completed"]);
    expect(traceparents(fetch)).toStrictEqual(["span of attempt 1", "span of attempt 2"]);
  });

  test("AwayAndUnanswered: an attempt to another origin is told so, and one the network fails ends with no status", async () => {
    const calls: string[] = [];
    fetches().mockRejectedValueOnce(new TypeError("Failed to fetch"));
    const away = prepared({ url: { parts: [{ kind: "text", text: "https://pharmacy.example/Stock" }], query: "" }, retry: { kind: "none" } });
    const reply = await attempt(away, routes, new AbortController().signal, "https://care.example/", tracing(calls)).then(answer => answer.reply);
    expect([reply.status, reply.outcome]).toStrictEqual([0, "network"]);
    expect(calls).toStrictEqual(["attempt 1 away GET /Stock", "attempt ended completed", "send completed"]);
  });

  test("CutInFlight: an attempt the page gives up records no answer and leaves its send to the run's end", async () => {
    const calls: string[] = [];
    fetches().mockImplementation((_, init) => new Promise((_resolve, reject) => {
      init?.signal?.addEventListener("abort", () => { reject(new DOMException("The operation was aborted.", "AbortError")); });
    }));
    const cut = new AbortController();
    const pending = attempt(prepared(), routes, cut.signal, "https://care.example/", tracing(calls));
    cut.abort();
    await expect(pending).rejects.toThrow("This operation was aborted");
    expect(calls).toStrictEqual(["attempt 1 home GET /Residents"]);
  });
});

describe("when a standard policy runs, D10.14 and Request.tla bound every attempt", () => {
  test("EveryStandardAttempt: exponential waits precede sends two, three and four, then the last failure is returned", async () => {
    const times: number[] = [];
    const fetch = fetches().mockImplementation(() => { times.push(performance.now()); return Promise.resolve(response(503)); });
    const pending = run();
    await vi.advanceTimersByTimeAsync(1999);
    expect(times).toStrictEqual([0]);
    await vi.advanceTimersByTimeAsync(1);
    expect(times).toStrictEqual([0, 2000]);
    await vi.advanceTimersByTimeAsync(3999);
    expect(times).toStrictEqual([0, 2000]);
    await vi.advanceTimersByTimeAsync(1);
    expect(times).toStrictEqual([0, 2000, 6000]);
    await vi.advanceTimersByTimeAsync(7999);
    expect(times).toStrictEqual([0, 2000, 6000]);
    await vi.advanceTimersByTimeAsync(1);
    const reply = await pending;
    expect([times, fetch.mock.calls.length, reply.status, reply.outcome, vi.getTimerCount()]).toStrictEqual([[0, 2000, 6000, 14000], 4, 503, "status", 0]);
  });

  test("AttemptLimitIsIndependentOfBudget: one send after the first stops with time remaining", async () => {
    const fetch = fetches().mockImplementation(() => Promise.resolve(response(503)));
    const pending = run(prepared({ retry: { kind: "standard", attempts: 1, firstWaitMilliseconds: 50 } }));
    await vi.advanceTimersByTimeAsync(50);
    expect([fetch.mock.calls.length, vi.getTimerCount()]).toStrictEqual([2, 0]);
    expect((await pending).status).toBe(503);
  });

  test.each([408, 429, 502, 503, 504])("RetryableStatus: %i retries and returns only the successful last answer", async status => {
    const fetch = fetches().mockResolvedValueOnce(response(status)).mockResolvedValueOnce(response(200));
    const pending = run();
    await vi.advanceTimersByTimeAsync(2000);
    const reply = await pending;
    expect([fetch.mock.calls.length, reply.status, reply.outcome, reply.body]).toStrictEqual([2, 200, "ok", { kind: "json", value: {} }]);
  });

  test.each([400, 401, 403, 404, 409, 422, 500, 501, 505])("OtherStatusNeverRetries: %i returns after the first send", async status => {
    const fetch = fetches().mockResolvedValue(response(status));
    const reply = await run();
    expect([fetch.mock.calls.length, reply.outcome, vi.getTimerCount()]).toStrictEqual([1, "status", 0]);
  });

  test.each<Send["method"]>(["GET", "HEAD", "OPTIONS", "PUT", "DELETE"])("IdempotentMethodRetries: %s retries a network failure", async method => {
    const fetch = fetches().mockRejectedValueOnce(new TypeError("offline")).mockResolvedValueOnce(response(200));
    const pending = run(prepared({ method }));
    await vi.advanceTimersByTimeAsync(2000);
    const reply = await pending;
    expect([fetch.mock.calls.length, reply.status, reply.outcome]).toStrictEqual([2, 200, "ok"]);
  });

  test("PostNeverRetries: a failed POST may already have changed the server", async () => {
    const fetch = fetches().mockResolvedValue(response(503));
    const pending = run(prepared({ method: "POST" }));
    await vi.advanceTimersByTimeAsync(0);
    expect(vi.getTimerCount()).toBe(0);
    const reply = await pending;
    expect([fetch.mock.calls.length, reply.status]).toStrictEqual([1, 503]);
  });

  test.each<Send["retry"]>([{ kind: "none" }, { kind: "standard", attempts: 0, firstWaitMilliseconds: 2000 }])("NoRetryOrNoSendAfterTheFirst: $kind returns the first network failure", async retry => {
    const fetch = fetches().mockRejectedValue(new TypeError("offline"));
    const reply = await run(prepared({ retry }));
    expect([fetch.mock.calls.length, reply.status, reply.outcome, vi.getTimerCount()]).toStrictEqual([1, 0, "network", 0]);
  });

  test("BrokenBodyRetries: interrupted response bytes are a network failure", async () => {
    const broken = response(200);
    vi.spyOn(broken, "arrayBuffer").mockRejectedValue(new TypeError("interrupted"));
    const fetch = fetches().mockResolvedValueOnce(broken).mockResolvedValueOnce(response(200));
    const pending = run();
    await vi.advanceTimersByTimeAsync(2000);
    expect([(await pending).outcome, fetch.mock.calls.length]).toStrictEqual(["ok", 2]);
  });

  test("ContentAndRedirectNeverRetry: a bad JSON body and an off-origin redirect are final", async () => {
    const fetch = fetches().mockResolvedValueOnce(new Response("{", { headers: { "Content-Type": "application/json" } }));
    const content = await run();
    const redirected = response(503);
    Object.defineProperties(redirected, { redirected: { value: true }, url: { value: "https://login.example/" } });
    fetch.mockResolvedValueOnce(redirected);
    const redirect = await run();
    expect([content.outcome, redirect.outcome, fetch.mock.calls.length, vi.getTimerCount()]).toStrictEqual(["content", "redirect", 2, 0]);
  });

  test("SameArgumentAndHoldAcrossAttempts: a PUT keeps its body, headers and busy until release after the response", async () => {
    const busy = { show: vi.fn(), hide: vi.fn() };
    const request: Send = { ...send, method: "PUT", body: { kind: "json", value: { name: "Ada" }, guarded: [], enums: [], inputs: { container: { kind: "none" }, individual: [] } }, busy: { kind: "page" }, contracts: ["busy"] };
    const held = prepare(request, { kind: "clock" }, culture, captureBusy(request.busy), () => busy);
    const fetch = fetches().mockResolvedValueOnce(response(503)).mockResolvedValueOnce(response(200));
    const pending = run(held);
    await vi.advanceTimersByTimeAsync(1000);
    expect([busy.show.mock.calls, busy.hide.mock.calls, fetch.mock.calls.length]).toStrictEqual([[[]], [], 1]);
    await vi.advanceTimersByTimeAsync(1000);
    expect((await pending).outcome).toBe("ok");
    expect(fetch.mock.calls.map(([url, init]) => [url, init?.method, init?.body])).toStrictEqual([["/Residents", "PUT", '{"name":"Ada"}'], ["/Residents", "PUT", '{"name":"Ada"}']]);
    expect(fetch.mock.calls[0]?.[1]?.headers).toBe(fetch.mock.calls[1]?.[1]?.headers);
    expect([busy.show.mock.calls, busy.hide.mock.calls]).toStrictEqual([[[]], []]);
    held.releaseBusy();
    expect(busy.hide.mock.calls).toStrictEqual([[]]);
  });

  test.each([[0, 1000], [0.5, 2000], [0.999, 2998]])("JitterIsApplied: random %s yields a first wait of %s ms", async (random, wait) => {
    vi.mocked(Math.random).mockReturnValue(random);
    const fetch = fetches().mockResolvedValueOnce(response(503)).mockResolvedValueOnce(response(200));
    const pending = run();
    await vi.advanceTimersByTimeAsync(wait - 1);
    expect(fetch.mock.calls.length).toBe(1);
    await vi.advanceTimersByTimeAsync(1);
    expect([(await pending).status, fetch.mock.calls.length]).toStrictEqual([200, 2]);
  });
});

describe("when Retry-After and the elapsed budget constrain a standard policy", () => {
  test.each([429, 503])("RetryAfterSeconds: %i waits for the server's seconds instead of jitter", async status => {
    const fetch = fetches().mockResolvedValueOnce(response(status, { "Retry-After": "3" })).mockResolvedValueOnce(response(200));
    const pending = run();
    await vi.advanceTimersByTimeAsync(2999);
    expect(fetch.mock.calls.length).toBe(1);
    await vi.advanceTimersByTimeAsync(1);
    expect([(await pending).status, fetch.mock.calls.length]).toStrictEqual([200, 2]);
  });

  test("RetryAfterDateUsesResponseDate: a wildly different local clock does not change the wait", async () => {
    vi.setSystemTime(new Date("2040-01-01T00:00:00Z"));
    const fetch = fetches().mockResolvedValueOnce(response(503, { Date: "Sun, 15 Nov 2026 12:00:00 GMT", "Retry-After": "Sun, 15 Nov 2026 12:00:03 GMT" })).mockResolvedValueOnce(response(200));
    const pending = run();
    await vi.advanceTimersByTimeAsync(2999);
    expect(fetch.mock.calls.length).toBe(1);
    await vi.advanceTimersByTimeAsync(1);
    expect([(await pending).status, fetch.mock.calls.length]).toStrictEqual([200, 2]);
  });

  test.each([
    "Sun, 15 Nov 2026 12:00:03 GMT",
    "Sunday, 15-Nov-26 12:00:03 GMT",
    "Sun Nov 15 12:00:03 2026",
  ])("HttpDateFormsUseGmt: %s is relative to the response Date", async header => {
    const fetch = fetches().mockResolvedValueOnce(response(503, { Date: "Sun, 15 Nov 2026 12:00:00 GMT", "Retry-After": header })).mockResolvedValueOnce(response(200));
    const pending = run();
    await vi.advanceTimersByTimeAsync(2999);
    expect(fetch.mock.calls.length).toBe(1);
    await vi.advanceTimersByTimeAsync(1);
    expect([(await pending).status, fetch.mock.calls.length]).toStrictEqual([200, 2]);
  });

  test("OtherStatusesIgnoreRetryAfter: a gateway 502 keeps the standard wait", async () => {
    const fetch = fetches().mockResolvedValueOnce(response(502, { "Retry-After": "29" })).mockResolvedValueOnce(response(200));
    const pending = run();
    await vi.advanceTimersByTimeAsync(2000);
    expect([(await pending).status, fetch.mock.calls.length]).toStrictEqual([200, 2]);
  });

  test.each(["later", "1.5", "-1", "Infinity", "2026-11-15T12:00:03Z"])("InvalidRetryAfterLogsOnce: %s falls back on each attempt's standard wait", async header => {
    const logged = vi.spyOn(console, "error").mockImplementation(() => undefined);
    const fetch = fetches().mockResolvedValueOnce(response(503, { "Retry-After": header })).mockResolvedValueOnce(response(429, { "Retry-After": header })).mockResolvedValueOnce(response(200));
    const pending = run();
    await vi.advanceTimersByTimeAsync(6000);
    expect([(await pending).status, fetch.mock.calls.length, logged.mock.calls]).toStrictEqual([200, 3, [["The response's Retry-After header is not seconds or an HTTP date relative to its Date header."]]]);
  });

  test("MissingResponseDateUsesStandardWait: an HTTP date with no server Date logs once", async () => {
    const logged = vi.spyOn(console, "error").mockImplementation(() => undefined);
    const fetch = fetches().mockResolvedValueOnce(response(503, { "Retry-After": "Sun, 15 Nov 2026 12:00:03 GMT" })).mockResolvedValueOnce(response(200));
    const pending = run();
    await vi.advanceTimersByTimeAsync(2000);
    expect([(await pending).status, fetch.mock.calls.length, logged.mock.calls.length]).toStrictEqual([200, 2, 1]);
  });

  test("BudgetIncludesTheAttempt: an answer after 29 seconds cannot start a two-second retry wait", async () => {
    const fetch = fetches().mockImplementation(() => new Promise(resolve => { setTimeout(() => { resolve(response(503)); }, 29000); }));
    const pending = run();
    await vi.advanceTimersByTimeAsync(29000);
    expect([fetch.mock.calls.length, vi.getTimerCount()]).toStrictEqual([1, 0]);
    expect((await pending).status).toBe(503);
  });

  test("RetryAfterPastBudgetEndsAtOnce: the last failure routes without waiting", async () => {
    const fetch = fetches().mockResolvedValue(response(503, { "Retry-After": "31" }));
    const reply = await run();
    expect([reply.status, fetch.mock.calls.length, vi.getTimerCount()]).toStrictEqual([503, 1, 0]);
  });

  test("LateTimerStartsNoAttempt: a throttled timer that wakes beyond the budget returns the last failure", async () => {
    const fetch = fetches().mockResolvedValueOnce(response(503)).mockResolvedValueOnce(response(200));
    const pending = run();
    await vi.advanceTimersByTimeAsync(1000);
    vi.spyOn(performance, "now").mockReturnValue(31000);
    await vi.advanceTimersByTimeAsync(1000);
    expect([(await pending).status, fetch.mock.calls.length, vi.getTimerCount()]).toStrictEqual([503, 1, 0]);
  });

  test("AbortDuringFetchEndsAttempts: a fetch rejected by disposal cannot become a retry", async () => {
    const controller = new AbortController();
    const fetch = fetches().mockImplementation(() => new Promise((_resolve, reject) => {
      controller.signal.addEventListener("abort", () => { reject(new DOMException("Aborted", "AbortError")); }, { once: true });
    }));
    const pending = run(prepared(), controller.signal);
    const rejected = expect(pending).rejects.toMatchObject({ name: "AbortError" });
    controller.abort();
    await rejected;
    expect([fetch.mock.calls.length, vi.getTimerCount()]).toStrictEqual([1, 0]);
  });

  test("AbortEndsTheWait: disposal cancels its timer and sends no retry", async () => {
    const controller = new AbortController();
    const fetch = fetches().mockResolvedValue(response(503));
    const pending = run(prepared(), controller.signal);
    const rejected = expect(pending).rejects.toMatchObject({ name: "AbortError" });
    await vi.advanceTimersByTimeAsync(1000);
    controller.abort();
    await rejected;
    expect(vi.getTimerCount()).toBe(0);
    await vi.advanceTimersByTimeAsync(10000);
    expect([fetch.mock.calls.length, vi.getTimerCount()]).toStrictEqual([1, 0]);
  });

  test("AlreadyAbortedNeverSends: an ended block cannot start an attempt", async () => {
    const controller = new AbortController();
    controller.abort();
    const fetch = fetches();
    await expect(run(prepared(), controller.signal)).rejects.toMatchObject({ name: "AbortError" });
    expect(fetch.mock.calls).toStrictEqual([]);
  });
});
