// http.ts - HTTP request execution using V3 Request type.
// Uses the shared value/gather/runtime concepts and keeps HTTP async isolated.

import type { Request, ResponseHandler, Plan, ExecContext, Reaction, ValueProducer } from "../types";
import { resolveGather, type GatherResult } from "./gather";
import { executeReaction } from "./execute";
import { validateContainer } from "../validation";
import { evaluateValue } from "../core/evaluate";
import { toString as convertToString } from "../core/shape-convert";
import { resolveRouteParams } from "../core/url-template";
import { scope } from "../core/trace";
import { isMissingRuntimeValue } from "../domain/runtime-value";
import { ExecutionContext } from "../domain/execution-context";
import { RuntimeShape } from "../domain/runtime-shape";
import { HttpRequestMethod } from "../domain/http-request-method";

const log = scope("http");

interface ResolvedFetch {
  readonly url: string;
  readonly init: RequestInit;
}

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

    await RequestReactionSequence.run(this.request.before, this.plan, this.context.current);
    await RequestDispatchAttempt.prepare(this.request, this.plan, this.context).dispatch();
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

class PreparedHttpRequest {
  private constructor(
    readonly fetch: ResolvedFetch,
    readonly context: RequestContext,
  ) {}

  static from(request: Request, plan: Plan, context: RequestContext): PreparedHttpRequest {
    const gathered = resolveGather(request.input, request.method, plan, context.current);
    const requestContext = context.withRequest(gathered);
    const fetch = HttpFetchBuilder
      .for(request, plan, context.current)
      .build(gathered);

    return new PreparedHttpRequest(fetch, requestContext);
  }
}

abstract class RequestDispatchAttempt {
  static prepare(request: Request, plan: Plan, context: RequestContext): RequestDispatchAttempt {
    try {
      return new PreparedRequestDispatch(request, plan, PreparedHttpRequest.from(request, plan, context));
    } catch (err) {
      return new FailedRequestPreparation(request, plan, context, err);
    }
  }

  abstract dispatch(): Promise<void>;
}

class PreparedRequestDispatch extends RequestDispatchAttempt {
  constructor(
    private readonly request: Request,
    private readonly plan: Plan,
    private readonly prepared: PreparedHttpRequest,
  ) {
    super();
  }

  async dispatch(): Promise<void> {
    const responsePlan = new HttpResponsePlan(this.request, this.plan, this.prepared.context);
    const outcome = await this.exchangeOutcome();
    await outcome.routeWith(responsePlan);
  }

  private async exchangeOutcome(): Promise<HttpExchangeOutcome> {
    try {
      return await HttpExchange.from(this.request, this.prepared.fetch).send();
    } catch (err) {
      return HttpExchangeOutcome.fromClientFailure(this.request, err);
    }
  }
}

class FailedRequestPreparation extends RequestDispatchAttempt {
  constructor(
    private readonly request: Request,
    private readonly plan: Plan,
    private readonly context: RequestContext,
    private readonly error: unknown,
  ) {
    super();
  }

  async dispatch(): Promise<void> {
    const outcome = HttpExchangeOutcome.fromClientFailure(this.request, this.error);
    await outcome.routeWith(new HttpResponsePlan(this.request, this.plan, this.context));
  }
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
    await RequestFollowUp.from(this.request.chain).run(this.plan, this.context);
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
    await RequestReactionSequence.run(this.request.complete, this.plan, this.context.current);
  }
}

abstract class RequestFollowUp {
  static from(chain: Request["chain"]): RequestFollowUp {
    const requestHasFollowUp = chain.kind === "follow-up";
    if (!requestHasFollowUp) return TerminalRequest.instance;

    return new ChainedRequest(chain.next);
  }

  abstract run(plan: Plan, context: RequestContext): Promise<void>;
}

class TerminalRequest extends RequestFollowUp {
  static readonly instance = new TerminalRequest();

  async run(): Promise<void> {
    return;
  }
}

class ChainedRequest extends RequestFollowUp {
  constructor(private readonly next: Request) {
    super();
  }

  async run(plan: Plan, context: RequestContext): Promise<void> {
    await new HttpRequestExecution(this.next, plan, context).run();
  }
}

class RequestReactionSequence {
  static async run(reactions: readonly Reaction[], plan: Plan, context: ExecContext): Promise<void> {
    for (const reaction of reactions) {
      await executeReaction(reaction, plan, context);
    }
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

class HttpFetchBuilder {
  private constructor(
    private readonly request: Request,
    private readonly plan: Plan,
    private readonly context: ExecContext,
  ) {}

  static for(request: Request, plan: Plan, context: ExecContext): HttpFetchBuilder {
    return new HttpFetchBuilder(request, plan, context);
  }

  build(gathered: GatherResult): ResolvedFetch {
    const url = RequestUrlBuilder
      .for(this.request, this.plan, this.context)
      .withQuery(gathered.urlParams);
    const init = new RequestInitBuilder(this.request, this.plan, this.context)
      .withBody(gathered)
      .withHeaders()
      .build();

    return { url, init };
  }
}

class RequestUrlBuilder {
  static for(request: Request, plan: Plan, context: ExecContext): RequestUrlBuilder {
    return new RequestUrlBuilder(request, plan, context);
  }

  private constructor(
    private readonly request: Request,
    private readonly plan: Plan,
    private readonly context: ExecContext,
  ) {}

  withQuery(urlParams: string[]): string {
    const url = this.withRouteParams();
    if (urlParams.length === 0) return url;

    return url + QueryStringSeparator.for(url).value + urlParams.join("&");
  }

  private withRouteParams(): string {
    return resolveRouteParams(this.request.url, this.request.routeParams, this.plan, this.context);
  }
}

class QueryStringSeparator {
  private constructor(readonly value: "?" | "&") {}

  static for(url: string): QueryStringSeparator {
    const queryStringAlreadyStarted = url.includes("?");
    if (queryStringAlreadyStarted) return new QueryStringSeparator("&");

    return new QueryStringSeparator("?");
  }
}

class RequestInitBuilder {
  private readonly init: RequestInit;

  constructor(
    private readonly request: Request,
    private readonly plan: Plan,
    private readonly context: ExecContext,
  ) {
    this.init = { method: request.method };
  }

  withBody(gathered: GatherResult): RequestInitBuilder {
    RequestBody.from(this.request, gathered).applyTo(this.init, this.headers);
    return this;
  }

  withHeaders(): RequestInitBuilder {
    for (const [name, producer] of Object.entries(this.request.headers)) {
      RuntimeHeaderResolution
        .from(name, producer, this.plan, this.context)
        .applyTo(this.headers);
    }

    return this;
  }

  build(): RequestInit {
    const headers = this.headers.toRecord();
    if (Object.keys(headers).length > 0) this.init.headers = headers;
    return this.init;
  }

  private get headers(): RuntimeHeaders {
    return RuntimeHeaders.for(this.init);
  }
}

abstract class RequestBody {
  static from(request: Request, gathered: GatherResult): RequestBody {
    const requestMethod = HttpRequestMethod.from(request.method);
    if (!requestMethod.acceptsRequestBody()) return NoRequestBody.instance;

    const gatheredBody = gathered.body;
    if (gatheredBody instanceof FormData) return new MultipartRequestBody(gatheredBody);

    return JsonRequestBody.fromJsonBody(gatheredBody);
  }

  abstract applyTo(init: RequestInit, headers: RuntimeHeaders): void;
}

class NoRequestBody extends RequestBody {
  static readonly instance = new NoRequestBody();

  applyTo(_init: RequestInit, _headers: RuntimeHeaders): void {
    return;
  }
}

class MultipartRequestBody extends RequestBody {
  constructor(private readonly body: FormData) {
    super();
  }

  applyTo(init: RequestInit): void {
    init.body = this.body;
  }
}

class JsonRequestBody extends RequestBody {
  private constructor(private readonly body: Record<string, unknown>) {
    super();
  }

  static fromJsonBody(body: Record<string, unknown>): RequestBody {
    const bodyHasFields = Object.keys(body).length > 0;
    if (!bodyHasFields) return NoRequestBody.instance;

    return new JsonRequestBody(body);
  }

  applyTo(init: RequestInit, headers: RuntimeHeaders): void {
    headers.set("Content-Type", "application/json");
    init.body = JSON.stringify(this.body);
  }
}

class RuntimeHeaders {
  private constructor(private readonly headers: Record<string, string>) {}

  static for(init: RequestInit): RuntimeHeaders {
    const existing = init.headers as Record<string, string> | undefined;
    if (existing !== undefined) return new RuntimeHeaders(existing);

    const headers: Record<string, string> = {};
    init.headers = headers;
    return new RuntimeHeaders(headers);
  }

  set(name: string, value: string): void {
    this.headers[name] = value;
  }

  toRecord(): Record<string, string> {
    return this.headers;
  }
}

abstract class RuntimeHeaderResolution {
  static from(name: string, producer: ValueProducer, plan: Plan, context: ExecContext): RuntimeHeaderResolution {
    const value = evaluateValue(producer, plan, context);
    if (isMissingRuntimeValue(value)) return MissingRuntimeHeader.instance;

    return PresentRuntimeHeader.from(name, producer, value);
  }

  abstract applyTo(headers: RuntimeHeaders): void;
}

class MissingRuntimeHeader extends RuntimeHeaderResolution {
  static readonly instance = new MissingRuntimeHeader();

  applyTo(): void {
    return;
  }
}

class PresentRuntimeHeader extends RuntimeHeaderResolution {
  private constructor(
    private readonly name: string,
    private readonly value: string,
  ) {
    super();
  }

  static from(name: string, producer: ValueProducer, value: unknown): PresentRuntimeHeader {
    return new PresentRuntimeHeader(name, RequestHeaderWireValue.from(name, producer, value));
  }

  applyTo(headers: RuntimeHeaders): void {
    headers.set(this.name, this.value);
  }
}

class RequestHeaderWireValue {
  static from(name: string, producer: ValueProducer, value: unknown): string {
    const wireValue = RuntimeShape.declaredBy(producer).formatForWire(value);
    const text = convertToString(wireValue);
    if (text.ok) return text.value;

    throw new Error(`[alis] header "${name}" cannot be serialized as a scalar: ${text.error}`);
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
