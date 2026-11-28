import type { BodyKind, HttpOutcome, HttpResponse, HttpSend, JsonValue, PathPart, RequestStatement, Route, SentField, SentHeader, Statement, StatusRoute, ValuePart } from "./plan";
import { assertNever, Stop } from "./read";
import { wireTexts } from "./wire";

export type Answer = { readonly route: readonly Statement[]; readonly reply: HttpResponse };

export interface Sendable {
  readonly method: string;
  readonly url: string;
  readonly headers: Headers;
}

export interface Reads {
  readonly route: readonly Statement[];
  readonly outcome: "success" | "status";
  readonly kind: "html" | "json";
}

export type Picked = { readonly kind: "answered"; readonly answer: Answer } | { readonly kind: "reads"; readonly reads: Reads };

type Decoded = { readonly kind: "decoded"; readonly body: JsonValue } | { readonly kind: "notOfKind"; readonly reason: string };

export function prepared(sent: HttpSend): Sendable {
  return { method: sent.method, url: composed(sent), headers: headersOf(sent.headers) };
}

export function picked(request: RequestStatement, sendable: Sendable, response: Response): Picked {
  if (response.redirected && new URL(response.url).origin !== window.location.origin) {
    return answered(caught(request, sendable, "redirect", response, "it was redirected to " + new URL(response.url).origin));
  }
  if (response.ok) return pickedBody(request, sendable, response, request.success, "success");
  const route = request.statuses.find((each) => each.status === response.status);
  if (route === undefined) return answered({ route: request.otherwise.do, reply: replyOf(response, "status", null) });
  return pickedBody(request, sendable, response, route, "status");
}

export function decoded(request: RequestStatement, sendable: Sendable, response: Response, reads: Reads, text: string): Answer {
  const body = reads.kind === "html" ? { kind: "decoded" as const, body: text } : parsed(text, response, reads.kind);
  if (body.kind === "decoded") return { route: reads.route, reply: replyOf(response, reads.outcome, body.body) };
  return notOfKindAnswer(request, sendable, response, reads.route, reads.outcome, body.reason);
}

export function unreachable(request: RequestStatement, sendable: Sendable, failed: unknown): Answer {
  return caught(request, sendable, "network", Response.error(), "the network failed: " + String(failed));
}

export function brokeOff(request: RequestStatement, sendable: Sendable, failed: unknown): Answer {
  return caught(request, sendable, "network", Response.error(), "its body broke off while it was read: " + String(failed));
}

function pickedBody(request: RequestStatement, sendable: Sendable, response: Response, route: Route | StatusRoute, outcome: "success" | "status"): Picked {
  const kind: BodyKind = route.kind;
  switch (kind) {
    case "none":
      return answered({ route: route.do, reply: replyOf(response, outcome, null) });
    case "html":
      if (mediaTypeOf(response) !== "text/html") return answered(notOfKindAnswer(request, sendable, response, route.do, outcome, notOfKind(response, kind)));
      return { kind: "reads", reads: { route: route.do, outcome, kind } };
    case "json":
      if (!isJson(mediaTypeOf(response))) return answered(notOfKindAnswer(request, sendable, response, route.do, outcome, notOfKind(response, kind)));
      return { kind: "reads", reads: { route: route.do, outcome, kind } };
    default:
      return assertNever(kind);
  }
}

function notOfKindAnswer(request: RequestStatement, sendable: Sendable, response: Response, route: readonly Statement[], outcome: "success" | "status", reason: string): Answer {
  switch (outcome) {
    case "success":
      return caught(request, sendable, "content", response, reason);
    case "status":
      return { route, reply: replyOf(response, "status", null) };
    default:
      return assertNever(outcome);
  }
}

function answered(answer: Answer): Picked {
  return { kind: "answered", answer };
}

function replyOf(response: Response, outcome: HttpOutcome, body: JsonValue): HttpResponse {
  return { status: response.status, outcome, headers: response.headers, body };
}

function composed(request: HttpSend): string {
  const path = request.path.map(segment).join("");
  const query = new URLSearchParams();
  for (const field of request.query) append(query, field);
  const added = query.toString();
  if (added === "") return path + request.search;
  return path + (request.search === "" ? "?" : request.search + "&") + added;
}

function segment(part: PathPart): string {
  switch (part.kind) {
    case "text":
      return part.text;
    case "value":
      return encodeURIComponent(pathText(part));
    default:
      return assertNever(part);
  }
}

function pathText(part: ValuePart): string {
  const [text] = wireTexts("request member '" + part.name + "'", part.value, part.form) ?? [];
  if (text === undefined) throw new Stop("The path value '" + part.name + "' reads nothing, so the request is not sent.");
  if (text === "") throw new Stop("The path value '" + part.name + "' is empty, so the request is not sent.");
  if (text.includes("/")) throw new Stop("The path value '" + part.name + "' holds '/', which no endpoint reads back as it was, so the request is not sent.");
  return text;
}

function append(query: URLSearchParams, field: SentField): void {
  const texts = wireTexts("request member '" + field.name + "'", field.value, field.form);
  if (texts !== undefined) {
    for (const text of texts) query.append(field.name, text);
    return;
  }
  switch (field.whenNothing) {
    case "empty":
      query.append(field.name, "");
      return;
    case "leave":
      return;
    default:
      assertNever(field.whenNothing);
  }
}

function headersOf(sent: readonly SentHeader[]): Headers {
  const headers = new Headers();
  for (const header of sent) {
    const [text] = wireTexts("header '" + header.name + "'", header.value, header.form) ?? [];
    if (text === undefined) continue;
    if (!carriedInAHeader(text)) throw new Stop("The header '" + header.name + "' holds a line break or a character past US-ASCII, which a header does not carry, so the request is not sent.");
    headers.append(header.name, text);
  }
  return headers;
}

function carriedInAHeader(text: string): boolean {
  return [...text].every((character) => character === "\t" || (character >= " " && character <= "~"));
}

function parsed(text: string, response: Response, kind: BodyKind): Decoded {
  if (text === "") return { kind: "decoded", body: null };
  try {
    return { kind: "decoded", body: JSON.parse(text) as JsonValue };
  } catch (failed) {
    return { kind: "notOfKind", reason: "its " + mediaTypeOf(response) + " body does not parse as " + kind + ": " + String(failed) };
  }
}

function notOfKind(response: Response, kind: BodyKind): string {
  return "its body is " + (response.headers.get("Content-Type") ?? "of no media type") + ", not " + kind;
}

function mediaTypeOf(response: Response): string {
  return (response.headers.get("Content-Type") ?? "").split(";")[0]?.trim().toLowerCase() ?? "";
}

function isJson(mediaType: string): boolean {
  return mediaType === "application/json" || mediaType.endsWith("+json");
}

function caught(request: RequestStatement, sendable: Sendable, outcome: HttpOutcome, response: Response, reason: string): Answer {
  console.error("[alis] " + sendable.method + " " + sendable.url + " ended '" + outcome + "': " + reason + ".");
  return { route: request.otherwise.do, reply: replyOf(response, outcome, null) };
}
