/**
 * rte-html-whitelist.ts
 *
 * HTML allowlist enforcement for Syncfusion EJ2 RichTextEditor using DOMPurify.
 * Load after Syncfusion and DOMPurify scripts.
 *
 * Three layers:
 *   1. Paste override  — patches SF's deny-list prototype to run DOMPurify allowlist
 *   2. Change listener — additive listener catches source-view edits (doesn't block app events)
 *   3. Clean button    — on-demand sanitization via toolbar button
 *
 * Dependency: DOMPurify 3.x (https://github.com/cure53/DOMPurify)
 */

declare const DOMPurify: {
  sanitize(html: string, config: Record<string, unknown>): string;
  addHook(hook: string, cb: (node: Element, data: Record<string, unknown>) => void): void;
  removeAllHooks(): void;
};

declare const ej: {
  richtexteditor: {
    PasteCleanupAction: { prototype: { deniedTags: (el: HTMLElement) => HTMLElement } };
  };
};

// ─── Public API ─────────────────────────────────────────────────────────────

export interface WhitelistConfig {
  /** Tags to allow (lowercase). Default: formatting + structure tags. */
  allowedTags?: string[];
  /** Attributes to allow. Default: ['href', 'alt']. */
  allowedAttr?: string[];
  /** Allow <img> only if src starts with /. Default: false. */
  allowImages?: boolean;
}

const DEFAULT_TAGS = [
  "p", "br", "strong", "em", "b", "i", "u",
  "ul", "ol", "li", "a", "span",
  "h1", "h2", "h3", "h4", "h5", "h6",
  "sub", "sup",
];

const DEFAULT_ATTR = ["href", "alt"];

/**
 * Apply HTML allowlist enforcement to a Syncfusion RichTextEditor instance.
 * Call once after `rte.appendTo()`.
 */
export function applyHtmlWhitelist(rte: any, config?: WhitelistConfig): void {
  const purifyConfig = buildPurifyConfig(config);

  patchPastePrototype(purifyConfig);
  addChangeListener(rte, purifyConfig);
}

/**
 * Sanitize the editor's current content. Call from a custom toolbar button handler.
 * Returns true if content was modified.
 */
export function cleanEditorContent(rte: any, config?: WhitelistConfig): boolean {
  const html = rte.getHtml();
  if (!html) return false;

  const purifyConfig = buildPurifyConfig(config);
  const clean = DOMPurify.sanitize(html, purifyConfig);

  if (clean !== html) {
    rte.value = clean;
    rte.dataBind();
    return true;
  }
  return false;
}

// ─── Internal ───────────────────────────────────────────────────────────────

interface PurifyConfig {
  ALLOWED_TAGS: string[];
  ALLOWED_ATTR: string[];
  ALLOW_ARIA_ATTR: false;
  ALLOW_DATA_ATTR: false;
}

function buildPurifyConfig(config?: WhitelistConfig): PurifyConfig {
  const tags = [...(config?.allowedTags ?? DEFAULT_TAGS)];
  const attr = [...(config?.allowedAttr ?? DEFAULT_ATTR)];

  if (config?.allowImages) {
    if (!tags.includes("img")) tags.push("img");
    if (!attr.includes("src")) attr.push("src");
    registerImageHook();
  }

  return {
    ALLOWED_TAGS: tags,
    ALLOWED_ATTR: attr,
    ALLOW_ARIA_ATTR: false,
    ALLOW_DATA_ATTR: false,
  };
}

let imageHookRegistered = false;

function registerImageHook(): void {
  if (imageHookRegistered) return;
  imageHookRegistered = true;

  DOMPurify.addHook("uponSanitizeAttribute", (node, data) => {
    if (node.tagName !== "IMG" || (data as any).attrName !== "src") return;
    const src = ((data as any).attrValue || "").trim();
    const isLocalPath = src.startsWith("/") && !src.startsWith("//") && src.length > 1;
    if (!isLocalPath) {
      node.remove();
      (data as any).keepAttr = false;
    }
  });
}

// ─── Layer 1: Paste override ────────────────────────────────────────────────

let pastePatched = false;

function patchPastePrototype(purifyConfig: PurifyConfig): void {
  if (pastePatched) return;
  pastePatched = true;

  const proto = ej.richtexteditor.PasteCleanupAction.prototype;
  const original = proto.deniedTags;

  proto.deniedTags = function (clipBoardElem: HTMLElement): HTMLElement {
    const before = clipBoardElem.innerHTML;
    const cleaned = DOMPurify.sanitize(before, purifyConfig);
    if (cleaned !== before) {
      clipBoardElem.innerHTML = cleaned;
    }
    return clipBoardElem;
  };
}

// ─── Layer 2: Change listener (additive) ────────────────────────────────────

function addChangeListener(rte: any, purifyConfig: PurifyConfig): void {
  rte.addEventListener("change", () => {
    const html = rte.getHtml();
    if (!html) return;
    const clean = DOMPurify.sanitize(html, purifyConfig);
    if (clean !== html) {
      rte.value = clean;
      rte.dataBind();
    }
  });
}
