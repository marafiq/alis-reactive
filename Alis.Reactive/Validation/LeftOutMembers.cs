using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.Validation;

internal static class LeftOutMembers
{
    internal static bool Checks(LambdaExpression read, JsonSerializerOptions app)
        => Unconverted(read.Body) is MemberExpression { Expression: ParameterExpression holder } access && holder == read.Parameters[0]
            && WrittenMember.LeftOutByItsClass(access, app);

    internal static TRead ReadAsStarts<T, TRead>(TRead read, T start, JsonSerializerOptions app)
        where TRead : LambdaExpression
        => new StartsOfLeftOutMembers<T>(read.Parameters[0], start, app).VisitAndConvert(read, nameof(ReadAsStarts));

    private static Expression Unconverted(Expression read)
    {
        while (read is UnaryExpression { NodeType: ExpressionType.Convert } conversion) read = conversion.Operand;
        return read;
    }

    private sealed class StartsOfLeftOutMembers<T>(ParameterExpression checkedModel, T start, JsonSerializerOptions app) : ExpressionVisitor
    {
        public override Expression? Visit(Expression? node)
        {
            var readsOnlyLeftOutMembers = node is not null and not LambdaExpression && node.Type != typeof(void) && new Census(checkedModel, app).ReadsOnlyLeftOutMembers(node);
            if (!readsOnlyLeftOutMembers) return base.Visit(node);
            var startOf = ServerExpressions.Read(Expression.Lambda<Func<T, object?>>(Expression.Convert(node!, typeof(object)), checkedModel));
            return Expression.Constant(startOf(start), node!.Type);
        }
    }

    private sealed class Census(ParameterExpression checkedModel, JsonSerializerOptions app) : ExpressionVisitor
    {
        private readonly HashSet<ParameterExpression> _declared = [];
        private int _leftOut;
        private bool _readsWhatTheBodyHolds;

        internal bool ReadsOnlyLeftOutMembers(Expression node)
        {
            Visit(node);
            return _leftOut > 0 && !_readsWhatTheBodyHolds;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != checkedModel || !WrittenMember.LeftOutByItsClass(node, app)) return base.VisitMember(node);
            _leftOut++;
            return node;
        }

        protected override Expression VisitLambda<TDelegate>(Expression<TDelegate> node)
        {
            _declared.UnionWith(node.Parameters);
            return base.VisitLambda(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (!_declared.Contains(node)) _readsWhatTheBodyHolds = true;
            return node;
        }
    }
}
