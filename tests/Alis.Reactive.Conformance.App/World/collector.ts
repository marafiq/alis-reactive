const spans = document.getElementById("spans");
if (spans !== null) {
  new EventSource("/world/collector/lines" + location.search).addEventListener("message", (message) => {
    if (typeof message.data === "string") spans.innerHTML = message.data;
  });
}
