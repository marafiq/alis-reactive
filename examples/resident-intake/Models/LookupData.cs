namespace ResidentIntake.Models;

public record LookupItem(string Id, string Name);

// Response shapes for API endpoints
public class FacilitiesResponse
{
    public List<LookupItem> Facilities { get; set; } = [];
}

public class CareLevelsResponse
{
    public List<LookupItem> Levels { get; set; } = [];
}

public class UnitsResponse
{
    public List<LookupItem> Units { get; set; } = [];
}

public class ConfirmationResponse
{
    public string Number { get; set; } = "";
}
