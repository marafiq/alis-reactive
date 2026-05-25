import type { ExecContext, Plan, Request, ValueProducer } from "../types";
import type { GatherResult } from "./gather";
import { evaluateValue } from "../core/evaluate";
import { toString as convertToString } from "../core/shape-convert";
import { resolveRouteParams } from "../core/url-template";
import { HttpRequestMethod } from "../domain/http-request-method";
import { isMissingRuntimeValue } from "../domain/runtime-value";
import { RuntimeShape } from "../domain/runtime-shape";

export interface ResolvedFetch {
  readonly url: string;
  readonly init: RequestInit;
}

export class HttpFetchBuilder {
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
