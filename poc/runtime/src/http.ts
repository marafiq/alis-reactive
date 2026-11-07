import type { BodyKind, HttpOutcome, HttpResponse, HttpSend } from "./plan";
import { assertNever } from "./read";

export const http = { send };

async function send(request: HttpSend): Promise<HttpResponse> {
  let response: Response;
  try {
    response = await fetch(request.url, { method: request.method });
  } catch (failed) {
    return unanswered(request, "network", false, 0, "the network failed: " + String(failed));
  }
  if (response.redirected && new URL(response.url).origin !== window.location.origin) {
    return unanswered(request, "redirect", response.ok, response.status, "it was redirected to " + new URL(response.url).origin);
  }
  if (!response.ok) return { ok: false, status: response.status, outcome: "status", body: null };
  if (!isOfKind(response, request.success)) {
    return unanswered(request, "kind", true, response.status, "its body is " + (response.headers.get("Content-Type") ?? "of no media type") + ", not " + request.success);
  }
  return { ok: true, status: response.status, outcome: "success", body: await response.text() };
}

function unanswered(request: HttpSend, outcome: HttpOutcome, ok: boolean, status: number, reason: string): HttpResponse {
  console.error("[alis] " + request.method + " " + request.url + " ran no route: " + reason + ".");
  return { ok, status, outcome, body: null };
}

function isOfKind(response: Response, kind: BodyKind): boolean {
  const mediaType = (response.headers.get("Content-Type") ?? "").split(";")[0]?.trim().toLowerCase();
  switch (kind) {
    case "html":
      return mediaType === "text/html";
    default:
      return assertNever(kind);
  }
}
