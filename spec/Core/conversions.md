# Conversions

The one conversion, `as(expr, shape)`, of a present value. A value that is nothing stays nothing, and
`nullable(inner)` reads nothing for a blank text. Each row is closed: a source not listed for a shape
converts to nothing. The browser implements exactly this table. The keys a comparison of a date or a time
reads (the last section) are no `as` row: each reads a value as .NET holds it, to the tick, and no value
position takes one (D3.2, D6.12).

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
| date | its local day `"yyyy-MM-dd"`, with `"THH:mm:ss"` and `".fff"` when not midnight (`spec/PathsAndWire/DateWire.tla`) |
| instant | its wall time in the view's time zone with that zone's offset for its own day (D3.12), `"yyyy-MM-ddTHH:mm:ss-08:00"`, `"Z"` at zero |
| list | each item converted to string, joined by `","` |

## number

| Source | Result |
|--------|--------|
| number, finite | itself |
| text, blank | nothing |
| an input's text | that number, parsed as the endpoint's host binds the member's .NET type under the view's culture (D3.11); text that host refuses converts to nothing |
| a URL parameter's text, or a number the app's form holds as text (D7.7) | that number, parsed under the invariant culture with `NumberStyles.Float` |
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
| text `"yyyy-MM-ddTHH:mm"`, `"yyyy-MM-ddTHH:mm:ss"`, or `"yyyy-MM-ddTHH:mm:ss"` with a fraction of one to seven digits | that local time, its fraction read to the millisecond, as System.Text.Json writes one to seven digits and a JavaScript `Date` holds milliseconds |
| text of those forms with `"Z"` or `"+hh:mm"`/`"-hh:mm"` | its wall fields as that local time, the offset dropped: a `DateTime` is its wall fields whatever its `Kind`, and System.Text.Json wrote them as its `Ticks` (D3.9, `probe:core-stj-date-time-forms`), where the moment shows another wall time (`probe:js-date-utc-text-as-moment`) |

## instant

| Source | Result |
|--------|--------|
| instant | itself |
| date | that moment, at the view's time zone's offset for its own day (D3.12) |
| text a date converts from | that moment, at the offset the text gives, else the view's time zone's offset for its own day (D3.12) |

## file

| Source | Result |
|--------|--------|
| a `File` | itself |

## array(item)

| Source | Result |
|--------|--------|
| a JavaScript array whose elements each convert to themselves | the array itself |
| any other JavaScript array | a new array, each element converted to `item`; an element that converts to nothing stays nothing |

## object(fields)

| Source | Result |
|--------|--------|
| an object whose declared fields each convert to themselves | the object itself, its identity and prototype kept |
| a plain object a declared field of which converts to another value | a copy, each declared field converted to its shape; fields it does not declare stay as they are |

## any

| Source | Result |
|--------|--------|
| any value | itself |

## Keys

A comparison, a sort or a check's comparison of a date or a time reads each operand through its type's key (D6.12,
D6.13, D16.16): text of fixed width whose code units order as .NET orders the values, to the tick, so `eq`, `neq`, `lt`,
`lte`, `gt`, `gte` and `compareOrdinal` compare two keys as texts (`probe:js-key-text-order`,
`probe:js-span-key-order`). No key builds a JavaScript `Date`, so neither a wall time daylight saving skips or repeats
nor a `Date`'s milliseconds move a comparison (`probe:js-date-wall-gap`, `probe:js-date-wall-overlap`,
`probe:js-date-milliseconds`). C# writes a key only as an argument of those operators, the two arguments of one
comparison keys of one type.

The forms, each as System.Text.Json writes it (`probe:core-stj-date-time-forms`) and MVC 10's binding reads it as its
fields, in a query and in form data under every culture whose calendar is Gregorian (`probe:core-query-date-forms`,
`probe:core-form-date-forms-every-culture`); MVC 5's binding reads each through `DateTime.Parse`,
`DateTimeOffset.Parse` and `TimeSpan.Parse` under the binding's culture (`probe:mvc5-datetime-parse`,
`probe:mvc5-datetimeoffset-parse`, `probe:mvc5-timespan-parse`):

- D: `"yyyy-MM-dd"`, a day of the Gregorian calendar in years 1 to 9999;
- T: `"HH:mm"`, `"HH:mm:ss"`, or `"HH:mm:ss"` with a fraction of one to seven digits, its hours 00 to 23, its minutes
  and seconds 00 to 59;
- O: `"Z"`, or `"+hh:mm"` or `"-hh:mm"` up to 14:00;
- S: `"hh:mm"`, or `"[-][d.]hh:mm:ss"` with a fraction of one to seven digits or none, its hours 00 to 23, its minutes
  and seconds 00 to 59, and the whole from `TimeSpan.MinValue` to `TimeSpan.MaxValue`.

Text in a form's shape whose fields name no value of the type (a date's time or a time of day at hour 24, a minute or
second of 60, a 30th of February, an offset past 14:00, an instant before year 1, a span past `TimeSpan.MaxValue`)
names no value, and the binding refuses it (`probe:core-bound-refused-values`). A `TimeSpan` text at hour 24 is outside
the forms, since the binding reads `"24:00:00"` as 24 days (the same probe).

| Key | Type | Reads | Key text |
|-----|------|-------|----------|
| `dateKey` | `DateTime` | a `Date`, by its local fields to the millisecond, the fields DateWire shows and sends (D3.8); text D, or D`T`T | `"yyyy-MM-ddTHH:mm:ss.fffffff"`, the day at midnight for D |
| `wallKey` | `DateTime` the app's form holds | text D`T`T, or D`T`TO with O dropped, the wall fields System.Text.Json wrote as its `Ticks` (D3.9, `probe:core-stj-datetime-kind`) | as `dateKey`'s |
| `instantKey` | `DateTimeOffset` | a `Date`, by its UTC fields from its time value; text D`T`TO, its wall less its offset | the instant's UTC `"yyyy-MM-ddTHH:mm:ss.fffffff"` |
| `dayKey` | `DateOnly`, net10's alone | text D | `"yyyy-MM-dd"` |
| `timeKey` | `TimeOnly`, net10's alone | text T | `"HH:mm:ss.fffffff"` |
| `spanKey` | `TimeSpan` | text S | its ticks plus 2^63, as 20 digits through `BigInt` |

A `DateTime` text with O is outside `dateKey`'s forms, since each host reads it its own way: MVC 10's binding as the
instant's UTC wall time, System.Text.Json as UTC's for `"Z"` and the server clock's for any other offset, MVC 5's
`DateTime.Parse` as the server clock's (`probe:core-bound-offset-into-datetime`). A `DateTimeOffset` text with no O is
outside `instantKey`'s, since the endpoint reads it at the server clock's offset (the same probe). Neither the view's
time zone (D3.12) nor the browser's clock decides a key. The endpoint's binding also reads texts outside the forms, as a
`TimeOnly` of `"7:00 PM"`, a `DateTime` of `"10/27/2026"` or a `TimeSpan` of `"5"`, five days
(`probe:core-bound-lenient-forms`), where a page URL member and an input read the member's start.

A key reads its first argument:

- nothing reads nothing, and null is null, so `eq` of two nulls holds, `neq` of null and a key holds, and no ordering
  with null holds (D8.9); a sort puts null first and stops on nothing (D6.13);
- a value in its forms reads its key text;
- text that names no value reads nothing, as a conversion that fails reads (D3.3) and as the endpoint binds none, or
  reads the second argument where C# wrote one;
- a value outside its forms, or a value of another kind (a number, a boolean, an object, a `Date` given to a key that
  reads only text, a `Date` whose time value is not a number, a `Date` whose year is outside 1 to 9999), stops the
  pipeline, naming the value, unless C# wrote a second argument: a literal of the member's start, in the forms or
  null, which the key then reads in the value's place.

C# picks the key and its second argument by the source, a closed set:

| Source | Key | Second argument |
|--------|-----|-----------------|
| the app's form: a response body, success or error, a pushed message, data past them (D7.7) | `wallKey` for a `DateTime`, the type's key for the others | none: its text is System.Text.Json's, and any other text is JSON the framework did not produce |
| the live form: a live object's member, payload, argument or result, data past them (D7.7) | `dateKey` for a `DateTime`, the type's key for the others | none: a vendor's text outside the forms stops |
| a page URL member (D9.18), its query or path text as the invariant culture binds it | the type's key, `dateKey` for a `DateTime` | the member's start, as N10.7 reads text that does not convert |
| an input (N3.3), the value its container sends (N13.1) | the type's key, `dateKey` for a `DateTime` | the member's start, for the text an input of a `DateOnly`, `TimeOnly` or `TimeSpan` hands; an input of a `DateTime` or `DateTimeOffset` hands the `Date` `as` converts its text to, or its start |
| a value the view holds | its partner's key, over the value written in its partner's form (D7.7, D8.10), a `DateTime` with no offset (D3.9) | none |
| a member a check's body lacks (D16.16) | the type's key, `dateKey` for a `DateTime`, over the start C# writes in the member's form with no offset (D16.6, D3.9) | none: C# wrote the start |
