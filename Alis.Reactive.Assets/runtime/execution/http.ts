// http.ts - HTTP request execution using V3 RequestPlan type.
// Uses the shared value/gather/runtime concepts and keeps HTTP async isolated.

import type { RequestPlan, ResponseRoute, PlanDocument, ExecContext, Reaction } from "../types";
import { resolveRequestInput, type ResolvedRequestInput } from "./gather";
import { executeReaction } from "./execute";
import { validateContainer } from "../validation";
import { scope } from "../core/trace";
import { isMissingRuntimeValue } from "../domain/runtime-value";
import { ExecutionContext } from "../domain/execution-context";
import { resolveFetch, type ResolvedFetch } from "./http-fetch";
import { assertNever } from "../core/assert-never";

const log = scope("http");

type HttpResponseBody =
  | { readonly kind: "available"; readonly value: unknown }
  | { readonly kind: "missing" };

const missingResponseBody: HttpResponseBody = { kind: "missing" };

type HttpExchangeOutcome =
  | { readonly kind: "success"; readonly status: RequestOutcomeStatus; readonly body: HttpResponseBody }
  | { readonly kind: "error"; readonly status: RequestOutcomeStatus; readonly body: HttpResponseBody }
  | { readonly kind: "response-unavailable"; readonly status: RequestOutcomeStatus };

type RequestOutcomeStatus =
  | { readonly kind: "http"; readonly value: number }
  | { readonly kind: "network-failure"; readonly value: 0 }
  | { readonly kind: "client-failure"; readonly value: -1 };

/** Execute a single HTTP request with gather, before, response routing, complete, and chaining. */
export async function executeRequest(req: RequestPlan, plan: PlanDocument, ctx?: ExecContext): Promise<void> {
  await runHttpRequest(req, plan, ExecutionContext.from(ctx));
}

async function runHttpRequest(request: RequestPlan, plan: PlanDocument, context: ExecutionContext): Promise<void> {
  if (!requestCanSend(request, plan, context)) return;

  await runRequestReactions(request.before, plan, context.asAvailable());

  const prepared = prepareHttpRequest(request, plan, context);
  const outcome = await sendHttpRequest(request, prepared.fetch);
  await routeExchangeOutcome(outcome, request, plan, prepared.context);
}

function requestCanSend(request: RequestPlan, plan: PlanDocument, context: ExecutionContext): boolean {
  const validation = request.validation;
  const requestRequiresClientValidation = validation.kind === "container";
  if (!requestRequiresClientValidation) return true;

  const valid = validateContainer(plan, validation.container, context.asAvailable());
  if (!valid) log.debug("validation.aborted", { id: validation.container, url: request.url });

  return valid;
}

interface PreparedHttpRequest {
  readonly fetch: ResolvedFetch;
  readonly context: ExecutionContext;
}

function prepareHttpRequest(request: RequestPlan, plan: PlanDocument, context: ExecutionContext): PreparedHttpRequest {
  const currentContext = context.asAvailable();
  const resolvedInput = resolveRequestInput(request.input, request.method, plan, currentContext);
  const requestContext = context.withRequest(requestPayloadSnapshotFrom(resolvedInput));
  const fetch = resolveFetch(request, resolvedInput);

  return { fetch, context: requestContext };
}

async function sendHttpRequest(request: RequestPlan, fetchRequest: ResolvedFetch): Promise<HttpExchangeOutcome> {
  try {
    log.debug("fetch.send", { method: request.method, url: fetchRequest.url });

    const start = performance.now();
    const response = await fetch(fetchRequest.url, fetchRequest.init);
    log.debug("fetch.response", {
      method: request.method,
      url: fetchRequest.url,
      status: response.status,
      ms: Math.round(performance.now() - start),
    });

    return exchangeOutcomeFromResponse(response, await readResponseBody(response));
  } catch (err) {
    return exchangeOutcomeFromClientFailure(request, err);
  }
}

function exchangeOutcomeFromResponse(response: Response, body: HttpResponseBody): HttpExchangeOutcome {
  const status = httpOutcomeStatus(response.status);
  const statusIsSuccessful = response.ok;
  return statusIsSuccessful
    ? { kind: "success", status, body }
    : { kind: "error", status, body };
}

function exchangeOutcomeFromClientFailure(request: RequestPlan, err: unknown): HttpExchangeOutcome {
  const failure = clientRequestFailureFrom(err);
  log.error(failure.traceEvent, {
    method: request.method,
    url: request.url,
    error: String(err),
  });
  return { kind: "response-unavailable", status: failure.status };
}

async function routeExchangeOutcome(
  outcome: HttpExchangeOutcome,
  request: RequestPlan,
  plan: PlanDocument,
  context: ExecutionContext,
): Promise<void> {
  switch (outcome.kind) {
    case "success":
      await routeSuccess(request, plan, context, outcome.status, outcome.body);
      return;
    case "error":
      await routeError(request, plan, context, outcome.status, outcome.body);
      return;
    case "response-unavailable":
      await routeResponseUnavailable(request, plan, context, outcome.status);
      return;
    default:
      return assertNever(outcome, "HTTP exchange outcome");
  }
}

type ClientRequestFailure = {
  readonly status: RequestOutcomeStatus;
  readonly traceEvent: "fetch.network-error" | "fetch.client-error";
};

function clientRequestFailureFrom(error: unknown): ClientRequestFailure {
  const requestFailedBeforeResponse = error instanceof TypeError;
  return requestFailedBeforeResponse
    ? { status: networkFailureStatus(), traceEvent: "fetch.network-error" }
    : { status: clientFailureStatus(), traceEvent: "fetch.client-error" };
}

function httpOutcomeStatus(value: number): RequestOutcomeStatus {
  return { kind: "http", value };
}

function networkFailureStatus(): RequestOutcomeStatus {
  return { kind: "network-failure", value: 0 };
}

function clientFailureStatus(): RequestOutcomeStatus {
  return { kind: "client-failure", value: -1 };
}

function statusMatchesExact(status: RequestOutcomeStatus, planStatus: number): boolean {
  return status.kind === "http" && status.value === planStatus;
}

function statusForLog(status: RequestOutcomeStatus): number {
  return status.value;
}

async function routeSuccess(
  request: RequestPlan,
  plan: PlanDocument,
  context: ExecutionContext,
  status: RequestOutcomeStatus,
  body: HttpResponseBody,
): Promise<void> {
  const responseContext = contextWithResponseBody(context, body);
  await routeAndComplete(request, plan, context, () =>
    routeResponseRoutes(request.success, status, plan, responseContext.asAvailable()));
  await runFollowUpRequest(request.chain, plan, responseContext);
}

async function routeError(
  request: RequestPlan,
  plan: PlanDocument,
  context: ExecutionContext,
  status: RequestOutcomeStatus,
  body: HttpResponseBody,
): Promise<void> {
  await routeAndComplete(request, plan, context, () =>
    routeResponseRoutes(request.error, status, plan, contextWithResponseBody(context, body).asAvailable()));
}

async function routeResponseUnavailable(
  request: RequestPlan,
  plan: PlanDocument,
  context: ExecutionContext,
  status: RequestOutcomeStatus,
): Promise<void> {
  await routeAndComplete(request, plan, context, () =>
    routeResponseRoutes(request.error, status, plan, context.asAvailable()));
}

async function routeAndComplete(
  request: RequestPlan,
  plan: PlanDocument,
  context: ExecutionContext,
  routeStage: () => Promise<void>,
): Promise<void> {
  try {
    await routeStage();
  } finally {
    await runRequestReactions(request.complete, plan, context.asAvailable());
  }
}

async function runFollowUpRequest(
  chain: RequestPlan["chain"],
  plan: PlanDocument,
  context: ExecutionContext,
): Promise<void> {
  switch (chain.kind) {
    case "terminal":
      return;
    case "follow-up":
      await runHttpRequest(chain.next, plan, context);
      return;
    default:
      return assertNever(chain, "request chain");
  }
}

async function runRequestReactions(
  reactions: readonly Reaction[],
  plan: PlanDocument,
  context: ExecContext,
): Promise<void> {
  for (const reaction of reactions) {
    await executeReaction(reaction, plan, context);
  }
}

function contextWithResponseBody(context: ExecutionContext, body: HttpResponseBody): ExecutionContext {
  const bodyCanBeReadByReactions = body.kind === "available";
  if (!bodyCanBeReadByReactions) return context;

  return context.withResponse(body.value);
}

function requestPayloadSnapshotFrom(resolvedInput: ResolvedRequestInput): Record<string, unknown> {
  const body = resolvedInput.body;
  const bodyIsFormData = body instanceof FormData;
  if (bodyIsFormData) return {};

  return body;
}

async function readResponseBody(response: Response): Promise<HttpResponseBody> {
  const kind = responseContentKind(response.headers.get("Content-Type"));
  switch (kind) {
    case "json":
      return jsonResponseBodyFrom(await response.text());
    case "text":
      return responseBodyFrom(await response.text());
    case "empty":
      return missingResponseBody;
    default:
      return assertNever(kind, "response content kind");
  }
}

function responseContentKind(contentType: string | null): "json" | "text" | "empty" {
  const mediaType = responseMediaType(contentType);
  if (mediaType === undefined) return "empty";
  if (mediaType === "application/json" || mediaType.endsWith("+json")) return "json";
  if (mediaType.startsWith("text/") || mediaType.includes("html")) return "text";
  return "empty";
}

function responseMediaType(contentType: string | null): string | undefined {
  if (contentType === null || contentType.length === 0) return undefined;
  return contentType.split(";")[0]?.trim().toLowerCase() ?? "";
}

function jsonResponseBodyFrom(textBody: string): HttpResponseBody {
  const bodyIsEmpty = textBody.trim().length === 0;
  if (bodyIsEmpty) return missingResponseBody;

  return responseBodyFrom(JSON.parse(textBody));
}

function responseBodyFrom(rawBody: unknown): HttpResponseBody {
  const bodyCanBeReadByReactions = !isMissingRuntimeValue(rawBody);
  if (!bodyCanBeReadByReactions) return missingResponseBody;

  return { kind: "available", value: rawBody };
}

async function routeResponseRoutes(
  routes: ResponseRoute[],
  status: RequestOutcomeStatus,
  plan: PlanDocument,
  context: ExecContext,
): Promise<void> {
  const route = routes.find(routeMatchesStatus(status)) ?? routes.find(routeMatchesAnyStatus);
  if (route) {
    await executeReaction(route.reaction, plan, context);
    return;
  }

  if (routes.length === 0) return;

  log.warn("response.unhandled", {
    status: statusForLog(status),
    routeCount: routes.length,
  });
}

function routeMatchesStatus(status: RequestOutcomeStatus): (route: ResponseRoute) => boolean {
  return route => {
    const match = route.match;
    const routeTargetsExactStatus = match.kind === "status";
    return routeTargetsExactStatus && statusMatchesExact(status, match.status);
  };
}

function routeMatchesAnyStatus(route: ResponseRoute): boolean {
  return route.match.kind === "any";
}
