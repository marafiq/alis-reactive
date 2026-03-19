# FusionFileUpload — Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development or superpowers:executing-plans.

**Goal:** Onboard FusionFileUpload as a vertical slice that sends files as part of FormData alongside other fields via `IncludeAll()`.

**Architecture:** SF Uploader in form mode (`saveUrl=null`, `autoUpload=false`). Files live in `filesData[].rawFile` as `File` objects. The gather system needs a File-aware emit path — `formData.append(name, file, file.name)` instead of `String(value)`.

**Tech Stack:** C# (ASP.NET Core + Syncfusion EJ2), TypeScript

---

## Why This Is NOT a Pure Vertical Slice

The Uploader is the first component whose `readExpr` resolves to `File[]` — not a string, number, boolean, Date, or string[]. The current `Transport` interface in `gather.ts` only handles:
- `emitScalar(name, value)` → `String(value)` or `setNested(body, name, value)`
- `emitArray(name, items)` → iterate and `String(item)`

`String(file)` produces `"[object File]"` — garbage. We need `formData.append(name, file, file.name)`.

**Changes required outside the vertical slice:**
1. `gather.ts` — new `emitFile(name, file)` on Transport + File detection in `emit()`
2. `types.ts` — FileUpload-specific types (optional, may use existing `unknown`)
3. `component.ts` — potentially, if readExpr resolution needs to handle `filesData` extraction

**Changes inside the vertical slice (standard 7 files):**
1. Component phantom type, events, extensions, builder, html extensions, reactive extensions
2. `readExpr` = `"filesData"` — returns the `FileInfo[]` array
3. Extensions: `Value()` returns `TypedComponentSource<IFormFile[]>` (or a custom type)

---

## Design Decisions Needed

### 1. ReadExpr — what does the plan carry?

**Option A:** `readExpr: "filesData"` — gather reads `ej2.filesData`, gets `FileInfo[]`, extracts `.rawFile` from each.
- Pro: Single readExpr, gather handles extraction
- Con: Gather needs to know about FileInfo structure (couples gather to SF)

**Option B:** Custom gather kind `"file-component"` with its own resolution logic.
- Pro: Clean separation — gather.ts just appends files
- Con: New descriptor kind, more plan complexity

**Recommendation:** Option A with a `vendor: "fusion"` + `readExpr: "filesData"` combination. The gather `emit()` function detects `File`/`FileList` objects and uses `formData.append(name, file, file.name)`. The `filesData[].rawFile` extraction happens in a small adapter in `component.ts` or inline in gather.

### 2. Gather Transport — how to handle Files

```typescript
// In gather.ts emit():
if (raw instanceof FileList || (Array.isArray(raw) && raw.length > 0 && raw[0] instanceof File)) {
  for (const file of raw) transport.emitFile(name, file);
  return;
}

// New Transport method:
emitFile(name: string, file: File): void {
  // Only FormData supports files — JSON and GET cannot carry files
  formData.append(name, file, file.name);
}
```

JSON and GET transports should throw if they encounter a File — files can only travel via FormData.

### 3. C# Model Type

`IFormFile[]` (or `List<IFormFile>`) for the model property. The `[FromForm]` binding handles multipart file parsing.

### 4. contentType enforcement

When any component in the gather produces File objects, the request MUST be `form-data`. The plan should enforce this — if a FileUpload component is in the gather and contentType is not `form-data`, throw at runtime.

---

## Task Breakdown

### Task 1: Extend gather.ts Transport with emitFile

**Files to modify:**
- `Scripts/gather.ts` — add `emitFile(name: File)` to Transport interface, implement for FormData (append file), throw for JSON/GET
- `Scripts/gather.ts` — add File detection in `emit()` before array/scalar dispatch

```typescript
interface Transport {
  emitScalar(name: string, value: unknown): void;
  emitArray(name: string, items: unknown[]): void;
  emitFile(name: string, file: File): void;
}
```

FormData transport: `formData.append(name, file, file.name)`
JSON transport: `throw new Error("[alis] File objects require contentType: form-data")`
GET transport: `throw new Error("[alis] File objects cannot be sent via GET")`

File detection in emit():
```typescript
function emit(name: string, raw: unknown): void {
  // File or FileInfo[] with rawFile — extract and emit as files
  if (isFileData(raw)) {
    const files = extractFiles(raw);
    for (const file of files) transport.emitFile(name, file);
    log.trace("file", { name, count: files.length });
    return;
  }
  // existing array/scalar paths...
}
```

`isFileData()` checks: is it a `FileList`? Is it an array where items have `.rawFile` (SF FileInfo pattern)?
`extractFiles()` returns `File[]` — either from `FileList` or by mapping `fileInfo.rawFile`.

- [ ] Write TS unit tests for emitFile (FormData, JSON throw, GET throw)
- [ ] Implement
- [ ] Verify existing gather tests still pass

### Task 2: FusionFileUpload vertical slice (7 files)

**Files to create under `Alis.Reactive.Fusion/Components/FusionFileUpload/`:**

1. `FusionFileUpload.cs` — sealed, `FusionComponent`, `IInputComponent`, `ReadExpr => "filesData"`
2. `Events/FusionFileUploadOnSelected.cs` — event args (file count, file names)
3. `FusionFileUploadEvents.cs` — singleton, `Selected` → `"selected"` (NOT "change")
4. `FusionFileUploadExtensions.cs` — `Value()` → `TypedComponentSource<IFormFile[]>` (or string[] for names)
5. `FusionFileUploadHtmlExtensions.cs` — factory using `Html.EJS().Uploader("id").AutoUpload(false).SaveUrl(null).Render()`. Register in ComponentsMap.
6. `FusionFileUploadReactiveExtensions.cs` — `.Reactive()` on Uploader

**Key SF configuration:**
```csharp
setup.Helper.EJS().Uploader(setup.ElementId)
    .AutoUpload(false)       // Don't upload automatically
    .AllowedExtensions(".pdf,.jpg,.png,.doc,.docx")
    .Multiple(true)
    .Render()
```

No `SaveUrl` / `RemoveUrl` — form mode only.

### Task 3: FusionFileUpload tests + sandbox

- Unit tests: snapshot + schema
- Sandbox page: file picker + other fields + FormData POST
- Playwright: select files via `SetInputFiles()`, verify POST body contains files

### Task 4: Add to ComponentGather

- Add `IFormFile[]? Documents` to ComponentGatherModel
- Add uploader to ComponentGather form
- Ensure FormData submit button sends files alongside other fields
- Field count → 21

---

## Constraints

1. **Files ONLY via FormData** — JSON POST cannot carry files. If user clicks "Submit JSON" with files selected, the framework should either: (a) skip file fields in JSON, or (b) throw a clear error.
2. **No SF async upload** — `saveUrl=null`, `autoUpload=false`. The framework's gather handles the POST.
3. **readExpr = "filesData"** — gather reads SF's `filesData` array, extracts `.rawFile` (File objects).
4. **Vertical slice for the component** — standard 7 files. Runtime changes are isolated to `gather.ts` (Transport.emitFile + File detection).

---

## Risk

This is the first component that requires a runtime change (`gather.ts`). The change is additive (new method on Transport, new detection in emit) and backwards-compatible (existing scalar/array paths unchanged). But it MUST be tested in isolation before the vertical slice.
