using System.Text.Json;

namespace Alis.Reactive.UnitTests;

/// <summary>
/// WhileLoading and OnAllSettled carry reaction graphs. If the DSL can express
/// a deterministic graph there, the plan must preserve it and let the runtime
/// select the immediate or async lane while executing.
/// </summary>
[TestFixture]
public class WhenBuildingLifecycleReactionGraphs : PlanTestBase
{
    private const string Branch = "branch";
    private const string Request = "request";
    private const string Sequence = "sequence";
    private const string Set = "set";

    public class Payload
    {
        public string Role { get; set; } = "";
    }

    [Test]
    public void while_loading_keeps_guarded_reaction_in_request_before_stage()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<Payload>("save", (args, p) =>
        {
            p.Post("/api/save")
             .WhileLoading(wl =>
             {
                 wl.When(args, x => x.Role).Eq("admin")
                   .Then(t => t.Element("spinner").Show());
             });
        });

        var before = RenderedPlan.From(plan).FirstRequestBeforeStage();

        before.AssertSingleReactionKind(Branch);
    }

    [Test]
    public void while_loading_keeps_nested_request_in_request_before_stage()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/save")
             .WhileLoading(wl =>
            {
                wl.Get("/api/other");
            });
        });

        var before = RenderedPlan.From(plan).FirstRequestBeforeStage();

        before.AssertSingleReactionKind(Request);
        Assert.That(before.SingleReaction.GetProperty("request").GetProperty("url").GetString(), Is.EqualTo("/api/other"));
    }

    [Test]
    public void while_loading_keeps_immediate_commands_in_request_before_stage()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/save")
             .WhileLoading(wl =>
            {
                wl.Element("spinner").Show();
                wl.Element("save-btn").Hide();
            });
        });

        var before = RenderedPlan.From(plan).FirstRequestBeforeStage();

        before.AssertReactionKinds(Set, Set);
    }

    [Test]
    public void on_all_settled_keeps_guarded_reaction_as_parallel_completion()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<Payload>("batch", (args, p) =>
        {
            p.Parallel(
                b => b.Post("/api/a"),
                b => b.Post("/api/b")
            ).OnAllSettled(oas =>
            {
                oas.When(args, x => x.Role).Eq("admin")
                   .Then(t => t.Element("result").Show());
            });
        });

        var completion = RenderedPlan.From(plan).FirstParallelCompletion();

        completion.AssertOnSettledReactionKind(Branch);
    }

    [Test]
    public void on_all_settled_keeps_nested_request_as_parallel_completion()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Parallel(
                b => b.Post("/api/a"),
                b => b.Post("/api/b")
            ).OnAllSettled(oas =>
            {
                oas.Get("/api/other");
            });
        });

        var completion = RenderedPlan.From(plan).FirstParallelCompletion();

        completion.AssertOnSettledReactionKind(Request);
        Assert.That(completion.Reaction.GetProperty("request").GetProperty("url").GetString(), Is.EqualTo("/api/other"));
    }

    [Test]
    public void on_all_settled_keeps_immediate_commands_as_parallel_completion_sequence()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Parallel(
                b => b.Post("/api/a"),
                b => b.Post("/api/b")
            ).OnAllSettled(oas =>
            {
                oas.Element("spinner").Hide();
                oas.Element("status").SetText("Done");
            });
        });

        var completion = RenderedPlan.From(plan).FirstParallelCompletion();

        completion.AssertOnSettledReactionKind(Sequence);
        Assert.That(completion.Reaction.GetProperty("steps").GetArrayLength(), Is.EqualTo(2));
    }

    private sealed class RenderedPlan
    {
        private readonly JsonElement _firstReaction;

        private RenderedPlan(JsonElement firstReaction)
        {
            _firstReaction = firstReaction;
        }

        internal static RenderedPlan From(ReactivePlan<TestModel> plan)
        {
            using var document = JsonDocument.Parse(plan.Render());
            var firstReaction = document.RootElement
                .GetProperty("behaviors")[0]
                .GetProperty("reaction")
                .Clone();

            return new RenderedPlan(firstReaction);
        }

        internal LifecycleReactionStage FirstRequestBeforeStage() =>
            new(_firstReaction.GetProperty("request").GetProperty("before"));

        internal ParallelCompletionStage FirstParallelCompletion() =>
            new(_firstReaction.GetProperty("completion"));
    }

    private readonly struct LifecycleReactionStage
    {
        private readonly JsonElement _stage;

        internal LifecycleReactionStage(JsonElement stage)
        {
            _stage = stage;
        }

        internal JsonElement SingleReaction
        {
            get
            {
                Assert.That(_stage.GetArrayLength(), Is.EqualTo(1));
                return _stage[0];
            }
        }

        internal void AssertSingleReactionKind(string kind)
        {
            AssertReactionKinds(kind);
        }

        internal void AssertReactionKinds(params string[] kinds)
        {
            Assert.That(_stage.GetArrayLength(), Is.EqualTo(kinds.Length));

            for (var i = 0; i < kinds.Length; i++)
                Assert.That(_stage[i].GetProperty("kind").GetString(), Is.EqualTo(kinds[i]));
        }
    }

    private readonly struct ParallelCompletionStage
    {
        private readonly JsonElement _completion;

        internal ParallelCompletionStage(JsonElement completion)
        {
            _completion = completion;
        }

        internal JsonElement Reaction => _completion.GetProperty("reaction");

        internal void AssertOnSettledReactionKind(string kind)
        {
            Assert.That(_completion.GetProperty("kind").GetString(), Is.EqualTo("on-settled"));
            Assert.That(Reaction.GetProperty("kind").GetString(), Is.EqualTo(kind));
        }
    }
}
