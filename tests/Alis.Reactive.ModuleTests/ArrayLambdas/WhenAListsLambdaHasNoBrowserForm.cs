using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.ArrayLambdas;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.ValuesAndSources;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.ArrayLambdas;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAListsLambdaHasNoBrowserForm
{
    [Test]
    public void a_method_the_framework_does_not_translate_is_refused_naming_the_call()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Flagged.Set(AnyOf(cart.Doses, d => Doubled(d.Units) > 4));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'Doubled(d.Units)' has no browser form in a list's lambda; use a member the framework translates, or move the work to the server."));
    }

    [Test]
    public void a_value_the_view_computes_by_a_call_is_refused_naming_a_local_computed_before_the_lambda()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();
        var ward = "north";

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Flagged.Set(AnyOf(cart.Doses, d => d.Ward == ward.ToUpperInvariant()));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'ward.ToUpperInvariant()' computes a value inside a list's lambda; compute it into a local before the lambda, such as var ward = Model.Ward.ToUpper();."));
    }

    [Test]
    public void a_comparison_of_two_values_the_view_holds_is_refused_since_it_reads_no_item()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();
        var ward = "North";

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Flagged.Set(AnyOf(cart.Doses, d => ward == "North"));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'ward == \"North\"' computes a value inside a list's lambda; compute it into a local before the lambda, such as var ward = Model.Ward.ToUpper();."));
    }

    [Test]
    [SetCulture("de-DE")]
    public void a_refusal_writes_a_fraction_as_csharp_writes_it_whatever_the_culture_the_view_renders_under()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();
        var limit = 2.0;

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Flagged.Set(AnyOf(cart.Doses, d => d.Volume > limit * 1.5));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'limit * 1.5' computes a value inside a list's lambda; compute it into a local before the lambda, such as var ward = Model.Ward.ToUpper();."));
    }

    [Test]
    public void a_refusal_writes_its_term_with_the_operators_and_the_grouping_csharp_writes()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();
        var late = true;
        var early = false;
        var ward = "North";
        var limit = 2;

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Flagged.Set(AnyOf(cart.Doses, d => d.Units > (late && !early || ward == "South" ? (limit + 1) * 2 : limit)));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'late && !early || ward == \"South\" ? (limit + 1) * 2 : limit' computes a value inside a list's lambda; " +
                "compute it into a local before the lambda, such as var ward = Model.Ward.ToUpper();."));
    }

    [Test]
    public void a_member_compared_with_an_object_the_view_holds_is_refused_since_the_browser_compares_objects_by_reference()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();
        var lead = new Carer();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Flagged.Set(AnyOf(cart.Doses, d => d.Carer == lead));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Carer == lead' compares objects, and the browser compares objects by reference; compare a member of each, such as d => d.Carer.Name == lead."));
    }

    [Test]
    public void two_members_that_hold_objects_compared_are_refused_since_the_browser_compares_objects_by_reference()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Flagged.Set(AnyOf(cart.Doses, d => d.Carer == d.Backup));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Carer == d.Backup' compares objects, and the browser compares objects by reference; compare a member of each, such as d => d.Carer.Name == lead."));
    }

    [Test]
    public void two_members_ordered_that_are_no_numbers_chars_or_dates_of_one_type_are_refused_since_the_browser_cannot_order_them_as_dotnet_does()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Flagged.Set(AnyOf(cart.Doses, d => d.Status > d.Previous));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Status > d.Previous' orders values the browser cannot order as .NET does; order numbers, a char, or a date or a time, or order on the server."));
    }

    [Test]
    public void a_cast_that_narrows_a_number_is_refused_since_csharp_changes_the_value_and_the_browser_does_not()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Flagged.Set(AnyOf(cart.Doses, d => (int)d.Volume > 2));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'(int)d.Volume' casts a number to a narrower one, which changes the value as C# computes it and not as the browser does; compare the member itself."));
    }

    [Test]
    public void a_coalesce_whose_left_side_csharp_converts_by_a_user_defined_conversion_is_refused_since_the_browser_has_none()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Flagged.Set(AnyOf(cart.Doses, d => (d.Seal ?? "none") == "S4"));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Seal ?? \"none\"' converts its left side by a user-defined conversion, which the browser does not have; coalesce values of one type, or move the work to the server."));
    }

    [Test]
    public void a_member_compared_through_a_user_defined_conversion_is_refused_since_the_browser_has_none()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Flagged.Set(AnyOf(cart.Doses, d => d.Seal == "S4"));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Seal' is converted by a user-defined conversion, which the browser does not have; compare a member it reads, such as d => d.Seal.Tag == \"S4\", or move the work to the server."));
    }

    [Test]
    public void a_cast_from_a_binary_fraction_to_decimal_is_refused_since_csharp_rounds_it_and_the_browser_does_not()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Flagged.Set(AnyOf(cart.Doses, d => (decimal)d.Volume > 2.5m));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Volume' is cast to or from decimal, which rounds as C# computes it and not as the browser does; keep the member's own type, or move the work to the server."));
    }

    [Test]
    public void a_value_the_view_reads_through_a_null_is_refused_naming_the_read()
    {
        var round = new MedicationRound("North", null);
        var html = Views.Of(round);
        var plan = html.ReactivePlan<MedicationRound>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Flagged.Set(cart.Doses.Any(d => d.Drug == round.Cover!.Name));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo("'round.Cover.Name' reads a member of a value that is null when the view renders."));
    }

    [Test]
    public void a_cast_that_takes_the_value_of_a_null_the_view_holds_is_refused_since_dotnet_refuses_it()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();
        int? none = null;

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Flagged.Set(cart.Doses.Any(d => d.Units == (int)none!));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'none' is null when the view renders, and the cast takes its value, which .NET refuses; compare the nullable itself, such as d => d.Priority == limit."));
    }

    [Test]
    public void a_read_past_a_member_its_own_converter_writes_is_refused_since_the_browser_holds_what_the_converter_writes()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Flagged.Set(cart.Doses.Any(d => d.Tagged.Tag == "S4"));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Tagged.Tag' reads past 'd.Tagged', which its own JSON converter writes, so the browser holds what the converter writes, not its members; " +
                "read 'd.Tagged' itself, or read past it on the server."));
    }

    private static TypedSource<bool> AnyOf(ITypedSource<IEnumerable<CartDose>> doses, Expression<Func<CartDose, bool>> predicate) => doses.Any(predicate);

    private static int Doubled(int units) => units * 2;
}
