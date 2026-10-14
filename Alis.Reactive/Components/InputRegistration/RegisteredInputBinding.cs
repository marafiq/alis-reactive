using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    internal sealed class RegisteredInputBinding
    {
        private RegisteredInputBinding(BindingPath bindingPath, ComponentInput input)
        {
            BindingPath = bindingPath;
            Input = input;
        }

        internal BindingPath BindingPath { get; }
        internal ComponentInput Input { get; }

        internal bool Matches(RegisteredInputBinding other) =>
            BindingPath.Equals(other.BindingPath) && Input.Matches(other.Input);

        internal static RegisteredInputBinding For(BindingPath bindingPath, ComponentInput input) =>
            new RegisteredInputBinding(bindingPath, input);
    }
}
