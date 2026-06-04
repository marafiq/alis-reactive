// HTTP request execution keeps gather, response routing, and chaining in the async lane.

import type { RequestPlan, ResponseRoute, PlanDocument, ExecContext, ReactionGraph } from "../../types/index";
import { resolveRequestInput, type ResolvedRequestInput } from "./gather";
import { executeReaction } from "../reactions/execute";
import { validateContainer } from "../../validation/index";
import { scope } from "../../diagnostics/trace";
import { isMissingRuntimeValue } from "../../browser-objects/runtime-value";
import { ExecutionContext } from "../../browser-objects/execution-context";
import { resolveFetch, type ResolvedFetch } from "./http-fetch";
import { assertNever } from "../../shared/assert-never";
import { toJavaScriptString } from "../../shared/javascript-string";

const log = scope("http");

type HttpResponseBody =
  | { readonly kind: "available"; readonly value: unknown }
  | { readonly kind: "missing" };

const missingResponseBody: HttpResponseBody = { kind: "missing" };

type HttpExchangeOutcome =
  | { readonly kind: "success"; readonly status: number; readonly body: HttpResponseBody }
  | { readonly kind: "error"; readonly status: number; readonly body: HttpResponseBody }
  | { readonly kind: "response-unavailable" };

export async function executeRequest(
  request: RequestPlan,
  planDocument: PlanDocument,
  rawContext?: ExecContext,
): Promise<void> {
  await runHttpRequest(request, planDocument, ExecutionContext.from(rawContext));
}

async function runHttpRequest(
  request: RequestPlan,
  planDocument: PlanDocument,
  context: ExecutionContext,
): Promise<void> {
  if (!requestCanSend(request, planDocument, context)) return;

  await runRequestReactions(request.whileLoading, planDocument, context.asAvailable());

  const preparedRequest = prepareHttpRequest(request, planDocument, context);
  const outcome = await sendHttpRequest(request, preparedRequest.fetchRequest);
  await routeExchangeOutcome(outcome, request, planDocument, preparedRequest.context);
}

function requestCanSend(request: RequestPlan, planDocument: PlanDocument, context: ExecutionContext): boolean {
  const validation = request.validation;
  const requestRequiresClientValidation = validation.kind === "container";
  if (!requestRequiresClientValidation) return true;

  const valid = validateContainer(planDocument, validation.container, context.asAvailable());
  if (!valid) log.debug("validation.aborted", { id: validation.container, url: request.url });

  return valid;
}

interface PreparedHttpRequest {
  readonly fetchRequest: ResolvedFetch;
  readonly context: ExecutionContext;
}

function prepareHttpRequest(
  request: RequestPlan,
  planDocument: PlanDocument,
  context: ExecutionContext,
): PreparedHttpRequest {
  const currentContext = context.asAvailable();
  const resolvedInput = resolveRequestInput(request.input, request.method, planDocument, currentContext);
  const requestContext = context.withRequest(requestPayloadSnapshotFrom(resolvedInput));
  const fetchRequest = resolveFetch(request, resolvedInput);

  return { fetchRequest, context: requestContext };
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
  const statusIsSuccessful = response.ok;
  return statusIsSuccessful
    ? { kind: "success", status: response.status, body }
    : { kind: "error", status: response.status, body };
}

function exchangeOutcomeFromClientFailure(request: RequestPlan, err: unknown): HttpExchangeOutcome {
  const traceEvent = clientRequestFailureTraceEvent(err);
  log.error(traceEvent, {
    method: request.method,
    url: request.url,
    error: toJavaScriptString(err),
  });
  return { kind: "response-unavailable" };
}

async function routeExchangeOutcome(
  outcome: HttpExchangeOutcome,
  request: RequestPlan,
  planDocument: PlanDocument,
  context: ExecutionContext,
): Promise<void> {
  switch (outcome.kind) {
    case "success":
      await routeSuccess(request, planDocument, context, outcome.status, outcome.body);
      return;
    case "error":
      await routeError(request, planDocument, context, outcome.status, outcome.body);
      return;
    case "response-unavailable":
      await routeResponseUnavailable(request, planDocument, context);
      return;
    default:
      return assertNever(outcome, "HTTP exchange outcome");
  }
}

function clientRequestFailureTraceEvent(error: unknown): "fetch.network-error" | "fetch.client-error" {
  const requestFailedBeforeResponse = error instanceof TypeError;
  return requestFailedBeforeResponse ? "fetch.network-error" : "fetch.client-error";
}

async function routeSuccess(
  request: RequestPlan,
  planDocument: PlanDocument,
  context: ExecutionContext,
  status: number,
  body: HttpResponseBody,
): Promise<void> {
  const responseContext = contextWithResponseBody(context, body);
  await routeAndComplete(request, planDocument, context, () =>
    routeResponseRoutes(request.success, status, planDocument, responseContext.asAvailable()));
  await runFollowUpRequest(request.chain, planDocument, responseContext);
}

async function routeError(
  request: RequestPlan,
  planDocument: PlanDocument,
  context: ExecutionContext,
  status: number,
  body: HttpResponseBody,
): Promise<void> {
  await routeAndComplete(request, planDocument, context, () =>
    routeResponseRoutes(request.error, status, planDocument, contextWithResponseBody(context, body).asAvailable()));
}

async function routeResponseUnavailable(
  request: RequestPlan,
  planDocument: PlanDocument,
  context: ExecutionContext,
): Promise<void> {
  await routeAndComplete(request, planDocument, context, () =>
    routeAnyResponseRoute(request.error, planDocument, context.asAvailable()));
}

async function routeAndComplete(
  request: RequestPlan,
  planDocument: PlanDocument,
  context: ExecutionContext,
  routeStage: () => Promise<void>,
): Promise<void> {
  try {
    await routeStage();
  } finally {
    await runRequestReactions(request.finally, planDocument, context.asAvailable());
  }
}

async function runFollowUpRequest(
  chain: RequestPlan["chain"],
  planDocument: PlanDocument,
  context: ExecutionContext,
): Promise<void> {
  switch (chain.kind) {
    case "terminal":
      return;
    case "follow-up":
      await runHttpRequest(chain.next, planDocument, context);
      return;
    default:
      return assertNever(chain, "request chain");
  }
}

async function runRequestReactions(
  reactions: readonly ReactionGraph[],
  planDocument: PlanDocument,
  context: ExecContext,
): Promise<void> {
  for (const reaction of reactions) {
    await executeReaction(reaction, planDocument, context);
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
  status: number,
  planDocument: PlanDocument,
  context: ExecContext,
): Promise<void> {
  const route = routes.find(routeMatchesStatus(status)) ?? routes.find(routeMatchesAnyStatus);
  if (route) {
    await executeReaction(route.reaction, planDocument, context);
    return;
  }

  if (routes.length === 0) return;

  log.warn("response.unhandled", {
    status,
    routeCount: routes.length,
  });
}

async function routeAnyResponseRoute(
  routes: ResponseRoute[],
  planDocument: PlanDocument,
  context: ExecContext,
): Promise<void> {
  const route = routes.find(routeMatchesAnyStatus);
  if (route) {
    await executeReaction(route.reaction, planDocument, context);
    return;
  }

  if (routes.length === 0) return;

  log.warn("response.unhandled", {
    outcome: "response-unavailable",
    routeCount: routes.length,
  });
}

function routeMatchesStatus(status: number): (route: ResponseRoute) => boolean {
  return route => {
    const match = route.match;
    const routeTargetsExactStatus = match.kind === "status";
    return routeTargetsExactStatus && status === match.status;
  };
}

function routeMatchesAnyStatus(route: ResponseRoute): boolean {
  return route.match.kind === "any";
}
