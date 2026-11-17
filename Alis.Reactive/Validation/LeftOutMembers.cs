using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.TypeKnowledge;
using Alis.Reactive.ValuesAndSources;
using MemberStep = Alis.Reactive.Schema.MemberStep;
using RegistrationStep = Alis.Reactive.Schema.RegistrationStep;

namespace Alis.Reactive.Validation;

internal sealed class CarriedMembers(JsonSerializerOptions app)
{
    private readonly List<CarriedMember> _carried = [];

    internal IReadOnlyList<RegistrationStep>? PathOf(LambdaExpression read)
    {
        var accesses = new List<MemberExpression>();
        var held = Unconverted(read.Body);
        while (held is MemberExpression { Expression: { } holder } access)
        {
            accesses.Insert(0, access);
            held = Unconverted(holder);
        }

        var passesALeftOutMember = held == read.Parameters[0] && accesses.Any(LeftOut);
        if (!passesALeftOutMember) return null;
        var readsPastAConverter = accesses.Take(accesses.Count - 1).Any(access => !LeftOut(access) && WrittenMember.Of(access, app.GetTypeInfo(access.Expression!.Type), read).HasItsOwnConverter);
        return readsPastAConverter ? null : [.. accesses.Select(access => Step(access, read))];
    }

    internal HeldForm Reading(HeldForm form, params LambdaExpression[] reads)
    {
        var leftOut = reads.SelectMany(read => new LeftOutReads(LeftOut).Of(read)).ToList();
        if (leftOut.Count == 0) return form;
        foreach (var access in leftOut) Carry(access);
        return form.Carrying(Writing);
    }

    internal JsonSerializerOptions Writing(JsonSerializerOptions liveForm)
    {
        if (_carried.Count == 0) return liveForm;
        var carried = _carried.ToArray();
        var starts = new JsonSerializerOptions(liveForm) { TypeInfoResolver = (liveForm.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver()).WithAddedModifier(holder => Carry(holder, carried)) };
        starts.MakeReadOnly();
        return starts;
    }

    private MemberStep Step(MemberExpression access, LambdaExpression read)
    {
        if (!LeftOut(access)) return new MemberStep(WrittenMember.Of(access, app.GetTypeInfo(access.Expression!.Type), read).Name);
        Carry(access);
        return new MemberStep(WrittenMember.AsCSharpDeclaresIt(access));
    }

    private void Carry(MemberExpression access)
    {
        var holder = access.Expression!.Type;
        var name = WrittenMember.AsCSharpDeclaresIt(access);
        if (_carried.Any(carried => carried.Holder == holder && carried.Name == name)) return;
        _carried.Add(new CarriedMember(access, holder, name, ReadOf(access, holder)));
    }

    private bool LeftOut(MemberExpression access)
        => access.Expression is { } holder && app.GetTypeInfo(holder.Type).Kind == JsonTypeInfoKind.Object && WrittenMember.LeftOutByItsClass(access, app);

    private static Func<object, object?> ReadOf(MemberExpression access, Type holder)
    {
        var instance = Expression.Parameter(typeof(object), "holder");
        return ServerExpressions.Read(Expression.Lambda<Func<object, object?>>(Expression.Convert(access.Update(Expression.Convert(instance, holder)), typeof(object)), instance));
    }

    private static void Carry(JsonTypeInfo holder, IReadOnlyList<CarriedMember> carried)
    {
        foreach (var member in carried.Where(member => member.Holder == holder.Type || WrittenMember.DeclaringClass(member.Access) == holder.Type))
            WrittenMember.Carry(holder, member.Access, member.Read);
    }

    private static Expression Unconverted(Expression read)
    {
        while (read is UnaryExpression { NodeType: ExpressionType.Convert } conversion) read = conversion.Operand;
        return read;
    }

    private sealed record CarriedMember(MemberExpression Access, Type Holder, string Name, Func<object, object?> Read);

    private sealed class LeftOutReads(Func<MemberExpression, bool> leftOut) : ExpressionVisitor
    {
        private readonly List<MemberExpression> _found = [];

        internal IReadOnlyList<MemberExpression> Of(LambdaExpression read)
        {
            Visit(read);
            return _found;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (ReadsAParameter(node) && leftOut(node)) _found.Add(node);
            return base.VisitMember(node);
        }

        private static bool ReadsAParameter(MemberExpression node)
        {
            var held = node.Expression;
            while (held is MemberExpression or UnaryExpression { NodeType: ExpressionType.Convert })
                held = held is MemberExpression member ? member.Expression : ((UnaryExpression)held).Operand;
            return held is ParameterExpression;
        }
    }
}
