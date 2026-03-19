using Alis.Reactive.Descriptors;

namespace Alis.Reactive.UnitTests;

/// <summary>
/// BDD tests for CoercionTypes.InferFromType and InferElementType —
/// every C# type maps to the correct coercion string.
/// </summary>
[TestFixture]
public class WhenInferringCoercionTypes
{
    // ── InferFromType — scalars ──

    [Test] public void String_infers_string() => Assert.That(CoercionTypes.InferFromType(typeof(string)), Is.EqualTo("string"));
    [Test] public void Bool_infers_boolean() => Assert.That(CoercionTypes.InferFromType(typeof(bool)), Is.EqualTo("boolean"));
    [Test] public void Nullable_bool_infers_boolean() => Assert.That(CoercionTypes.InferFromType(typeof(bool?)), Is.EqualTo("boolean"));
    [Test] public void Int_infers_number() => Assert.That(CoercionTypes.InferFromType(typeof(int)), Is.EqualTo("number"));
    [Test] public void Long_infers_number() => Assert.That(CoercionTypes.InferFromType(typeof(long)), Is.EqualTo("number"));
    [Test] public void Double_infers_number() => Assert.That(CoercionTypes.InferFromType(typeof(double)), Is.EqualTo("number"));
    [Test] public void Float_infers_number() => Assert.That(CoercionTypes.InferFromType(typeof(float)), Is.EqualTo("number"));
    [Test] public void Decimal_infers_number() => Assert.That(CoercionTypes.InferFromType(typeof(decimal)), Is.EqualTo("number"));
    [Test] public void Short_infers_number() => Assert.That(CoercionTypes.InferFromType(typeof(short)), Is.EqualTo("number"));
    [Test] public void Byte_infers_number() => Assert.That(CoercionTypes.InferFromType(typeof(byte)), Is.EqualTo("number"));
    [Test] public void Nullable_int_infers_number() => Assert.That(CoercionTypes.InferFromType(typeof(int?)), Is.EqualTo("number"));
    [Test] public void Nullable_decimal_infers_number() => Assert.That(CoercionTypes.InferFromType(typeof(decimal?)), Is.EqualTo("number"));
    [Test] public void DateTime_infers_date() => Assert.That(CoercionTypes.InferFromType(typeof(DateTime)), Is.EqualTo("date"));
    [Test] public void Nullable_DateTime_infers_date() => Assert.That(CoercionTypes.InferFromType(typeof(DateTime?)), Is.EqualTo("date"));
    [Test] public void DateTimeOffset_infers_date() => Assert.That(CoercionTypes.InferFromType(typeof(DateTimeOffset)), Is.EqualTo("date"));
    [Test] public void DateOnly_infers_date() => Assert.That(CoercionTypes.InferFromType(typeof(DateOnly)), Is.EqualTo("date"));
    [Test] public void Enum_infers_string() => Assert.That(CoercionTypes.InferFromType(typeof(DayOfWeek)), Is.EqualTo("string"));
    [Test] public void Object_infers_raw() => Assert.That(CoercionTypes.InferFromType(typeof(object)), Is.EqualTo("raw"));

    // ── InferFromType — arrays and collections ──

    [Test] public void String_array_infers_array() => Assert.That(CoercionTypes.InferFromType(typeof(string[])), Is.EqualTo("array"));
    [Test] public void Int_array_infers_array() => Assert.That(CoercionTypes.InferFromType(typeof(int[])), Is.EqualTo("array"));
    [Test] public void Bool_array_infers_array() => Assert.That(CoercionTypes.InferFromType(typeof(bool[])), Is.EqualTo("array"));
    [Test] public void DateTime_array_infers_array() => Assert.That(CoercionTypes.InferFromType(typeof(DateTime[])), Is.EqualTo("array"));
    [Test] public void List_string_infers_array() => Assert.That(CoercionTypes.InferFromType(typeof(List<string>)), Is.EqualTo("array"));
    [Test] public void List_int_infers_array() => Assert.That(CoercionTypes.InferFromType(typeof(List<int>)), Is.EqualTo("array"));
    [Test] public void IEnumerable_string_infers_array() => Assert.That(CoercionTypes.InferFromType(typeof(IEnumerable<string>)), Is.EqualTo("array"));
    [Test] public void ICollection_int_infers_array() => Assert.That(CoercionTypes.InferFromType(typeof(ICollection<int>)), Is.EqualTo("array"));
    [Test] public void IReadOnlyList_string_infers_array() => Assert.That(CoercionTypes.InferFromType(typeof(IReadOnlyList<string>)), Is.EqualTo("array"));

    // ── InferElementType — element types of arrays/collections ──

    [Test] public void String_array_element_is_string() => Assert.That(CoercionTypes.InferElementType(typeof(string[])), Is.EqualTo("string"));
    [Test] public void Int_array_element_is_number() => Assert.That(CoercionTypes.InferElementType(typeof(int[])), Is.EqualTo("number"));
    [Test] public void Bool_array_element_is_boolean() => Assert.That(CoercionTypes.InferElementType(typeof(bool[])), Is.EqualTo("boolean"));
    [Test] public void DateTime_array_element_is_date() => Assert.That(CoercionTypes.InferElementType(typeof(DateTime[])), Is.EqualTo("date"));
    [Test] public void List_string_element_is_string() => Assert.That(CoercionTypes.InferElementType(typeof(List<string>)), Is.EqualTo("string"));
    [Test] public void List_int_element_is_number() => Assert.That(CoercionTypes.InferElementType(typeof(List<int>)), Is.EqualTo("number"));
    [Test] public void Non_collection_element_is_raw() => Assert.That(CoercionTypes.InferElementType(typeof(string)), Is.EqualTo("raw"));
    [Test] public void Object_element_is_raw() => Assert.That(CoercionTypes.InferElementType(typeof(object)), Is.EqualTo("raw"));
}
