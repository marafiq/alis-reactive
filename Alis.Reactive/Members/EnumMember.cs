using System;

namespace Alis.Reactive.Members
{
    internal static class EnumMember
    {
        internal static Type? TypeOf(Type memberType)
        {
            if (memberType == null) throw new ArgumentNullException(nameof(memberType));
            var type = Nullable.GetUnderlyingType(memberType) ?? memberType;
            return type.IsEnum ? type : null;
        }

        internal static object Of(Type enumType, object value)
        {
            if (enumType == null) throw new ArgumentNullException(nameof(enumType));
            if (value == null) throw new ArgumentNullException(nameof(value));
            return value is Enum ? value : Enum.ToObject(enumType, value);
        }

        internal static Enum? Default(Type enumType)
        {
            if (enumType == null) throw new ArgumentNullException(nameof(enumType));
            var zero = (Enum)Enum.ToObject(enumType, 0);
            return Enum.IsDefined(enumType, zero) ? zero : null;
        }
    }
}
