// The server, as a test stands in for it: every request is kept, and each path answered as the test says.

import { vi } from "vitest";

export interface Sent {
  readonly url: string;
  readonly method: string;
  readonly body: unknown;
}

export interface Answer {
  readonly json?: unknown;
  readonly html?: string;
  readonly status?: number;
}

/** Answers each path with what the test says, anything else with an empty 200, and keeps what was sent. */
export function server(answers: Record<string, Answer> = {}): Sent[] {
  const sent: Sent[] = [];
  vi.stubGlobal("fetch", vi.fn(async (url: string, init: RequestInit = {}) => {
    sent.push({ url, method: init.method ?? "GET", body: typeof init.body === "string" ? JSON.parse(init.body) : init.body });
    const answer = answers[url.split("?")[0]!] ?? {};
    const headers = new Headers();
    let body: string | null = null;
    if (answer.json !== undefined) {
      headers.set("Content-Type", "application/json");
      body = JSON.stringify(answer.json);
    } else if (answer.html !== undefined) {
      headers.set("Content-Type", "text/html");
      body = answer.html;
    }
    return new Response(body, { status: answer.status ?? 200, headers });
  }));
  return sent;
}
