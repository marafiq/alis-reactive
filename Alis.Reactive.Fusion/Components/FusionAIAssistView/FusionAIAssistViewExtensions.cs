using System.Collections.Generic;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Runtime behavior for Syncfusion AIAssistView. Initial render options remain on
    /// Syncfusion's AIAssistViewBuilder.
    /// </summary>
    public static class FusionAIAssistViewExtensions
    {
        private static readonly ComponentProperty<string> PromptProperty =
            ComponentProperty<string>.Named("prompt");

        private static readonly ComponentProperty<int> ActiveViewProperty =
            ComponentProperty<int>.Named("activeView");

        private static readonly ComponentMethod ExecutePromptMethod =
            ComponentMethod.Named("executePrompt").WithArgs<string>();

        private static readonly ComponentMethod AddPromptResponseTextMethod =
            ComponentMethod.Mapped("addPromptResponseText", "addPromptResponse").WithArgs<string, bool>();

        private static readonly ComponentMethod AddPromptResponseObjectMethod =
            ComponentMethod.Mapped("addPromptResponseObject", "addPromptResponse").WithArgs<object, bool>();

        private static readonly ComponentMethod ScrollToBottomMethod =
            ComponentMethod.Named("scrollToBottom");

        public static TypedComponentSource<string> Prompt<TModel>(
            this ComponentRef<FusionAIAssistView, TModel> self)
            where TModel : class
            => self.Read(PromptProperty);

        public static TypedComponentSource<int> ActiveView<TModel>(
            this ComponentRef<FusionAIAssistView, TModel> self)
            where TModel : class
            => self.Read(ActiveViewProperty);

        public static ComponentRef<FusionAIAssistView, TModel> SetPrompt<TModel>(
            this ComponentRef<FusionAIAssistView, TModel> self,
            string prompt)
            where TModel : class
            => self.EmitSet(PromptProperty, ValueExpression.Literal(prompt));

        public static ComponentRef<FusionAIAssistView, TModel> ExecutePrompt<TModel>(
            this ComponentRef<FusionAIAssistView, TModel> self,
            string prompt)
            where TModel : class
            => self.EmitCall(
                ExecutePromptMethod,
                new List<ValueExpression> { ValueExpression.Literal(prompt) });

        public static ComponentRef<FusionAIAssistView, TModel> AddPromptResponse<TModel>(
            this ComponentRef<FusionAIAssistView, TModel> self,
            string response,
            bool isFinalUpdate = true)
            where TModel : class
            => self.EmitCall(
                AddPromptResponseTextMethod,
                new List<ValueExpression>
                {
                    ValueExpression.Literal(response),
                    ValueExpression.Literal(isFinalUpdate)
                });

        public static ComponentRef<FusionAIAssistView, TModel> AddPromptResponse<TModel>(
            this ComponentRef<FusionAIAssistView, TModel> self,
            string prompt,
            string response,
            bool isFinalUpdate = true)
            where TModel : class
        {
            var promptResponse = ValueExpression.Object(new Dictionary<string, ValueExpression>
            {
                ["prompt"] = ValueExpression.Literal(prompt),
                ["response"] = ValueExpression.Literal(response)
            });

            return self.EmitCall(
                AddPromptResponseObjectMethod,
                new List<ValueExpression>
                {
                    promptResponse,
                    ValueExpression.Literal(isFinalUpdate)
                });
        }

        public static ComponentRef<FusionAIAssistView, TModel> ScrollToBottom<TModel>(
            this ComponentRef<FusionAIAssistView, TModel> self)
            where TModel : class
            => self.EmitCall(ScrollToBottomMethod);
    }
}
