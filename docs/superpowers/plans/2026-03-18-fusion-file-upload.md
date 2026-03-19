# FusionFileUpload — Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development or superpowers:executing-plans.

**Goal:** Onboard FusionFileUpload as a pure vertical slice. Files sent as part of FormData alongside other fields via `IncludeAll()`.

**Architecture:** SF Uploader in form mode (`saveUrl=null`, `autoUpload=false`). `readExpr: "element.files"` resolves to the browser's native `FileList` — zero vendor wrapper extraction. Gather's FormData transport handles `File` objects natively via `formData.append(name, file, file.name)`.

**Tech Stack:** C# (ASP.NET Core + Syncfusion EJ2), TypeScript

---

## Pure Vertical Slice Confirmation

**This IS a pure vertical slice.** The experiment (FileUploadLab) proved:

1. `readExpr: "element.files"` → `walk(ej2, "element.files")` → `ej2.element.files` → native `FileList`
2. `FileList` contains real `File` objects (`instanceof File === true`, `instanceof Blob === true`)
3. `FormData.append(name, file, file.name)` works — server receives `IFormFile` with correct name + size
4. Files + scalar fields coexist in the same FormData POST

**No vendor coupling:** `FileList` and `File` are browser APIs, not Syncfusion concepts. The gather Transport just needs to handle the browser's native `File` type in its FormData path — same level of awareness as knowing `FormData.append()` exists.

**What changes:**

| Layer | Change | Vendor-agnostic? |
|-------|--------|-----------------|
| gather.ts `emitArray` | When item is `File`, use `formData.append(name, file, file.name)` instead of `String(file)` | Yes — `File` is browser API |
| gather.ts `emitScalar` | When value is `FileList`, iterate and append each `File` | Yes — `FileList` is browser API |
| Vertical slice (7 files) | Standard component pattern | Yes |

**What does NOT change:** `component.ts`, `resolver.ts`, `walk.ts`, `types.ts`, `conditions.ts`, `element.ts`, `boot.ts`, `auto-boot.ts`, `execute.ts`, `trigger.ts`, JSON schema, C# descriptors.

---

## Experiment Results (FileUploadLab)

```
ej2.filesData[0].rawFile  →  File (instanceof File === true)
ej2.element.files          →  FileList (native browser, no SF wrapper)
ej2.element.files[0]       →  File (instanceof File === true)

walk(ej2, "element.files") →  FileList (same as ej2.element.files)

FormData.append("Docs", file, file.name)  →  Server receives IFormFile ✓
Scalar fields + files in same POST         →  Both received correctly ✓
```

`readExpr: "element.files"` is the clean path. No `.rawFile` extraction, no SF FileInfo wrappers, no vendor coupling.

---

## Task Breakdown

### Task 1: Gather Transport — handle native File objects (3 lines)

**File to modify:** `Scripts/gather.ts`

In `createTransport` (FormData path), update `emitArray` to handle `File` items:

```typescript
emitArray: (name, items) => {
  for (const item of items) {
    if (item instanceof File) formData.append(name, item, item.name);
    else formData.append(name, String(item ?? ""));
  }
},
```

In `createTransport` (GET path), `emitArray` should throw on File:

```typescript
emitArray: (name, items) => {
  for (const item of items) {
    if (item instanceof File) throw new Error("[alis] File objects cannot be sent via GET");
    urlParams.push(`${encodeURIComponent(name)}=${encodeURIComponent(String(item))}`);
  }
},
```

In `createJsonTransport`, `emitArray` should throw on File:

```typescript
emitArray: (name, items) => {
  if (items.some(item => item instanceof File))
    throw new Error("[alis] File objects require contentType: form-data");
  setNested(body, name, items);
},
```

Also handle `FileList` in `emit()` — `FileList` is array-like but not `Array.isArray`:

```typescript
function emit(name: string, raw: unknown): void {
  // FileList — browser native, array-like but not Array
  if (raw instanceof FileList) {
    for (let i = 0; i < raw.length; i++) transport.emitArray(name, [raw[i]]);
    log.trace("file", { name, count: raw.length });
    return;
  }
  // existing array/scalar paths...
}
```

Wait — simpler: just convert FileList to array and let emitArray handle it:

```typescript
if (raw instanceof FileList) {
  transport.emitArray(name, Array.from(raw));
  log.trace("file", { name, count: raw.length });
  return;
}
```

- [ ] Update `emitArray` in FormData transport — File instanceof check (1 line)
- [ ] Update `emitArray` in GET transport — throw on File (1 line)
- [ ] Update `emitArray` in JSON transport — throw on File (1 line)
- [ ] Add FileList detection in `emit()` before array check (3 lines)
- [ ] Write TS unit tests: File in FormData, File in GET throws, File in JSON throws, FileList conversion
- [ ] Verify ALL existing gather tests still pass
- [ ] `npm run build` — rebuild bundle

### Task 2: FusionFileUpload vertical slice (6 files)

**Files to create under `Alis.Reactive.Fusion/Components/FusionFileUpload/`:**

1. **`FusionFileUpload.cs`** — sealed, `FusionComponent`, `IInputComponent`, `ReadExpr => "element.files"`

```csharp
public sealed class FusionFileUpload : FusionComponent, IInputComponent
{
    public string ReadExpr => "element.files";
}
```

2. **`Events/FusionFileUploadOnSelected.cs`** — event args

```csharp
public class FusionFileUploadSelectedArgs
{
    public int FilesCount { get; set; }
    public bool IsInteracted { get; set; }
    public FusionFileUploadSelectedArgs() { }
}
```

3. **`FusionFileUploadEvents.cs`** — singleton, `Selected` → `"selected"`

4. **`FusionFileUploadExtensions.cs`** — `Value()` → `TypedComponentSource<string>` (reads as string for conditions — file input .files is not useful in conditions, but the pattern requires it)

5. **`FusionFileUploadHtmlExtensions.cs`** — factory. SF Uploader has no `UploaderFor()`, so use direct `EJS().Uploader(id)`:

```csharp
public static void FileUpload<TModel, TProp>(
    this InputFieldSetup<TModel, TProp> setup,
    Action<UploaderBuilder> configure) where TModel : class
{
    setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
        setup.ElementId, Component.Vendor, setup.BindingPath, Component.ReadExpr));

    var builder = setup.Helper.EJS().Uploader(setup.ElementId)
        .AutoUpload(false)
        .HtmlAttributes(new Dictionary<string, object> { ["name"] = setup.BindingPath });
    configure(builder);
    setup.Render(builder.Render());
}
```

6. **`FusionFileUploadReactiveExtensions.cs`** — `.Reactive()` on `UploaderBuilder`

**File to modify:** `FusionTestBase.cs` — add `public string? Documents { get; set; }` (string for snapshot tests — real files only in Playwright)

### Task 3: FusionFileUpload unit tests

Under `tests/Alis.Reactive.Fusion.UnitTests/Components/FusionFileUpload/`:

1. `WhenDescribingFusionFileUploadEvents.cs` — singleton, jsEvent = "selected", args type
2. `WhenMutatingAFusionFileUpload.cs` — Value source type (TypedComponentSource<string>), readExpr = "element.files"

### Task 4: FusionFileUpload sandbox page

1. `FileUploadModel.cs` — `string? ResidentName`, `IFormFile[]? Documents`
2. `FileUploadController.cs` — Index + EchoFormData `[FromForm]` POST (returns file names + sizes + form fields)
3. `Views/FileUpload/Index.cshtml`:
   - Section 1: File picker + NativeTextBox for ResidentName
   - Section 2: Submit FormData button — `p.Post(...).AsFormData()` with `IncludeAll()`
   - Section 3: Echo result — shows file names received by server
   - Section 4: Plan JSON
4. Nav link in `_Layout.cshtml`
5. Home page card

### Task 5: FusionFileUpload Playwright tests

`tests/Alis.Reactive.PlaywrightTests/Components/Fusion/WhenUsingFusionFileUpload.cs`:
- Page loads
- File picker renders
- Select files via `SetInputFiles()`, submit, intercept POST, verify files received
- Verify scalar fields also gathered alongside files

### Task 6: Add to ComponentGather

- Add `IFormFile[]? Documents` to ComponentGatherModel
- Add uploader to ComponentGather form
- FormData submit sends files + all other fields
- Field count → 21

### Task 7: Full test suite

```bash
npm test
dotnet test tests/Alis.Reactive.UnitTests
dotnet test tests/Alis.Reactive.Native.UnitTests
dotnet test tests/Alis.Reactive.Fusion.UnitTests
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests
dotnet test tests/Alis.Reactive.PlaywrightTests
```

---

## Constraints

1. **Files ONLY via FormData** — JSON POST and GET throw on `File` objects. This is fail-fast, not silent fallback.
2. **No SF async upload** — `saveUrl` not set, `autoUpload=false`. Framework gather handles POST.
3. **`readExpr: "element.files"`** — walks to native `FileList` on the underlying `<input type="file">`. Zero vendor wrapper extraction.
4. **Vertical slice purity** — 6 component files + sandbox + tests. The gather change (File handling in Transport) is browser API awareness, not vendor coupling.
5. **No changes to:** `component.ts`, `resolver.ts`, `walk.ts`, `types.ts`, `conditions.ts`, `element.ts`, JSON schema, C# descriptors.
