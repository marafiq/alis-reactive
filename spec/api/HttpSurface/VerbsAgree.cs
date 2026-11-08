using Alis.Reactive.Pipelines;
using Alis.Reactive.Requests;

namespace Intake;

public static class VerbsAgree
{
    public static void EachVerbNamesOnlyItsOwnTypeArgument(Pipeline<IntakeView> pipeline, PipelineVerbs<IntakeView> verbs)
    {
        var query = (QueryParts<BedQuery> parts) => { parts.Route(q => q.Ward, "north"); };
        var body = (BodyParts<BedRequest> parts) => { parts.Route(r => r.Ward, "north"); };

        Agree<GetRequest<IntakeView, NoModel>>(RequestVerbs.Get(pipeline, "/Wards"), verbs.Get("/Wards"));
        Agree<GetRequest<IntakeView, BedQuery>>(
            pipeline.Get<IntakeView, BedQuery>("/Wards/{ward}/Beds", query),
            verbs.Get<BedQuery>("/Wards/{ward}/Beds", query));

        Agree<HeadRequest<IntakeView, NoModel>>(pipeline.Head("/Wards"), verbs.Head("/Wards"));
        Agree<HeadRequest<IntakeView, BedQuery>>(
            pipeline.Head<IntakeView, BedQuery>("/Wards/{ward}/Beds", query),
            verbs.Head<BedQuery>("/Wards/{ward}/Beds", query));

        Agree<OptionsRequest<IntakeView, NoModel>>(pipeline.Options("/Wards"), verbs.Options("/Wards"));
        Agree<OptionsRequest<IntakeView, BedQuery>>(
            pipeline.Options<IntakeView, BedQuery>("/Wards/{ward}/Beds", query),
            verbs.Options<BedQuery>("/Wards/{ward}/Beds", query));

        Agree<PostRequest<IntakeView, NoModel>>(pipeline.Post("/Wards"), verbs.Post("/Wards"));
        Agree<PostRequest<IntakeView, BedRequest>>(
            pipeline.Post<IntakeView, BedRequest>("/Wards/{ward}/Beds", body),
            verbs.Post<BedRequest>("/Wards/{ward}/Beds", body));

        Agree<PutRequest<IntakeView, NoModel>>(pipeline.Put("/Wards"), verbs.Put("/Wards"));
        Agree<PutRequest<IntakeView, BedRequest>>(
            pipeline.Put<IntakeView, BedRequest>("/Wards/{ward}/Beds", body),
            verbs.Put<BedRequest>("/Wards/{ward}/Beds", body));

        Agree<DeleteRequest<IntakeView, NoModel>>(pipeline.Delete("/Wards"), verbs.Delete("/Wards"));
        Agree<DeleteRequest<IntakeView, BedRequest>>(
            pipeline.Delete<IntakeView, BedRequest>("/Wards/{ward}/Beds", body),
            verbs.Delete<BedRequest>("/Wards/{ward}/Beds", body));

        Agree<PageUrl<IntakePage>>(pipeline.PageUrl<IntakeView, IntakePage>(), verbs.PageUrl<IntakePage>());
        Agree<PageUrl<IntakePage>>(pipeline.PageUrl<IntakeView, IntakePage>("/Wards/{ward}/Intake"), verbs.PageUrl<IntakePage>("/Wards/{ward}/Intake"));
    }

    private static void Agree<TRequest>(TRequest stub, TRequest design)
    {
        _ = stub;
        _ = design;
    }
}
