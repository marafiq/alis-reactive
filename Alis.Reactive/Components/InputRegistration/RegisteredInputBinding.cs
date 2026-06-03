using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    internal sealed class RegisteredInputBinding
    {
        private RegisteredInputBinding(BindingPath bindingPath, MemberName valueMember)
        {
            BindingPath = bindingPath;
            ValueMember = valueMember;
        }

        internal BindingPath BindingPath { get; }
        internal MemberName ValueMember { get; }

        internal bool Matches(RegisteredInputBinding other) =>
            BindingPath.Equals(other.BindingPath) && ValueMember.Equals(other.ValueMember);

        internal static RegisteredInputBinding For(string bindingPath, string valueMember) =>
            new RegisteredInputBinding(
                BindingPath.Of(bindingPath),
                MemberName.Of(valueMember));

        internal static RegisteredInputBinding For(BindingPath bindingPath, MemberName valueMember) =>
            new RegisteredInputBinding(bindingPath, valueMember);
    }
}
