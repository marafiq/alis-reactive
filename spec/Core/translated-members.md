# Translated members

The .NET members an array lambda translates, keyed by the `MethodInfo` or `MemberInfo` the expression tree
carries and built from typed lambdas, as `Alis.Reactive/PlanAuthoring/Arrays/ElementExpressionCompiler.cs:18-41`
builds them today, without the `PureMethodAttribute` scan (`Alis.Reactive/PlanAuthoring/Arrays/ElementExpressionCompiler.cs:51-55`).
A member not listed is refused where the view renders. Every row reads nothing on a missing value and stops
its pipeline on a present value that lacks the member.

Where .NET's meaning depends on the culture, the plan carries the culture the view renders under, so the
browser and .NET agree.

## Text

| .NET member | Browser meaning |
|-------------|-----------------|
| `Length` | the count of UTF-16 code units, as JavaScript's `length` |
| `Trim()`, `TrimStart()`, `TrimEnd()` | removes the characters `char.IsWhiteSpace` accepts, the grammar's `trim` operators |
| `ToUpperInvariant()`, `ToLowerInvariant()` | JavaScript's `toUpperCase`, `toLowerCase` |
| `ToUpper()`, `ToLower()` | JavaScript's `toLocaleUpperCase`, `toLocaleLowerCase` with the view's culture name |
| `Contains(text)` | ordinal, as JavaScript's `includes` |
| `StartsWith(text)`, `EndsWith(text)` | ordinal, as `StringComparison.Ordinal` compares |
| `string.IsNullOrEmpty(text)` | nothing or `""` |
| `string.IsNullOrWhiteSpace(text)` | blank, as `spec/Core/conversions.md` defines it |
| `Regex.IsMatch(text, pattern)` | JavaScript's `RegExp` test of the pattern; a pattern JavaScript reads differently from .NET is refused |

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
