---
name: project_grammar_tree_data
description: Complete API signatures extracted from source code — use to build the Reactive Mental Model grammar tree
type: project
---

## Task: Build complete grammar tree for mental-model.mdx

**Status:** API signatures extracted, tree needs to be written

**File to update:** `docs-site/src/content/docs/csharp-modules/mental-model.mdx`

**User requirements:**
- Complete grammar tree — NO method left out
- Organized by Prop/Method/Read patterns (like a language spec)
- Use `pipeline` not `p` throughout
- Show full generic signatures (`<TModel>`, `<TProp>`, etc.)
- Legends for patterns: where pipeline repeats, where grammar patterns apply
- Split into sections: View Tree, Components, Events/Args, Pipeline
- The tree IS the mental model — a dev should be able to use 100% of the API from this tree alone

**Key grammar patterns to express:**
- `Component<T>().{Prop}(value)` = property write (SetValue, SetChecked, SetText, SetDataSource)
- `Component<T>().{Method}()` = void method call (DataBind, FocusIn, ShowPopup, etc.)
- `Component<T>().Value()` = read for conditions (returns TypedComponentSource<TProp>)
- `Element("id").{Prop}(value)` = set-prop (SetText, SetHtml)
- `Element("id").{Method}(args)` = call (AddClass, RemoveClass, ToggleClass)
- `Element("id").{Visibility}()` = Show/Hide
- `Native{Name}` = HTML element, `{Name}` = Syncfusion EJ2
- `evt.{Event}` varies per component, `args.{Property}` typed per event
- Pipeline repeats (↻) in OnSuccess, OnError, WhileLoading, Then, Else, OnAllSettled
- HTTP repeats (↺) in Chained

**Why:** "this tree tells the whole feature list right there. its mental model for devs to use 100% api"
