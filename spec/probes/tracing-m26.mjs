import { createRequire } from "node:module";
import { createServer } from "node:http";

const require = createRequire(import.meta.url);
const { chromium } = require(process.env.PLAYWRIGHT_PACKAGE);

const traceparent = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01";

const listening = handler => new Promise(resolve => {
  const server = createServer(handler);
  server.listen(0, "127.0.0.1", () => resolve(server));
});

const originOf = server => `http://127.0.0.1:${server.address().port}`;

const received = [];

const record = (host, request) => received.push({
  host,
  method: request.method,
  path: request.url,
  asks: request.headers["access-control-request-headers"] ?? "",
  traceparent: request.headers["traceparent"] ?? "",
  tracestate: request.headers["tracestate"] ?? "",
  baggage: request.headers["baggage"] ?? "",
});

const page = pharmacy => `<!doctype html><html><body><main id="probe">
<button id="plain">Check pharmacy</button><output id="plain-out"></output>
<button id="traced">Check pharmacy traced</button><output id="traced-out"></output>
<button id="own">Check ward</button><output id="own-out"></output>
</main><script>
const settle = (id, sent) => sent.then(response => { document.getElementById(id).textContent = "status " + response.status; },
  error => { document.getElementById(id).textContent = error.name; });
document.getElementById("plain").addEventListener("click", () => settle("plain-out", fetch("${pharmacy}/plain-stock")));
document.getElementById("traced").addEventListener("click", () => settle("traced-out",
  fetch("${pharmacy}/traced-stock", { headers: { traceparent: "${traceparent}" } })));
document.getElementById("own").addEventListener("click", () => settle("own-out",
  fetch("/census", { headers: { traceparent: "${traceparent}", tracestate: "alis=1", baggage: "http.request.resend_count=1" } })));
</script></body></html>`;

const requestsTo = (host, path) => received.filter(entry => entry.host === host && entry.path === path);

const methods = entries => entries.map(entry => entry.method).join(",") || "none";

(async () => {
  let pageOrigin = "";
  const pharmacy = await listening((request, response) => {
    record("pharmacy", request);
    response.setHeader("Access-Control-Allow-Origin", pageOrigin);
    if (request.method === "OPTIONS") {
      response.writeHead(204);
      response.end();
      return;
    }
    response.writeHead(200, { "Content-Type": "text/plain" });
    response.end("12");
  });
  const ward = await listening((request, response) => {
    record("ward", request);
    if (request.url === "/") {
      response.writeHead(200, { "Content-Type": "text/html" });
      response.end(page(originOf(pharmacy)));
      return;
    }
    response.writeHead(200, { "Content-Type": "text/plain" });
    response.end("12");
  });
  pageOrigin = originOf(ward);
  const browser = await chromium.launch();
  const browserPage = await browser.newPage();
  await browserPage.goto(pageOrigin + "/");

  await browserPage.click("#plain");
  await browserPage.waitForFunction(() => document.getElementById("plain-out").textContent !== "");
  const plain = requestsTo("pharmacy", "/plain-stock");
  console.log(`browser-cross-origin-plain-get-no-preflight: ${methods(plain)}; fetch ${await browserPage.textContent("#plain-out")}`);

  await browserPage.click("#traced");
  await browserPage.waitForFunction(() => document.getElementById("traced-out").textContent !== "");
  const traced = requestsTo("pharmacy", "/traced-stock");
  const asked = traced.filter(entry => entry.method === "OPTIONS").map(entry => entry.asks).join(",");
  console.log(`browser-cross-origin-traceparent-preflight: ${methods(traced)} asking ${asked || "nothing"}; fetch ${await browserPage.textContent("#traced-out")}`);

  await browserPage.click("#own");
  await browserPage.waitForFunction(() => document.getElementById("own-out").textContent !== "");
  const own = requestsTo("ward", "/census");
  const heard = own.filter(entry => entry.method === "GET").map(entry => `traceparent ${entry.traceparent}, tracestate ${entry.tracestate}, baggage ${entry.baggage}`).join("; ");
  console.log(`browser-same-origin-trace-headers-sent: ${methods(own)} with ${heard}; fetch ${await browserPage.textContent("#own-out")}`);

  await browser.close();
  pharmacy.close();
  ward.close();
})();
