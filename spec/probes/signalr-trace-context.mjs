import { createRequire } from "node:module";
import { createServer } from "node:http";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";

const require = createRequire(import.meta.url);
const { chromium } = require(process.env.PLAYWRIGHT_PACKAGE);
const WebSocket = require("ws");
const signalr = dirname(require.resolve("@microsoft/signalr/package.json"));
const version = JSON.parse(readFileSync(join(signalr, "package.json"), "utf8")).version;
const bundle = readFileSync(join(signalr, "dist", "browser", "signalr.js"), "utf8");

const separator = "\u001e";
const received = [];
const outgoing = [];
let waiting = null;

const traceHeadersOf = request => ({
  traceparent: request.headers["traceparent"] ?? "",
  tracestate: request.headers["tracestate"] ?? "",
  baggage: request.headers["baggage"] ?? "",
});

const record = (transport, kind, body, request) => received.push({ transport, kind, body, ...traceHeadersOf(request) });

const replies = body => body.split(separator).filter(Boolean).map(frame => JSON.parse(frame)).flatMap(message => {
  if ("protocol" in message) return [{}];
  if (message.type === 1) return [{ type: 3, invocationId: message.invocationId, result: message.arguments[0] }];
  return [];
}).map(message => JSON.stringify(message) + separator);

const deliver = () => {
  if (waiting && outgoing.length) {
    const response = waiting;
    waiting = null;
    response.writeHead(200, { "Content-Type": "text/plain" });
    response.end(outgoing.splice(0).join(""));
  }
};

const negotiation = transport => JSON.stringify({
  negotiateVersion: 1,
  connectionId: transport,
  connectionToken: transport + "-token",
  availableTransports: [{ transport, transferFormats: ["Text", "Binary"] }],
});

const page = `<!doctype html><html><body><main id="probe">
<button id="poll">Invoke by long polling</button><output id="poll-out"></output>
<button id="socket">Invoke by WebSockets</button><output id="socket-out"></output>
<button id="headers">Invoke by WebSockets with a header</button><output id="headers-out"></output>
</main><script src="/signalr.js"></script><script>
const invoke = (path, transport, shown, headers) => async () => {
  const connection = new signalR.HubConnectionBuilder().withUrl(path, headers ? { transport, headers } : { transport }).build();
  try {
    await connection.start();
    const result = await connection.invoke("Echo", 7);
    await connection.stop();
    document.getElementById(shown).textContent = "invoked " + result;
  } catch (error) {
    document.getElementById(shown).textContent = "error " + error.message;
  }
};
document.getElementById("poll").addEventListener("click", invoke("/poll-hub", signalR.HttpTransportType.LongPolling, "poll-out"));
document.getElementById("socket").addEventListener("click", invoke("/socket-hub", signalR.HttpTransportType.WebSockets, "socket-out"));
document.getElementById("headers").addEventListener("click", invoke("/headers-hub", signalR.HttpTransportType.WebSockets, "headers-out",
  { traceparent: "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01" }));
</script></body></html>`;

const pollKind = request => {
  if (request.url.startsWith("/poll-hub/negotiate")) return "negotiate";
  if (request.method === "GET") return "poll";
  if (request.method === "POST") return "send";
  return "close";
};

const server = createServer((request, response) => {
  const chunks = [];
  request.on("data", chunk => chunks.push(chunk));
  request.on("end", () => {
    const body = Buffer.concat(chunks).toString("utf8");
    if (request.url === "/") {
      response.writeHead(200, { "Content-Type": "text/html" });
      response.end(page);
      return;
    }
    if (request.url === "/signalr.js") {
      response.writeHead(200, { "Content-Type": "text/javascript" });
      response.end(bundle);
      return;
    }
    if (request.url.startsWith("/socket-hub/negotiate") || request.url.startsWith("/headers-hub/negotiate")) {
      record(request.url.startsWith("/headers-hub") ? "Headers" : "WebSockets", "negotiate", body, request);
      response.writeHead(200, { "Content-Type": "application/json" });
      response.end(negotiation("WebSockets"));
      return;
    }
    const kind = pollKind(request);
    record("LongPolling", kind, body, request);
    if (kind === "negotiate") {
      response.writeHead(200, { "Content-Type": "application/json" });
      response.end(negotiation("LongPolling"));
      return;
    }
    if (kind === "poll") {
      if (received.filter(entry => entry.kind === "poll").length === 1) {
        response.writeHead(200, { "Content-Type": "text/plain" });
        response.end("");
        return;
      }
      waiting = response;
      deliver();
      return;
    }
    if (kind === "send") {
      response.writeHead(200, { "Content-Type": "text/plain" });
      response.end("");
      outgoing.push(...replies(body));
      deliver();
      return;
    }
    if (waiting) {
      waiting.writeHead(204);
      waiting.end();
      waiting = null;
    }
    response.writeHead(202);
    response.end("");
  });
});

const sockets = new WebSocket.Server({ noServer: true });
server.on("upgrade", (request, socket, head) => sockets.handleUpgrade(request, socket, head, upgraded => sockets.emit("connection", upgraded, request)));
sockets.on("connection", (socket, request) => {
  const transport = request.url.startsWith("/headers-hub") ? "Headers" : "WebSockets";
  record(transport, "upgrade", "", request);
  socket.on("message", data => {
    const body = data.toString();
    received.push({ transport, kind: "message", body, traceparent: "", tracestate: "", baggage: "" });
    for (const reply of replies(body)) socket.send(reply);
  });
});

const sent = transport => ["negotiate", "poll", "send", "close", "upgrade", "message"]
  .filter(kind => received.some(entry => entry.transport === transport && entry.kind === kind)).join(", ");

const invocationKeys = transport => {
  const invocation = received
    .filter(entry => entry.transport === transport && entry.body)
    .flatMap(entry => entry.body.split(separator).filter(Boolean).map(frame => JSON.parse(frame)))
    .find(message => message.type === 1);
  return invocation ? Object.keys(invocation).sort().join(",") : "none";
};

server.listen(0, "127.0.0.1", async () => {
  const origin = `http://127.0.0.1:${server.address().port}`;
  const browser = await chromium.launch();
  const browserPage = await browser.newPage();
  await browserPage.goto(origin + "/");
  await browserPage.click("#poll");
  await browserPage.waitForFunction(() => document.getElementById("poll-out").textContent !== "");
  await browserPage.click("#socket");
  await browserPage.waitForFunction(() => document.getElementById("socket-out").textContent !== "");
  const shown = `${await browserPage.textContent("#poll-out")} and ${await browserPage.textContent("#socket-out")}`;
  await browserPage.click("#headers");
  await browserPage.waitForFunction(() => document.getElementById("headers-out").textContent !== "");
  const carrying = received.filter(entry => entry.transport !== "Headers" && (entry.traceparent || entry.tracestate || entry.baggage)).length;
  const reached = ["negotiate", "upgrade"].map(kind => `${kind} ${received.some(entry => entry.transport === "Headers" && entry.kind === kind && entry.traceparent) ? "with" : "without"} it`).join(", ");

  console.log(`signalr-requests-carry-no-trace-context: @microsoft/signalr ${version} sent ${sent("LongPolling")} by long polling and ${sent("WebSockets")} by WebSockets; ${carrying} carrying a trace header; ${shown}`);
  console.log(`signalr-url-headers-skip-the-upgrade: a traceparent given to withUrl reaches the WebSocket transport's ${reached}; ${await browserPage.textContent("#headers-out")}`);
  console.log(`signalr-invocation-message-keys: @microsoft/signalr ${version} invokes with ${invocationKeys("LongPolling")} by long polling and ${invocationKeys("WebSockets")} by WebSockets`);

  await browser.close();
  sockets.close();
  server.close();
});
