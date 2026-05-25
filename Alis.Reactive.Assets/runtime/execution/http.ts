// http.ts - HTTP request execution using V3 Request type.
// Uses the shared value/gather/runtime concepts and keeps HTTP async isolated.

import type { Request, ResponseHandler, Plan, ExecContext, Reaction } from "../types";
import { resolveGather, type GatherResult } from "./gather";
import { executeReaction } from "./execute";
import { validateContainer } from "../validation";
import { scope } from "../core/trace";
import { isMissingRuntimeValue } from "../domain/runtime-value";
import { ExecutionContext } from "../domain/execution-context";
import { HttpFetchBuilder, type ResolvedFetch } from "./http-fetch";
import { assertNever } from "../core/assert-never";

const log = scope("http");

type HttpResponseBody =
  | { readonly kind: "available"; readonly value: unknown }
  | { readonly kind: "missing" };

const missingResponseBody: HttpResponseBody = { kind: "missing" };

class CapturedResponseBody {
  static from(rawBody: unknown): HttpResponseBody {
    const bodyCanBeReadByReactions = !isMissingRuntimeValue(rawBody);
    if (!bodyCanBeReadByReactions) return missingResponseBody;

    return { kind: "available", value: rawBody };
  }

  static missing(): HttpResponseBody {
    return missingResponseBody;
  }
}

/** Execute a single HTTP request with gather, before, response routing, complete, and chaining. */
export async function executeRequest(req: Request, plan: Plan, ctx?: ExecContext): Promise<void> {
  await new HttpRequestExecution(req, plan, RequestContext.start(ctx)).run();
}

class HttpRequestExecution {
  constructor(
    private readonly request: Request,
    private readonly plan: Plan,
    private readonly context: RequestContext,
  ) {}

  async run(): Promise<void> {
    const validationGate = RequestValidationGate.for(this.request, this.plan, this.context);
    if (!validationGate.allowsSend()) return;

    await runRequestReactions(this.request.before, this.plan, this.context.current);
    await this.dispatch();
  }

  private async dispatch(): Promise<void> {
    let prepared: PreparedHttpRequest;
    try {
      prepared = prepareHttpRequest(this.request, this.plan, this.context);
    } catch (err) {
      await routeClientFailure(this.request, this.plan, this.context, err);
      return;
    }

    const responsePlan = new HttpResponsePlan(this.request, this.plan, prepared.context);
    const outcome = await this.exchangeOutcome(prepared.fetch);
    await outcome.routeWith(responsePlan);
  }

  private async exchangeOutcome(fetchRequest: ResolvedFetch): Promise<HttpExchangeOutcome> {
    try {
      return await HttpExchange.from(this.request, fetchRequest).send();
    } catch (err) {
      return HttpExchangeOutcome.fromClientFailure(this.request, err);
    }
  }
}

class RequestValidationGate {
  private constructor(private readonly openForSend: boolean) {}

  static open(): RequestValidationGate {
    return new RequestValidationGate(true);
  }

  static for(request: Request, plan: Plan, context: RequestContext): RequestValidationGate {
    const validation = request.validation;
    const requestRequiresClientValidation = validation.kind === "container";
    if (!requestRequiresClientValidation) return RequestValidationGate.open();

    const valid = validateContainer(plan, validation.container, context.current);
    if (!valid) log.debug("validation.aborted", { id: validation.container, url: request.url });

    return new RequestValidationGate(valid);
  }

  allowsSend(): boolean {
    return this.openForSend;
  }
}

interface PreparedHttpRequest {
  readonly fetch: ResolvedFetch;
  readonly context: RequestContext;
}

function prepareHttpRequest(request: Request, plan: Plan, context: RequestContext): PreparedHttpRequest {
  const gathered = resolveGather(request.input, request.method, plan, context.current);
  const requestContext = context.withRequest(gathered);
  const fetch = HttpFetchBuilder
    .for(request, plan, context.current)
    .build(gathered);

  return { fetch, context: requestContext };
}

async function routeClientFailure(
  request: Request,
  plan: Plan,
  context: RequestContext,
  error: unknown,
): Promise<void> {
  const outcome = HttpExchangeOutcome.fromClientFailure(request, error);
  await outcome.routeWith(new HttpResponsePlan(request, plan, context));
}

class HttpExchange {
  private constructor(
    private readonly request: Request,
    private readonly fetchRequest: ResolvedFetch,
  ) {}

  static from(request: Request, fetchRequest: ResolvedFetch): HttpExchange {
    return new HttpExchange(request, fetchRequest);
  }

  async send(): Promise<HttpExchangeOutcome> {
    log.debug("fetch.send", { method: this.request.method, url: this.fetchRequest.url });

    const start = performance.now();
    const response = await fetch(this.fetchRequest.url, this.fetchRequest.init);
    log.debug("fetch.response", {
      method: this.request.method,
      url: this.fetchRequest.url,
      status: response.status,
      ms: Math.round(performance.now() - start),
    });

    const body = await ResponseBody.read(response);
    return HttpExchangeOutcome.fromResponse(response, body);
  }
}

abstract class HttpExchangeOutcome {
  static fromResponse(response: Response, body: HttpResponseBody): HttpExchangeOutcome {
    const status = RequestOutcomeStatus.http(response.status);
    const statusIsSuccessful = response.ok;
    if (statusIsSuccessful) return new SuccessfulHttpExchange(status, body);

    return new FailedHttpExchange(status, body);
  }

  static fromClientFailure(request: Request, err: unknown): HttpExchangeOutcome {
    const failure = ClientRequestFailure.from(err);
    log.error(failure.traceEvent, {
      method: request.method,
      url: request.url,
      error: String(err),
    });
    return new ResponseUnavailableHttpExchange(failure.status);
  }

  abstract routeWith(plan: HttpResponsePlan): Promise<void>;
}

class SuccessfulHttpExchange extends HttpExchangeOutcome {
  constructor(
    private readonly status: RequestOutcomeStatus,
    private readonly body: HttpResponseBody,
  ) {
    super();
  }

  async routeWith(plan: HttpResponsePlan): Promise<void> {
    await plan.routeSuccess(this.status, this.body);
  }
}

class FailedHttpExchange extends HttpExchangeOutcome {
  constructor(
    private readonly status: RequestOutcomeStatus,
    private readonly body: HttpResponseBody,
  ) {
    super();
  }

  async routeWith(plan: HttpResponsePlan): Promise<void> {
    await plan.routeError(this.status, this.body);
  }
}

class ResponseUnavailableHttpExchange extends HttpExchangeOutcome {
  constructor(private readonly status: RequestOutcomeStatus) {
    super();
  }

  async routeWith(plan: HttpResponsePlan): Promise<void> {
    await plan.routeResponseUnavailable(this.status);
  }
}

class ClientRequestFailure {
  private constructor(
    readonly status: RequestOutcomeStatus,
    readonly traceEvent: "fetch.network-error" | "fetch.client-error",
  ) {}

  static from(error: unknown): ClientRequestFailure {
    const requestFailedBeforeResponse = error instanceof TypeError;
    if (requestFailedBeforeResponse) {
      return new ClientRequestFailure(RequestOutcomeStatus.networkFailure(), "fetch.network-error");
    }

    return new ClientRequestFailure(RequestOutcomeStatus.clientFailure(), "fetch.client-error");
  }
}

class RequestOutcomeStatus {
  private constructor(
    private readonly kind: "http" | "network-failure" | "client-failure",
    private readonly value: number,
  ) {}

  static http(value: number): RequestOutcomeStatus {
    return new RequestOutcomeStatus("http", value);
  }

  static networkFailure(): RequestOutcomeStatus {
    return new RequestOutcomeStatus("network-failure", 0);
  }

  static clientFailure(): RequestOutcomeStatus {
    return new RequestOutcomeStatus("client-failure", -1);
  }

  matchesExact(planStatus: number): boolean {
    const status = ResponseHandlerStatus.fromPlan(planStatus);
    const responseHasHttpStatusCode = this.kind === "http";
    return responseHasHttpStatusCode && this.value === status.value;
  }

  forLog(): number {
    return this.value;
  }
}

class ResponseHandlerStatus {
  private constructor(readonly value: number) {}

  static fromPlan(value: number): ResponseHandlerStatus {
    const statusComesFromStandardHttpRange = value >= 100 && value <= 599;
    if (statusComesFromStandardHttpRange) return new ResponseHandlerStatus(value);

    throw new Error(
      `[alis] response handler status ${value} is invalid; expected an HTTP status from 100 to 599`,
    );
  }
}

class HttpResponsePlan {
  constructor(
    private readonly request: Request,
    private readonly plan: Plan,
    private readonly context: RequestContext,
  ) {}

  async routeSuccess(status: RequestOutcomeStatus, body: HttpResponseBody): Promise<void> {
    await this.routeAndComplete(() =>
      this.route(this.request.success, status, this.context.withResponse(body).current));
    await runFollowUpRequest(this.request.chain, this.plan, this.context);
  }

  async routeError(status: RequestOutcomeStatus, body: HttpResponseBody): Promise<void> {
    await this.routeAndComplete(() =>
      this.route(this.request.error, status, this.context.withResponse(body).current));
  }

  async routeResponseUnavailable(status: RequestOutcomeStatus): Promise<void> {
    await this.routeAndComplete(() =>
      this.route(this.request.error, status, this.context.current));
  }

  private async route(
    handlers: ResponseHandler[],
    status: RequestOutcomeStatus,
    context: ExecContext,
  ): Promise<void> {
    await new ResponseRouter(handlers, status, this.plan, context).run();
  }

  private async routeAndComplete(routeStage: () => Promise<void>): Promise<void> {
    try {
      await routeStage();
    } finally {
      await this.runComplete();
    }
  }

  private async runComplete(): Promise<void> {
    await runRequestReactions(this.request.complete, this.plan, this.context.current);
  }
}

async function runFollowUpRequest(
  chain: Request["chain"],
  plan: Plan,
  context: RequestContext,
): Promise<void> {
  switch (chain.kind) {
    case "terminal":
      return;
    case "follow-up":
      await new HttpRequestExecution(chain.next, plan, context).run();
      return;
    default:
      return assertNever(chain, "request chain");
  }
}

async function runRequestReactions(
  reactions: readonly Reaction[],
  plan: Plan,
  context: ExecContext,
): Promise<void> {
  for (const reaction of reactions) {
    await executeReaction(reaction, plan, context);
  }
}

class RequestContext {
  private constructor(private readonly context: ExecutionContext) {}

  get current(): ExecContext {
    return this.context.asAvailable();
  }

  static start(context: ExecContext | undefined): RequestContext {
    return new RequestContext(ExecutionContext.from(context));
  }

  withRequest(gathered: GatherResult): RequestContext {
    return new RequestContext(this.context.withRequest(RequestPayloadSnapshot.from(gathered)));
  }

  withResponse(body: HttpResponseBody): RequestContext {
    const bodyCanBeReadByReactions = body.kind === "available";
    if (!bodyCanBeReadByReactions) return this;

    return new RequestContext(this.context.withResponse(body.value));
  }
}

class RequestPayloadSnapshot {
  static from(gathered: GatherResult): Record<string, unknown> {
    const body = gathered.body;
    const bodyUsesMultipartTransport = body instanceof FormData;
    if (bodyUsesMultipartTransport) return {};

    return body;
  }
}

class ResponseBody {
  static async read(response: Response): Promise<HttpResponseBody> {
    const contentType = response.headers.get("Content-Type");
    const kind = ResponseContentType.from(contentType);
    return kind.read(response);
  }
}

abstract class ResponseContentType {
  static from(contentType: string | null): ResponseContentType {
    const header = ResponseContentTypeHeader.from(contentType);
    if (!header.isPresent) return NoResponseContent.instance;
    if (header.isJson) return JsonResponseContent.instance;
    if (header.isText) return TextResponseContent.instance;
    return NoResponseContent.instance;
  }

  abstract read(response: Response): Promise<HttpResponseBody>;
}

class ResponseContentTypeHeader {
  private constructor(private readonly mediaType: string | undefined) {}

  static from(value: string | null): ResponseContentTypeHeader {
    if (value === null || value.length === 0) {
      return new ResponseContentTypeHeader(undefined);
    }

    return new ResponseContentTypeHeader(ResponseMediaType.from(value).value);
  }

  get isPresent(): boolean {
    return this.mediaType !== undefined;
  }

  get isJson(): boolean {
    if (this.mediaType === undefined) return false;

    return this.mediaType === "application/json" || this.mediaType.endsWith("+json");
  }

  get isText(): boolean {
    if (this.mediaType === undefined) return false;

    return this.mediaType.startsWith("text/") || this.mediaType.includes("html");
  }
}

class ResponseMediaType {
  private constructor(readonly value: string) {}

  static from(contentType: string): ResponseMediaType {
    return new ResponseMediaType(contentType.split(";")[0]?.trim().toLowerCase() ?? "");
  }
}

class JsonResponseContent extends ResponseContentType {
  static readonly instance = new JsonResponseContent();

  async read(response: Response): Promise<HttpResponseBody> {
    const textBody = await response.text();
    return JsonResponseText.from(textBody).toResponseBody();
  }
}

class JsonResponseText {
  private constructor(private readonly value: string) {}

  static from(value: string): JsonResponseText {
    return new JsonResponseText(value);
  }

  toResponseBody(): HttpResponseBody {
    const bodyIsEmpty = this.value.trim().length === 0;
    if (bodyIsEmpty) return CapturedResponseBody.missing();

    return CapturedResponseBody.from(JSON.parse(this.value));
  }
}

class TextResponseContent extends ResponseContentType {
  static readonly instance = new TextResponseContent();

  async read(response: Response): Promise<HttpResponseBody> {
    const textBody = await response.text();
    return CapturedResponseBody.from(textBody);
  }
}

class NoResponseContent extends ResponseContentType {
  static readonly instance = new NoResponseContent();

  async read(): Promise<HttpResponseBody> {
    return CapturedResponseBody.missing();
  }
}

class ResponseRouter {
  constructor(
    private readonly handlers: ResponseHandler[],
    private readonly status: RequestOutcomeStatus,
    private readonly plan: Plan,
    private readonly context: ExecContext,
  ) {}

  async run(): Promise<void> {
    const selected = ResponseHandlerSelection.from(this.handlers, this.status);
    await selected.route(this.plan, this.context);
  }
}

abstract class ResponseHandlerSelection {
  static from(handlers: ResponseHandler[], status: RequestOutcomeStatus): ResponseHandlerSelection {
    const exactStatusHandler = handlers.find(handlerMatchesStatus(status));
    if (exactStatusHandler) return new MatchedResponseHandler(exactStatusHandler);

    const anyStatusHandler = handlers.find(handlerMatchesAnyStatus);
    if (anyStatusHandler) return new MatchedResponseHandler(anyStatusHandler);

    return new UnmatchedResponseStatus(status, handlers.length);
  }

  abstract route(plan: Plan, context: ExecContext): Promise<void>;
}

class MatchedResponseHandler extends ResponseHandlerSelection {
  constructor(private readonly handler: ResponseHandler) {
    super();
  }

  async route(plan: Plan, context: ExecContext): Promise<void> {
    await executeReaction(this.handler.reaction, plan, context);
  }
}

class UnmatchedResponseStatus extends ResponseHandlerSelection {
  constructor(
    private readonly status: RequestOutcomeStatus,
    private readonly handlerCount: number,
  ) {
    super();
  }

  async route(): Promise<void> {
    if (this.handlerCount === 0) return;

    log.warn("response.unhandled", {
      status: this.status.forLog(),
      handlerCount: this.handlerCount,
    });
  }
}

function handlerMatchesStatus(status: RequestOutcomeStatus): (handler: ResponseHandler) => boolean {
  return handler => {
    const match = handler.match;
    const handlerTargetsExactStatus = match.kind === "status";
    return handlerTargetsExactStatus && status.matchesExact(match.status);
  };
}

function handlerMatchesAnyStatus(handler: ResponseHandler): boolean {
  return handler.match.kind === "any";
}

export async function routeHandlers(
  handlers: ResponseHandler[],
  status: number,
  plan: Plan,
  ctx?: ExecContext,
): Promise<void> {
  await new ResponseRouter(handlers, RequestOutcomeStatus.http(status), plan, RequestContext.start(ctx).current).run();
}
