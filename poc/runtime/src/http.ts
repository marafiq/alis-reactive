import type { BodyKind, HttpOutcome, HttpResponse, HttpSend, JsonValue, PathPart, RequestStatement, SentField, SentHeader, Statement, ValuePart } from "./plan";
import { assertNever, Stop } from "./read";
import { wireTexts } from "./wire";

type Decoded = { readonly kind: "decoded"; readonly body: JsonValue } | { readonly kind: "notOfKind"; readonly reason: string };

export type Answer = { readonly route: readonly Statement[]; readonly reply: HttpResponse };

export async function answered(sent: HttpSend, request: RequestStatement): Promise<Answer> {
  const url = composed(sent);
  const headers = headersOf(sent.headers);
  let response: Response;
  try {
    response = await fetch(url, { method: sent.method, headers });
  } catch (failed) {
    return caught(request, sent, url, "network", Response.error(), "the network failed: " + String(failed));
  }
  if (response.redirected && new URL(response.url).origin !== window.location.origin) {
    return caught(request, sent, url, "redirect", response, "it was redirected to " + new URL(response.url).origin);
  }
  if (!response.ok) return statusAnswer(request, response);
  const decoded = await decode(response, request.success.kind);
  if (decoded.kind === "notOfKind") return caught(request, sent, url, "content", response, decoded.reason);
  return { route: request.success.do, reply: replyOf(response, "success", decoded.body) };
}

async function statusAnswer(request: RequestStatement, response: Response): Promise<Answer> {
  const route = request.statuses.find((each) => each.status === response.status);
  if (route === undefined) return { route: request.otherwise.do, reply: replyOf(response, "status", null) };
  const decoded = await decode(response, route.kind);
  return { route: route.do, reply: replyOf(response, "status", decoded.kind === "decoded" ? decoded.body : null) };
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

async function decode(response: Response, kind: BodyKind): Promise<Decoded> {
  const mediaType = mediaTypeOf(response);
  switch (kind) {
    case "html":
      return mediaType === "text/html" ? { kind: "decoded", body: await response.text() } : notOfKind(response, kind);
    case "json":
      return isJson(mediaType) ? parsed(await response.text(), response, kind) : notOfKind(response, kind);
    case "none":
      return { kind: "decoded", body: null };
    default:
      return assertNever(kind);
  }
}

function parsed(text: string, response: Response, kind: BodyKind): Decoded {
  if (text === "") return { kind: "decoded", body: null };
  try {
    return { kind: "decoded", body: JSON.parse(text) as JsonValue };
  } catch (failed) {
    return { kind: "notOfKind", reason: "its " + mediaTypeOf(response) + " body does not parse as " + kind + ": " + String(failed) };
  }
}

function notOfKind(response: Response, kind: BodyKind): Decoded {
  return { kind: "notOfKind", reason: "its body is " + (response.headers.get("Content-Type") ?? "of no media type") + ", not " + kind };
}

function mediaTypeOf(response: Response): string {
  return (response.headers.get("Content-Type") ?? "").split(";")[0]?.trim().toLowerCase() ?? "";
}

function isJson(mediaType: string): boolean {
  return mediaType === "application/json" || mediaType.endsWith("+json");
}

function caught(request: RequestStatement, sent: HttpSend, url: string, outcome: HttpOutcome, response: Response, reason: string): Answer {
  console.error("[alis] " + sent.method + " " + url + " ended '" + outcome + "': " + reason + ".");
  return { route: request.otherwise.do, reply: replyOf(response, outcome, null) };
}
