import { afterEach, describe, expect, test, vi } from "vitest";
import { attempt, prepare, routeFor } from "./index";

type Send = Parameters<typeof prepare>[0];
type Routes = Parameters<typeof attempt>[1];

const get: Send = {
  method: "GET",
  url: { parts: [{ kind: "text", text: "/Residents" }], query: "" },
  query: { fields: [], inputs: { container: { kind: "none" }, individual: [] } },
  body: { kind: "none" },
  headers: [{ name: "Accept", value: { kind: "held", value: "application/json" }, form: { kind: "text" }, when: true }],
  antiforgery: { kind: "none" },
  retry: { kind: "none" },
  busy: { kind: "none" },
  respondedJson: false,
  contracts: [],
};
const routes = (kind: Routes["success"]["kind"] = "json"): Routes => ({
  success: { kind, do: [{ kind: "stop", reason: "success" }] },
  statuses: [{ status: 409, kind: "json", do: [{ kind: "stop", reason: "conflict" }] }],
  otherwise: { kind: "json", do: [{ kind: "stop", reason: "otherwise" }] },
});
const pageUrl = "https://care.example/Ward";
const untraced: Parameters<typeof attempt>[4] = {
  attempt: () => ({ headers: {}, answered: () => undefined, end: () => undefined }),
  end: () => undefined,
};
const signal = (): AbortSignal => new AbortController().signal;
const prepared = (send: Send = get) => prepare(send, { kind: "clock" }, { name: "en-US", collation: "en-US", decimal: ".", group: ",", integer: { group: false, exponent: false }, fraction: { group: true, exponent: true } });
const fetches = (response: Response) => {
  const fetch = vi.fn<typeof globalThis.fetch>().mockResolvedValue(response);
  vi.stubGlobal("fetch", fetch);
  return fetch;
};

afterEach(() => {
  vi.unstubAllGlobals();
  vi.restoreAllMocks();
});

describe("when one Attempt reaches fetch, D10 chooses and decodes its answer", () => {
  test.each<Send["method"]>(["GET", "HEAD", "OPTIONS"])("SafeMethod: sends %s once with the supplied headers and abort signal and follows redirects", async method => {
    const fetch = fetches(new Response(null, { status: 204, headers: { Allow: "GET, HEAD, OPTIONS" } }));
    const abort = signal();
    const { reply } = await attempt(prepared({ ...get, method }), routes("none"), abort, pageUrl, untraced);
    expect(fetch.mock.calls.map(([url, init]) => [url, init?.method, init?.redirect, init?.body])).toStrictEqual([
      ["/Residents", method, "follow", null],
    ]);
    expect(fetch.mock.calls[0]?.[1]?.signal).toBe(abort);
    expect(fetch.mock.calls[0]?.[1]?.headers).toStrictEqual(new Headers({ Accept: "application/json" }));
    expect([reply.status, reply.outcome, reply.body, reply.headers.get("allow")]).toStrictEqual([204, "ok", { kind: "none" }, "GET, HEAD, OPTIONS"]);
  });

  test("JsonUsesTheDeclaredKind: a structured suffix is JSON decoded as UTF-8 regardless of its charset", async () => {
    fetches(new Response('{"name":"café","beds":[2,4]}', { status: 201, headers: { "Content-Type": "Application/ward+json; charset=windows-1252" } }));
    const { reply } = await attempt(prepared(), routes(), signal(), pageUrl, untraced);
    expect([reply.status, reply.outcome, reply.body]).toStrictEqual([201, "ok", { kind: "json", value: { name: "café", beds: [2, 4] } }]);
    expect(routeFor(reply, routes()).do).toStrictEqual([{ kind: "stop", reason: "success" }]);
  });

  test("EmptyJsonReadsNothing: an empty JSON body remains a successful response", async () => {
    fetches(new Response("", { headers: { "Content-Type": "application/json" } }));
    const { reply } = await attempt(prepared(), routes(), signal(), pageUrl, untraced);
    expect([reply.outcome, reply.body]).toStrictEqual(["ok", { kind: "none" }]);
  });

  test.each([
    ["text", "text/plain; charset=windows-1252", "text"],
    ["html", 'TEXT/HTML; CHARSET="windows-1252"', "html"],
    ["html", "application/xhtml+xml; charset=windows-1252", "html"],
  ] as const)("CharsetDecodesTheBody: %s decodes %s with TextDecoder", async (kind, contentType, bodyKind) => {
    fetches(new Response(new Uint8Array([99, 97, 102, 233]), { headers: { "Content-Type": contentType } }));
    const { reply } = await attempt(prepared(), routes(kind), signal(), pageUrl, untraced);
    expect([reply.outcome, reply.body]).toStrictEqual(["ok", { kind: bodyKind, value: "café" }]);
  });

  test("TextDefaultsToUtf8: text with no charset is decoded as UTF-8", async () => {
    fetches(new Response("Zoë", { headers: { "Content-Type": "text/plain" } }));
    const { reply } = await attempt(prepared(), routes("text"), signal(), pageUrl, untraced);
    expect(reply.body).toStrictEqual({ kind: "text", value: "Zoë" });
  });

  test.each([
    ["json", "application/json", "{"],
    ["json", "text/html", "<p>Login</p>"],
    ["html", "text/plain", "<p>Login</p>"],
    ["text", "application/json", "{}"],
    ["text", "text/plain; charset=not-an-encoding", "Ada"],
  ] as const)("ContentFailure: a success declared as %s refuses %s with its body %s", async (kind, type, body) => {
    fetches(new Response(body, { headers: { "Content-Type": type, ETag: '"v7"' } }));
    const { reply } = await attempt(prepared(), routes(kind), signal(), pageUrl, untraced);
    expect([reply.status, reply.outcome, reply.body, reply.headers.get("etag")]).toStrictEqual([200, "content", { kind: "none" }, '"v7"']);
    expect(routeFor(reply, routes()).do).toStrictEqual([{ kind: "stop", reason: "otherwise" }]);
  });

  test("OneRouteRuns: a status route wins over the typed catch-all and uses its own declared kind", async () => {
    const own: Routes = { ...routes("html"), statuses: [{ status: 409, kind: "text", do: [{ kind: "stop", reason: "conflict text" }] }] };
    fetches(new Response("Bed taken", { status: 409, headers: { "Content-Type": "text/plain" } }));
    const { reply } = await attempt(prepared(), own, signal(), pageUrl, untraced);
    expect([reply.status, reply.outcome, reply.body]).toStrictEqual([409, "status", { kind: "text", value: "Bed taken" }]);
    expect(routeFor(reply, own).do).toStrictEqual([{ kind: "stop", reason: "conflict text" }]);
  });

  test("TypedCatchAll: a status without its own route reads the catch-all's JSON", async () => {
    fetches(new Response('{"message":"Beds unavailable"}', { status: 503, headers: { "Content-Type": "application/problem+json" } }));
    const { reply } = await attempt(prepared(), routes(), signal(), pageUrl, untraced);
    expect([reply.status, reply.outcome, reply.body]).toStrictEqual([503, "status", { kind: "json", value: { message: "Beds unavailable" } }]);
    expect(routeFor(reply, routes()).do).toStrictEqual([{ kind: "stop", reason: "otherwise" }]);
  });

  test.each(["{", "<p>Bed taken</p>"])("ErrorContentReadsNothing: status 409 keeps its route when its body is %s", async body => {
    fetches(new Response(body, { status: 409, headers: { "Content-Type": body === "{" ? "application/json" : "text/html" } }));
    const { reply } = await attempt(prepared(), routes(), signal(), pageUrl, untraced);
    expect([reply.status, reply.outcome, reply.body]).toStrictEqual([409, "status", { kind: "none" }]);
    expect(routeFor(reply, routes()).do).toStrictEqual([{ kind: "stop", reason: "conflict" }]);
  });

  test("NoneDoesNotRead: a none route leaves the body unread even when it is not JSON", async () => {
    const response = new Response("<p>Login</p>", { headers: { "Content-Type": "text/html" } });
    const read = vi.spyOn(response, "arrayBuffer");
    fetches(response);
    const { reply } = await attempt(prepared(), routes("none"), signal(), pageUrl, untraced);
    expect([reply.outcome, reply.body, read.mock.calls.length, response.bodyUsed]).toStrictEqual(["ok", { kind: "none" }, 0, false]);
  });

  test("NetworkFailure: a rejected fetch becomes status zero with empty headers and body", async () => {
    vi.stubGlobal("fetch", vi.fn<typeof globalThis.fetch>().mockRejectedValue(new TypeError("connection lost")));
    const { reply } = await attempt(prepared(), routes(), signal(), pageUrl, untraced);
    expect([reply.status, reply.outcome, [...reply.headers], reply.body]).toStrictEqual([0, "network", [], { kind: "none" }]);
    expect(routeFor(reply, routes()).do).toStrictEqual([{ kind: "stop", reason: "otherwise" }]);
  });

  test("BrokenBodyIsNetwork: a body that breaks while read loses status and headers too", async () => {
    const response = new Response("{}", { headers: { "Content-Type": "application/json", ETag: '"v7"' } });
    vi.spyOn(response, "arrayBuffer").mockRejectedValue(new TypeError("body interrupted"));
    fetches(response);
    const { reply } = await attempt(prepared(), routes(), signal(), pageUrl, untraced);
    expect([reply.status, reply.outcome, [...reply.headers], reply.body]).toStrictEqual([0, "network", [], { kind: "none" }]);
  });

  test.each([
    ["https://care.example/Answer", "ok", { kind: "json", value: { beds: 4 } }],
    ["https://login.example/Answer", "redirect", { kind: "none" }],
  ] as const)("RedirectOrigin: a followed redirect ending at %s yields %s", async (url, outcome, body) => {
    const response = new Response('{"beds":4}', { headers: { "Content-Type": "application/json", "X-Ward": "East" } });
    Object.defineProperties(response, { redirected: { value: true }, url: { value: url } });
    fetches(response);
    const { reply } = await attempt(prepared(), routes(), signal(), pageUrl, untraced);
    expect([reply.status, reply.outcome, reply.body, reply.headers.get("x-ward")]).toStrictEqual([200, outcome, body, "East"]);
  });

  test("DirectOtherOrigin: a direct CORS response is not a redirect", async () => {
    const response = new Response("{}", { headers: { "Content-Type": "application/json" } });
    Object.defineProperty(response, "url", { value: "https://data.example/Residents" });
    fetches(response);
    const { reply } = await attempt(prepared({ ...get, url: { parts: [{ kind: "text", text: "https://data.example/Residents" }], query: "" } }), routes(), signal(), pageUrl, untraced);
    expect(reply.outcome).toBe("ok");
  });

  test("RespondedJson: Katana's JSON status replaces 200 only when the plan says so", async () => {
    const headers = { "Content-Type": "application/json", "X-Responded-JSON": '{"status":401,"headers":{"location":"/Login"}}' };
    fetches(new Response('{"message":"Expired"}', { headers }));
    const { reply } = await attempt(prepared({ ...get, respondedJson: true }), routes(), signal(), pageUrl, untraced);
    expect([reply.status, reply.outcome, reply.body]).toStrictEqual([401, "status", { kind: "json", value: { message: "Expired" } }]);
    fetches(new Response("{}", { headers }));
    const { reply: ordinary } = await attempt(prepared(), routes(), signal(), pageUrl, untraced);
    expect([ordinary.status, ordinary.outcome]).toStrictEqual([200, "ok"]);
  });

  test("HeadersAreTheBrowsers: preserves case-insensitive lookup, repeated values and missing headers", async () => {
    fetches(new Response("{}", { headers: [["Content-Type", "application/json"], ["ETag", '"v7"'], ["X-Ward", "East"], ["X-Ward", "West"]] }));
    const { reply } = await attempt(prepared(), routes(), signal(), pageUrl, untraced);
    expect([reply.headers.get("etag"), reply.headers.get("X-WARD"), reply.headers.get("missing")]).toStrictEqual(['"v7"', "East, West", null]);
  });

  test("SentInputsKeepTheirEntries: the response holds the captured input under its sent binding name", async () => {
    const entry = { id: "room", binding: { name: "Room", path: ["room"], converted: { kind: "as", expr: { kind: "lit", value: 7 }, shape: { kind: "nullable", inner: { kind: "number", of: "integer", text: "invariant", overflow: "nothing" } } }, blank: { kind: "lit", value: null }, sent: { kind: "lit", value: { kind: "field", form: { kind: "text" }, whenNothing: "empty", whenBlank: "send" } }, committed: "change", invalidClass: "invalid", message: "room-message" } } as const;
    const sent = { kind: "field", form: { kind: "text" }, whenNothing: "empty", whenBlank: "send" } as const;
    const send: Send = { ...get, query: { fields: [], inputs: { container: { kind: "captured", id: "admission", body: { members: {}, hidden: [] }, inputs: [{ name: "Room", path: ["room"], entry, sent }] }, individual: [{ kind: "captured", input: { name: "OtherRoom", path: ["otherRoom"], entry, sent } }] } } };
    fetches(new Response(null, { status: 204 }));
    const { reply } = await attempt(prepared(send), routes("none"), signal(), pageUrl, untraced);
    expect(reply.inputs.map(input => [input.name, input.entry.id])).toStrictEqual([["Room", "room"], ["OtherRoom", "room"]]);
    expect(reply.inputs[0]?.entry).toBe(entry);
    expect(reply.inputs[1]?.entry).toBe(entry);
  });

  test("FileNamedByItsExtendedName: filename* names the file before filename, and the body gives its size and type", async () => {
    fetches(new Response("Ada Hart", { headers: { "Content-Type": "application/pdf", "Content-Disposition": "attachment; filename=\"Resume.pdf\"; filename*=UTF-8''R%C3%A9sum%C3%A9.pdf" } }));
    const { reply } = await attempt(prepared(), routes("file"), signal(), pageUrl, untraced);
    expect([reply.outcome, reply.body]).toStrictEqual(["ok", { kind: "file", value: { name: "Résumé.pdf", size: 8, type: "application/pdf" } }]);
  });

  test("FileNamedByItsFilename: a quoted filename names the file when no filename* does", async () => {
    fetches(new Response("Care plan", { headers: { "Content-Type": "application/pdf", "Content-Disposition": "attachment; filename=\"Plan.pdf\"" } }));
    const { reply } = await attempt(prepared(), routes("file"), signal(), pageUrl, untraced);
    expect(reply.body).toStrictEqual({ kind: "file", value: { name: "Plan.pdf", size: 9, type: "application/pdf" } });
  });

  test("FileNamedByItsPath: a response that names no file is named by the last segment of the request's path", async () => {
    fetches(new Response("Name,Ward\n", { headers: { "Content-Type": "text/csv" } }));
    const { reply } = await attempt(prepared({ ...get, url: { parts: [{ kind: "text", text: "/Residents/Exports/census%20North.csv" }], query: "" } }), routes("file"), signal(), pageUrl, untraced);
    expect(reply.body).toStrictEqual({ kind: "file", value: { name: "census North.csv", size: 10, type: "text/csv" } });
  });

  test("FileWithNoName: a response that names no file from a path that ends with no segment stops the request", async () => {
    fetches(new Response("bytes", { headers: { "Content-Type": "text/csv" } }));
    await expect(attempt(prepared({ ...get, url: { parts: [{ kind: "text", text: "/Residents/" }], query: "" } }), routes("file"), signal(), pageUrl, untraced)).rejects.toThrow(
      "The file that '/Residents/' answered has no name: its Content-Disposition names none, and its path ends with no segment.",
    );
  });
});

describe("when response metadata comes from the server boundary", () => {
  test.each(["not JSON", '{}', '{"status":"401"}', '{"status":700}'])("UnstatedRespondedJson: %s keeps the response's own status", async header => {
    fetches(new Response("{}", { status: 409, headers: { "Content-Type": "application/json", "X-Responded-JSON": header } }));
    const { reply } = await attempt(prepared({ ...get, respondedJson: true }), routes(), signal(), pageUrl, untraced);
    expect([reply.status, reply.outcome]).toStrictEqual([409, "status"]);
  });

  test("AbsentRespondedJson: net48 keeps the HTTP status when Katana writes no override", async () => {
    fetches(new Response("{}", { status: 409, headers: { "Content-Type": "application/json" } }));
    const { reply } = await attempt(prepared({ ...get, respondedJson: true }), routes(), signal(), pageUrl, untraced);
    expect([reply.status, reply.outcome]).toStrictEqual([409, "status"]);
  });

  test.each([
    [299, "ok", "success"],
    [300, "status", "otherwise"],
    [599, "status", "otherwise"],
  ] as const)("HttpStatusRange: %s yields %s", async (status, outcome, reason) => {
    fetches(new Response(null, { status }));
    const allNone: Routes = { success: { kind: "none", do: [{ kind: "stop", reason: "success" }] }, statuses: [], otherwise: { kind: "none", do: [{ kind: "stop", reason: "otherwise" }] } };
    const { reply } = await attempt(prepared(), allNone, signal(), pageUrl, untraced);
    expect(reply.outcome).toBe(outcome);
    expect(routeFor(reply, allNone).do).toStrictEqual([{ kind: "stop", reason }]);
  });
});
