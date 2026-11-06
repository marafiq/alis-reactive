# Conversions

The one conversion, `as(expr, shape)`, of a present value. A value that is nothing stays nothing, and
`nullable(inner)` reads nothing for a blank text. Each row is closed: a source not listed for a shape
converts to nothing. The browser implements exactly this table.

The rule behind every scalar row: a value converts as the endpoint's binder converts its text, so the
browser's verdict on a value and the server's agree. Blank means nothing, or text that is empty or holds
only characters .NET's `char.IsWhiteSpace` accepts.

Today's runtime differs in four ways, each refused here: a failed conversion passes the raw value on
(`Alis.Reactive.Assets/runtime/grammar/shape.ts:17-21`), a blank text converts to the number 0 through
`Number("")` (`Alis.Reactive.Assets/runtime/grammar/shape.ts:58-61`), any text `new Date` parses becomes a
date (`Alis.Reactive.Assets/runtime/grammar/shape.ts:93-94`), and a scalar read as a list becomes a list of one
(`Alis.Reactive.Assets/runtime/grammar/shape.ts:156`).

## string

| Source | Result |
|--------|--------|
| text | itself |
| number, finite | its text as .NET writes it under the view's culture (D3.11): the shortest digits, the culture's decimal separator, no group separator, and below 1E-04 or from 1E+17 up the form `1,234565E+17` |
| boolean | `"true"` or `"false"` |
| date | its local day `"yyyy-MM-dd"`, with `"THH:mm:ss"` and `".fff"` when not midnight (`spec/Core/DateWire.tla`) |
| instant | the browser's clock with that clock's offset, `"yyyy-MM-ddTHH:mm:ss-08:00"`, `"Z"` at zero |
| list | each item converted to string, joined by `","` |

## number

| Source | Result |
|--------|--------|
| number, finite | itself |
| text, blank | nothing |
| an input's text | that number, parsed as the endpoint's host binds the member's .NET type under the view's culture (D3.11); text that host refuses converts to nothing |
| a URL parameter's text | that number, parsed under the invariant culture with `NumberStyles.Float` |
| boolean | 1 or 0 |
| date or instant | its milliseconds since 1970-01-01T00:00:00Z |

## boolean

| Source | Result |
|--------|--------|
| boolean | itself |
| text that, with .NET white space trimmed, is `true` or `false` in any case | that boolean, as `bool.Parse` reads it |

## date

| Source | Result |
|--------|--------|
| date | itself |
| instant | the same moment as a local date |
| text `"yyyy-MM-dd"` naming a real calendar day | that day at local midnight |
| text `"yyyy-MM-ddTHH:mm"`, `"yyyy-MM-ddTHH:mm:ss"` or `"yyyy-MM-ddTHH:mm:ss.fff"` | that local time |
| text of those forms with `"Z"` or `"+hh:mm"`/`"-hh:mm"` | that moment as a local date |

## instant

| Source | Result |
|--------|--------|
| instant | itself |
| date | that moment, at the browser clock's offset for its own day |
| text a date converts from | that moment, at the offset the text gives, else the browser clock's offset for its own day |

## file

| Source | Result |
|--------|--------|
| a `File` | itself |

## array(item)

| Source | Result |
|--------|--------|
| a JavaScript array | each element converted to `item`; an element that converts to nothing stays nothing |

## object(fields)

| Source | Result |
|--------|--------|
| a plain object | each declared field converted to its shape; fields it does not declare stay as they are |

## any

| Source | Result |
|--------|--------|
| any value | itself |
