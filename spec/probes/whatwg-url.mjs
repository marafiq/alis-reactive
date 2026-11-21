const texts = ["http://a.com/b c", "http://a b.com", "http://", "http://a", "https://exa_mple.com", "http://a.com:99999", "HTTP://A.COM"];
function isHttpUrl(text) {
  try {
    const url = new URL(text);
    return url.protocol === "http:" || url.protocol === "https:";
  } catch {
    return false;
  }
}
console.log("whatwg-url-http: " + texts.map(text => text + " " + isHttpUrl(text)).join("; "));
