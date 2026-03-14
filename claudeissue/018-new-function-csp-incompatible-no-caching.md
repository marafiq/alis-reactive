# SECURITY-018: new Function() Is CSP-Incompatible and Uncached

## Status: Security / Performance Concern

## File
`Scripts/element.ts:15`

## How to Reproduce

### CSP Incompatibility
1. Add a Content-Security-Policy header to the application:
   ```
   Content-Security-Policy: script-src 'self'
   ```
2. Load any page with a reactive plan that contains `mutate-element` commands.
3. The runtime calls `new Function("el", "val", cmd.jsEmit)`.
4. Browser blocks execution: `Refused to evaluate a string as JavaScript because 'unsafe-eval' is not an allowed source of script`.
5. All DOM mutations fail. The application is non-functional.

### Performance (No Caching)
1. Create a reactive form with 20 fields, each with live validation clearing.
2. On every keystroke, `wireLiveClearing` fires, which calls `clearFieldError`, which may trigger re-validation.
3. Each validation cycle calls `mutateElement` for error display, creating a `new Function` for every mutation.
4. The same jsEmit strings (`el.textContent = val`, `el.classList.add(val)`) create identical functions hundreds of times.
5. Each `new Function()` call involves parsing and compiling JavaScript. The functions are immediately GC'd.

## Deep Reasoning: Why This Is a Real Issue

### CSP
The `new Function()` call is semantically equivalent to `eval()`. Modern web security standards (OWASP, CSP Level 3) recommend against `unsafe-eval` because it opens the door to code injection attacks. While the framework controls the jsEmit strings (they come from the C# DSL, not user input), the CSP policy cannot distinguish between "safe eval of trusted strings" and "dangerous eval of untrusted strings." The policy must allow ALL eval or NONE.

Organizations with strict security requirements (government, finance, healthcare) mandate CSP headers without `unsafe-eval`. The framework cannot be used in these environments without the `unsafe-eval` exception, which security auditors flag as a finding.

### Architecture Defense
The jsEmit pattern is architecturally intentional — it is the mechanism by which the plan carries behavior. The C# DSL generates a small set of fixed jsEmit strings (`el.textContent = val`, `el.classList.add(val)`, `el.checked=(val==='true')`, etc.). These are NOT user-controlled and NOT dynamic. The security risk is theoretical, not practical — but CSP enforcement is binary.

### Performance
The framework has approximately 15 distinct jsEmit strings across all component types. A `Map<string, Function>` cache would reduce the `new Function()` calls from "once per mutation" to "once per unique jsEmit string per page lifetime." For hot paths (live validation, component event handlers firing on every keystroke), this eliminates repeated parsing.

## How Fixing This Improves the Codebase

1. **CSP compatibility**: A function cache pre-compiles all jsEmit strings at boot time. The `new Function()` calls happen during boot (before CSP can distinguish them from inline scripts in a module), or a nonce/hash approach can whitelist the known set.
2. **Performance**: Cache lookup (`Map.get()`) is O(1) vs `new Function()` which involves lexing, parsing, and compilation every time.
3. **Maintainability**: The cache makes the set of jsEmit strings explicit and auditable.

## How This Fix Will Not Break Existing Features

- A `Map<string, Function>` cache is a transparent optimization. The same function is called with the same arguments — only the construction is cached.
- The jsEmit API contract does not change — the plan still carries jsEmit strings.
- CSP compatibility is opt-in via the application's CSP header. Applications without CSP see no behavioral difference.
- All tests pass unchanged because the cached function produces identical results.
