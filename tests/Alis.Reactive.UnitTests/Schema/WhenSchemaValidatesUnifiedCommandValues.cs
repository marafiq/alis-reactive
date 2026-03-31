namespace Alis.Reactive.UnitTests.Schema;

[TestFixture]
public class WhenSchemaValidatesUnifiedCommandValues : PlanTestBase
{
    [Test]
    public void unified_command_values_conform_to_the_plan_schema()
    {
        var json = """
                   {
                     "planId": "Test.Model",
                     "components": {},
                     "entries": [
                       {
                         "trigger": { "kind": "dom-ready" },
                         "reaction": {
                           "kind": "sequential",
                           "commands": [
                             {
                               "kind": "mutate-element",
                               "target": "status",
                               "mutation": {
                                 "kind": "set-prop",
                                 "prop": "textContent",
                                 "value": { "kind": "literal", "value": "loaded" }
                               }
                             },
                             {
                               "kind": "mutate-event",
                               "mutation": {
                                 "kind": "set-prop",
                                 "prop": "preventDefaultAction",
                                 "value": { "kind": "source", "source": { "kind": "event", "path": "evt.flags.prevent" }, "coerce": "boolean" }
                               }
                             },
                             {
                               "kind": "dispatch",
                               "event": "saved",
                               "payload": {
                                 "status": { "kind": "literal", "value": "ok" },
                                 "count": { "kind": "literal", "value": 5 }
                               }
                             }
                           ]
                         }
                       }
                     ]
                   }
                   """;

        AssertSchemaValid(json);
    }
}
