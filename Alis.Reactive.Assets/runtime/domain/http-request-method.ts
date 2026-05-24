import type { HttpMethod } from "../types";
import { assertNever } from "../core/assert-never";

export class HttpRequestMethod {
  private constructor(readonly value: HttpMethod) {}

  static from(value: HttpMethod): HttpRequestMethod {
    return new HttpRequestMethod(value);
  }

  sendsInputInQueryString(): boolean {
    switch (this.value) {
      case "GET":
        return true;
      case "POST":
      case "PUT":
      case "DELETE":
      case "PATCH":
        return false;
      default:
        return assertNever(this.value, "HTTP method");
    }
  }

  acceptsRequestBody(): boolean {
    return !this.sendsInputInQueryString();
  }
}
