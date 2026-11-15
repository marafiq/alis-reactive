using System;
using System.Collections.Generic;

namespace Alis.Reactive.ModuleTests.Validation;

public sealed class Admission
{
    public string? ResidentName { get; set; }
    public int Room { get; set; }
    public int Limit { get; set; }
    public int? OptionalRoom { get; set; }
    public int? OptionalLimit { get; set; }
    public bool Required { get; set; }
    public decimal Fee { get; set; }
    public decimal? OptionalFee { get; set; }
    public CareLevel Care { get; set; }
    public CareLevel? OptionalCare { get; set; }
    public Contact? Kin { get; set; }
    public List<Contact> Contacts { get; set; } = [];
    public List<int> Rooms { get; set; } = [];
    public DateTime AdmittedAt { get; set; }
    public DateTime ExpectedAt { get; set; }
    public DateTimeOffset SeenAt { get; set; }
    public DateTimeOffset ExpectedSeenAt { get; set; }
    public DateOnly AdmissionDay { get; set; }
    public TimeOnly? Starts { get; set; }
    public TimeSpan Stay { get; set; }
}

public enum CareLevel
{
    Residential = 1,
    Nursing = 2,
}

public sealed class Contact
{
    public string? FullName { get; set; }
}
