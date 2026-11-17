import { assertNever, type HttpResponse, type RequestStatement, type ResponseBody, type Route, type SuccessBody } from "../Schema/index";
import { isThePagesOrigin, type Attempting, type Sending, type TraceHeaders } from "../Tracing/index";
import type { PreparedRequest } from "./request";
import { Retry, waitToRetry } from "./retry";

type Routes = Pick<RequestStatement, "success" | "statuses" | "otherwise">;
type Decoded =
  | { readonly kind: "body"; readonly body: ResponseBody }
  | { readonly kind: "file"; readonly file: Blob; readonly name: string }
  | { readonly kind: "content" }
  | { readonly kind: "network" };

export interface Answer {
  readonly reply: HttpResponse;
  readonly saveDownload: () => void;
}

const dispositionParameter = /;\s*([^\s;=]+)\s*=\s*(?:"((?:[^"\\]|\\.)*)"|([^\s;]*))/gu;

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

export async function attempt(request: PreparedRequest, routes: Routes, signal: AbortSignal, pageUrl: string, sending: Sending): Promise<Answer> {
  const retry = new Retry(request.retry, request.method);
  const toThePagesOrigin = isThePagesOrigin(request.url, pageUrl);
  let sent = 1;
  for (;;) {
    signal.throwIfAborted();
    const attempting = sending.attempt(toThePagesOrigin, request.method, request.template);
    const answer = await sendOnce(request, routes, signal, pageUrl, attempting.headers);
    signal.throwIfAborted();
    attemptAnswered(attempting, answer.reply);
    const wait = retry.next(answer.reply, sent);
    if (wait === undefined) return sendEnded(sending, answer);
    await waitToRetry(wait, signal);
    signal.throwIfAborted();
    if (!retry.withinBudget(0)) return sendEnded(sending, answer);
    sent += 1;
  }
}

function attemptAnswered(attempting: Attempting, reply: HttpResponse): void {
  if (reply.outcome === "network") {
    attempting.end("completed");
    return;
  }
  attempting.answered(reply.status);
}

function sendEnded(sending: Sending, answer: Answer): Answer {
  sending.end("completed");
  return answer;
}

async function sendOnce(request: PreparedRequest, routes: Routes, signal: AbortSignal, pageUrl: string, trace: TraceHeaders): Promise<Answer> {
  let response: Response;
  try {
    response = await fetch(request.url, { method: request.method, headers: traced(request.headers, trace), body: request.body, redirect: "follow", signal });
  } catch {
    return answerOf(networkFailure(request));
  }
  const status = statusOf(response, request.respondedJson);
  const reply: HttpResponse = {
    status, outcome: status >= 200 && status < 300 ? "ok" : "status",
    headers: response.headers, request: request.request, inputs: request.inputs, body: { kind: "none" },
  };
  if (response.redirected && new URL(response.url).origin !== new URL(pageUrl).origin) return answerOf({ ...reply, outcome: "redirect" });
  const decoded = await readBody(response, routeFor(reply, routes).kind, new URL(request.url, pageUrl));
  switch (decoded.kind) {
    case "body":
      return answerOf({ ...reply, body: decoded.body });
    case "file":
      return {
        reply: { ...reply, body: { kind: "file", value: { name: decoded.name, size: decoded.file.size, type: decoded.file.type } } },
        saveDownload: () => {
          saveAs(decoded.file, decoded.name);
        },
      };
    case "content":
      return answerOf({ ...reply, outcome: reply.outcome === "ok" ? "content" : "status" });
    case "network":
      return answerOf(networkFailure(request));
    default:
      return assertNever(decoded);
  }
}

function traced(headers: Headers, trace: TraceHeaders): Headers {
  const entries = Object.entries(trace);
  if (entries.length === 0) return headers;
  const sent = new Headers(headers);
  for (const [name, value] of entries) sent.set(name, value);
  return sent;
}

function answerOf(reply: HttpResponse): Answer {
  return { reply, saveDownload: savesNothing };
}

function savesNothing(): void {
  return undefined;
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
    return response.status;
  }
  if (typeof parsed === "object" && parsed !== null && Object.hasOwn(parsed, "status")) {
    const status: unknown = Reflect.get(parsed, "status");
    if (typeof status === "number" && Number.isInteger(status) && status >= 100 && status <= 599) return status;
  }
  return response.status;
}

async function readBody(response: Response, kind: SuccessBody, url: URL): Promise<Decoded> {
  switch (kind) {
    case "none":
      return { kind: "body", body: { kind: "none" } };
    case "file": {
      const name = fileName(response.headers.get("Content-Disposition"), url);
      let file: Blob;
      try {
        file = await response.blob();
      } catch {
        return { kind: "network" };
      }
      return { kind: "file", file, name };
    }
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

function fileName(disposition: string | null, url: URL): string {
  const named = disposition === null ? "" : nameIn(disposition);
  if (named !== "") return named;
  const segment = url.pathname.slice(url.pathname.lastIndexOf("/") + 1);
  if (segment === "") throw new Error("The file that '" + url.pathname + "' answered has no name: its Content-Disposition names none, and its path ends with no segment.");
  return decodedSegment(segment);
}

function nameIn(disposition: string): string {
  const parameters = new Map<string, string>();
  for (const found of disposition.matchAll(dispositionParameter)) {
    const name = (found[1] ?? "").toLowerCase();
    const quoted = found[2];
    if (!parameters.has(name)) parameters.set(name, quoted === undefined ? found[3] ?? "" : quoted.replace(/\\(.)/gu, "$1"));
  }
  const extended = extendedValue(parameters.get("filename*") ?? "");
  return extended === "" ? parameters.get("filename") ?? "" : extended;
}

function extendedValue(value: string): string {
  const parts = value.split("'");
  if (parts.length !== 3 || parts[0]?.toLowerCase() !== "utf-8") return "";
  try {
    return decodeURIComponent(parts[2] ?? "");
  } catch {
    return "";
  }
}

function decodedSegment(segment: string): string {
  try {
    return decodeURIComponent(segment);
  } catch {
    return segment;
  }
}

function saveAs(file: Blob, name: string): void {
  const address = URL.createObjectURL(file);
  const link = document.createElement("a");
  link.href = address;
  link.download = name;
  link.click();
  URL.revokeObjectURL(address);
}
