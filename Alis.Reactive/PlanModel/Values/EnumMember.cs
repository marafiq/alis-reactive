using System;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// The enum behind a member's type, and a value as that enum's member. A member binds as its name
    /// (<c>ToString()</c>), which is how MVC reads it back; its JavaScript text is the literal writer's.
    /// </summary>
    internal static class EnumMember
    {
        /// <summary>The enum a member's type is, nullable or not; null when it is no enum.</summary>
        internal static Type? TypeOf(Type memberType)
        {
            if (memberType == null) throw new ArgumentNullException(nameof(memberType));
            var type = Nullable.GetUnderlyingType(memberType) ?? memberType;
            return type.IsEnum ? type : null;
        }

        /// <summary>The value as the enum's member: a member stays one, a number becomes the member it names.</summary>
        internal static object Of(Type enumType, object value)
        {
            if (enumType == null) throw new ArgumentNullException(nameof(enumType));
            if (value == null) throw new ArgumentNullException(nameof(value));
            return value is Enum ? value : Enum.ToObject(enumType, value);
        }

        /// <summary>The member the enum's default binds as, the one with value 0; null when there is none.</summary>
        internal static Enum? Default(Type enumType)
        {
            if (enumType == null) throw new ArgumentNullException(nameof(enumType));
            var zero = (Enum)Enum.ToObject(enumType, 0);
            return Enum.IsDefined(enumType, zero) ? zero : null;
        }
    }
}
