const escaped = (text) => [...text].map((c) => (c.codePointAt(0) > 126 ? "\\u" + c.codePointAt(0).toString(16).padStart(4, "0") : c)).join("");

const latin = new Uint8Array([0x63, 0x61, 0x66, 0xe9]);
const declared = new Response(latin, { headers: { "Content-Type": "text/plain; charset=windows-1252" } });
console.log("fetch-text-ignores-charset: text() reads " + escaped(await declared.text()) + "; TextDecoder(\"windows-1252\") reads " + escaped(new TextDecoder("windows-1252").decode(latin)));

const form = new FormData();
form.append("Documents[0].Caption", "Front");
form.append("Documents[0].File", new File(["%PDF"], "Résumé \"a\".pdf", { type: "application/pdf" }));
const written = await new Response(form).text();
const disposition = written.split("\r\n").find((line) => line.includes("filename="));
console.log("fetch-multipart-file-name: a File named R\\u00e9sum\\u00e9 \"a\".pdf is written " + escaped(disposition));

const search = new URLSearchParams([["Ward", "North 1+2"], ["__RequestVerificationToken", "t/k="]]);
const urlencoded = new Request("http://a.test/", { method: "POST", body: search });
console.log("fetch-urlencoded-body: a URLSearchParams body is sent as " + urlencoded.headers.get("Content-Type") + " with " + (await urlencoded.text()));

const text = new Request("http://a.test/", { method: "POST", body: "note" });
const blob = new Request("http://a.test/", { method: "POST", body: new Blob(["%PDF"], { type: "application/pdf" }) });
console.log("fetch-body-media-types: a string body is sent as " + text.headers.get("Content-Type") + "; a Blob body as " + blob.headers.get("Content-Type"));

const head = await new Response(null, { status: 200 }).text();
console.log("fetch-null-body-reads-empty: a response with no body reads " + JSON.stringify(head));
