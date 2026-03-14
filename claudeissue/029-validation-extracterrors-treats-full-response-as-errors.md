# FAIL-FAST-029: validation.ts extractErrors Treats Entire Response as Error Map

## Status: Fail-Fast Violation (Rule 8)

## File
`Scripts/validation.ts:218-224`

## How to Reproduce

1. Configure a POST with validation error handling:
   ```csharp
   p.Post("/api/save", g => g.IncludeAll())
    .Response(r => r.OnError(e => e.ValidationErrors("form")));
   ```
2. Server returns a 400 response with a non-standard body:
   ```json
   { "success": false, "message": "Invalid request", "requestId": "abc-123" }
   ```
3. `extractErrors` checks for an `errors` property (line 220). Not found.
4. Line 223: `return data as Record<string, unknown>;` — the entire response is treated as an error map.
5. `showServerErrors` iterates the entries: `success`, `message`, `requestId`.
6. For each, it looks for `span[data-valmsg-for="success"]`, `span[data-valmsg-for="message"]`, `span[data-valmsg-for="requestId"]`.
7. If any of these spans exist (unlikely but possible if field names collide), they display the response values as "validation errors."

## Deep Reasoning: Why This Is a Real Issue

The `extractErrors` function has a two-step resolution:
1. If `data.errors` exists → use it (standard ASP.NET validation response format)
2. Otherwise → assume the entire response object is the error map

Step 2 is a **silent fallback** that interprets any JSON object as field-to-error mappings. This assumption works for the ASP.NET convention where the response IS the error dictionary:
```json
{ "Email": ["Email is required"], "Name": ["Name is too short"] }
```

But it fails for any non-standard error response format. The function cannot distinguish between:
- A validation error response: `{ "Email": ["required"] }`
- A general error response: `{ "code": "INVALID", "details": "..." }`
- A redirect response: `{ "redirectUrl": "/login" }`

All three are treated identically as validation error maps.

The danger: `showServerErrors` applies the error messages to DOM spans using `span[data-valmsg-for="${name}"]`. If a field happens to share a name with a response property (e.g., a form field named "message"), the response value is displayed as a validation error. This is a data leak — internal response metadata is shown to the user.

## How Fixing This Improves the Codebase

1. **Require explicit structure**: Only accept responses that have an `errors` property. If the response has no `errors` key, log a warning and return `null` — no errors to display.
2. **Or validate the structure**: Check that the error values are arrays of strings (the ASP.NET convention), not arbitrary objects.
3. **Consistent with fail-fast**: The developer configured `ValidationErrors("form")`, expecting ASP.NET validation responses. A non-standard response should surface a warning, not be silently reinterpreted.

## How This Fix Will Not Break Existing Features

- All existing server endpoints that return validation errors use the ASP.NET format: either `{ "errors": { ... } }` or direct `{ "field": ["msg"] }` dictionaries.
- The fix changes the fallback (step 2) from "treat as error map" to "log warning, return null." The primary path (step 1) is unchanged.
- If any existing endpoint returns a direct error dictionary (no `errors` wrapper), those responses would stop being processed. This is a behavioral change, but it surfaces a non-standard API response format that should be standardized.
