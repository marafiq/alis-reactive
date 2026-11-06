const value = "a/b c+d?é";
console.log("js-route-segment-encoding: encodeURIComponent writes " + JSON.stringify(value) + " as " + encodeURIComponent(value));
console.log("js-query-encoding: URLSearchParams writes q=" + JSON.stringify("a b+c&d") + " as " + new URLSearchParams([["q", "a b+c&d"]]).toString());
const outcomes = ["a\nb", " padded ", "é", "€"].map(text => {
  try {
    new Headers().set("X-Note", text);
    return JSON.stringify(text) + " taken";
  } catch (error) {
    return JSON.stringify(text) + " " + error.constructor.name;
  }
});
console.log("js-header-values: " + outcomes.join("; "));
const trimmed = new Headers();
trimmed.set("X-Note", " padded ");
console.log("js-header-trim: a value with outer spaces is sent as " + JSON.stringify(trimmed.get("X-Note")));
