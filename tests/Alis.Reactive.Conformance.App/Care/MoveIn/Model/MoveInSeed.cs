using System.Text;

namespace Alis.Reactive.Conformance.App.Care;

internal static class MoveInSeed
{
    internal static MoveInWorld World()
    {
        var clock = Clock();
        return new MoveInWorld
        {
            Clock = clock,
            Communities = [HarborHouse(), MapleCourt()],
            CareLevels =
            [
                new CareLevelDefinition { Level = CareLevel.Independent, Name = "Independent", MonthlyBasePrice = 2400.00m },
                new CareLevelDefinition { Level = CareLevel.Assisted, Name = "Assisted", MonthlyBasePrice = 2800.00m },
                new CareLevelDefinition { Level = CareLevel.Memory, Name = "Memory", MonthlyBasePrice = 3600.00m },
                new CareLevelDefinition { Level = CareLevel.Skilled, Name = "Skilled", MonthlyBasePrice = 4200.00m },
            ],
            Residents =
            [
                new ResidentState { Id = "R101", Identity = HelenWard(), Address = Albany("18 Oak Street"), Allergies = ["penicillin"] },
                new ResidentState { Id = "R202", Identity = new ResidentIdentity { Name = "Nora Patel", BirthDate = new DateTime(1940, 2, 3), Email = "nora@example.test" }, Address = Albany("2 River Road") },
                new ResidentState { Id = "R203", Identity = new ResidentIdentity { Name = "Omar King", BirthDate = new DateTime(1938, 7, 11), Email = "omar@example.test" }, Address = Albany("9 Lake Road"), ActiveAdmissionId = "ADM203" },
                new ResidentState { Id = "R104", Identity = new ResidentIdentity { Name = "James Reed", BirthDate = new DateTime(1945, 1, 2), Email = "james@example.test" }, Address = Albany("5 Pine Road") },
            ],
            Drafts = [HelensDraft(clock), TheEmptyDraft(clock), OmarsSubmittedDraft(clock)],
            Policies =
            [
                new PolicyState { Number = "POL-100", ResidentId = "R101", Provider = "Harbor Mutual", Active = true, AuthorisedContactId = "CT1", MonthlyLimit = 3500.00m },
                new PolicyState { Number = "POL-200", ResidentId = "R101", Provider = "Cedar Cover", AuthorisedContactId = "CT1", MonthlyLimit = 2000.00m },
            ],
            Drugs =
            [
                new DrugDefinition { Code = "ASP75", Name = "Aspirin", Unit = "mg", MaximumDose = 300.00m },
                new DrugDefinition { Code = "AMOX", Name = "Amoxicillin", Unit = "mg", MaximumDose = 1000.00m, ConflictingAllergy = "penicillin", AlternativeCode = "AZI" },
                new DrugDefinition { Code = "AZI", Name = "Azithromycin", Unit = "mg", MaximumDose = 500.00m },
            ],
            Services =
            [
                new ServiceDefinition { Code = "DIN", Name = "Dining", MonthlyPrice = 180.00m, Revision = 3, Available = true, Capacity = 8 },
                new ServiceDefinition { Code = "PHA", Name = "Pharmacy support", MonthlyPrice = 60.00m, Revision = 3, Available = true, Capacity = 5 },
                new ServiceDefinition { Code = "TRA", Name = "Transport", MonthlyPrice = 90.00m, Revision = 3, Available = true },
            ],
            Coordinators =
            [
                new CoordinatorState { Id = "CO1", Name = "Maya Chen", CommunityIds = ["C1", "C2"], Online = true, CanAdmit = true, CanTakeDeposit = true },
                new CoordinatorState { Id = "CO2", Name = "Luis Ortiz", CommunityIds = ["C1"], Online = true, CanAdmit = true },
            ],
            Admissions = [new AdmissionState { Id = "ADM203", DraftId = "D300", ResidentId = "R203", BedId = "BED12" }],
            Rules = new ServerRules { QuarantinedFileContents = ["DENIED"], DeclinedCardNumbers = ["4000000000000002"], MinimumAdmissionAge = 55, MaximumContacts = 4 },
        };
    }

    internal static void MakeReady(MoveInWorld world)
    {
        var draft = HelensDraftIn(world);
        draft.PayerVerificationId = "V100";
        draft.Data.Payer.VerificationId = "V100";
        draft.PharmacyClearanceId = "CL100";
        draft.AcceptedQuoteRevision = 3;
        draft.Data.AcceptedQuoteRevision = 3;
        QuoteOf(draft).Pricing.Assistance.Accepted = true;
        world.Documents.Add(new DocumentState { Id = "DOC1", DraftId = "D100", Kind = DocumentKind.Consent, Name = "consent.txt", ContentType = "text/plain", Size = 14, LastModified = 1730000000000, Bytes = Encoding.UTF8.GetBytes("Helen agrees.\n"), Scan = DocumentScan.Accepted });
        world.Documents.Add(new DocumentState { Id = "DOC2", DraftId = "D100", Kind = DocumentKind.MedicationList, Name = "medications.txt", ContentType = "text/plain", Size = 14, LastModified = 1730000000001, Bytes = Encoding.UTF8.GetBytes("Aspirin 75 mg\n"), Scan = DocumentScan.Accepted });
        draft.Data.Documents.Ids = ["DOC1", "DOC2"];
        world.Payments.Add(new PaymentState { ReceiptId = "PAY0", DraftId = "D100", Amount = 500.00m, Currency = "USD", LastFour = "1111", Status = PaymentStatus.Authorised });
        draft.DepositReceiptId = "PAY0";
    }

    internal static void ChangeThePrice(MoveInWorld world)
    {
        var pharmacySupport = world.Services.Single(service => service.Code == "PHA");
        pharmacySupport.MonthlyPrice = 80.00m;
        pharmacySupport.Revision = 4;
        foreach (var draft in world.Drafts)
        {
            if (draft.Quote is not { } quote || quote.Pricing.Lines.Find(line => line.ServiceCode == "PHA") is not { } pharmacyLine) continue;
            if (pharmacyLine.Revision == pharmacySupport.Revision && pharmacyLine.Amount == pharmacySupport.MonthlyPrice) continue;
            pharmacyLine.Amount = pharmacySupport.MonthlyPrice;
            pharmacyLine.Revision = pharmacySupport.Revision;
            quote.Revision = Math.Max(quote.Revision + 1, pharmacyLine.Revision);
            quote.Pricing.MonthlyTotal = quote.Pricing.Base + quote.Pricing.Lines.Sum(line => line.Amount);
            quote.Pricing.Assistance = AssistanceTerms.For(world, draft, quote.PayerKind, quote.Pricing.MonthlyTotal);
            draft.AcceptedQuoteRevision = draft.Data.AcceptedQuoteRevision = null;
        }
    }

    internal static void TakeTheBed(MoveInWorld world)
    {
        var windowBed = BedsOf(world).Single(bed => bed.Id == "BED4");
        windowBed.State = BedState.Held;
        windowBed.Revision++;
        windowBed.Holder = new BedHolder { ResidentId = "R104", Name = "James Reed" };
        windowBed.Hold = new BedHold { CoordinatorId = "CO2", ExpiresAt = new DateTimeOffset(2026, 11, 18, 9, 45, 0, TimeSpan.FromHours(-5)) };
    }

    private static WorldClock Clock() => new()
    {
        Today = new DateTime(2026, 11, 18),
        Now = new DateTimeOffset(2026, 11, 18, 9, 0, 0, TimeSpan.FromHours(-5)),
        Culture = "en-US",
        TimeZoneId = "America/New_York",
    };

    private static Community HarborHouse() => new()
    {
        Id = "C1",
        Name = "Harbor House",
        UnavailableMeals = DietaryNeeds.Pureed,
        Site = "North",
        Buildings =
        [
            new Building { Id = "B1", Name = "Cedar", Apartments = [Apartment7(), Apartment12()] },
            new Building { Id = "B2", Name = "Willow", Apartments = [Apartment9()] },
        ],
    };

    private static Community MapleCourt() => new()
    {
        Id = "C2",
        Name = "Maple Court",
        Site = "East",
        Buildings = [new Building { Id = "B3", Name = "Elm", Apartments = [Apartment21()] }],
    };

    private static Apartment Apartment7() => new()
    {
        Id = "A7",
        Number = "7",
        CareLevels = [CareLevel.Independent, CareLevel.Assisted],
        Beds =
        [
            new Bed { Id = "BED4", Label = "Window", Revision = 12, State = BedState.Available },
            new Bed { Id = "BED5", Label = "Door", Revision = 8, State = BedState.Held, Holder = new BedHolder { ResidentId = "R202", Name = "Nora Patel" }, Hold = new BedHold { CoordinatorId = "CO2", ExpiresAt = new DateTimeOffset(2026, 11, 18, 9, 30, 0, TimeSpan.FromHours(-5)) } },
        ],
    };

    private static Apartment Apartment12() => new()
    {
        Id = "A12",
        Number = "12",
        CareLevels = [CareLevel.Memory],
        Beds = [new Bed { Id = "BED12", Label = "Bed 12", Revision = 4, State = BedState.Occupied, Holder = new BedHolder { ResidentId = "R203", Name = "Omar King", AdmissionId = "ADM203" } }],
    };

    private static Apartment Apartment9() => new()
    {
        Id = "A9",
        Number = "9",
        CareLevels = [CareLevel.Skilled],
        Beds = [new Bed { Id = "BED9", Label = "Bed 9", Revision = 2, State = BedState.Available }],
    };

    private static Apartment Apartment21() => new()
    {
        Id = "A21",
        Number = "21",
        CareLevels = [CareLevel.Independent, CareLevel.Assisted],
        Beds = [new Bed { Id = "BED21", Label = "Bed 21", Revision = 6, State = BedState.Available }],
    };

    private static ResidentIdentity HelenWard() => new()
    {
        Name = "Helen Ward",
        PreferredName = "Helen",
        BirthDate = new DateTime(1942, 5, 14),
        Email = "helen@example.test",
        EmailConfirmation = "helen@example.test",
    };

    private static Address Albany(string street) => new() { Street = street, City = "Albany", Region = "NY", PostalCode = "12207", CountryCode = "US" };

    private static DraftState HelensDraft(WorldClock clock) => new()
    {
        Id = "D100",
        Version = 7,
        Status = DraftStatus.Open,
        Data = HelensPage(clock),
        Quote = HelensQuote(),
        SavedBy = "CO1",
        SavedAt = clock.Now,
    };

    private static MoveInPage HelensPage(WorldClock clock) => new()
    {
        DraftId = "D100",
        Version = 7,
        Today = clock.Today,
        ResidentId = "R101",
        Location = new LocationInput { CommunityId = "C1", BuildingId = "B1", ApartmentId = "A7", BedId = "BED4", BedRevision = 12 },
        Care = new CareInput { Level = CareLevel.Assisted, Assisted = new AssistedCareInput { AssessmentCode = "AS-100", AssessmentDate = new DateTime(2026, 11, 17), AssistanceHours = 2.50m, MobilityNote = "Uses a walking frame" } },
        Resident = new ResidentInput { Identity = HelenWard(), Address = Albany("18 Oak Street"), MoveInDate = new DateTime(2026, 11, 20), ArrivalAt = new DateTimeOffset(2026, 11, 20, 10, 0, 0, TimeSpan.FromHours(-5)), Allergies = ["penicillin"], Notes = "Family brings clothing" },
        Contacts = new ContactSection { Rows = [AliceWard(), BenWard()] },
        Payer = HelensPayer(),
        Medications = HelensMedications(),
        Services = HelensServices(),
        Documents = new DocumentSelection { RequiredKinds = [DocumentKind.Consent, DocumentKind.MedicationList] },
        Diet = new DietInput { Needs = DietaryNeeds.Vegetarian | DietaryNeeds.LowSodium, KitchenNote = "No added salt", MealCount = 3, Confirmed = true },
        CoordinatorId = "CO1",
        OperationId = Guid.Parse("11111111-1111-4111-8111-111111111111"),
    };

    private static ContactInput AliceWard() => new()
    {
        Id = "CT1",
        Name = "Alice Ward",
        Relationship = ContactRelationship.Daughter,
        Email = "alice@example.test",
        Phone = "5185550101",
        AlternatePhone = "5185550102",
        Role = AuthorityRole.PowerOfAttorney,
        Authority = new AuthorityInput { Reference = "POA-100", StartsOn = new DateTime(2026, 1, 1), EndsOn = new DateTime(2027, 1, 1), Consent = true },
    };

    private static ContactInput BenWard() => new()
    {
        Id = "CT2",
        Name = "Ben Ward",
        Relationship = ContactRelationship.Son,
        Email = "ben@example.test",
        Phone = "5185550201",
        AlternatePhone = "5185550202",
        Role = AuthorityRole.Ordinary,
    };

    private static PayerInput HelensPayer() => new()
    {
        Kind = PayerKind.Insurance,
        ContactId = "CT1",
        Primary = new CoverageInput { PolicyNumber = "POL-100", Provider = "Harbor Mutual", StartsOn = new DateTime(2026, 1, 1), EndsOn = new DateTime(2026, 12, 31), ContributionPercent = 80.00m },
        PortalUrl = "https://payer.example.test/policies/POL-100",
    };

    private static MedicationSection HelensMedications() => new()
    {
        Rows = [new MedicationInput { Id = "M1", DrugCode = "ASP75", Dose = 75.00m, Unit = "mg", Schedule = ["Morning"], StartsOn = new DateTime(2026, 11, 18), Instructions = "With food" }],
        MinimumRoundDose = 0.50m,
        RoundDoses = [0.50m, 1.00m, null],
    };

    private static ServiceInput HelensServices() => new()
    {
        Codes = ["DIN", "PHA"],
        Visits =
        [
            new ServiceVisit { Code = "DIN", Count = 2, Active = true },
            new ServiceVisit { Code = "PHA", Count = 1, Active = true },
            new ServiceVisit { Code = "TRA" },
        ],
        PreferredDays = [new DateTime(2026, 11, 20), new DateTime(2026, 11, 21)],
        OptionalLabels = ["Window table", null],
    };

    private static QuoteState HelensQuote() => new()
    {
        Id = "Q100",
        Revision = 3,
        CareLevel = CareLevel.Assisted,
        PayerKind = PayerKind.Insurance,
        Pricing = HelensPricing(),
        ExpiresAt = new DateTimeOffset(2026, 11, 18, 10, 0, 0, TimeSpan.FromHours(-5)),
    };

    private static QuotePricing HelensPricing() => new()
    {
        Base = 2800.00m,
        Lines =
        [
            new PriceLine { ServiceCode = "DIN", Name = "Dining", Amount = 180.00m, Revision = 3 },
            new PriceLine { ServiceCode = "PHA", Name = "Pharmacy support", Amount = 60.00m, Revision = 3 },
        ],
        MonthlyTotal = 3040.00m,
        Deposit = 500.00m,
        Assistance = new AssistanceTerms { Required = true, Reference = "AST-100", Contribution = 2432.00m },
    };

    private static DraftState TheEmptyDraft(WorldClock clock) => new()
    {
        Id = "D200",
        Version = 3,
        Status = DraftStatus.Open,
        Data = new MoveInPage { DraftId = "D200", Version = 3, Today = clock.Today, ResidentId = "R101", CoordinatorId = "CO1" },
    };

    private static DraftState OmarsSubmittedDraft(WorldClock clock) => new()
    {
        Id = "D300",
        Version = 9,
        Status = DraftStatus.Submitted,
        Data = OmarsPage(clock),
    };

    private static MoveInPage OmarsPage(WorldClock clock) => new()
    {
        DraftId = "D300",
        Version = 9,
        Today = clock.Today,
        ResidentId = "R203",
        Location = new LocationInput { CommunityId = "C1", BuildingId = "B1", ApartmentId = "A12", BedId = "BED12", BedRevision = 4 },
        Care = new CareInput { Level = CareLevel.Memory },
    };

    private static DraftState HelensDraftIn(MoveInWorld world) => world.Drafts.Single(draft => draft.Id == "D100");

    private static QuoteState QuoteOf(DraftState draft) => draft.Quote ?? throw new InvalidOperationException($"Draft {draft.Id} holds no quote.");

    private static IEnumerable<Bed> BedsOf(MoveInWorld world) => world.Communities.SelectMany(community => community.Buildings).SelectMany(building => building.Apartments).SelectMany(apartment => apartment.Beds);
}
