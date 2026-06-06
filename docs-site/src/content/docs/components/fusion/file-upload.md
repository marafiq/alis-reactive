---
title: FusionFileUpload
description: Multi-file picker in form mode. Read-only from the framework side.
sidebar:
  order: 14
---

A file upload component in form mode -- no auto-upload. The user picks files; the framework can read `filesData` into a gather payload; your controller receives the files when the form submits. Read-only from the plan side: files are set by user interaction only, there is no `SetValue` method.

**Model type:** `List<IFormFile>` &nbsp; **ValueMember:** `"filesData"` &nbsp; **Events:** `Selected`

## How do I render one?

```csharp
Html.InputField(plan, m => m.Documents, o => o.Label("Supporting Documents"))
    .FusionFileUpload(b => b);
```

## Reference

No component write extensions -- files are chosen by the user, gathered into the request payload on submit, and received by the controller as `IFormFile[]`.
