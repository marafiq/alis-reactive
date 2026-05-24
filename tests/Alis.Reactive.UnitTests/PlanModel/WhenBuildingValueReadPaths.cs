using Alis.Reactive.PlanModel;

namespace Alis.Reactive.UnitTests.PlanModel;

[TestFixture]
public class WhenBuildingValueReadPaths
{
    [Test]
    public void payload_member_reads_carry_a_structured_path()
    {
        var producer = (ReadProducer)ValueProducer.Read(
            PayloadSource.Event(),
            "resident.address.zipCode");

        Assert.That(producer.Member, Is.EqualTo("resident.address.zipCode"));
        Assert.That(producer.Path.ToString(), Is.EqualTo("resident.address.zipCode"));
        Assert.That(producer.Path.Segments, Has.Count.EqualTo(3));
    }

    [Test]
    public void whole_response_body_reads_keep_an_empty_path()
    {
        var producer = (ReadProducer)ValueProducer.Read(
            PayloadSource.Success(),
            "responseBody");

        Assert.That(producer.Member, Is.EqualTo("responseBody"));
        Assert.That(producer.Path.Segments, Is.Empty);
    }

    [Test]
    public void component_member_reads_do_not_treat_member_names_as_payload_paths()
    {
        var producer = (ReadProducer)ValueProducer.Read(
            ComponentSource.Of("resident-name"),
            "value");

        Assert.That(producer.Member, Is.EqualTo("value"));
        Assert.That(producer.Path.Segments, Is.Empty);
    }

    [Test]
    public void dotted_paths_reject_empty_segments()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Alis.Reactive.PlanModel.Path.Parse("resident..zipCode"));

        Assert.That(exception!.Message, Does.Contain("resident..zipCode"));
        Assert.That(exception.Message, Does.Contain("empty segment"));
    }

    [Test]
    public void payload_paths_overlap_when_one_claims_a_parent_path()
    {
        var parent = Alis.Reactive.PlanModel.Path.Parse("resident.address");
        var child = Alis.Reactive.PlanModel.Path.Parse("resident.address.city");

        Assert.That(parent.Overlaps(child), Is.True);
        Assert.That(child.Overlaps(parent), Is.True);
    }

    [Test]
    public void payload_paths_do_not_overlap_for_sibling_paths()
    {
        var city = Alis.Reactive.PlanModel.Path.Parse("resident.address.city");
        var zip = Alis.Reactive.PlanModel.Path.Parse("resident.address.zipCode");

        Assert.That(city.Overlaps(zip), Is.False);
        Assert.That(zip.Overlaps(city), Is.False);
    }
}
