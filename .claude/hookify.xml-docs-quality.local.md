---
name: check-xml-docs-on-csharp-edit
enabled: true
event: file
conditions:
  - field: file_path
    operator: regex_match
    pattern: Alis\.Reactive(\.Native|\.Fusion)?/.*\.cs$
  - field: new_text
    operator: regex_match
    pattern: (public|internal|protected)\s+(class|interface|struct|enum|sealed|static|void|string|bool|int|IHtmlContent|ReactivePlan|InputBoundField)
---

**XML Documentation Quality Check Required**

You just edited a C# file with public/internal API surface. Before proceeding:

1. **Load the `dotnet-xml-docs` skill** if not already loaded this session.
2. **Verify XML docs follow gold standard** (see `Alis.Reactive.Native/Extensions/HtmlExtensions.cs` and `PlanExtensions.cs`):
   - Dev-facing voice: no "runtime", no "JSON", no "serialization" in summary/remarks
   - No em-dashes in XML docs: use colons or restructure
   - Internal constructors: MUST have "NEVER make public" with WHY
   - Property voice: "Gets or sets the..." for read-write, "Gets the..." for read-only
   - Every public member has `<summary>` ending with full stop
   - Code examples in docs MUST be verified against actual source
3. **Run Rider diagnostics** on the file: `mcp__jetbrains__get_file_problems`
4. **Read style memory**: `feedback_xml_docs_style.md` for nuanced rules
