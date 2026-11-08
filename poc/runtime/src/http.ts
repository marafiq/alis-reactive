import type { BodyKind, HttpOutcome, HttpResponse, HttpSend, JsonValue, PathPart, QueryField, ValuePart } from "./plan";
import { assertNever, Stop } from "./read";
import { wireTexts } from "./wire";

type Decoded = { readonly kind: "decoded"; readonly body: JsonValue } | { readonly kind: "notOfKind"; readonly reason: string };

export async function send(request: HttpSend): Promise<HttpResponse> {
  const url = composed(request);
  let response: Response;
  try {
    response = await fetch(url, { method: request.method });
  } catch (failed) {
    return unanswered(request, url, "network", false, 0, "the network failed: " + String(failed));
  }
  if (response.redirected && new URL(response.url).origin !== window.location.origin) {
    return unanswered(request, url, "redirect", response.ok, response.status, "it was redirected to " + new URL(response.url).origin);
  }
  if (!response.ok) return { ok: false, status: response.status, outcome: "status", body: await errorBody(response) };
  const decoded = await decode(response, request.success);
  if (decoded.kind === "notOfKind") return unanswered(request, url, "kind", true, response.status, decoded.reason);
  return { ok: true, status: response.status, outcome: "success", body: decoded.body };
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
      return encodeURIComponent(routeText(part));
    default:
      return assertNever(part);
  }
}

function routeText(part: ValuePart): string {
  const [text] = wireTexts(part.name, part.value, part.form) ?? [];
  if (text === undefined) throw new Stop("The route value '" + part.name + "' reads nothing, so the request is not sent.");
  if (text === "") throw new Stop("The route value '" + part.name + "' is empty, so the request is not sent.");
  if (text.includes("/")) throw new Stop("The route value '" + part.name + "' holds '/', which no endpoint reads back as it was, so the request is not sent.");
  return text;
}

function append(query: URLSearchParams, field: QueryField): void {
  const texts = wireTexts(field.name, field.value, field.form);
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

async function decode(response: Response, kind: BodyKind): Promise<Decoded> {
  const mediaType = mediaTypeOf(response);
  switch (kind) {
    case "html":
      return mediaType === "text/html" ? { kind: "decoded", body: await response.text() } : notOfKind(response, kind);
    case "json":
      return isJson(mediaType) ? parsed(await response.text(), response, kind) : notOfKind(response, kind);
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

async function errorBody(response: Response): Promise<JsonValue> {
  if (!isJson(mediaTypeOf(response))) return null;
  const decoded = parsed(await response.text(), response, "json");
  return decoded.kind === "decoded" ? decoded.body : null;
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

function unanswered(request: HttpSend, url: string, outcome: HttpOutcome, ok: boolean, status: number, reason: string): HttpResponse {
  console.error("[alis] " + request.method + " " + url + " ended '" + outcome + "': " + reason + ".");
  return { ok, status, outcome, body: null };
}
