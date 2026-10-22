using FeatureFlags.Domain;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FeatureFlags.Models;

/// <summary>The flag list: which app, which filter, what search text.</summary>
public class FlagsPageModel
{
    public string App { get; set; } = Apps.Web;

    /// <summary>all, everyone, some or off.</summary>
    public string Filter { get; set; } = FlagFilters.All;

    public string Query { get; set; } = "";
}

public static class FlagFilters
{
    public const string All = "all";
    public const string Everyone = "everyone";
    public const string Some = "some";
    public const string Off = "off";
}

/// <summary>Payload of the flags-changed event: the app the list should show.</summary>
public class FlagsChanged
{
    public string App { get; set; } = Apps.Web;
}

/// <summary>
/// One flag form. Create and edit are the same screen: an empty Id means the flag is new.
/// The whole form is posted on every change so the server can answer "who has it" from
/// the draft before anything is saved.
/// </summary>
public class FlagEditModel
{
    public string Id { get; set; } = "";
    public string App { get; set; } = Apps.Web;

    /// <summary>App the draft was copied from, empty when the flag was not copied.</summary>
    public string CopiedFrom { get; set; } = "";

    /// <summary>Flag id the draft was copied from, so Cancel can return to it.</summary>
    public string CopySource { get; set; } = "";

    /// <summary>"true" once the key was typed by hand, so the name stops filling it in.</summary>
    public string KeyTouched { get; set; } = "";

    public string? Name { get; set; } = "";
    public string? Key { get; set; } = "";
    public string? Description { get; set; } = "";
    public bool On { get; set; } = true;
    public bool Everyone { get; set; }
    public string[] Sites { get; set; } = [];
    public string[] Companies { get; set; } = [];
    public string[] Communities { get; set; } = [];
    public string[] KeepOff { get; set; } = [];

    /// <summary>Site opened in the "Who has it" strip: a site id, "none", or empty for the default.</summary>
    public string WhoSite { get; set; } = "";

    /// <summary>Site the user just tapped in the strip. Not an input; sent with that tap only.</summary>
    public string PickSite { get; set; } = "";

    /// <summary>
    /// Not an input. MVC validation walks every property of a posted form before the action
    /// runs, and a posted form can carry a JSON null Id (see <see cref="Normalized"/>).
    /// </summary>
    [ValidateNever]
    public bool IsNew => Id.Length == 0;

    /// <summary>
    /// A posted form carries JSON nulls for empty pickers; turn them back into empty values
    /// so the rest of the code reads one shape.
    /// </summary>
    public FlagEditModel Normalized()
    {
        Id ??= "";
        App = Apps.Exists(App ?? "") ? App! : Apps.Web;
        CopiedFrom ??= "";
        CopySource ??= "";
        KeyTouched ??= "";
        Name ??= "";
        Key ??= "";
        Description ??= "";
        Sites ??= [];
        Companies ??= [];
        Communities ??= [];
        KeepOff ??= [];
        WhoSite ??= "";
        PickSite ??= "";
        return this;
    }

    public static FlagEditModel From(Flag flag) => new()
    {
        Id = flag.Id,
        App = flag.App,
        Name = flag.Name,
        Key = flag.Key,
        Description = flag.Description,
        On = flag.On,
        Everyone = flag.Everyone,
        Sites = flag.Sites.ToArray(),
        Companies = flag.Companies.ToArray(),
        Communities = flag.Communities.ToArray(),
        KeepOff = flag.KeepOff.ToArray()
    };

    /// <summary>
    /// The draft as a flag. Existing flags keep their stored key; the key is fixed once code uses it.
    /// </summary>
    public Flag ToDraft(Flag? saved) => new()
    {
        Id = saved?.Id ?? "",
        App = saved?.App ?? App,
        Name = (Name ?? "").Trim(),
        Key = saved?.Key ?? (Key ?? "").Trim(),
        Description = (Description ?? "").Trim(),
        On = On,
        Everyone = Everyone,
        Sites = Flag.Set(Sites),
        Companies = Flag.Set(Companies),
        Communities = Flag.Set(Communities),
        KeepOff = Flag.Set(KeepOff)
    };
}

/// <summary>"What's on for a community?" for one app.</summary>
public class CommunityCheckModel
{
    public string App { get; set; } = Apps.Web;
    public string CommunityId { get; set; } = "";
}

/// <summary>One option in a site, company or community picker.</summary>
public sealed record PickItem(string Id, string Name, string Group);
