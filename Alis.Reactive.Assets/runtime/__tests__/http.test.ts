// alis.http (grammar.md 6.2): the wire rules a request sentence relies on.

import { afterEach, describe, expect, it, vi } from "vitest";
import { send, type HttpRequest } from "../modules/http";
import { shaped } from "../grammar/shape";

interface Captured {
  url: string;
  init: RequestInit;
}

function respondWith(body: string | null, contentType: string | null, status = 200): Captured[] {
  const calls: Captured[] = [];
  vi.stubGlobal("fetch", vi.fn(async (url: string, init: RequestInit) => {
    calls.push({ url, init });
    const headers = new Headers();
    if (contentType) headers.set("Content-Type", contentType);
    return new Response(body, { status, headers });
  }));
  return calls;
}

function request(overrides: Partial<HttpRequest>): HttpRequest {
  return { method: "GET", url: "/residents", route: {}, values: {}, arrays: [], headers: {}, format: "json", ...overrides };
}

afterEach(() => {
  vi.unstubAllGlobals();
});

describe("route", () => {
  it("fills each placeholder, encoded", async () => {
    const calls = respondWith(null, null);
    await send(request({ url: "/residents/{id}/notes", route: { id: "a b" } }));
    expect(calls[0]!.url).toBe("/residents/a%20b/notes");
  });

  it("throws before anything is sent when a route value is missing", async () => {
    const calls = respondWith(null, null);
    await expect(send(request({ url: "/residents/{id}", route: { id: null } }))).rejects.toThrow(/route param "id"/);
    expect(calls).toHaveLength(0);
  });
});

describe("GET", () => {
  it("sends values as the query, an array as repeated keys, a missing scalar empty, a missing array not at all", async () => {
    const calls = respondWith(null, null);
    await send(request({
      url: "/census?facility=3",
      values: { wing: ["east", "west"], name: null, aids: null, admitted: new Date(2026, 0, 2), seen: new Date(2026, 0, 2, 9, 5) },
      arrays: ["aids"],
    }));
    expect(calls[0]!.url).toBe(
      "/census?facility=3&wing=east&wing=west&name=&admitted=2026-01-02&seen=2026-01-02T09%3A05%3A00");
  });
});

describe("JSON", () => {
  it("sends values as the body, with \"\" as null and dates as the local day and time picked", async () => {
    const calls = respondWith(null, null);
    await send(request({
      method: "POST",
      values: { name: "", tags: ["", "a"], due: new Date(2026, 5, 1), dose: new Date(2026, 5, 1, 21, 30, 0, 250), visits: [new Date(2026, 5, 2)] },
    }));
    expect(calls[0]!.init.body).toBe(
      '{"name":null,"tags":["","a"],"due":"2026-06-01","dose":"2026-06-01T21:30:00.250","visits":["2026-06-02"]}');
    expect((calls[0]!.init.headers as Record<string, string>)["Content-Type"]).toBe("application/json");
  });

  it("sends an instant (a DateTimeOffset) as the exact moment, in JSON and as text", async () => {
    const calls = respondWith(null, null);
    const arrives = shaped(new Date(Date.UTC(2026, 9, 11, 5, 30)), { kind: "instant" });
    await send(request({ method: "POST", values: { arrives, visits: [arrives] } }));
    await send(request({ method: "GET", values: { arrives } }));
    expect(calls[0]!.init.body).toBe('{"arrives":"2026-10-11T05:30:00.000Z","visits":["2026-10-11T05:30:00.000Z"]}');
    expect(calls[1]!.url).toContain("arrives=2026-10-11T05%3A30%3A00.000Z");
  });

  it("sends no body for an empty record", async () => {
    const calls = respondWith(null, null);
    await send(request({ method: "POST", values: {} }));
    expect(calls[0]!.init.body).toBeUndefined();
  });

  it("omits a header whose value is missing", async () => {
    const calls = respondWith(null, null);
    await send(request({ method: "POST", headers: { "X-Shift": "Night", "X-Ward": undefined } }));
    expect(calls[0]!.init.headers).toEqual({ "X-Shift": "Night" });
  });
});

describe("form data", () => {
  it("repeats array keys, appends files with their names, and leaves out a missing array", async () => {
    const calls = respondWith(null, null);
    const file = new File(["x"], "care-plan.pdf");
    await send(request({ method: "POST", format: "form-data", values: { wing: ["east", "west"], upload: [file], aids: undefined, note: null }, arrays: ["aids"] }));
    const body = calls[0]!.init.body as FormData;
    expect(body.getAll("wing")).toEqual(["east", "west"]);
    expect((body.get("upload") as File).name).toBe("care-plan.pdf");
    expect(body.has("aids")).toBe(false);
    expect(body.get("note")).toBe("");
  });
});

describe("response", () => {
  it("parses JSON, keeps HTML as text, and reads an empty body as missing", async () => {
    respondWith('{"message":"ok"}', "application/json; charset=utf-8");
    expect(await send(request({}))).toEqual({ status: 200, ok: true, body: { message: "ok" } });

    respondWith("<p>partial</p>", "text/html");
    expect((await send(request({}))).body).toBe("<p>partial</p>");

    respondWith("", "application/json", 204);
    expect((await send(request({}))).body).toBeUndefined();
  });

  it("reports status 0 when the network fails or the JSON cannot be read", async () => {
    vi.stubGlobal("fetch", vi.fn(async () => { throw new TypeError("Failed to fetch"); }));
    expect(await send(request({}))).toEqual({ status: 0, ok: false, body: undefined });

    respondWith("{not json", "application/json");
    expect((await send(request({}))).status).toBe(0);
  });

  it("keeps an error status and its body", async () => {
    respondWith('{"errors":{"Name":["Required"]}}', "application/problem+json", 400);
    expect(await send(request({}))).toEqual({ status: 400, ok: false, body: { errors: { Name: ["Required"] } } });
  });
});
