// Plugins this app adds to the Alis.Reactive runtime. They queue on window.__alisPlugins
// before the runtime script loads; the runtime registers them before any plan runs.
(function () {
  var plugins = (window.__alisPlugins = window.__alisPlugins || []);

  // Copy text for the "For developers" snippets. Falls back to a hidden textarea where the
  // async Clipboard API is unavailable (plain http on a LAN host, older browsers).
  function copyWithTextarea(text) {
    var area = document.createElement("textarea");
    area.value = text;
    area.setAttribute("readonly", "");
    area.style.position = "fixed";
    area.style.opacity = "0";
    document.body.appendChild(area);
    area.select();
    try {
      document.execCommand("copy");
    } finally {
      document.body.removeChild(area);
    }
  }

  plugins.push({
    name: "clipboard",
    instance: {
      writeText: function (text) {
        var value = String(text == null ? "" : text);
        if (navigator.clipboard && window.isSecureContext) {
          navigator.clipboard.writeText(value).catch(function () { copyWithTextarea(value); });
          return;
        }
        copyWithTextarea(value);
      }
    }
  });
})();
