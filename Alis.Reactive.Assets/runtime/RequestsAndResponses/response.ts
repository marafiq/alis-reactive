import { assertNever, notBuilt, type HttpResponse, type RequestStatement, type ResponseBody, type Route, type SuccessBody } from "../Schema/index";
import type { PreparedRequest } from "./request";

type Routes = Pick<RequestStatement, "success" | "statuses" | "otherwise">;
type Decoded = { readonly kind: "body"; readonly body: ResponseBody } | { readonly kind: "content" } | { readonly kind: "network" };

export function routeFor(reply: HttpResponse, routes: Routes): Route {
  switch (reply.outcome) {
    case "ok":
      return routes.success;
    case "status": {
      const route = routes.statuses.find(route => route.status === reply.status);
      return route ?? routes.otherwise;
    }
    case "network":
    case "redirect":
    case "content":
      return routes.otherwise;
    default:
      return assertNever(reply.outcome);
  }
}

export async function attempt(request: PreparedRequest, routes: Routes, signal: AbortSignal, pageUrl: string): Promise<HttpResponse> {
  let response: Response;
  try {
    response = await fetch(request.url, { method: request.method, headers: request.headers, body: request.body, redirect: "follow", signal });
  } catch {
    return networkFailure(request);
  }
  const status = statusOf(response, request.respondedJson);
  const reply: HttpResponse = {
    status, outcome: status >= 200 && status < 300 ? "ok" : "status",
    headers: response.headers, request: request.request, inputs: request.inputs, body: { kind: "none" },
  };
  if (response.redirected && new URL(response.url).origin !== new URL(pageUrl).origin) return { ...reply, outcome: "redirect" };
  const decoded = await readBody(response, routeFor(reply, routes).kind);
  switch (decoded.kind) {
    case "body":
      return { ...reply, body: decoded.body };
    case "content":
      return { ...reply, outcome: reply.outcome === "ok" ? "content" : "status" };
    case "network":
      return networkFailure(request);
    default:
      return assertNever(decoded);
  }
}

function networkFailure(request: PreparedRequest): HttpResponse {
  return { status: 0, outcome: "network", headers: new Headers(), request: request.request, inputs: request.inputs, body: { kind: "none" } };
}

function statusOf(response: Response, respondedJson: boolean): number {
  if (!respondedJson) return response.status;
  const header = response.headers.get("X-Responded-JSON");
  if (header === null) return response.status;
  let parsed: unknown;
  try {
    parsed = JSON.parse(header);
  } catch {
    throw notBuilt("an invalid X-Responded-JSON header: D10.7 states only Katana's JSON status");
  }
  if (typeof parsed === "object" && parsed !== null && Object.hasOwn(parsed, "status")) {
    const status: unknown = Reflect.get(parsed, "status");
    if (typeof status === "number" && Number.isInteger(status) && status >= 100 && status <= 599) return status;
  }
  throw notBuilt("an X-Responded-JSON header without an HTTP status: D10.7 states only Katana's JSON status");
}

async function readBody(response: Response, kind: SuccessBody): Promise<Decoded> {
  switch (kind) {
    case "none":
      return { kind: "body", body: { kind: "none" } };
    case "file":
      throw notBuilt("a file response: U18's download");
    case "json":
    case "text":
    case "html": {
      const contentType = response.headers.get("Content-Type");
      const type = contentType === null ? "" : contentType.split(";", 1).join("").trim().toLowerCase();
      if (!takes(kind, type)) return { kind: "content" };
      let bytes: ArrayBuffer;
      try {
        bytes = await response.arrayBuffer();
      } catch {
        return { kind: "network" };
      }
      return decode(bytes, kind, contentType);
    }
    default:
      return assertNever(kind);
  }
}

function takes(kind: "json" | "text" | "html", type: string): boolean {
  switch (kind) {
    case "json":
      return type === "application/json" || type.endsWith("+json");
    case "text":
      return type.startsWith("text/");
    case "html":
      return type === "text/html" || type === "application/xhtml+xml";
    default:
      return assertNever(kind);
  }
}

function decode(bytes: ArrayBuffer, kind: "json" | "text" | "html", contentType: string | null): Decoded {
  switch (kind) {
    case "json": {
      const text = new TextDecoder("utf-8").decode(bytes);
      if (text === "") return { kind: "body", body: { kind: "none" } };
      try {
        const value: unknown = JSON.parse(text);
        return { kind: "body", body: { kind: "json", value } };
      } catch {
        return { kind: "content" };
      }
    }
    case "text":
    case "html":
      try {
        const value = new TextDecoder(charset(contentType)).decode(bytes);
        return { kind: "body", body: { kind, value } };
      } catch {
        return { kind: "content" };
      }
    default:
      return assertNever(kind);
  }
}

function charset(contentType: string | null): string {
  if (contentType === null) return "utf-8";
  const found = /;\s*charset\s*=\s*(?:"([^"]*)"|([^;\s]*))/iu.exec(contentType);
  return found === null ? "utf-8" : found[1] ?? found[2] ?? "";
}
