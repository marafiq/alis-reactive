# Translated members

The .NET members an array lambda translates, keyed by the `MethodInfo` or `MemberInfo` the expression tree
carries and built from typed lambdas, as `Alis.Reactive/PlanAuthoring/Arrays/ElementExpressionCompiler.cs:18-41`
builds them today, without the `PureMethodAttribute` scan (`Alis.Reactive/PlanAuthoring/Arrays/ElementExpressionCompiler.cs:51-55`).
That table maps `Trim` to JavaScript's `trim` and `ToUpperInvariant` to `toUpperCase`, and both disagree with .NET
(`spec/NOTES.md`, the probe of 2026-11-06). A member not listed is refused where the view renders. Every row reads
nothing on a missing value and stops its pipeline on a present value that lacks the member.

A text operator reads nothing when its text reads nothing or holds null, and stops its pipeline when its text holds
a value that is not text. A text it takes as an argument reads nothing when it reads nothing, and stops the pipeline
when it holds null, as .NET throws `ArgumentNullException`.

Where .NET's meaning depends on the culture, the plan carries the culture the view renders under, so the
browser and .NET agree.

## Text

| .NET member | Browser meaning |
|-------------|-----------------|
| `Length` | the count of UTF-16 code units, as JavaScript's `length` |
| `Trim()`, `TrimStart()`, `TrimEnd()` | the `trim`, `trimStart` and `trimEnd` operators, which remove the 25 characters `char.IsWhiteSpace` accepts: U+0085 is removed and U+FEFF kept, where JavaScript's `trim` does the reverse. net48 has no `TrimStart()` or `TrimEnd()`: the call binds to `TrimStart(params char[])`, translated only with an empty array. net10 keys the same overload: `TrimStart(new char[] { })` removes white space, as .NET does, and `TrimStart('A', 's')` is refused (X6.2) |
| `ToUpperInvariant()`, `ToLowerInvariant()` | the `upperInvariant` and `lowerInvariant` operators, one code point at a time, so a final `Σ` lowers to `σ` as in .NET. A code point JavaScript maps to more than one keeps .NET's mapping: itself (`ß` stays `ß`), except the 27 Greek letters with a subscript iota (U+1F80 to U+1F87, U+1F90 to U+1F97, U+1FA0 to U+1FA7, U+1FB3, U+1FC3, U+1FF3), which map to the letter .NET gives (U+1FB3 to U+1FBC). `ı` stays `ı` in upper case |
| `ToUpper()`, `ToLower()` | the same rule with `toLocaleUpperCase` and `toLocaleLowerCase` and the view's culture name, built with the plan's culture in POC 4 unit 3c |
| `Contains(text)` | the `contains` operator, ordinal, as .NET's `Contains(string)` and JavaScript's `includes` compare |
| `StartsWith(text, StringComparison.Ordinal)`, `EndsWith(text, StringComparison.Ordinal)` | the `startsWith` and `endsWith` operators, ordinal. `StartsWith(text)` and `EndsWith(text)` compare by the culture in .NET (`"\u00ADward".StartsWith("ward")` holds, an ordinal test does not) and are refused, naming the `Ordinal` overload; a `StringComparison` other than `Ordinal` is refused |
| `string.IsNullOrEmpty(text)` | the `nullOrEmpty` operator: holds on nothing, null and `""` |
| `string.IsNullOrWhiteSpace(text)` | `nullOrEmpty` of `trim`: blank, as `spec/Core/conversions.md` defines it |
| `Regex.IsMatch(text, pattern)` | JavaScript's `RegExp` test of the pattern; a pattern JavaScript reads differently from .NET is refused. The core's ban on `Regex` refuses its key, and the owner decides between an exception for the key and refusing the member with the server as its door (`spec/NOTES.md`, 2026-11-06) |

The probe behind the rows (`spec/NOTES.md`, 2026-11-06) maps every code point in .NET 10 and in Chromium 153: the
operators above agree with .NET on all 1,112,064, and JavaScript's own methods disagree on 103 in upper case, 1 in
lower case and 2 in white space.

## Dates

For `DateTime` and `DateTimeOffset`, read on the browser's clock.

| .NET member | Browser meaning |
|-------------|-----------------|
| `Year` | `getFullYear()` |
| `Month` | `getMonth() + 1` |
| `Day` | `getDate()` |
| `Hour`, `Minute`, `Second` | `getHours()`, `getMinutes()`, `getSeconds()` |
| `DayOfWeek` compared with a `DayOfWeek` | `getDay()` compared with the day's number, Sunday 0 |

## Elements

| .NET member | Browser meaning |
|-------------|-----------------|
| `ChildElement.GetAttribute(name)` | `getAttribute(name)` |
| `ChildElement.HasAttribute(name)` | `hasAttribute(name)` |

## Lists of fixed values

| .NET member | Browser meaning |
|-------------|-----------------|
| `Enumerable.Contains(values, item)` with values the view supplies | membership of the item in those values as literals |
