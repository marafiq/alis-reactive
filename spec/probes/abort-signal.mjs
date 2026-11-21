import { createServer } from "node:http";

const twice = new AbortController();
twice.abort("replace");
twice.abort("dispose");
console.log(`abort-signal-keeps-first-reason: ${twice.signal.reason}`);

let received = 0;
const server = createServer((request, response) => {
  received += 1;
  response.writeHead(200, { "Content-Type": "text/plain" });
  response.end("sent");
});

await new Promise(resolve => server.listen(0, "127.0.0.1", resolve));
const origin = `http://127.0.0.1:${server.address().port}`;
try {
  const aborted = new AbortController();
  aborted.abort("replace");
  const refused = await fetch(`${origin}/save`, { method: "POST", signal: aborted.signal }).then(() => "answered", reason => `rejected with ${reason}`);
  const receivedWhileAborted = received;
  const answered = await fetch(`${origin}/save`, { method: "POST", signal: new AbortController().signal }).then(response => response.text());
  console.log(`fetch-aborted-signal-sends-nothing: ${refused}, received ${receivedWhileAborted}; unaborted ${answered}, received ${received}`);
} finally {
  server.close();
}
