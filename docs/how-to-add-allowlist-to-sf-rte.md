# How to Add an HTML Allowlist to Syncfusion RichTextEditor

SF RTE only provides `deniedTags` (blacklist). A blacklist is fundamentally insecure —
new HTML elements, custom elements, and encoding tricks bypass it. This guide implements
a **whitelist** (allowlist) approach using DOMPurify on the client and HtmlSanitizer on the server.

## Architecture

```
Client (UX convenience)                 Server (security guarantee)
┌──────────────────────┐                ┌──────────────────────────┐
│ Layer 1: Paste patch │──on paste───►  │                          │
│ Layer 2: Change event│──on edit────►  │ FluentValidation rule    │
│ Layer 3: Clean button│──on click───►  │ + HtmlSanitizer (Ganss)  │
└──────────────────────┘                └──────────────────────────┘
        DOMPurify 3.x                         AngleSharp parser
```

Client layers prevent disallowed HTML from appearing in the editor.
Server validation rejects it on submit — **the only layer that matters for security**.

## Dependencies

**Client:** [DOMPurify](https://github.com/cure53/DOMPurify) 3.x (15KB gzipped, all modern browsers)

**Server:** [HtmlSanitizer](https://github.com/mganss/HtmlSanitizer) 9.x + [FluentValidation](https://github.com/FluentValidation/FluentValidation) 12.x

```bash
dotnet add package HtmlSanitizer
dotnet add package FluentValidation
```

## Client-Side: `rte-html-whitelist.ts`

Load after Syncfusion and DOMPurify scripts. See `scripts/rte-html-whitelist.ts` for
the production-ready source.

### What each layer does

| Layer | Trigger | Covers | How |
|-------|---------|--------|-----|
| Paste override | Automatic on Ctrl+V | Paste from clipboard | Patches `PasteCleanupAction.prototype.deniedTags` to run DOMPurify instead of SF's deny-list |
| Change listener | Automatic on content change | Source code view toggle | Additive `addEventListener` — doesn't block app-layer change handlers |
| Clean button | User clicks toolbar button | Manual on-demand cleanup | Custom toolbar item calls `DOMPurify.sanitize()` on `getHtml()` |

### Usage

```typescript
import { applyHtmlWhitelist } from './rte-html-whitelist';

// After creating the RTE instance:
const rte = new ej.richtexteditor.RichTextEditor({ /* your config */ });
rte.appendTo('#editor');

applyHtmlWhitelist(rte, {
    allowedTags: ['p','br','strong','em','b','i','u','ul','ol','li','a','span',
                   'h1','h2','h3','sub','sup'],
    allowedAttr: ['href','alt'],
    allowImages: true,  // allows <img> only if src starts with /
});
```

### Custom toolbar button

Add this to your `toolbarSettings.items`:

```javascript
{
    tooltipText: 'Clean HTML',
    template: '<button class="e-tbar-btn e-btn">Clean</button>',
}
```

Then in `toolbarClick`:

```javascript
toolbarClick: (args) => {
    if (args.item?.tooltipText === 'Clean HTML') {
        cleanEditorContent(rte);  // exported from rte-html-whitelist
    }
}
```

### SF RTE attack surface

| Input method | Possible? | Protected by |
|---|---|---|
| Paste (Ctrl+V) | Yes | Paste override (DOMPurify) |
| Source code view | Yes | Change listener (DOMPurify) |
| Drag & drop HTML | No — SF blocks it | N/A |
| Typing raw tags | No — contentEditable | N/A |
| Programmatic `setHtml()` | Dev-only | Change listener + Clean button |

### Why DOMPurify, not custom DOM walking

A hand-rolled `querySelectorAll('*')` loop misses:
- **mXSS** (mutation XSS) — parser/serializer disagreements
- **Encoding tricks** — nested entities, UTF-7, double encoding
- **Malformed HTML** — unclosed tags, tag soup edge cases

DOMPurify is battle-tested against all of these. 60M+ weekly npm downloads.

## Server-Side: `HtmlWhitelistValidator.cs`

See `scripts/HtmlWhitelistValidator.cs` for the production-ready source.

### Usage

```csharp
public class CarePlanValidator : AbstractValidator<CarePlanModel>
{
    public CarePlanValidator()
    {
        RuleFor(x => x.CarePlan).NotEmpty().HtmlWhitelist();
        RuleFor(x => x.Notes).HtmlWhitelist(allowImages: true);
    }
}
```

The `.HtmlWhitelist()` extension works on any `string` property. It:
1. Parses the HTML with AngleSharp (W3C-compliant)
2. Sanitizes using the same tag/attribute allowlist
3. Compares sanitized to original — different = **validation failure**

### Image `src` restriction

When `allowImages: true`:
- `/images/photo.jpg` — **allowed** (starts with `/`)
- `/uploads/2026/care-plan.png` — **allowed** (starts with `/`)
- `https://evil.com/track.gif` — **rejected**
- `data:image/png;base64,...` — **rejected**
- `//cdn.evil.com/img.jpg` — **rejected** (protocol-relative)
- `images/relative.jpg` — **rejected** (no leading `/`)

### Why HtmlSanitizer, not custom parsing

Same as DOMPurify on the client — battle-tested W3C parser (AngleSharp) handles all edge
cases. Compare-mode (sanitize then compare) is the correct validation pattern.

## Proven in playground

The `playground/` directory contains a working proof-of-concept:
- `rte-whitelist.html` — full interactive test page with 6 paste samples
- `HtmlValidator/` — .NET 10 console app running all 21 validation test cases

Both use the exact same allowlist. Client and server agree on what's allowed.
