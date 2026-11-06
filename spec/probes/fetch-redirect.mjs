import { createServer } from "node:http";

const server = createServer((request, response) => {
  if (request.url === "/save") {
    response.writeHead(302, { Location: "/list" });
    response.end();
    return;
  }
  response.writeHead(200, { "Content-Type": "text/html" });
  response.end("<ul><li>saved</li></ul>");
});

await new Promise(resolve => server.listen(0, "127.0.0.1", resolve));
const origin = `http://127.0.0.1:${server.address().port}`;
try {
  const followed = await fetch(`${origin}/save`, { method: "POST" });
  const path = new URL(followed.url).pathname;
  console.log(`fetch-follows-relative-redirect: status ${followed.status}, redirected ${followed.redirected}, ended at ${path}, ${await followed.text()}`);
  const refused = await fetch(`${origin}/save`, { method: "POST", redirect: "error" }).then(() => "answered", error => error.name);
  console.log(`fetch-redirect-error-mode: ${refused}`);
} finally {
  server.close();
}
