**Resident move-in: complete application specification**

Derived from detached HEAD `e9b49a1ebd498fb9de1a290b501d51dcf35e1eaa`. This document specifies work to build. No build or test ran. No repository file changed.

The governing sources are [AGENTS.md, Vision and What governs what](AGENTS.md:75), [the schema](spec/Core/schema.md:1), [the door map](spec/Fluent/sentences.json:1), [core PublicAPI](Alis.Reactive/PublicAPI/net10.0/PublicAPI.Unshipped.txt:1), [Native PublicAPI](Alis.Reactive.Native/PublicAPI/net10.0/PublicAPI.Unshipped.txt:1), and the supplied [build-a-screen skill](.claude/skills/build-a-screen/SKILL.md:8).

`MI.f.i` identifies feature `f`, interaction `i`. Application classes, endpoints, declarations and views named below are the proposed application’s contracts. Framework doors retain their source names.

**1. THE DOMAIN**

The application is an admissions desk for senior living. Coordinators prepare a resident’s move-in, secure clinical and financial clearance, collect documents, accept a price, and admit the resident to an available bed.

Each test owns one in-memory `MoveInWorld`. No mutable data crosses worlds. HTTP endpoints, component methods, server events and hub methods use that same world. A page refresh reads the world again.

`Today` is model data. The browser does not calculate it from its clock.

**Model**

The notation below lists every member of each application record. `?` denotes nullable domain data; it does not denote an optional plan field. Collections start empty unless the seed specifies their contents.

| Class or record | Members and types |
|---|---|
| `MoveInWorld` | `Clock: WorldClock`; `Communities: List<Community>`; `CareLevels: List<CareLevelDefinition>`; `Residents: List<ResidentState>`; `Drafts: List<DraftState>`; `Policies: List<PolicyState>`; `Drugs: List<DrugDefinition>`; `Services: List<ServiceDefinition>`; `Coordinators: List<CoordinatorState>`; `Documents: List<DocumentState>`; `Payments: List<PaymentState>`; `Admissions: List<AdmissionState>`; `Waitlist: List<WaitlistState>`; `Reviews: List<ReviewState>`; `Messages: List<CoordinatorMessage>`; `Acknowledgements: List<AcknowledgementState>`; `History: List<HistoryEntry>`; `Rules: ServerRules` |
| `WorldClock` | `Today: DateTime`; `Now: DateTimeOffset`; `Culture: string`; `TimeZoneId: string` |
| `Community` | `Id: string`; `Name: string`; `Site: string`; `TimeZoneId: string`; `Buildings: List<Building>` |
| `Building` | `Id: string`; `Name: string`; `Apartments: List<Apartment>` |
| `Apartment` | `Id: string`; `Number: string`; `CareLevels: List<CareLevel>`; `Beds: List<Bed>` |
| `Bed` | `Id: string`; `Label: string`; `Revision: int`; `State: BedState`; `Holder: BedHolder?`; `Hold: BedHold?`; `NeedsWater: bool` |
| `BedHolder` | `ResidentId: string`; `Name: string`; `AdmissionId: string?` |
| `BedHold` | `CoordinatorId: string`; `ExpiresAt: DateTimeOffset` |
| `CareLevelDefinition` | `Level: CareLevel`; `Name: string`; `MonthlyBasePrice: decimal` |
| `ResidentState` | `Id: string`; `Identity: ResidentIdentity`; `Address: Address`; `Allergies: List<string>`; `ActiveAdmissionId: string?`; `Restricted: bool` |
| `ResidentIdentity` | `Name: string`; `PreferredName: string?`; `BirthDate: DateTime`; `Email: string`; `EmailConfirmation: string` |
| `Address` | `Street: string`; `City: string`; `Region: string`; `PostalCode: string`; `CountryCode: string` |
| `DraftState` | `Id: string`; `Version: int`; `Status: DraftStatus`; `Data: MoveInPage`; `PayerVerificationId: string?`; `PharmacyClearanceId: string?`; `Quote: QuoteState?`; `AcceptedQuoteRevision: int?`; `DepositReceiptId: string?`; `SavedBy: string`; `SavedAt: DateTimeOffset` |
| `MoveInPage` | `DraftId: string`; `Version: int`; `Today: DateTime`; `ResidentId: string`; `Location: LocationInput`; `Care: CareInput`; `Resident: ResidentInput`; `Contacts: ContactSection`; `Payer: PayerInput`; `Medications: MedicationSection`; `Services: ServiceInput`; `Diet: DietInput`; `Documents: DocumentSelection`; `Review: ReviewInput`; `AcceptedQuoteRevision: int?`; `CoordinatorId: string`; `Rehearsal: bool`; `OperationId: Guid` |
| `LocationInput` | `CommunityId: string`; `BuildingId: string`; `ApartmentId: string`; `BedId: string`; `BedRevision: int` |
| `CareInput` | `Level: CareLevel`; `Independent: IndependentCareInput?`; `Assisted: AssistedCareInput?`; `Memory: MemoryCareInput?`; `Skilled: SkilledCareInput?` |
| `IndependentCareInput` | `EmergencyCallAcknowledged: bool` |
| `AssistedCareInput` | `AssessmentCode: string`; `AssessmentDate: DateTime`; `AssistanceHours: decimal`; `MobilityNote: string` |
| `MemoryCareInput` | `AssessmentCode: string`; `SecureUnitConsent: bool`; `RepresentativeContactId: string` |
| `SkilledCareInput` | `OrderReference: string`; `OrderDate: DateTime`; `Prescriber: string` |
| `ResidentInput` | `Identity: ResidentIdentity`; `Address: Address`; `MoveInDate: DateTime`; `ArrivalAt: DateTimeOffset`; `Allergies: List<string>`; `Notes: string` |
| `ContactSection` | `Rows: List<ContactInput>` |
| `ContactInput` | `Id: string`; `Name: string`; `Relationship: ContactRelationship`; `Email: string`; `Phone: string`; `AlternatePhone: string`; `Role: AuthorityRole`; `Authority: AuthorityInput?` |
| `AuthorityInput` | `Reference: string`; `StartsOn: DateTime`; `EndsOn: DateTime?`; `Consent: bool` |
| `PayerInput` | `Kind: PayerKind`; `ContactId: string`; `Primary: CoverageInput`; `HasSecondary: bool`; `Secondary: SecondaryCoverageInput?`; `PortalUrl: string`; `VerificationId: string?` |
| `CoverageInput` | `PolicyNumber: string`; `Provider: string`; `StartsOn: DateTime`; `EndsOn: DateTime`; `ContributionPercent: decimal` |
| `SecondaryCoverageInput` | `PolicyNumber: string`; `Provider: string`; `StartsOn: DateTime`; `EndsOn: DateTime`; `ContributionPercent: decimal` |
| `MedicationSection` | `Rows: List<MedicationInput>`; `MinimumRoundDose: decimal?`; `RoundDoses: List<decimal?>` |
| `MedicationInput` | `Id: string`; `DrugCode: string`; `Dose: decimal`; `Unit: string`; `Schedule: List<string>`; `StartsOn: DateTime`; `EndsOn: DateTime?`; `Instructions: string` |
| `DrugDefinition` | `Code: string`; `Name: string`; `Unit: string`; `MaximumDose: decimal`; `ConflictingAllergy: string?`; `AlternativeCode: string?` |
| `ServiceInput` | `Codes: List<string>`; `Visits: List<ServiceVisit>`; `PreferredDays: List<DateTime>`; `OptionalLabels: List<string?>` |
| `ServiceVisit` | `Code: string`; `Count: int`; `Active: bool` |
| `ServiceDefinition` | `Code: string`; `Name: string`; `MonthlyPrice: decimal`; `Revision: int`; `Available: bool`; `Capacity: int` |
| `DietInput` | `Needs: DietaryNeeds`; `KitchenNote: string`; `MealCount: int`; `Confirmed: bool` |
| `DocumentSelection` | `Ids: List<string>`; `RequiredKinds: List<DocumentKind>` |
| `ReviewInput` | `Reference: string?`; `SelectedServiceCount: int`; `VisitCount: int`; `Ready: bool`; `Result: Components.Record?`; `ReviewedAt: DateTimeOffset?` |
| `QuoteState` | `Id: string`; `Revision: int`; `CareLevel: CareLevel`; `PayerKind: PayerKind`; `Pricing: QuotePricing`; `ExpiresAt: DateTimeOffset` |
| `QuotePricing` | `Base: decimal`; `Lines: List<PriceLine>`; `MonthlyTotal: decimal`; `Deposit: decimal`; `Assistance: AssistanceTerms` |
| `PriceLine` | `ServiceCode: string`; `Name: string`; `Amount: decimal`; `Revision: int` |
| `AssistanceTerms` | `Required: bool`; `Reference: string`; `Contribution: decimal`; `Accepted: bool` |
| `PolicyState` | `Number: string`; `ResidentId: string`; `Provider: string`; `Active: bool`; `AuthorisedContactId: string`; `MonthlyLimit: decimal`; `Blocked: bool` |
| `ReviewState` | `Id: string`; `DraftId: string`; `Kind: ReviewKind`; `DraftVersion: int`; `Passed: bool`; `Reference: string`; `Message: string` |
| `DocumentState` | `Id: string`; `DraftId: string`; `Kind: DocumentKind`; `Name: string`; `ContentType: string`; `Size: long`; `LastModified: long`; `Bytes: byte[]`; `Revision: int`; `Scan: DocumentScan`; `UploadedBy: string` |
| `PaymentState` | `ReceiptId: string`; `DraftId: string`; `OperationId: Guid`; `Amount: decimal`; `Currency: string`; `LastFour: string`; `Status: PaymentStatus`; `CreatedAt: DateTimeOffset` |
| `AdmissionState` | `Id: string`; `DraftId: string`; `ResidentId: string`; `BedId: string`; `QuoteRevision: int`; `MonthlyTotal: decimal`; `DepositReceiptId: string`; `Arrival: ArrivalState` |
| `ArrivalState` | `At: DateTimeOffset`; `ChecklistRequired: bool`; `ChecklistId: string`; `Completed: bool` |
| `WaitlistState` | `Id: string`; `DraftId: string`; `CommunityId: string`; `CareLevel: CareLevel`; `ResidentId: string`; `Reason: string`; `Position: int` |
| `CoordinatorState` | `Id: string`; `Name: string`; `CommunityIds: List<string>`; `Online: bool`; `CanAdmit: bool`; `CanTakeDeposit: bool` |
| `CoordinatorMessage` | `Id: string`; `DraftId: string`; `CoordinatorId: string`; `Text: string`; `Sequence: int`; `SentAt: DateTimeOffset` |
| `AcknowledgementState` | `Id: string`; `DraftId: string`; `Kind: string`; `Reference: string`; `CoordinatorId: string`; `Text: string` |
| `HistoryEntry` | `Sequence: int`; `DraftId: string`; `OperationId: Guid`; `Action: string`; `Actor: string`; `At: DateTimeOffset`; `ResultReference: string` |
| `ServerRules` | `ClosedBuildingIds: List<string>`; `RevokedAuthorityReferences: List<string>`; `BlockedPolicyNumbers: List<string>`; `QuarantinedFileContents: List<string>`; `DeclinedCardNumbers: List<string>`; `RoomReviewLocked: bool`; `DocumentUploadLocked: bool`; `MinimumAdmissionAge: int`; `MaximumContacts: int` |

The domain enums are:

| Enum | Values |
|---|---|
| `CareLevel` | `Independent=1`, `Assisted=2`, `Memory=3`, `Skilled=4` |
| `BedState` | `Available`, `Held`, `Occupied`, `Closed` |
| `DraftStatus` | `Open`, `Submitted`, `Discarded` |
| `ContactRelationship` | `Daughter`, `Son`, `Spouse`, `Friend`, `Professional` |
| `AuthorityRole` | `Ordinary`, `PowerOfAttorney`, `Guardian` |
| `PayerKind` | `Private`, `Insurance`, `Benefits` |
| `DocumentKind` | `Consent`, `MedicationList`, `Authority`, `Signature` |
| `DocumentScan` | `Accepted`, `Quarantined` |
| `PaymentStatus` | `Authorised`, `Declined`, `Voided` |
| `ReviewKind` | `Allocation`, `Payer`, `Pharmacy`, `Services`, `Room`, `Mobility` |
| `DietaryNeeds` | `None=0`, `Vegetarian=1`, `LowSodium=2`, `Pureed=4`; registered with `Flags<DietaryNeeds>()` |

**Seed**

Every scenario starts with the following values unless its situation names an explicit replacement.

| Seed | Literal data |
|---|---|
| Clock | `Today=2026-11-18T00:00:00`, `Now=2026-11-18T09:00:00-05:00`, `Culture="en-US"`, `TimeZoneId="America/New_York"` |
| Community C1 | `"Harbor House"`, site `"North"`, building B1 `"Cedar"` and B2 `"Willow"` |
| Community C2 | `"Maple Court"`, site `"East"`, building B3 `"Elm"` |
| B1/A7 | Apartment `"7"`, Independent and Assisted care; BED4 `"Window"` available, revision `12`, no holder or hold; BED5 `"Door"` held, revision `8`, holder R202 `"Nora Patel"`, coordinator CO2, expiry `2026-11-18T09:30:00-05:00` |
| B1/A12 | Apartment `"12"`, Memory care; BED12 occupied, revision `4`, holder R203 `"Omar King"`, admission ADM203 |
| B2/A9 | Apartment `"9"`, Skilled care; BED9 available, revision `2` |
| B3/A21 | Apartment `"21"`, Independent and Assisted care; BED21 available, revision `6` |
| Bed water | `NeedsWater=false` on every bed at start |
| Care prices | Independent `2400.00`; Assisted `2800.00`; Memory `3600.00`; Skilled `4200.00` |
| Resident R101 | Helen Ward; preferred name `"Helen"`; birth date `1942-05-14`; email and confirmation `"helen@example.test"`; address `"18 Oak Street"`, `"Albany"`, `"NY"`, `"12207"`, `"US"`; allergies `["penicillin"]`; no admission; unrestricted |
| Resident R202 | Nora Patel; birth date `1940-02-03`; email `"nora@example.test"`; address `"2 River Road"`, `"Albany"`, `"NY"`, `"12207"`, `"US"`; no allergies; no admission; unrestricted |
| Resident R203 | Omar King; birth date `1938-07-11`; email `"omar@example.test"`; address `"9 Lake Road"`, `"Albany"`, `"NY"`, `"12207"`, `"US"`; no allergies; admission ADM203; unrestricted |
| Competing resident R104 | James Reed; birth date `1945-01-02`; email `"james@example.test"`; address `"5 Pine Road"`, `"Albany"`, `"NY"`, `"12207"`, `"US"`; no allergies; no admission; unrestricted |
| Coordinator CO1 | `"Maya Chen"`; communities `[C1,C2]`; online; admission and deposit permissions true |
| Coordinator CO2 | `"Luis Ortiz"`; communities `[C1]`; online; admission permission true; deposit permission false |
| Draft D100 | Version `7`, Open, R101, CO1, saved by CO1 at the world’s `Now`; location C1/B1/A7/BED4/revision12; care Assisted |
| Assisted details | `AssessmentCode="AS-100"`, date `2026-11-17`, hours `2.50`, note `"Uses a walking frame"` |
| Resident move-in | Date `2026-11-20`, arrival `2026-11-20T10:00:00-05:00`, notes `"Family brings clothing"` |
| Contact CT1 | `"Alice Ward"`, Daughter, `"alice@example.test"`, `"5185550101"`, alternate `"5185550102"`, PowerOfAttorney; authority `"POA-100"`, starts `2026-01-01`, ends `2027-01-01`, consent true |
| Contact CT2 | `"Ben Ward"`, Son, `"ben@example.test"`, `"5185550201"`, alternate `"5185550202"`, Ordinary, authority null |
| Payer | Insurance, contact CT1, policy `"POL-100"`, provider `"Harbor Mutual"`, coverage `2026-01-01` through `2026-12-31`, contribution `80.00`, secondary false/null, portal `"https://payer.example.test/policies/POL-100"`, verification null |
| Policy POL-100 | R101, Harbor Mutual, active, authorised contact CT1, monthly limit `3500.00`, not blocked |
| Policy POL-200 | R101, `"Cedar Cover"`, inactive, authorised contact CT1, monthly limit `2000.00`, not blocked |
| Medication M1 | Drug ASP75, dose `75.00`, unit `"mg"`, schedule `["Morning"]`, starts `2026-11-18`, no end date, instructions `"With food"` |
| Drug ASP75 | `"Aspirin"`, mg, maximum `300.00`, no conflicting allergy, no alternative |
| Drug AMOX | `"Amoxicillin"`, mg, maximum `1000.00`, conflicting allergy `"penicillin"`, alternative `"AZI"` |
| Drug AZI | `"Azithromycin"`, mg, maximum `500.00`, no conflicting allergy, no alternative |
| Round doses | Minimum `0.50`; doses `[0.50,1.00,null]` |
| Service DIN | `"Dining"`, `180.00`, revision `3`, available, capacity `8` |
| Service PHA | `"Pharmacy support"`, `60.00`, revision `3`, available, capacity `5` |
| Service TRA | `"Transport"`, `90.00`, revision `3`, available, capacity `0` |
| Selected services | Codes `[DIN,PHA]`; visits `[(DIN,2,true),(PHA,1,true),(TRA,0,false)]`; preferred days `[2026-11-20,2026-11-21]`; optional labels `["Window table",null]` |
| Diet | `Vegetarian | LowSodium`, numeric value `3`; note `"No added salt"`; meal count `3`; confirmed true |
| Quote Q100 | Revision `3`, Assisted/Insurance; base `2800.00`; DIN `180.00`, PHA `60.00`; total `3040.00`; deposit `500.00`; expiry `2026-11-18T10:00:00-05:00`; assistance required, reference `"AST-100"`, contribution `2432.00`, accepted false |
| Draft clearances | Payer verification null; pharmacy clearance null; accepted quote revision null; deposit receipt null; document IDs empty |
| Review input | Reference null; selected service count `0`; visit count `0`; ready false; result null; reviewed time null |
| Operation | `11111111-1111-4111-8111-111111111111`; rehearsal false |
| Draft D200 | Version `3`, Open, R101, empty bed, empty resident name, no contacts, no medications, no clearances, no payment |
| Submitted draft D300 | Version `9`, Submitted, R203, ADM203, BED12; discard forbidden |
| Server rules | All lists empty except declined cards `["4000000000000002"]` and quarantined file contents `["DENIED"]`; both locks false; minimum age `55`; maximum contacts `4` |
| Uploaded consent | Name `"consent.txt"`, type `"text/plain"`, bytes UTF-8 `"Helen agrees.\n"`, size `14`, last modified `1730000000000` |
| Uploaded medication list | Name `"medications.txt"`, type `"text/plain"`, bytes UTF-8 `"Aspirin 75 mg\n"`, size `14`, last modified `1730000000001` |
| Signature file | Name `"signature.txt"`, type `"text/plain"`, bytes UTF-8 `"Helen Ward\n"`, size `11`, last modified `1730000000002` |
| Deposit entry | Card `"4111111111111111"`, amount `500.00`, currency `"USD"`; the stored payment keeps last four `"1111"` |
| Initial collections | Payments, waitlist, messages, acknowledgements and history empty; admissions contain ADM203 |

The **Ready** seed is D100 with verification `V100`, pharmacy clearance `CL100`, quote revision `3` accepted, assistance accepted, accepted documents DOC1 and DOC2 containing the two specified uploads, and authorised deposit PAY0 for `500.00`. It supplies the valid admission starting point.

The **Changed price** seed replaces PHA’s price with `80.00`, revision `4`; Q100 revision4 totals `3060.00`.

The **Bed taken** transition changes BED4 to Held, revision13, holder R104 `"James Reed"`, CO2, expiry `2026-11-18T09:45:00-05:00`.

Each successful aggregate edit increments D100 from version7 to version8. Separate situations start again at version7. Review, document, payment and message records have their own identities; they do not silently overwrite the draft’s editing version.

**Component data**

The six existing declarations keep their real members and receive these domain roles. Their source contracts are [the component declarations](tests/Alis.Reactive.Conformance.App/Components/ConformanceComponent/ConformanceComponent.cs:10) and [native.json](spec/Reference/native.json:1).

| Declaration | Domain role and starting values |
|---|---|
| `ConformanceComponent` | Allocation and service review panels; `Text="Service review"`, `Value="DIN"`, `Count=2`, `Amount=240.00`, `Enabled=true`, `ReadOnlyText="C1"`; `Outer.Middle.Inner.Text="North allocation desk"` |
| Its `Items` | Dining: value2, active true; Pharmacy support: value1, active true; Transport: value0, active false |
| Its `Records` | Three `Components.Record` values with names Dining, Pharmacy support, Transport; codes DIN, PHA, TRA; counts2,1,0; day `2026-11-18`; detail names `"Meals"`, `"Medication help"`, `"Travel"`; multiword values2,1,0; ratios1.0,1.0,0.0 |
| Its `Record` / `Snapshot()` | Name `"Service review"`, code `"Q3"`, count2, day `2026-11-18`, detail name `"Capacity checked"`, multiword2, ratio1.0 |
| Awaited review answers | `SettleCheck()` returns true; `SettleCount()` returns2; `SettleRecord()` returns the specified service review record; refusal situations supply the named server rejection |
| `ConformancePeer` | Mobility assessment handed to the plan by the allocation component; ratio25; text starts empty and the app sets `"Mobility assessment"` |
| `ConformanceRegistered` | Admission capacity gauge found through `conformance-registry`; reading2; app threshold2; its `Alarmed` event has no payload |
| `ConformanceStore` | Ward preparation messages; its real `Posted.Message` is `"Bed 4 needs water"` |
| `ConformanceCallbacks` | Room handoff console; `OnRaised` carries `RaisedRoom.Room="7"`; its own label is `"Desk"`; attached listeners append `"Admission"` and `"Payer"` |
| `ConformanceElement` | Arrival room opener; `Opened.Detail.Room="7"`; `Opened.Type="opened"`; `Keydown.Key` supplies the actual key |

**Server-owned rules**

The server alone decides:

- Whether a bed remains free at admission, belongs to the selected apartment, and supports the care level.
- Whether another admission already exists for a resident.
- Whether a building is closed or a coordinator’s permission was revoked.
- Whether an authority reference was revoked.
- Whether a payer policy is active, blocked, belongs to the resident, and recognises the selected representative.
- Whether medication and allergy records conflict.
- Whether service capacity and price revisions remain current.
- Whether document bytes pass inspection, regardless of filename and declared MIME type.
- Whether the fake payment authority accepts a card and whether an operation already produced a receipt.
- Whether the room handoff is locked.
- Whether a submitted draft can change.
- Whether a provided revision agrees with the saved revision.

Admission is one atomic operation: check all server facts, occupy the bed, create the admission, mark the draft Submitted and append its history entry. A rejected operation changes none of those records.

Cancellation in the browser does not roll back a server operation already committed. A subsequent read resolves an uncertain outcome.

**2. THE ENDPOINTS**

All application endpoints are world-scoped by the existing world session. The addresses below omit no application path segment.

Common request facts:

- Query member `Culture: string` is `"en-US"` unless the situation explicitly exercises another rendering culture.
- Default header `Care-Site: string` is `"North"`; an East request explicitly overrides it with `"East"`.
- Unsafe requests in the admission desk send the host-issued antiforgery token in its configured header.
- The reception profile sends the host-issued token as a form field.
- Mutations carry `X-Coordinator: string="CO1"` and `X-Operation: Guid`.
- Versioned draft mutations carry `If-Match: string="7"` at the seed.
- A field listed in a body is sent in that body; a path, query or header field is also sent in a body only where that body lists it.
- An omitted query, header or body in an endpoint row means no such application data.
- JSON uses the host’s contract; form and query keys use C# member paths.

**Request and reply records**

| Record | Members |
|---|---|
| `MoveInAddress` | `DraftId:string`, `CommunityId:string`, `Referral:string`, `ReviewOnly:bool`, `Culture:string` |
| `LocationQuery` | `DraftId:string`, `CommunityId:string`, `BuildingId:string`, `ApartmentId:string`, `Level:CareLevel`, `Culture:string` |
| `SectionQuery` | `DraftId:string`, `Section:SectionKind`, `ContactId:string?`, `Level:CareLevel`, `PayerKind:PayerKind`, `Culture:string` |
| `ContactEdit` | `DraftId:string`, `Version:int`, `Contacts:ContactSection`, `NewContact:ContactInput?`, `RemoveId:string?` |
| `MedicationEdit` | `DraftId:string`, `Version:int`, `Medications:MedicationSection`, `NewMedication:MedicationInput?`, `RemoveId:string?` |
| `AllocationRequest` | `DraftId:string`, `Version:int`, `Location:LocationInput`, `Level:CareLevel`, `ResidentId:string`, `Ready:bool`, `Reference:string?` |
| `PayerVerificationPage` | `DraftId:string`, `ResidentId:string`, `ContactId:string`, `Policy:CoverageInput`, `Reference:string?`, `Consent:bool` |
| `PharmacyRequest` | `DraftId:string`, `Version:int`, `ResidentId:string`, `Allergies:List<string>`, `Medications:MedicationSection`, `Review:ReviewInput` |
| `QuoteRequest` | `DraftId:string`, `Version:int`, `CareLevel:CareLevel`, `PayerKind:PayerKind`, `Services:ServiceInput`, `Review:ReviewInput` |
| `QuoteAcceptance` | `DraftId:string`, `Version:int`, `QuoteId:string`, `Revision:int`, `AcceptAssistance:bool` |
| `WaitlistPage` | `DraftId:string`, `ResidentId:string`, `CommunityId:string`, `CareLevel:CareLevel`, `Reason:string`, `AcceptTerms:bool` |
| `DocumentsPage` | `DraftId:string`, `Version:int`, `Kind:DocumentKind`, `Caption:string`, `Files:List<IFormFile>`, `AuthorityFile:IFormFile?`, `IncludeAuthority:bool` |
| `SignatureRequest` | `DraftId:string`, `DocumentId:string`, `Name:string`, `Type:string`, `Size:long`, `LastModified:long` |
| `DepositPage` | `DraftId:string`, `OperationId:Guid`, `CardNumber:string`, `Cardholder:string`, `Amount:decimal`, `Currency:string`, `Consent:bool` |
| `DietRequest` | `DraftId:string`, `Diet:DietInput`, `Kinds:List<DocumentKind>`, `OptionalLabels:List<string?>` |
| `RoomReviewRequest` | `DraftId:string`, `Room:string`, `Reference:string`, `CoordinatorId:string` |
| `MobilityRequest` | `DraftId:string`, `Ratio:double`, `Description:string`, `PreviousDescription:string?` |
| `ReceptionPage` | `DraftId:string`, `ResidentId:string`, `VisitorName:string`, `Relationship:ContactRelationship`, `Consent:bool`, `Desk:string`, `Copies:int` |
| `SaveReply` | `Receipt:SaveReceipt`; `Review:ReceiptReview` |
| `SaveReceipt` | `DraftId:string`, `Version:int`, `Reference:string`, `SavedBy:PersonLabel`, `SavedAt:DateTimeOffset` |
| `PersonLabel` | `Id:string`, `Name:string` |
| `ReceiptReview` | `Required:bool`, `Reference:string` |
| `AllocationReply` | `Allocation:AllocationDecision` |
| `AllocationDecision` | `Status:string`, `Bed:BedSummary`, `Instructions:InstructionReference` |
| `BedSummary` | `Id:string`, `Apartment:ApartmentSummary`, `Revision:int`, `Available:bool` |
| `ApartmentSummary` | `Id:string`, `Number:string`, `Building:BuildingSummary` |
| `BuildingSummary` | `Id:string`, `Name:string`, `Community:CommunitySummary` |
| `CommunitySummary` | `Id:string`, `Name:string` |
| `InstructionReference` | `Required:bool`, `Id:string` |
| `VerificationReply` | `Policy:VerifiedPolicy` |
| `VerifiedPolicy` | `Number:string`, `Verification:VerificationDecision` |
| `VerificationDecision` | `Id:string`, `CanProceed:bool`, `Details:InstructionReference` |
| `PharmacyReply` | `Review:PharmacyReview` |
| `PharmacyReview` | `Id:string`, `Clearance:PharmacyClearance` |
| `PharmacyClearance` | `Passed:bool`, `InstructionsRequired:bool`, `Instructions:InstructionReference` |
| `QuoteReply` | `Quote:QuoteState` |
| `AdmissionReply` | `Admission:AdmissionState` |
| `DocumentReply` | `Admission:DocumentAdmission` |
| `DocumentAdmission` | `DraftId:string`, `Documents:DocumentBatch` |
| `DocumentBatch` | `Accepted:List<DocumentMetadata>`, `Receipt:InstructionReference` |
| `DocumentMetadata` | `Id:string`, `Name:string`, `Type:string`, `Size:long`, `Revision:int`, `LastModified:long` |
| `PaymentReply` | `Deposit:DepositResult` |
| `DepositResult` | `Receipt:PaymentState`, `Balance:DepositBalance` |
| `DepositBalance` | `Due:decimal`, `Satisfied:bool` |
| `AvailabilityReply` | `Community:CommunitySummary`, `Revision:int`, `Beds:List<BedSummary>` |
| `BedChanged` | `CommunityId:string`, `Bed:BedSummary`, `PriceRevision:int` |
| `CoordinatorLineReply` | `Line:CoordinatorLine` |
| `CoordinatorLine` | `Sequence:int`, `Coordinators:List<PersonLabel>`, `Messages:List<CoordinatorMessage>` |
| `BedConflict` | `Conflict:BedConflictDetail` |
| `BedConflictDetail` | `Bed:ConflictingBed`, `Waitlist:WaitlistOffer` |
| `ConflictingBed` | `Id:string`, `Holder:PersonLabel`, `Hold:ConflictHold` |
| `ConflictHold` | `ExpiresAt:DateTimeOffset` |
| `WaitlistOffer` | `Allowed:bool`, `TermsId:string` |
| `VersionConflict` | `Conflict:VersionConflictDetail` |
| `VersionConflictDetail` | `Draft:ConflictingDraft` |
| `ConflictingDraft` | `Id:string`, `Revision:int`, `SavedBy:PersonLabel`, `Comparison:InstructionReference` |
| `PayerDependency` | `Dependency:PayerDependencyDetail` |
| `PayerDependencyDetail` | `Payer:DependentPayer` |
| `DependentPayer` | `ContactId:string`, `PolicyNumber:string`, `Explanation:InstructionReference` |
| `PharmacyConflict` | `Conflict:PharmacyConflictDetail` |
| `PharmacyConflictDetail` | `MedicationId:string`, `Allergy:AllergyConflict` |
| `AllergyConflict` | `Name:string`, `Resolution:ConflictResolution` |
| `ConflictResolution` | `HasAlternatives:bool`, `Id:string` |
| `ErrorEnvelope` | `Error:ErrorDetail` |
| `ErrorDetail` | `Code:string`, `Message:string`, `Recovery:RecoveryAdvice` |
| `RecoveryAdvice` | `Allowed:bool`, `Reference:string` |
| `UploadPolicyReply` | `Upload:UploadPolicy` |
| `UploadPolicy` | `Allowed:bool`, `MaximumBytes:long`, `Types:List<string>`, `Revision:int` |

`SectionKind` contains `Buildings`, `Apartments`, `Beds`, `IndependentCare`, `AssistedCare`, `MemoryCare`, `SkilledCare`, `Contacts`, `Authority`, `PrimaryPayer`, `SecondaryPayer`, `Medications`, `AssistanceTerms`, `AllocationOwner`, and `PayerOwner`.

For ordinary application errors, the literal `ErrorEnvelope` is:

- 403: code `"forbidden"`, message `"This coordinator cannot perform this action."`, recovery false, reference `""`.
- 404: code `"missing"`, message `"The admission record was not found."`, recovery false, reference `""`.
- 410: code `"closed"`, message `"This draft is closed."`, recovery false, reference `""`.
- 422: the feature’s named server rule message.
- 429: code `"desk-busy"`, message `"The admission desk is busy."`, recovery true, reference `"desk-status"`.
- 503: code `"unavailable"`, message `"The admission service is unavailable."`, recovery true, reference `"desk-status"`.

Validation failures use the actual `ValidationProblem` contract with C# binding names and lists of literal messages.

**Endpoint catalogue**

| ID | Method and address | Typed request | Replies and changes |
|---|---|---|---|
| E1 | GET `/move-ins/{DraftId}` | Path DraftId; query `MoveInAddress` members | 200 HTML `MoveInPage`; 404/403 `ErrorEnvelope`; reads saved draft |
| E2 | GET `/move-ins/{DraftId}/buildings` | Path DraftId; query CommunityId, Level, Culture from `LocationQuery`; `X-Community` | 200 HTML Buildings partial; 403/422 error; no mutation |
| E3 | GET `/move-ins/{DraftId}/apartments` | Path DraftId; query CommunityId, BuildingId, Level, Culture | 200 HTML Apartments partial; 403/422 error; no mutation |
| E4 | GET `/move-ins/{DraftId}/beds` | Path DraftId; query CommunityId, BuildingId, ApartmentId, Level, Culture | 200 HTML Beds partial; 403/422 error; no mutation |
| E5 | GET `/move-ins/{DraftId}/sections/{Section}` | Path DraftId and Section; remaining `SectionQuery` members in query | 200 HTML section named by Section; 403/410/422 error; no mutation |
| E6 | POST `/move-ins/{DraftId}/identity-review` | JSON ResidentInput; ResidentId query | 200 `SaveReply` reference `"ID100"`; 400 validation; 409 duplicate-resident `ErrorEnvelope`; adds passed review only |
| E7 | POST `/move-ins/{DraftId}/allocation-review` | JSON `AllocationRequest` | 200 `AllocationReply` status `"Cleared"`, BED4, instructions `"ALLOC100"`; 409 `BedConflict`; adds review AR100 on success |
| E8 | POST `/move-ins/{DraftId}/contacts` | JSON `ContactEdit`; version header | 200 `SaveReply`, version8, reference `"CONTACTS8"`; 400; 409 version; 422 maximum contacts; replaces retained rows and adds CT3 |
| E9 | DELETE `/move-ins/{DraftId}/contacts/{ContactId}` | Path IDs; JSON ContactEdit; version header | 200 SaveReply version8; 409 PayerDependency or VersionConflict; removes the named row only after checking dependency |
| E10 | GET `/move-ins/{DraftId}/payer-explanation` | Query ContactId, PolicyNumber | 200 HTML explanation `"Alice Ward is the authorised payer contact for POL-100."`; 403/404 |
| E11 | POST `/move-ins/{DraftId}/payer-verifications` | URL-encoded `PayerVerificationPage`; query `Reference`; header `X-Community` | 200 VerificationReply V100, CanProceed true, details `"POL100"`; 400; 422 inactive/blocked policy; writes verification V100 |
| E12 | GET `/move-ins/{DraftId}/policy-details/{Reference}` | Path reference | 200 HTML `"Harbor Mutual covers up to 3500.00 per month."`; 403/404 |
| E13 | POST `/move-ins/{DraftId}/medications` | Multipart fields from MedicationEdit; version header | 200 SaveReply version8, `"MEDICATIONS8"`; 400; 409 version; saves rows and adds M2 |
| E14 | DELETE `/move-ins/{DraftId}/medications/{MedicationId}` | JSON MedicationEdit; path IDs; version header | 200 SaveReply version8; 400/409/410; removes named row |
| E15 | POST `/move-ins/{DraftId}/pharmacy-review` | JSON PharmacyRequest; guarded `X-Review`; query `Reference` | 200 PharmacyReply CL100; 400; 409 PharmacyConflict; writes clearance only when all checks pass |
| E16 | GET `/move-ins/{DraftId}/pharmacy-instructions/{Reference}` | Path IDs | 200 HTML `"Continue Aspirin 75 mg with food."`; 403/404 |
| E17 | GET `/move-ins/{DraftId}/pharmacy-alternatives/{Reference}` | Path IDs | 200 HTML `"Ask the prescriber about Azithromycin."`; 403/404 |
| E18 | GET `/move-ins/{DraftId}/services` | Query CareLevel, PayerKind, Culture | 200 JSON `List<ServiceDefinition>`; 403/503; no mutation |
| E19 | POST `/move-ins/{DraftId}/quotes` | JSON QuoteRequest | 200 QuoteReply Q100/revision3/3040.00; 400; 409 capacity or changed facts; stores quote Q100 |
| E20 | GET `/move-ins/{DraftId}/assistance/{Reference}` | Path IDs; query Revision | 200 HTML `"Harbor Mutual contribution: 2432.00"`; 403/409 |
| E21 | POST `/move-ins/{DraftId}/quote-acceptances` | JSON QuoteAcceptance | 200 SaveReply reference `"ACCEPT-Q3"`; 400; 409 QuoteReply with revision4/3060.00; records acceptance only for current revision |
| E22 | PUT `/move-ins/{DraftId}` | JSON MoveInPage; query Referral, Rehearsal; version header | 200 SaveReply version8, `"SAVE8"`; `ETag: "8"`; 400; 409 VersionConflict; replaces draft data after shared and server checks |
| E23 | GET `/move-ins/{DraftId}/receipts/{Reference}` | Path IDs; query Referral | 200 HTML receipt; 403/404; reads saved state |
| E24 | GET `/move-ins/{DraftId}/comparisons/{Revision}` | Path DraftId and Revision | 200 HTML saved comparison, `"Version 8 saved by Luis Ortiz"` in conflict seed; 403/404 |
| E25 | POST `/move-ins/{DraftId}/admissions` | JSON MoveInPage; version header | 201 AdmissionReply ADM100; 400; 409 BedConflict/VersionConflict; 422 clearance error; atomically submits and occupies BED4 |
| E26 | GET `/move-ins/{DraftId}/arrival-checklists/{ChecklistId}` | Path IDs | 200 HTML `"Welcome Helen Ward to apartment 7, Window bed."`; 403/404 |
| E27 | GET `/move-ins/{DraftId}/waitlist-terms/{TermsId}` | Path IDs | 200 HTML `"No bed is reserved by joining the waitlist."`; 403/404 |
| E28 | POST `/move-ins/{DraftId}/waitlist` | JSON WaitlistPage | 201 WaitlistState W100, position1; 400; 409 duplicate; 422 community closed; creates waitlist entry |
| E29 | DELETE `/move-ins/{DraftId}` | Path ID; version header; no body | 204 none; 409 version; 422 `"A submitted admission cannot be discarded."`; marks open draft Discarded |
| E30 | GET `/move-ins/{DraftId}/availability` | Query CommunityId, BedId, SinceRevision | 200 AvailabilityReply; 403/404/503; no mutation |
| E31 | GET `/move-ins/{DraftId}/availability-stream` | Query CommunityId | 200 server-event stream of `BedChanged`; 403; owns no bed mutation |
| E32 | OPTIONS `/move-ins/{DraftId}/documents` | Query Kind, Culture | 200 UploadPolicyReply: allowed true, max1048576, types `["text/plain","application/pdf"]`, revision2; `Allow: GET, HEAD, OPTIONS, POST, PUT`; 403/423 error |
| E33 | POST `/move-ins/{DraftId}/documents` | Multipart DocumentsPage: repeated Files parts, Caption, Kind, Version; conditional AuthorityFile | 201 DocumentReply with DOC1/DOC2; 400; 409 version; 422 scan failure; stores bytes and metadata atomically per batch |
| E34 | PUT `/move-ins/{DraftId}/documents/{DocumentId}/content` | Whole file body; path IDs; query Name, Type, Size, LastModified; `If-Match: "1"` | 200 DocumentReply DOC3/revision2; 400/409/422; replaces signature bytes |
| E35 | HEAD `/move-ins/{DraftId}/documents/{DocumentId}` | Path IDs; no body | 200 no body, Content-Length14, Content-Type text/plain, ETag `"1"` for DOC1; 404/403 with no body |
| E36 | GET `/move-ins/{DraftId}/packet` | Query QuoteRevision, Culture | 200 file `admission-D100.txt`, text/plain, literal packet text below; 409 ErrorEnvelope; records no payment or admission |
| E37 | POST `/move-ins/{DraftId}/packet-notes` | Whole text body `"Family brings clothing"`; query CoordinatorId; header `X-Page-Url` | 200 text `"Packet note saved"`; 422 `"Packet is sealed."`; stores acknowledgement PN100 |
| E38 | POST `/move-ins/{DraftId}/deposits` | JSON DepositPage; X-Operation | 201 PaymentReply PAY100, due0.00, satisfied true; 400; 402 `"The deposit was declined."`; 409 changed deposit; idempotently stores one payment |
| E39 | GET `/move-ins/{DraftId}/deposits/{OperationId}` | Path IDs | 200 PaymentReply for existing receipt; 404 ErrorEnvelope; resolves an uncertain submission |
| E40 | PUT `/move-ins/{DraftId}/diet` | JSON DietRequest | 200 SaveReply `"DIET8"`, version8; 400; 422 kitchen restriction; updates diet |
| E41 | GET `/move-ins/{DraftId}/meal-plan` | Query DietaryNeeds, MealCount, Kinds, OptionalLabels | 200 HTML `"Vegetarian, low sodium: 3 meals"`; 400/422; no mutation |
| E42 | POST `/move-ins/{DraftId}/room-reviews` | JSON RoomReviewRequest | 201 SaveReply `"ROOM7"`; 400; 423 `"Room handoff is locked."`; creates room review |
| E43 | GET `/move-ins/{DraftId}/room-search` | Query Query:string, CommunityId:string | 200 JSON `List<string>`: `["7 — Cedar","12 — Cedar"]` for `"Cedar"`; 403/503 |
| E44 | POST `/move-ins/{DraftId}/mobility-reviews` | JSON MobilityRequest | 201 SaveReply `"MOB25"`; 400; 422 `"Assessment is superseded."`; records ratio25 and its description |
| E45 | POST `/move-ins/{DraftId}/ward-preparation` | Condensed multipart fields Message, BedId, CoordinatorId | 200 text `"Water requested for apartment 7, Window bed"`; 400; 409 `"Bed 4 is closed."`; sets BED4.NeedsWater true |
| E46 | POST `/move-ins/{DraftId}/reception` | URL-encoded ReceptionPage; antiforgery form field | 200 HTML `"Alice Ward signed in for Helen Ward at North desk"`; 400/403/422; creates acknowledgement REC100 |
| E47 | GET `/move-ins/{DraftId}/history` | Path ID | 200 HTML admission history from HistoryEntry records; 403/404 |
| E48 | GET `/move-ins/{DraftId}/review/check` | Path ID | 200 JSON bool true; 409 false; component readiness result derived from current world |
| E49 | GET `/move-ins/{DraftId}/review/count` | Path ID | 200 JSON int2; 503; derives selected service count |
| E50 | GET `/move-ins/{DraftId}/review/record` | Path ID | 200 JSON the specified Components.Record; 503 |
| E51 | GET `/move-ins/{DraftId}/review/settle` | Path ID | 204; 503; the component’s awaited completion boundary |
| E52 | POST `/world/session/renew` | Existing session-plugin request; no application body | 204 while CO1 session valid; 401 otherwise; extends the world’s session |
| E53 | POST `/world/session/flush` | Existing session-plugin request; no application body | 204; 503; flushes already recorded admission work |
| E54 | GET `/move-ins/desk-status` | No application body | 200 text `"Admission desk available"`; 503 |
| E55 | POST `/move-ins/{DraftId}/print-intents` | JSON `PrintIntent {DraftId,PacketRevision:int,PageUrl:string,Signer:string}` | 201 SaveReply `"PRINT100"`; 400; 409 packet changed; records intent, not successful physical printing |

The packet’s literal text is:

```text
Admission D100
Helen Ward
Harbor House / Cedar / 7 / Window
Monthly total: 3040.00 USD
Deposit: 500.00 USD
```

The coordinator hub is `/move-ins/coordinator-line`. Its proposed application declaration is `CoordinatorHub : SignalRHub`.

- `Join(DraftId, CoordinatorId)` uses `Send` and changes membership.
- `Snapshot(DraftId)` uses `Invoke<CoordinatorLineReply>` and returns sequence0, Maya Chen and Luis Ortiz, and no messages at the seed.
- `Post(DraftId, CoordinatorId, Text)` uses `Invoke<CoordinatorMessage>`; posting `"Helen arrives at 10:00"` creates MSG1, sequence1, at the world’s Now.
- `Changed` is `Push<CoordinatorLineReply>`.
- Revoked membership rejects the operation; no message is stored.
- Connection lifecycle uses the inherited `Reconnecting`, `Reconnected`, `Closed`, `State` and `ConnectionId`.

**3. THE SCREENS AND PARTIALS**

Every view opens its own `ReactivePlan` and ends with `RenderPlan`. Plans do not travel through `ViewData`, request state or another view’s model.

| View | Own model, position and ownership |
|---|---|
| MoveIn | `MoveInPage`; main request container `move-in`; desk-options container beside it |
| Location | `LocationInput`; attached to `MoveInPage.Location`; buildings, apartments and beds occupy named slots inside `move-in` |
| Resident | `ResidentInput`; attached to `.Resident`; Identity and Address are nested member partials |
| Identity | `ResidentIdentity`; attached to `.Resident.Identity` |
| Address | `Address`; attached to `.Resident.Address` |
| Care | `CareInput`; attached to `.Care`; exactly one care branch loaded |
| IndependentCare | `IndependentCareInput`; attached to `.Care.Independent` |
| AssistedCare | `AssistedCareInput`; attached to `.Care.Assisted` |
| MemoryCare | `MemoryCareInput`; attached to `.Care.Memory` |
| SkilledCare | `SkilledCareInput`; attached to `.Care.Skilled` |
| Contacts | `ContactSection`; attached to `.Contacts`; each Contact view attaches to `.Rows[index]` |
| Contact | `ContactInput`; row identity is hidden; Authority is a nested conditional member partial |
| Authority | `AuthorityInput`; attached to the selected contact’s `.Authority`; unloaded for Ordinary role |
| Payer | `PayerInput`; attached to `.Payer`; Primary and Secondary retain their full prefixes |
| PrimaryCoverage | `CoverageInput`; attached to `.Payer.Primary` |
| SecondaryCoverage | `SecondaryCoverageInput`; attached to `.Payer.Secondary`; unloaded with its checkbox or parent |
| Medications | `MedicationSection`; attached to `.Medications`; each Medication view attaches to `.Rows[index]` |
| Medication | `MedicationInput`; its row has stable domain Id and current contiguous binding position |
| Services | `ServiceInput`; attached to `.Services`; shares the main validator |
| Diet | `DietInput`; attached to `.Diet`; shares the main validator |
| Documents selection | `DocumentSelection`; attached to `.Documents`; contains document IDs, not file bytes |
| Allocation review | `AllocationReviewPage {DraftId,Reference,ComponentStart}`; own plan inside the main page; no independent submitted form |
| Service review | `ServiceReviewPage {DraftId,ComponentStart}`; own plan; its component is read by the main plan |
| Payer verification | `PayerVerificationPage`; independent `payer-verification` container in `NativeDrawer.ContentId`, outside `move-in` |
| Waitlist | `WaitlistPage`; independent `waitlist` container outside `move-in` |
| Documents upload | `DocumentsPage`; independent `admission-documents` container outside `move-in` |
| Deposit | `DepositPage`; independent `deposit` container outside `move-in` |
| Reception | `ReceptionPage`; sibling native `BeginRequestForm`, never inside another form or the move-in container |
| Bed card | `BedCardPage {DraftId,BedId,Revision}`; independent plan and shared availability connection |
| Coordinator line | `CoordinatorLinePage {DraftId,CoordinatorId,Message}`; independent container `coordinator-line` |
| Allocation owner A | `AllocationOwnerPage {DraftId,Reference}`; slot `allocation-owner`; owns the dynamic component listeners |
| Payer owner B | `PayerOwnerPage {DraftId,Reference}`; slot `payer-owner` inside A; owns an additional callback listener |
| Receipt, comparison, checklist, policy, alternatives and terms | Each has its own named view-model record containing exactly its endpoint’s displayed reply data; each has its own plan |

An endpoint that returns an attached partial renders its owning envelope and uses `PartialFor` at the declared member. It does not invent a prefix string in the browser.

The main summary is outside all replaceable main-section slots. Independent containers keep their own summaries. Closing payer verification does not remove the main payer rules. Replacing contact rows does not move a sent failure onto a different contact.

`Into` and `Unload` replace or release owned content. `Element.Text.Set` and `Element.Html.Set` target status and document-preview elements outside those slots. `Append` adds a receipt line without destroying its children.

**4. THE FEATURES**

**MI.1. Residence selection**

The coordinator chooses a community, building, apartment and bed, and sees choices belonging to that combination.

1. `DomReady` and the community drop-down’s `Change` start a `Replace` handler, read no Native payload member, read `Location.CommunityId` and `Care.Level` through the main `Container.Read`, clear dependent slots with `Unload`, and call E2 with those query values, DraftId from `PageUrl<MoveInAddress>`, and `X-Community` from the separately rendered allocation component’s `ReadOnlyText`, with `Lanes.FireFirst`, `FireSecond`, `Respond` and `Route` fixing replacement.
2. The building and apartment `Change` handlers read their current container values, require `NotEmpty` parent IDs, call E3 then E4 with the complete parent chain, and load only the current reply with `Into`, while `ContainerRead.Read` includes fields joined by `LoadSlot`.
3. Selecting BED4 records revision12 in the hidden field and displays `"Window bed available"`; selecting a held bed displays its holder without reserving it, and later admission rechecks it in MI.17.

**MI.2. Care-specific admission**

Changing care replaces the relevant questions only after confirmation.

1. The care radio’s `Change` reads no event members, compares the selected level with the saved level using `NotEq`, asks `NativeConfirm.Ask("Replace the current care answers?")`, and uses `When`, `Then`, `ElseIf` and `Else` to load E5’s matching branch after the answer, with `Run.Fire`, `Step` and `Settle` holding the continuation.
2. The Memory branch requires resident age eligibility and either a guardian or power of attorney, expressed as a nested `And` of the care choice with an `Or` of contact roles and `Not` of the ordinary-contact condition, before E5 loads MemoryCare.
3. Cancel restores the previous selected level; confirmation unloads the old branch before the new branch joins, following `Ownership.UnloadA`, `LoadA` and `SlotHolds.Unload`, `Load`.

**MI.3. Resident details**

The coordinator checks identity, address and arrival dates before asking the server to match the resident.

1. Review’s `Click` reads no pointer metadata, runs pipeline `Validate(moveIn.Container)`, and shows `"Resident details ready"` only after the Resident rules pass; a failed check produces the schema’s `stop`.
2. Verify identity posts E6 with ResidentInput read from the main container and ResidentId from its hidden field, handles 400 through `ShowValidationErrors`, and handles the server’s duplicate-resident 409 without creating a review.
3. The 200 route reads `Receipt.Reference` and `Receipt.SavedBy.Name`, displays `"Identity checked by Maya Chen"`, and opens E23 only when `Review.Required` is true.

**MI.4. Allocation review**

The coordinator requests a review based on local readiness and the awaited server readiness result.

1. Review’s `Click` reads no payload member and evaluates `Location.BedId.NotEmpty().And(component.Check()).And(component.SettleCheck())`, so a false earlier term prevents the awaited call, then posts E7 with the current location, resident, version, ready result and `Outer.Middle.Inner.Text`, with `Run.Settle` before `Request.Check`.
2. The 200 route reads `Allocation.Bed.Apartment.Building.Community.Name` and `Allocation.Status`, writes the component’s `Text` and `WriteOnlyText`, and calls E23 for `Allocation.Instructions.Id` only when `Allocation.Instructions.Required` is true.
3. The 409 route reads `Conflict.Bed.Holder.Name` and `Conflict.Waitlist.Allowed`, then enters MI.18; an absent optional review reference uses `IfNull("Unassigned")`, while a malformed required reply holder stops the route instead of inventing a value.

**MI.5. Contact rows**

Adding a contact retains all current answers.

1. Add contact dispatches the typed shared draft command with `Kind=AddContact`, new contact CT3 and the current version, whose `Skip` handler posts E8 with every current contact from the main container and the new row.
2. The 200 route reads `Receipt.Version`, updates the hidden version, and uses `Chained` to GET E5 Contacts, with `Composition.Begin`, `Answer`, `Route` and `DefaultFinally` preceding the row replacement.
3. `PartialFor` renders the returned rows at contiguous indexes with stable IDs; the next parent validation checks each through `RuleForEach` and `SetValidator`.

**MI.6. Contact authority**

The coordinator supplies legal authority only for a role requiring it.

1. A contact role’s `Change` reads no event members, reads its row’s role and ID through the parent container, and loads E5 Authority when the role is `In(PowerOfAttorney,Guardian)` and the resident has no active admission.
2. The inner branch requires consent or a guardian order, combines `And`, `Or` and `Not`, and otherwise displays `"Authority consent is required"` without sending.
3. Ordinary contact unloads Authority, and the next contact save sends Authority null rather than stale fields, while `Partials.LoadSlot`, `ContainerRead.UnloadSlot` and `Validation.ChildSkipAgrees` govern the resulting body.

**MI.7. Contact removal**

Removing a contact preserves the remaining rows and explains payer dependencies.

1. A `Rows<ContactInput>` `NativeActionButton` reads `Row.Id`, changes that action’s `Element.Classes`, and dispatches the shared command, which DELETEs E9 with retained contacts, RemoveId, DraftId, version and the version header.
2. The 409 PayerDependency route reads `Dependency.Payer.ContactId`, `PolicyNumber` and `Explanation.Required`, compares the returned contact with the requested removal, and GETs E10 only for that dependency.
3. The success route updates the version and chains E5 Contacts before the action’s final status, with row-local busy ownership held through `Composition.DefaultFinally`; a `NativeActionLink` beside it GETs that contact’s authority section from `Row.Id`.

**MI.8. Primary and secondary payer**

The coordinator chooses funding and supplies its active coverage.

1. Payer kind `Change` reads no payload fields, uses `When`/`ElseIf`/`Else` on the current `Payer.Kind`, and loads E5 PrimaryPayer with current DraftId and Kind.
2. The secondary checkbox’s `Change` reads the current boolean, requires primary coverage to contain the move-in date and `ConformanceComponent.Check()` to be true, then loads E5 SecondaryPayer; otherwise it unloads that nested slot.
3. Saving applies coverage dates and contribution rules to the exact active body, and a parent replacement releases B before A under `Ownership.UnloadB` and `UnloadA`.

**MI.9. Independent payer verification**

The coordinator verifies payer details without completing the entire admission.

1. Open verification loads E5’s independent verification content into `NativeDrawer.ContentId`, sets `NativeDrawer.Size`, and calls `Open`.
2. Verify posts E11 using `UrlEncoded` from the drawer’s own container, adds Reference from the allocation component in another plan, conditionally adds a header only when that reference is `NotNull`, and uses `Validate<PayerVerificationRules>()`.
3. The success route walks `Policy.Verification.CanProceed` and `Policy.Verification.Details.Id`, then chains E12 when details are required; the main draft remains editable while this independent check runs.
4. `BeforeClose` reads `ClosedBy` and writes `Cancel` synchronously while verification is pending, before any awaited step, with `Run.FirstReturns`, `SecondReturns` and `VendorReads` fixing the vendor’s decision; `Closed` unloads the independent content.

**MI.10. Medication rows**

The coordinator adds, removes and edits medications with retained values and stable identities.

1. Add medication dispatches the shared command and POSTs E13 with condensed `Inputs` for the medication container’s submission model and conditional `Set` values for the proposed row, using multipart fields and current version.
2. Remove medication DELETEs E14 with its ID and the captured retained rows as JSON, then chains E5 Medications after success.
3. Each replacement renders `PartialFor` rows and rechecks dose precision, schedules and date order; the list of round doses compares every item with `MedicationSection.MinimumRoundDose`, not a member invented on the item.

**MI.11. Pharmacy review**

The server compares the submitted medications with the resident’s allergy record.

1. Check medications posts E15 with the main container’s medications and allergies, an `Object<ReviewInput>` assembled from `Snapshot()`, `SettleRecord()`, `Total()` and current reference, and guarded query/header values whose conditions are evaluated before their values.
2. The 200 route reads `Review.Clearance.Passed` and `InstructionsRequired`, then GETs E16 using `Review.Clearance.Instructions.Id` only when both hold.
3. The 409 route reads `Conflict.Allergy.Name`, `Conflict.MedicationId` and `Conflict.Allergy.Resolution.HasAlternatives`, then GETs E17 using its nested resolution ID when alternatives exist, with `Lanes.SettleTheTail` holding the enclosing review until that request finishes.

**MI.12. A medication changes during review**

A late server failure belongs to the body that was sent.

1. The page-owned pharmacy handler captures the medication inputs and begins E15 at `Ownership.SendsSlotInput`; while its reply is held, the coordinator removes or replaces the relevant row through MI.10.
2. The released 400 route applies `ShowValidationErrors` at `Ownership.ShowsFailures`, always adding `"Amoxicillin conflicts with penicillin allergy."` to the surviving main summary and adding it inline only if the original sent input still lives.
3. A fresh check reads the current rows through `Container.Read`, so the removed medication is absent and the replacement row is checked under its own identity.

**MI.13. Services and price calculation**

The coordinator receives service availability and a complete quote together.

1. Calculate uses `Parallel` to GET E18 and POST E19, with both requests holding `quote-panel`, and sends CareLevel, PayerKind, service codes, visits and review values from the main container and the separate service component.
2. E18’s route renders available services; E19’s route reads `Quote.Pricing.MonthlyTotal` and the nested Assistance fields, and starts E20 when assistance is required.
3. The final page status becomes `"Quote ready: 3040.00 USD"` only after both branches and E20 settle, following `Lanes.Settle`, `SettleTheTail`, `AllSettled` and `After`; a stopped quote branch leaves the catalogue branch able to complete.

**MI.14. Service review component**

The coordinator inspects selected services and their calculated review values.

1. The component’s `Change` reads `Value`, `PreviousValue` and `IsInteracted`, then reads `Current().Name`, `Current().Value`, `Matching().Count()` and the deep allocation label before updating the review summary.
2. Review uses `Items.Where`, `Select`, `At`, `Find`, `Any`, `All` and `Count`, and data `Records.OrderBy`, `OrderByDescending` and `Sum`, to show two active services, three visits and the ordered labels `"Dining, Pharmacy support"`.
3. The declaration’s `Answer` callback returns each item’s name; `Pair` reads `PairArgs.Key` and `PairArgs.Record`; `SortItems` passes an `Answer` function; subsequent E19 receives the synchronous `Total()` and awaited `SettleCount()` and `SettleRecord()` values only after `Run.Settle`.
4. A rejected awaited member ends that route, runs owed cleanup and leaves the quote unaccepted under `Lanes.Throws` and `Finally`.

**MI.15. Price acceptance**

The coordinator accepts the exact quote and assistance terms displayed.

1. Accept evaluates the quote revision with `Eq`, the selected care and payer with `NotEq` checks, contribution with `Gte`, and local/awaited readiness before POST E21 sends QuoteId, Revision and AcceptAssistance.
2. The success route reads the receipt and chains E20 when assistance applies; the 409 route reads the returned Quote’s revision and price, displays `"Price changed to 3060.00 USD"`, and clears acceptance.
3. A second explicit acceptance sends revision4; neither a late revision3 reply nor a price push accepts revision4 on the coordinator’s behalf.

**MI.16. Save draft**

The coordinator saves incomplete but well-formed data.

1. Save dispatches the shared `Skip` command, reads DraftId and Referral from `PageUrl<MoveInAddress>`, reads the current main container, and PUTs E22 as JSON with `If-Match`, `NoRetry`, explicit `"Draft"` validation and conditionally added review metadata.
2. The 200 route reads `Receipt.Version`, `Receipt.Reference`, `Headers.Get("ETag")` and `Response.Request.Read` for the sent Referral query member, updates the version and displays `"Draft saved: version 8"`.
3. After request `Finally` and default `Finally`, `Chained` GETs E23 using the receipt reference; a 409 walks `Conflict.Draft.Comparison.Required` and `.Revision` to GET E24 without overwriting current edits.

**MI.17. Submit admission**

The coordinator commits the prepared admission once.

1. Submit dispatches the shared draft command, whose `Skip` handler POSTs E25 with the main JSON body and explicit `"Submit"` validator after all active fields are captured.
2. The 201 route reads `Admission.Id`, `Admission.MonthlyTotal` and `Admission.Arrival.ChecklistRequired`, shows `"Helen Ward admitted to apartment 7"`, and chains E26 using `Admission.Arrival.ChecklistId`.
3. A second click during the request or checklist tail starts nothing under `Lanes.FireSecond`; after completion a new gesture starts a new request and the server returns the existing admission rather than creating a second one.
4. A 409 bed conflict takes MI.18; a 422 clearance failure leaves the draft Open and supplies the specific server message.

**MI.18. Bed conflict and waitlist**

A failed admission offers an explicit alternative.

1. E25’s 409 route reads `Conflict.Bed.Holder.Name`, `Conflict.Bed.Hold.ExpiresAt` and `Conflict.Waitlist.Allowed`, uses `Concat` for `"Bed held by James Reed until 09:45"`, and GETs E27 only when the returned offer allows it.
2. Join waitlist validates its independent container and POSTs E28 with the resident ID read from the main container, the selected community and care level, reason and accepted terms.
3. The success route displays `"Helen Ward is number 1 on the waitlist"` and opens the saved waitlist record; it leaves BED4 with James Reed and does not mark the draft Submitted.

**MI.19. Discard draft**

The coordinator confirms closure of an open draft.

1. Discard asks `NativeConfirm.Ask("Discard Helen Ward’s draft?")`, and only its true answer DELETEs E29 with the current version, `NoValidation` and `NoRetry`.
2. The 204 `Content.None` route unloads the draft’s slots and displays `Notice.Success("Draft discarded")`; cancellation displays `Notice.Info("Draft kept")`.
3. Disposing the owner while the question waits aborts its declared signal and closes the question under `Run.Abort`; no continuation is interpreted as consent.

**MI.20. Bed availability by server push**

Availability updates remain current while other work runs.

1. `ServerPush` handles `BedChanged`, reads `CommunityId`, `Bed.Id`, `Bed.Revision`, `Bed.Available` and `PriceRevision`, and ignores messages whose community differs or whose revision is `Lte` the displayed revision.
2. An accepted message starts parallel E30 and E18 refreshes with the selected bed and new revision, while a separately owned bed card handles the same message without waiting for the page handler.
3. `Realtime.BeginPage`, `Message`, `Answer` and `DefaultFinally` keep a page request and a message request independently busy; the displayed revision advances only from an accepted reply.

**MI.21. Availability reconnects or closes**

The coordinator sees whether the displayed availability is confirmed.

1. Stream `Reconnecting` writes `"Availability unconfirmed"` without acquiring request busy, and `Reconnected` starts E30 before writing `"Availability confirmed"`.
2. `Closed` displays the reconnect offer; retry follows `Realtime.Drop`, `GiveUp`, `Retry`, `Connect` and `Reconnected`, and the offer remains until every failed held connection opens or leaves.
3. Closing the bed card performs `Unload` while the page retains its connection; closing the final availability owner calls `Stream.Close` and releases the offer under `Ownership.ServerHangsUp`, `Retry` and `Realtime.Unload`.

**MI.22. Admission documents**

The coordinator submits several files as one inspected batch.

1. File selection’s native `change` reads no event data, reads the declared file list’s `Count`, each `BrowserFile.Name`, `Type`, `Size` and `LastModified`, and refuses names with unsupported extensions or types before any upload.
2. Upload first calls E32 with Kind and DraftId, reads `Upload.Allowed`, `MaximumBytes` and `Types`, and uses nested `When` conditions to require every selected file to fit the policy.
3. The accepted branch POSTs E33 with `Multipart`, repeated `File` parts from the live file list, Caption and Kind from the documents container, and a conditional AuthorityFile whose source is never read when IncludeAuthority is false.
4. The 201 route reads `Admission.Documents.Accepted`, displays `"2 documents accepted"`, and chains E23 using `Admission.Documents.Receipt.Id`; a 422 scan refusal saves neither file.

The app’s proposed `AdmissionFileInput` declaration mirrors the native file input’s `files` collection, and `AdmissionFileList` exposes that collection through `LiveIterator<BrowserFile>`. They are app declarations on `ComponentVendor.Native`, not new framework members.

**MI.23. Signature replacement and document metadata**

The coordinator checks the saved document before replacing its bytes.

1. Inspect uses HEAD E35 and reads only `Status` and `Headers.Get` to show `"consent.txt — 14 bytes"`.
2. Replace checks the selected file’s Name with `EndsWith(".txt")`, Type with `In("text/plain")`, Size with `Between(1,1048576)`, and the document ID with `NotEmpty`, then PUTs E34 with the file as the entire body and metadata in typed query fields.
3. A 200 reply reads the nested accepted document’s revision and shows `"Signature replaced: revision 2"`; a 409 retains the existing bytes and offers a fresh HEAD.
4. Optional metadata sources distinguish held null from a missing value; an absent non-nullable metadata value stops the send, and a nullable field uses the contract’s empty or omitted representation without converting absence to an invented filename.

**MI.24. Admission packet and page plugin**

The coordinator saves a packet, records a note and opens its printable form.

1. Download checks the accepted quote revision and calls E36 through `Download`, using the response filename `admission-D100.txt`.
2. Save packet note POSTs E37 with the entire text body from the resident notes, CoordinatorId in query and `X-Page-Url` from the page-address plugin’s readable URL, then reads `Content.Text`.
3. Print gets the current packet revision, obtains `"HW"` through the existing function-plugin declaration `Initials.Of`, POSTs E55 with that signer and the page URL, and calls the app’s print plugin only after the receipt accepts that revision.
4. The proposed `AdmissionPagePlugin` exposes the real page URL and print operation; its declaration uses `Plugin`, `ReadOnly`, `Method`, `MethodAsync`, `Fixed` for the admission print mode and `Pass<IAdmissionPrintPacket,AdmissionPrintPacket>` for a typed packet argument, while `Initials.Of` uses the existing `CallItself`.
5. The print helper uses a named `Mirrors` Module and Name; the app’s reference-field render helper uses `InputField.Html`, `Member`, `Name`, `Id`, `ValueJson`, `Json`, `Render` and `WriteTo` to render the draft reference as a bound scalar.

The print receipt means “print requested.” It does not claim that paper emerged from a printer.

**MI.25. Admission deposit**

The coordinator authorises a fake deposit and receives a persistent receipt.

1. Pay validates the independent Deposit container, including `CreditCard`, consent, amount precision and exact required amount, then POSTs E38 with the entered card, cardholder, amount, currency and operation ID.
2. The success route reads `Deposit.Receipt.ReceiptId`, `LastFour`, and `Deposit.Balance.Satisfied`, shows `"Deposit authorised: 500.00 USD, card ending 1111"`, and updates the admission readiness display.
3. A 402 shows `"The deposit was declined."`; a lost reply leaves `"Deposit outcome unknown"` and a subsequent E39 read resolves the same operation without another charge.

**MI.26. Dietary needs**

The kitchen receives a valid flags combination and the current meal count.

1. Diet changes read the main container’s typed `DietaryNeeds`, validate `IsInEnum` under its flags registration, and choose `"Kitchen review required"` or `"Meal plan ready"` with `Choose`.
2. Save PUTs E40 with JSON enum translation, Kinds and OptionalLabels; the service uses `None`, Vegetarian and LowSodium as declared enum values rather than treating a combined flag as an unknown scalar.
3. Preview GETs E41 with the same flags as form/query text, repeated list fields and a nullable list item, showing `"Vegetarian, low sodium: 3 meals"`; forbidden kitchen combinations receive the server’s 422.

**MI.27. Coordinators’ line and ward preparation**

Coordinators exchange real messages and request preparation for the chosen bed.

1. Opening the line uses `Hub<CoordinatorHub>`, requires `State` Open, calls Join through `Send`, and calls Snapshot through `Invoke`, whose returned `Line.Coordinators` and `Messages` populate the card.
2. Send message requires text `MinLength(3)`, `NotEmpty`, `NotIn("test","spam")`, and a current draft, then invokes Post with DraftId, CO1 and `"Helen arrives at 10:00"`; the resulting MSG1 is appended to the visible line.
3. `SignalR` handles Changed and reads `Line.Sequence`; reconnect reads `HubReconnected.ConnectionId`, reloads the snapshot and leaves the draft untouched.
4. `ConformanceStore.Posted` reads `Message`, requires `Contains("Bed 4")` and `StartsWith("Bed")`, then POSTs E45 with condensed `Set` fields Message, BedId and CoordinatorId, and displays the returned preparation instruction.
5. The store’s declaration uses `DefineReturnedUnsubscriber` and `ReturnedUnsubscriber`; the hub’s named methods use `Invoke`, `Send` and their fixed method names, and the app’s plugin declarations select `EventTarget`, `Methods`, or `CallbackProperties` according to the objects they mirror.

**MI.28. Assessment objects and their owners**

A handed assessment remains active only while its card and object live.

1. The allocation component’s `Handed` event supplies a `ConformancePeer`; the handler reads its Ratio and Text and calls `Listen(peer, Change, …)`, whose later payload reads Value, PreviousValue and IsInteracted before POST E44.
2. The component’s `Raised` event supplies another peer through `Raised.Peer`, and a repeated handoff of that same peer adds no second listener under `Ownership.ListenA`; a different peer receives its own listener.
3. Opening A and B loads E5 AllocationOwner and PayerOwner, replacing A releases B first, and closing A while E44 waits disposes its run, completes owed cleanup and releases listeners under `Lanes.Dispose`, `Finally`, `Release`.
4. The host restarts the component, destroys a listened peer or drops its last reference, and subsequent release skips obsolete objects under `Ownership.HostDestroys`, `HostDestroysListened` and `Collect`.
5. A release whose vendor unsubscriber throws still releases every nested owner and replaces or empties its content under `Ownership.UnsubscribeThrows`; the surviving page receives the next event once.
6. A save initiated outside an independent card after that card leaves meets a missing container or individual input and stops before transport; an input guarded false is `leftOut` and is never looked up.
7. The capacity gauge uses `ConformanceRegistered.Reading`, `Threshold`, `Alarmed`, `Raise` and `Lower`; its declaration uses `Define` with `ByPlugin`, and its no-payload event admits no invented reading in its arguments.

**MI.29. Room handoff callback**

A vendor with one callback property supports the page and nested admission work.

1. `ConformanceCallbacks.OnRaised` reads Room, appends `"Admission"` after the vendor’s `"Desk"` word, and POSTs E42 with Room, DraftId, reference `"ROOM7"` and CO1 when Room `Matches("^[0-9]+$")`.
2. Loading A and nested B adds their listeners in order, so one raise produces `"Desk Admission Payer"` and the distinct room-review and payer work, with `CallbackProperty.LoadA`, `LoadB`, `Step` and `Raise` fixing installation and invocation.
3. Unloading B or A removes only their listeners and restores the vendor’s prior callback at the last release, under `CallbackProperty.UnloadB` and `UnloadA`.
4. Vendor callback replacement, locking and host restart exercise `CallbackProperty.Script`, `Freeze` and `Restart`; release never overwrites a newer vendor callback or writes into the destroyed generation.
5. `OnSearch` reads `SearchArgs.Query` through `Argument(0)`, rejects `IsEmpty` or fewer than three characters, GETs E43 for `"Cedar"`, and passes the returned names to `SearchArgs.Show` through its existing `MethodAt(1, …)`.
6. The declaration uses `DefineCallbackProperties`, `CallbackProperties`, `Listening` and `Vendor`; a throwing prior callback does not prevent the remaining live listeners from hearing the event.

**MI.30. Reception’s native submission**

Reception sends the browser’s actual successful-control set.

1. The reception view uses `BeginRequestForm`; its Save action POSTs E46 through `UrlEncoded(form, …)`, sending framework VisitorName and native Relationship, Consent, Desk and Copies controls, including a Desk control associated by its `form` attribute.
2. Disabled controls, unchecked checkboxes and controls assigned to a different form contribute exactly the browser’s submission set; `Form` on the document intake companion sends its native file controls as multipart.
3. A submit button with native form submission attributes sends the same reception form directly to E46 and receives the HTML receipt as navigation, while its reactive Save action keeps the receipt in the page.
4. `ConformanceElement.Opened` reads Type and nullable `Detail.Room`, uses `IsNull`/`NotNull` before opening apartment7, and `Keydown.Key` handles Enter without treating an absent detail as a room.

**MI.31. Request defaults and recovery**

The admissions desk has one deliberate request policy.

1. Startup calls `AddReactiveRequests` with `Header("Care-Site","North")`, `Antiforgery(AntiforgeryIn.Header)`, `Retry(3, two seconds)`, `ShowBusy`, `ShowValidationErrors`, default 401 and 503 routes, a typed catch-all, default `WhileLoading` calling `CareSession.Renew`, and default `Finally` calling `CareSession.Flush`.
2. The reception profile instead selects `Antiforgery(AntiforgeryIn.FormField)`; the public information profile explicitly selects `NoAntiforgery` and serves no private mutation.
3. Saving follows `Request.Check`, `DefaultLoading`, `Loading`, `Guard`, `Attempt`, `Route`, `Finally`, `DefaultFinally`, `Chained` and `After`, with the request’s loading showing `"Saving admission"` and its cleanup restoring `"Ready"`.
4. E32’s policy inspection uses `NoDefaults` and explicit routes; a background E30 snapshot uses `NoBusy`; E22 and E29 use `NoRetry`; safe catalogue reads use `NoValidation`.
5. The catch-all reads `Outcome`, reads a typed error body only under `FailureOutcome.Status`, distinguishes `Network`, `Redirect` and `Content`, and issues E54 only when `Error.Recovery.Allowed` is true.
6. Parallel quote and availability requests share one busy target without hiding it early, while a request with no inferred target uses page busy and a row request derives its component target from the row action’s Element.

`NoDefaults` does not remove same-origin antiforgery configuration. This follows [D9.22 and D10.12](spec/Core/DESIGN.md:1927).

**5. THE VALIDATORS**

Every request model is registered explicitly with `Add<TValidator>()`. DietaryNeeds is registered with `Flags<DietaryNeeds>()`. The browser checks the sent body, including binder starts for unsent members; it does not inspect every input on the page.

The grammar and server-only boundary come from [Validation/module.md](spec/Validation/module.md:8) and [the rule API](Alis.Reactive/PublicAPI/net10.0/PublicAPI.Unshipped.txt:606).

| Validator/member | Rules and condition | Browser and server responsibility |
|---|---|---|
| `MoveInRules` | `Include` shared identity rules; `RuleSet("Draft")`, `"Submit"`, `"Pharmacy"`, `"Pricing"` | Draft permits incomplete values; Submit requires active sections |
| `MoveInRules.Location` | `RuleFor`, `SetValidator(LocationRules)` | Shared completeness; server checks current ownership and care eligibility |
| CommunityId, BuildingId, ApartmentId, BedId | `NotEmpty`, `WithName`, literal `WithMessage` | `"Choose a bed."` is a browser refusal |
| ResidentId | `NotEmpty`; `Must` against world resident identity | Existence and duplicate admission stay server-side |
| Resident.Identity | `NotNull`, `ChildRules` | Child rules execute only for a present holder |
| Identity.Name | `Cascade(Stop)`, `NotEmpty`, `Length(2,100)` | `"Enter the resident’s name."` precedes length checks |
| Identity.PreferredName | `MaximumLength(50).Unless(name is null)` | Shared |
| Identity.Email | `NotEmpty`, `EmailAddress` | `"Enter a valid resident email."` |
| Identity.EmailConfirmation | `Equal(Email)` | `"The email addresses must match."` |
| Identity.BirthDate | `LessThan(Today)`, `LessThanOrEqualTo` admission-age cutoff supplied by server model | Shared date comparison; server recomputes eligibility from world Today |
| Resident.MoveInDate | `GreaterThanOrEqualTo(Today)` | `"Move-in cannot precede today."` |
| Resident.ArrivalAt | `GreaterThanOrEqualTo` the proposed move-in’s start instant | Shared instant comparison |
| Resident.Address | `SetValidator(AddressRules)` | Nested paths retain Resident.Address |
| CountryCode | `Length(2)` | `"Use a two-letter country code."` |
| PostalCode | `Matches("^[0-9]{5}$")` | Shared for CountryCode US |
| Notes | `MaximumLength(500)` | Shared |
| Care.Level | `IsInEnum` | Shared |
| Care.Assisted | `When(Level==Assisted)` with `Otherwise` rules for inactive branches | Active branch required; inactive branch absent |
| AssessmentCode | `NotEmpty`, `Matches("^AS-[0-9]+$")`, `DependentRules` for assessment date | Shared |
| AssistanceHours | `InclusiveBetween(0,24)`, `PrecisionScale(4,2,true)` | Shared |
| Memory consent | `Equal(true)` under Memory | Shared |
| Skilled order | `NotEmpty`, `MinimumLength(5)` under Skilled | Shared |
| Contacts.Rows | `RuleForEach`, `SetValidator(ContactRules)` | Every sent row |
| Contact.Name | `NotEmpty`, `Length(2,100)` | Shared |
| Contact.Email | `EmailAddress` | Shared |
| Contact.Phone | `NotEmpty`, `Matches("^[0-9]{10}$")` | Shared |
| AlternatePhone | `NotEqual(Phone)` when nonempty | Shared two-member comparison |
| Contact.Authority | Group `When(Role!=Ordinary)` and `Otherwise`; `NotNull`, `SetValidator` | Ordinary role uses `Empty` for authority reference |
| Authority.Reference | `NotEmpty`, `MinimumLength(5)` | Shared shape |
| Authority dates | `EndsOn.GreaterThanOrEqualTo(StartsOn)`; nullable end allowed | Shared |
| Authority consent | `Equal(true).Unless(Role==Guardian)` | Shared |
| Authority.Reference | `Must` against revoked-reference set | Server only |
| Payer.Primary | `SetValidator(CoverageRules)` | Shared nested rules |
| PolicyNumber | `NotEmpty`, `StartsWith` expressed in the rule condition and `Matches("^POL-[0-9]+$")` as the check | Shared format |
| PortalUrl | `Url` | Shared |
| Coverage dates | `StartsOn.LessThanOrEqualTo(MoveInDate)` and `EndsOn.GreaterThanOrEqualTo(MoveInDate)` | Shared |
| Secondary | `When(HasSecondary)`, `NotNull`, `ChildRules`; `Otherwise` absent secondary | Shared |
| Secondary contribution | `ExclusiveBetween(0,100)` | Shared |
| Combined contributions | `Custom`; `CustomContext.Model`; `AddFailure(member,message)` and `AddFailure(message)` | Server verifies total100 and produces both field and summary failures |
| Policy eligibility | `MustAsync` | Server only |
| Payer reference | `NotEmpty().ServerOnly()` under benefits requiring a server-issued reference | Browser does not invent that reference |
| Payer rules | Group `WhenOnServer` for policy hold; group `UnlessOnServer` for authorised exemptions | Server only |
| Payer document | Rule-level `WhenOnServer` and `UnlessOnServer` | Server only |
| External coverage review | Group `WhenAsync`; independent review `UnlessAsync` | Server only; endpoint uses ValidateAsync |
| Medications.Rows | `RuleForEach`, `SetValidator(MedicationRules)` | Shared |
| DrugCode | `NotEmpty`, `Matches("^[A-Z0-9]+$")` | Shared |
| Dose | `GreaterThan(0)`, `LessThanOrEqualTo(1000)`, `PrecisionScale(6,2,true)` | Shared; drug-specific ceiling stays server-side |
| Schedule | `NotEmpty` | Shared |
| Medication dates | `EndsOn.GreaterThanOrEqualTo(StartsOn)` | Shared |
| RoundDoses | `RuleForEach`, `GreaterThanOrEqualTo(MinimumRoundDose)` | Item versus owning-model peer, nullable items retained |
| Pharmacy conflict | `CustomAsync`, adding failure to the offending medication | Server only |
| Services.Codes | `NotEmpty` for pricing and submission | Shared |
| Service visits | `RuleForEach`, `ChildRules`, `InclusiveBetween(0,14)` on Count | Shared |
| Capacity | `Must` against current service records | Server only |
| Accepted quote | `NotNull` for Submit, `Must` for current revision | Browser presence; server currentness |
| Deposit.CardNumber | `NotEmpty`, `CreditCard` | Shared checksum; authority decision server-only |
| Deposit.Amount | `GreaterThan(0)`, `Equal(500.00)`, `PrecisionScale(7,2,true)` | Shared; current deposit amount rechecked server-side |
| Deposit consent | `Equal(true)` | Shared |
| Diet.Needs | `IsInEnum` with registered flags | Shared; combination3 accepted, undefined bit8 refused |
| Diet.MealCount | `InclusiveBetween(1,6)` | Shared |
| Kitchen capacity | `Must`; model-built `WithMessage(model => …)` | Entire model-message rule stays server-side; `"Pureed meals are unavailable at Harbor House."` |
| Documents.Caption | `NotEmpty`, `MaximumLength(120)` | Shared |
| Documents bytes | `CustomAsync` | Server-only scan; metadata is not clearance |
| Waitlist.Reason | `NotEmpty`, `MinimumLength(10)` | Shared |
| Waitlist.AcceptTerms | `Equal(true)` | Shared |
| Reception.VisitorName | `NotEmpty`, `Length(2,100)` | Shared in reactive send; native navigation is rechecked by endpoint |
| Reception.Copies | `InclusiveBetween(1,3)` | Shared/binder refusal |
| RoomReview.Room | `NotEmpty`, `Matches("^[0-9]+$")` | Shared; room lock server-only |
| Mobility.Ratio | `InclusiveBetween(0,100)` | Shared; superseded assessment server-only |

`RuleLevelCascadeMode=Continue` retains independent failures. Selected fields use `Cascade(Stop)` to avoid a dependent message after a missing prerequisite.

Synchronous endpoints use `EndpointChecks.Validate`; asynchronous rules use `EndpointChecks.ValidateAsync`, including named rule-set overloads. They inspect `IsValid`, preserve `Errors`, `PropertyName` and `ErrorMessage`, and return `ToErrorResponse`, whose `ErrorsByField` becomes `ValidationProblem.Errors`.

**6. THE SITUATIONS**

Each numbered situation is a separate world and browser promise.

For compactness, **unchanged D100** means the exact version7 seed, and **read again** means a normal GET of the affected draft, receipt, document, availability or history view. It does not mean inspecting a component object or evaluating JavaScript.

The transport case below drops transport before the endpoint executes, on every configured attempt, except where an explicit after-commit failure is named. Thus “unchanged” has a defined server boundary.

| Feature | Numbered observable situations |
|---|---|
| MI.1 | **1.** Choose C1/B1/A7: page offers Window and held Door; model unchanged; read again lists BED4 revision12. **2.** Leave Community empty: `"Choose a community."`, no lookup; model unchanged; read again still C1. **3.** Server closes B1: selecting it shows `"Cedar is closed."`; model holds B1 closed; next read lists no Cedar beds. **4.** Lookup transport fails: `"The request failed."`; no choices replaced; next read retains saved location. **5.** Hold C1 lookup, choose C2, release C2 then C1: page keeps Elm/apartment21; saved draft stays C1 until save; read again confirms that distinction. |
| MI.2 | **1.** Confirm Memory: Memory fields appear; saving replaces Assisted with Memory; next read shows Memory only. **2.** Cancel confirmation: Assisted and AS-100 remain; draft unchanged; next read agrees. **3.** Server refuses secure-unit admission: `"Memory care admission is closed."`; no care mutation; next read retains Assisted. **4.** Section GET fails: previous confirmed section remains recoverable; draft unchanged; next read retains Assisted. **5.** Change care while its previous partial loads: only the new branch joins; subsequent save and read contain no old-branch fields. |
| MI.3 | **1.** Verify seed identity: `"Identity checked by Maya Chen"`; ID100 exists; history read shows identity review. **2.** Mismatched email confirmation: `"The email addresses must match."`; no review; next read has no ID100. **3.** Server seeds an active admission for R101: duplicate-admission message; no review; next read shows that existing admission. **4.** Identity transport fails: error notice; no review; next read has no ID100. **5.** Close payer verification with invalid independent details, then verify resident: resident review succeeds and the main summary does not inherit the drawer’s failures. |
| MI.4 | **1.** Review BED4: `"Cleared"` and North allocation instructions; AR100 exists; next history read shows BED4/revision12. **2.** Local Check false: `"Allocation is not ready"`; no readiness call or review; next read has no AR100. **3.** BED4 becomes held: James Reed’s conflict appears; AR100 absent; availability read shows revision13. **4.** Awaited readiness fails: quote remains unaccepted, busy clears; no review; next read confirms none. **5.** New bed selection replaces an awaiting review: its late answer changes no current card; next saved read identifies the newer selection. |
| MI.5 | **1.** Add CT3 `"Cara Ward"` with valid details: three contacts shown; version8 holds CT1/CT2/CT3; next read preserves Alice’s edits. **2.** Existing contact email malformed: email message; no CT3; next read has two contacts. **3.** Four contacts already stored: `"An admission has at most four contacts."`; fifth absent; next read has four. **4.** POST fails before execution: error notice; version7/two rows; next read agrees. **5.** Add during save: shared Skip admits one command; next read shows one version increment and the command that actually ran. |
| MI.6 | **1.** Set CT2 Guardian with `"GUARD-200"`: authority fields save; next read holds that reference. **2.** End before start: authority date message; no change; next read has Ordinary CT2. **3.** Server revokes `"GUARD-200"`: revoked-authority message; no authority saved; next read agrees. **4.** Authority load fails: no false saved status; model unchanged; next read agrees. **5.** Switch Guardian to Ordinary while authority loads: section stays unloaded; next saved read has Authority null. |
| MI.7 | **1.** Remove CT2: Alice remains at row0; version8 contains CT1 only; next read agrees. **2.** Removal request lacks a row ID: validation notice `"Choose a contact."`; no delete; next read has both. **3.** Remove CT1: payer explanation names POL-100; both contacts remain; next read still links CT1. **4.** DELETE transport fails: error notice and retained rows; next read has both. **5.** Payer changes to CT2 before CT2 removal executes: server refuses removal; next read shows CT2 as payer contact. |
| MI.8 | **1.** Enable secondary coverage at20% beside primary80%: saved page shows both; next read contains the two policies. **2.** Secondary contribution100: exclusive-range message; no save; next read has no secondary. **3.** Combined contribution110: server field and summary failures; saved payer unchanged; next read agrees. **4.** Secondary load fails: error notice; saved payer unchanged; next read agrees. **5.** Change Insurance to Private while secondary loads: late secondary never joins; next save/read contains Private and no secondary. |
| MI.9 | **1.** Verify POL-100: coverage detail shows3500.00; V100 stored; next read shows V100. **2.** Consent false: `"Consent is required."`; no verification; next read has none. **3.** POL-200: inactive-policy message; no verification; next read has none. **4.** Verification transport fails: drawer remains usable, busy clears; next read has none. **5.** Attempt close during held verification: Cancel keeps drawer open; save draft independently succeeds; next read shows saved draft and, after release, V100. |
| MI.10 | **1.** Add M2 AZI250: both rows appear; version8 holds ASP75 and AZI; next read agrees. **2.** Dose `12.345`: precision message; no save; next read keeps75.00. **3.** ASP75 dose500: server maximum-dose refusal; model keeps75.00; next read agrees. **4.** Mutation transport fails: no new row; next read contains M1 only. **5.** Delete M1 during pharmacy review: next read contains only retained rows; MI.12 governs the old failure. |
| MI.11 | **1.** Review ASP75: clearance and instructions shown; CL100 stored; next read shows CL100. **2.** Empty drug code: field refusal; no clearance; next read has none. **3.** AMOX with penicillin: alternatives show Azithromycin; no clearance; next read records no accepted pharmacy review. **4.** Instructions GET fails after clearance: clearance remains CL100, instructions unavailable; next read still shows CL100. **5.** Payer section unloads during pharmacy work: pharmacy’s main-owned result survives; next read contains only active payer fields. |
| MI.12 | **1.** Keep the sent AMOX row: failure appears inline and in summary; no clearance; next read confirms none. **2.** Correct the row to an empty drug before another check: browser refuses the new body; next read retains the prior saved medication. **3.** Replace the sent row before server400: summary shows conflict, replacement field stays unmarked; next read shows the replacement row. **4.** Old review transport fails: no inline error attaches to replacement; saved rows unchanged; next read agrees. **5.** Unload the medication section after send: main summary still receives the server failure; next save/read omits the unloaded rows. |
| MI.13 | **1.** Calculate DIN/PHA:3040.00 appears after both tails; Q100 stored; next read shows revision3. **2.** No service selected: pricing refuses while catalogue completes; no quote stored; next read has none. **3.** Select TRA with capacity0: `"Transport has no capacity."`; no quote; next read confirms capacity0. **4.** Catalogue transport fails while quote succeeds: catalogue error and stored Q100 coexist; next read shows Q100. **5.** Release quote first but hold assistance: `"Calculating"` remains until assistance settles; next read shows the same single quote. |
| MI.14 | **1.** Review seed services: two active services and three visits shown; E19 stores those review facts; next quote read agrees. **2.** Deactivate all items: `"Choose an active service."`; no new quote; next read unchanged. **3.** Service capacity changes after component review: quote endpoint refuses; next read shows current capacity. **4.** SettleRecord503: review stops and busy clears; no accepted quote; next read agrees. **5.** Replace service panel while awaiting its record: old continuation stops; the new panel’s next quote uses its own values. |
| MI.15 | **1.** Accept revision3: `"Quote accepted"`; accepted revision3 stored; next read shows3040.00. **2.** Assistance unchecked: `"Accept the assistance terms."`; no acceptance; next read has null revision. **3.** Changed price seed:3060.00 and revision4 appear; no acceptance recorded; next read has revision4 unaccepted. **4.** Acceptance reply lost before execution: no acceptance; next read proves null. **5.** Price push meets acceptance: server checks current revision; next read never pairs accepted3 with quote4. |
| MI.16 | **1.** Save: version8/SAVE8 and ETag8 shown; next read has entered data. **2.** Malformed dose: browser field message; version7 remains; next read agrees. **3.** CO2 saved version8 first: comparison names Luis Ortiz; current edits stay visible; next read shows CO2’s version8. **4.** PUT transport fails: error notice; version7 remains; next read agrees. **5.** Unload secondary before Save reads: saved version8 has Secondary null; next read agrees. |
| MI.17 | **1.** Submit Ready seed: ADM100 and checklist shown; BED4 occupied by R101, draft Submitted/version8; next read shows that admission. **2.** Missing bed: `"Choose a bed."`; no admission; next read remains Open. **3.** Bed taken between list and submit: James Reed conflict; ADM100 absent; next read shows revision13/James. **4.** POST fails before execution: error notice; no admission; next read remains Open. **5.** Double submit while checklist tail waits: one ADM100; next read shows one bed allocation and one admission history entry. |
| MI.18 | **1.** Accept waitlist terms: W100/position1 shown; next read shows W100 and BED4 still held by James. **2.** Terms unchecked: browser refusal; no W100; next read confirms none. **3.** Community closes before join: server refuses; no W100; next read shows closure. **4.** Waitlist transport fails: no success message; next read has no W100. **5.** Bed becomes free while terms display: joining waitlist does not occupy it; next availability read shows free BED4. |
| MI.19 | **1.** Confirm discard: `"Draft discarded"`; D100 Discarded; next read shows closed draft. **2.** Cancel: `"Draft kept"`; D100 Open/version7; next read agrees. **3.** Discard D300: submitted-admission refusal; D300 remains Submitted/version9; next read agrees. **4.** DELETE transport fails: error notice; D100 remains Open; next read agrees. **5.** Owner unloads while confirmation waits: question closes without delete; next read remains Open. |
| MI.20 | **1.** Push BED4 revision13: James Reed shown; world holds revision13; next availability read agrees. **2.** Deliver revision12 after13: page keeps13; next read agrees. **3.** Coordinator loses C1 access before refresh:403 notice; world retains revision13; authorised next read shows it. **4.** Refresh transport fails: `"Availability unconfirmed"`; bed state is still13; next read resolves it. **5.** Page request held while card receives push: card updates independently; final next read shows one current revision. |
| MI.21 | **1.** Reconnect succeeds: unconfirmed then confirmed; bed data unchanged; next read shows revision12. **2.** Refresh while deliberately Closed: `"Open availability before refreshing."`; no request; next read unchanged. **3.** Reconnect unauthorised: offer remains with unavailable status; domain unchanged; next authorised read shows12. **4.** Both connections fail: reconnect offer remains until both resolve; next data read unchanged. **5.** Close nested card while page holds connection: page still updates; close final owner and offer disappears; next page opening reads current world. |
| MI.22 | **1.** Upload two literal files: `"2 documents accepted"`; DOC1/DOC2 sizes14/14 stored; next documents read shows both names and sizes. **2.** Select unsupported executable: browser refuses type; no records; next read empty. **3.** Upload bytes `"DENIED"` with valid metadata: scan refusal; entire batch absent; next read empty. **4.** POST transport fails: no accepted notice; next read empty. **5.** Close upload card while batch is held before commit: no route writes into replacement; next read determines whether the server accepted the batch after release. |
| MI.23 | **1.** HEAD DOC1 then replace signature:14-byte metadata and signature revision2 shown; DOC3 has11 bytes; next HEAD reports11. **2.** Empty file list: `"Choose a signature file."`; no PUT; next HEAD unchanged. **3.** Document revision changed:409; old stored bytes retained; next HEAD reports current ETag. **4.** HEAD503: metadata unavailable; no replacement issued; next documents read unchanged. **5.** Delete/unload selected document before awaited replacement captures its source: send stops at missing source; next read confirms no replacement. |
| MI.24 | **1.** Download and record note: file has the literal packet text; PN100 stores the note; next history read shows note saved. **2.** No accepted quote: `"Accept a quote before creating the packet."`; no download; next draft read unaccepted. **3.** Seal packet before note save: `"Packet is sealed."`; PN100 absent; next history read confirms none. **4.** Download returns an unnamed503 or wrong media type: failure route; no false download receipt; next model read unchanged. **5.** Price changes between print preview and intent: E55 refuses old revision; next quote read shows3060.00 and no PRINT100. |
| MI.25 | **1.** Pay4111…1111: PAY100 and due0.00 shown; one authorised500.00 payment; next receipt read agrees. **2.** Invalid checksum: browser card message; no payment; next lookup404. **3.** Card4000…0002: decline message; no authorised payment; next lookup has no authorised receipt. **4.** Lose response after committed payment: `"Deposit outcome unknown"`; lookup returns PAY100; subsequent same-operation pay returns that receipt. **5.** Deposit amount changes during submit: server refuses stale amount; next quote/payment reads show the new required amount and no duplicate charge. |
| MI.26 | **1.** Save flags3: meal plan shows Vegetarian, low sodium,3 meals; next draft read holds3. **2.** Undefined flag8: enum refusal; no save; next read holds3. **3.** Pureed kitchen restriction: model-built server message; no change; next read holds3. **4.** Meal preview fails: saved diet remains3; next draft read proves it. **5.** Care changes while preview waits: old preview is replaced; next save/read contains the new care with explicitly retained or re-entered diet. |
| MI.27 | **1.** Send arrival message and ward update: MSG1 appears and water instruction shows; next reads show sequence1 and BED4.NeedsWater true. **2.** Two-character message: browser refusal; no message; next snapshot sequence0. **3.** Membership revoked: hub refuses; no MSG1; authorised next snapshot remains0. **4.** Ward request fails: no preparation acknowledgement; next bed read has NeedsWater false. **5.** Unload line while quote runs: hub hold leaves, quote continues; next quote read shows Q100 and next line snapshot reflects only accepted messages. |
| MI.28 | **1.** Hand one peer twice then press Change: one MOB25 review; next history read contains one mobility review. **2.** Ratio101: browser range refusal; no MOB25; next read has none. **3.** Assessment superseded: server422; no new review; next read shows current assessment reference. **4.** Mobility request fails: busy clears; no review; next read has none. **5.** Restart/destroy/drop peer before owner release: replacement works, old listeners produce no work; next read contains only new-owner reviews. **6.** Throw from nested unsubscribe: both owners still leave and replacement content appears; next raise adds one review, confirmed by history. |
| MI.29 | **1.** Raise room7 with A/B: `"Desk Admission Payer"`; room review ROOM7 stored; next history read shows one accepted review. **2.** Search two characters: `"Enter at least three characters."`; no search result replacement; domain unchanged; next room read unchanged. **3.** Room lock true: `"Room handoff is locked."`; no ROOM7; next read confirms none. **4.** Search transport fails: names are not replaced by an error body; next normal search returns the two literal names. **5.** Replace callback, freeze or restart while owners change: only current listeners act; next model read contains no duplicate old-owner review. **6.** Prior vendor callback throws: its error appears, live Admission/Payer listeners still run; next read shows their accepted work. |
| MI.30 | **1.** Native submission signs Alice in: receipt names Helen and North desk; REC100 stored; next history read shows sign-in. **2.** Reactive save with Copies0: browser range message; no REC100; next read confirms none. **3.** Visitor barred by server:422; no REC100; next read confirms none. **4.** Reactive form transport fails: error notice; no sign-in; next read confirms none. **5.** Main move-in has invalid medication while reception submits: REC100 succeeds independently; next read shows sign-in and unchanged medication. |
| MI.31 | **1.** Save through defaults: Saving admission, receipt, Ready; version8 and flushed history; next read shows SAVE8. **2.** Check fails: no renewal/loading/send/cleanup lifecycle begins; version7; next read agrees. **3.** Session renewal401: main request never sends; version7; next authorised read agrees. **4.** GET receives503 twice then200: one lifecycle and one final route; model unchanged; next read gives current data. **5.** Quote and availability share a target: first completion does not hide the second’s busy indicator; next reads show both accepted results. **6.** Remove target while default loading waits: guard stops before send, cleanup pairs correctly; next read confirms no mutation. |

Host restart and collection cases use the existing harness’s `Restart` and `CollectGarbage` operations. Test code does not use `page.evaluate()` to simulate them.

**7. COVERAGE**

These tables map specified uses. They do not report executed proof.

**The 142 door-map names**

| Name | Feature.interaction |
|---|---|
| Add | MI.7.1; MI.14.2 |
| AddReactiveRequests | MI.31.1 |
| All | MI.14.2; MI.22.2 |
| And | MI.2.2; MI.4.1 |
| Answer | MI.14.3 |
| Antiforgery | MI.31.1–2 |
| Any | MI.14.2 |
| Argument | MI.29.5; MI.14.3 |
| At | MI.14.2; MI.20.1 |
| Body | MI.4.2; MI.11.2–3 |
| Busy | MI.13.1; MI.31.6 |
| ByPlugin | MI.28.7 |
| CallItself | MI.24.3–4 |
| Callback | MI.14.3 |
| CallbackProperties | MI.29.6 |
| Chained | MI.5.2; MI.16.3 |
| Choose | MI.26.1 |
| Classes | MI.7.1 |
| Component | MI.4.1; MI.14.1 |
| Container | MI.1.1; MI.16.1 |
| Content | MI.19.2; MI.24.1–2 |
| Count | MI.14.1–2; MI.22.1 |
| Define | MI.28.7 |
| DefineCallbackProperties | MI.29.6 |
| DefineReturnedUnsubscriber | MI.27.5 |
| Delete | MI.7.1; MI.19.1 |
| Download | MI.24.1 |
| Element | MI.7.1; MI.31.6 |
| Else | MI.2.1; MI.8.1 |
| ElseIf | MI.2.1; MI.8.1 |
| Eq | MI.15.1 |
| Error | MI.31.5 |
| Errors | MI.12.2 |
| Event | MI.28.1; MI.29.1 |
| EventTarget | MI.27.5 |
| File | MI.22.3; MI.23.2 |
| Finally | MI.16.3; MI.31.1–3 |
| Find | MI.14.2 |
| Fixed | MI.24.4; MI.27.5 |
| Form | MI.30.2 |
| FormField | MI.31.2 |
| Get | MI.1.1–2; MI.23.1 |
| Gt | MI.20.1, accepting a newer revision |
| Gte | MI.15.1 |
| Head | MI.23.1 |
| Header | MI.16.1; MI.31.1 |
| Headers | MI.16.2; MI.23.1 |
| Html | MI.24.5; MI.30.3 |
| Id | MI.24.5 |
| IfNull | MI.4.3 |
| Info | MI.19.2 |
| Input | MI.28.6; MI.1.1 |
| InputField | MI.3.1; MI.24.5 |
| Inputs | MI.10.1 |
| Into | MI.1.1–2; MI.9.1 |
| Invoke | MI.27.1–2 |
| Json | MI.16.1; MI.24.5 |
| LastModified | MI.22.1; MI.23.2 |
| Listen | MI.28.1–2 |
| Listening | MI.29.6 |
| LiveObject | MI.4.1; MI.28.2 |
| LiveObjects | MI.14.2; MI.22.1 |
| Lt | MI.23.2, file below maximum |
| Lte | MI.20.1 |
| Member | MI.24.5 |
| Method | MI.24.4; MI.28.7 |
| MethodAsync | MI.24.4; MI.31.1 |
| MethodAt | MI.29.5 |
| Methods | MI.27.5 |
| Module | MI.24.5 |
| Multipart | MI.22.3 |
| Name | MI.22.1; MI.24.5 |
| Native | MI.22 declaration; MI.30.4 |
| NativeActionButton | MI.7.1 |
| NativeActionLink | MI.7.3 |
| Network | MI.31.5 |
| NoAntiforgery | MI.31.2 |
| NoBusy | MI.31.4 |
| NoDefaults | MI.31.4 |
| NoRetry | MI.16.1; MI.19.1 |
| NoValidation | MI.19.1; MI.31.4 |
| None | MI.19.2 |
| Not | MI.2.2; MI.6.2 |
| NotEq | MI.2.1; MI.15.1 |
| Notice | MI.19.2; MI.31.5 |
| OnError | MI.7.2; MI.31.5 |
| OnSuccess | MI.4.2; MI.16.2 |
| Options | MI.22.2 |
| Or | MI.2.2; MI.6.2 |
| OrNull | MI.11.1, nullable review reference |
| OrderBy | MI.14.2 |
| OrderByDescending | MI.14.2 |
| Outcome | MI.31.5 |
| PageUrl | MI.1.1; MI.16.1 |
| Parallel | MI.13.1; MI.20.2 |
| PartialFor | MI.5.3; MI.10.3 |
| Pass | MI.24.4 |
| Path | MI.7.1; MI.16.1 |
| Plugin | MI.24.4; MI.31.1 |
| Post | MI.17.1 |
| Property | MI.14.1; MI.28.7 |
| Put | MI.16.1; MI.23.2 |
| Query | MI.1.1; MI.23.2 |
| Reactive | MI.1.1; MI.29.1 |
| ReactivePlan | MI.1.1 and every named view |
| Read | MI.4.2; MI.16.1 |
| ReadOnly | MI.24.4; MI.28.7 |
| Redirect | MI.31.5 |
| Remove | MI.7.3, row action cleanup |
| Render | MI.7.1; MI.24.5 |
| RenderPlan | MI.1.1 and every named view |
| Request | MI.16.2 |
| Retry | MI.31.1 |
| ReturnedObject | MI.14.1 |
| ReturnedObjects | MI.14.1 |
| ReturnedUnsubscriber | MI.27.5 |
| Row | MI.7.1 |
| Rows | MI.7.1 |
| Select | MI.14.2 |
| Send | MI.27.1 |
| Set | MI.4.2; MI.22.3; MI.27.4 |
| ShowBusy | MI.31.1 |
| ShowValidationErrors | MI.3.2; MI.12.2 |
| Size | MI.22.1; MI.23.2 |
| Status | MI.23.1; MI.31.5 |
| Success | MI.19.2 |
| Sum | MI.14.2 |
| Text | MI.24.2 |
| Then | MI.2.1; MI.22.2 |
| Toggle | MI.7.1, selected row action state |
| Type | MI.22.1; MI.30.4 |
| Unload | MI.2.3; MI.28.3 |
| UrlEncoded | MI.9.2; MI.30.1 |
| Validate | MI.3.1; MI.17.1 |
| ValueJson | MI.24.5 |
| Vendor | MI.28.7; MI.29.6 |
| Warning | MI.21.2; MI.31.1 |
| When | MI.2.1; MI.22.2 |
| Where | MI.14.2 |
| WhileLoading | MI.31.1–3 |
| WriteOnly | MI.4.2 |
| WriteTo | MI.24.5 |

The additional condition doors identified in the brief also have explicit uses:

| Door | Feature.interaction |
|---|---|
| Between | MI.23.2 |
| In | MI.6.1; MI.23.2 |
| NotIn | MI.27.2 |
| Contains | MI.27.4 |
| StartsWith | MI.27.4 |
| EndsWith | MI.23.2 |
| Matches | MI.29.1 |
| MinLength | MI.27.2 |
| IsEmpty | MI.29.5 |
| IsNull | MI.30.4 |
| NotEmpty | MI.1.2; MI.27.2 |
| NotNull | MI.9.2; MI.30.4 |

**Rule-grammar doors**

| Door | Feature.interaction |
|---|---|
| `ReactiveValidator<T>` | MI.3.1; MI.17.1 |
| `RuleFor` | MI.3.1 |
| `RuleForEach` | MI.5.3; MI.10.3 |
| `Include` | MI.3.1 |
| `RuleSet` | MI.16.1; MI.17.1 |
| `SetValidator` | MI.5.3; MI.8.3 |
| `ChildRules` | MI.3.1; MI.13.1 |
| `InlineValidator<T>` | MI.3.1, identity ChildRules |
| `When`, group | MI.2.2; MI.8.3 |
| `When`, rule | MI.6.2 |
| `Unless`, group | MI.6.3 |
| `Unless`, rule | MI.3.1; MI.6.2 |
| `Otherwise` | MI.2.3; MI.8.3 |
| `Cascade` | MI.3.1 |
| `CascadeMode.Stop` | MI.3.1 |
| `CascadeMode.Continue` | MI.10.3 |
| `RuleLevelCascadeMode` | MI.10.3 |
| `DependentRules` | MI.2.2; MI.3.1 |
| `NotEmpty` | MI.3.1; MI.5.3 |
| `NotNull` | MI.8.3 |
| `Empty` | MI.6.3 |
| `Equal`, literal/member | MI.3.1; MI.25.1 |
| `NotEqual`, literal/member | MI.5.3; MI.6.2 |
| `GreaterThan`, literal/member | MI.10.3 |
| `GreaterThanOrEqualTo`, literal/member | MI.3.1; MI.10.3 |
| `LessThan`, literal/member | MI.3.1 |
| `LessThanOrEqualTo`, literal/member | MI.8.3; MI.10.3 |
| Nullable comparison overloads | MI.6.2; MI.10.3 |
| `InclusiveBetween` | MI.2.2; MI.30.1 |
| `ExclusiveBetween` | MI.8.3 |
| `Length`, exact/range | MI.3.1 |
| `MinimumLength` | MI.6.2; MI.18.2 |
| `MaximumLength` | MI.3.1; MI.22.3 |
| `Matches`, plain/options | MI.3.1; MI.10.3 |
| `EmailAddress` | MI.3.1; MI.5.3 |
| `Url` | MI.8.3 |
| `CreditCard` | MI.25.1 |
| `PrecisionScale`, nullable/non-nullable | MI.10.3; MI.25.1 |
| `IsInEnum`, nullable/non-nullable | MI.2.2; MI.26.1 |
| `Must`, value/model-and-value | MI.7.1; MI.15.1 |
| `MustAsync`, both forms | MI.9.2 |
| `Custom` | MI.8.3 |
| `CustomAsync` | MI.11.1 |
| `CustomContext.Model` | MI.8.3; MI.11.1 |
| `AddFailure(message)` | MI.8.3 |
| `AddFailure(member,message)` | MI.8.3; MI.11.1 |
| `ServerOnly` | MI.8.3 |
| `WhenOnServer`, group/rule | MI.9.2 |
| `UnlessOnServer`, group/rule | MI.9.2 |
| `WhenAsync` | MI.9.2 |
| `UnlessAsync` | MI.11.1 |
| `WithMessage(string)` | MI.3.1 |
| `WithMessage(model function)` | MI.26.2 |
| `WithName` | MI.3.1 |
| `Validate`, default/named rule set | MI.3.1; MI.16.1 |
| `ValidateAsync`, default/named rule set | MI.9.2; MI.11.1 |
| `EndpointChecks.Validate` | MI.16.1 |
| `EndpointChecks.ValidateAsync` | MI.11.1 |
| `Add<TValidator>` | MI.31.1 |
| `Flags<TEnum>` | MI.26.1 |
| `IsValid` | MI.17.1 |
| `Errors` | MI.12.2 |
| `ErrorsByField` | MI.12.2 |
| `PropertyName` | MI.12.2 |
| `ErrorMessage` | MI.12.2 |
| `ToErrorResponse` | MI.3.2; MI.12.2 |

**Statement, expression and shape kinds**

Source: [schema statements](spec/Core/schema.md:215), [expressions](spec/Core/schema.md:257), and [shapes](spec/Core/schema.md:352).

| Union/name | Use |
|---|---|
| statement `set` | MI.4.2 component writes; MI.9.4 synchronous Cancel |
| statement `do` | MI.19.2 Unload and Notice |
| statement `let` | MI.4.1 awaited readiness captured at its line |
| statement `if` | MI.2.1–2 nested care conditions |
| statement `all` | MI.13.1 |
| statement `request` | MI.16.1–3 |
| statement `listen` | MI.28.1–2 |
| statement `stop` | MI.3.1 failed validation; no view-authored stop |
| expression `lit` | MI.25.1 required500.00 and USD |
| expression `obj` | MI.14.1 located review component |
| expression `ref` | MI.4.1 awaited readiness binding |
| expression `get` | MI.14.1 live member reads |
| expression `data` | MI.11.2 nested clearance reply |
| expression `as` | MI.10.3 numeric input; MI.16.1 JSON conversion |
| expression `call` | MI.14.3 methods and list operations |
| expression `fn` | MI.14.2 list predicates and selectors |
| expression `answer` | MI.14.3 callbacks and SortItems |
| expression `record` | MI.11.1 Object and JSON body |
| expression `list` | MI.18.1 Concat parts; MI.11.1 structured request writing |
| expression `op` | MI.2.2 And/Or/Not |
| expression `signal` | MI.19.1–3 NativeConfirm cancellation |
| shape `string` | MI.3.1 identity and address |
| shape `number` | MI.10.3 dose; MI.25.1 amount |
| shape `boolean` | MI.9.2 consent |
| shape `date` | MI.3.1 birth and move-in dates |
| shape `instant` | MI.18.1 hold expiry; MI.3.1 arrival |
| shape `file` | MI.22.3; MI.23.2 |
| shape `any` | MI.26.2 enum-source retention inside JSON member conversion before enum translation; it is not an untyped application door and is never `as(any)` |
| shape `array` | MI.10.3 round doses; MI.26.3 lists |
| shape `nullable` | MI.6.2 authority end; MI.26.3 nullable labels |
| shape `object` | MI.11.1 copied review record with date conversion |

The `any` use follows the current [JSON body writer](Alis.Reactive/RequestsAndResponses/JsonBodyValues.cs:64), which retains an enum source with `AnyShape` before applying its endpoint rows.

The historical `try` row is removed from the current grammar; MI.31.3 exercises the request layers that replaced it.

The other schema cases also have a place:

| Case | Use |
|---|---|
| locator `element` / `plugin` | MI.28.7 / MI.24.4 |
| listening `method` / `assignment` / `returned` | MI.28.1 / MI.29.6 / MI.27.5 |
| release `living` | MI.27.5 app evented-object listener |
| release `held` | MI.28.1 handed peer and its declared element |
| release `located` | MI.28.3 listener reached through a component ID |
| absence `start` / `nothing` / `unbound` | MI.3.1 omitted member starts / MI.4.3 nullable holder / MI.4.3 malformed unconstructible reply holder |
| path `each` | MI.10.3 per-item rules |
| plugin data `none` / `model` / `form` | MI.24.4 / MI.16.1 / MI.17.1 |
| plugin data `stream` / `hub` | MI.20.1 / MI.27.1 |
| time zone `zone` / `clock` | Main community rendering / reception browser-local display |
| trace `off` | All specified application runs |
| trace `on` | Excluded: needs the tracing host |

**Every request-contract field and case**

Source: [schema contracts](spec/Core/schema.md:377).

| Field or case | Use |
|---|---|
| `HttpSend.method` GET | MI.1.1 |
| HEAD | MI.23.1 |
| OPTIONS | MI.22.2 |
| POST | MI.17.1 |
| PUT | MI.16.1 |
| DELETE | MI.19.1 |
| `url.parts.text.text` | All endpoint literal segments |
| `url.parts.member.name` | MI.16.1 DraftId |
| `url.parts.member.value` | MI.16.1 page URL DraftId |
| `url.parts.member.form` | MI.16.1 text; MI.25.3 Guid operation lookup |
| `url.query` | MI.16.1 referral/culture address data |
| `query.fields` | MI.1.1; MI.26.3 |
| `query.inputs` | MI.1.1 individually sent parent choice |
| `body.none` | MI.23.1; MI.19.1 |
| `body.json.value` | MI.16.1 |
| `body.json.guarded` | MI.11.1 review metadata |
| `body.json.enums` | MI.26.2 |
| `body.json.inputs` | MI.16.1 main captured inputs |
| `body.urlEncoded.fields` | MI.9.2 |
| `body.urlEncoded.inputs` | MI.9.2; MI.30.1 |
| `body.multipart.parts` | MI.22.3 |
| multipart `field.SentField` | MI.22.3 caption/kind |
| multipart `file.name` | MI.22.3 Files and AuthorityFile |
| multipart `file.value` | MI.22.3 chosen files |
| multipart `file.when` | MI.22.3 IncludeAuthority |
| `body.multipart.inputs` | MI.10.1; MI.30.2 |
| `body.text.value` | MI.24.2 packet note |
| `body.text.inputs` | MI.24.2 no captured container, explicit source |
| `body.file.value` | MI.23.2 signature |
| `body.file.inputs` | MI.23.2 no captured container |
| `headers[].name` | MI.31.1 Care-Site; MI.16.1 If-Match |
| `headers[].value` | MI.16.1 version; MI.24.2 page URL |
| `headers[].form` | MI.16.1 text; MI.25.1 written Guid |
| `headers[].when` | MI.11.1 guarded X-Review |
| antiforgery `none` | MI.31.2 public profile; safe methods |
| antiforgery `token.value` | MI.17.1 host-issued token |
| antiforgery `place.header.name` | MI.31.1 |
| antiforgery `place.field.name` | MI.31.2; MI.30.1 |
| retry `none` | MI.16.1 |
| retry `standard.attempts` | MI.31.1, literal3 |
| retry `standard.firstWaitMilliseconds` | MI.31.1, literal2000 |
| busy `none` | MI.31.4 |
| busy `page` | MI.31.6 request with no inferred target |
| busy `targets.values` | MI.13.1 shared quote panel |
| busy target `element.id` | MI.13.1 |
| busy target `component.id` | MI.7.1 action Element |
| busy target `missing` | MI.31.6 removed target before Guard |
| `respondedJson=false` | All net10 requests |
| `respondedJson=true` | Excluded: exists on net48 alone |
| `contracts` busy | MI.31.1; MI.13.1 |
| `contracts` notice | MI.31.5 |
| `SentField.name` | MI.26.3 DietaryNeeds |
| `SentField.path` | MI.9.2 Policy.PolicyNumber |
| `SentField.value` | MI.9.2 container read |
| `SentField.form` | MI.26.3 flags and lists |
| `SentField.whenNothing=leave` | MI.23.4 absent non-nullable metadata |
| `SentField.whenNothing=empty` | MI.26.3 nullable label |
| `SentField.when` | MI.11.1 guarded reference |
| `ValueRead.held.value`, including null | MI.26.3 |
| `ValueRead.nothing` | MI.23.4 |
| `SentInputs.container.none` | MI.19.1; MI.23.2 |
| container `missing.id` | MI.28.6 |
| container `captured.id` | MI.16.1 move-in |
| container `captured.body` | MI.16.1 |
| container `captured.inputs` | MI.12.1 |
| `SentInputs.individual` | MI.1.1; MI.28.6 |
| individual `missing.id/name` | MI.28.6 |
| individual `captured.input` | MI.1.1 |
| individual `leftOut` | MI.28.6 guarded absent authority input |
| `SentInput.name` | MI.12.1 medication binding name |
| `SentInput.entry.id` | MI.12.1 sent input identity |
| `SentInput.entry.binding` | MI.12.2 exact failure destination |
| binding `name/path/value` | MI.10.3 indexed medication |
| binding `committed` | MI.10.3 Native Change |
| binding `invalidClass/message` | MI.3.1 field error rendering |
| `GuardedMember.path` | MI.11.1 Review.Reference |
| `GuardedMember.when` | MI.11.1 condition and combined holder condition |
| wire `text` | MI.9.2 policy |
| wire `number` | MI.10.1 dose |
| wire `boolean` | MI.9.2 consent |
| wire `date` | MI.9.2 coverage dates |
| wire `instant` | MI.3.2 arrival |
| wire `written` | MI.25.1 operation Guid |
| wire `enum.values` | MI.22.3 DocumentKind |
| `EnumValue.held/name` | MI.22.3 |
| wire `flags.values` | MI.26.3 |
| `FlagValue.held/name/number` | MI.26.3 flags3 |
| wire `list.item` | MI.26.3 Kinds and OptionalLabels |
| wire `list.itemWhenNothing=empty` | MI.26.3 null label |
| wire `list.itemWhenNothing=stop` | MI.13.1 missing non-nullable service code refusal |
| `SentEnum.name/path/form` | MI.26.2 Diet.Needs |
| JSON enum `values` | MI.26.2 Kinds |
| `JsonEnumValue.held/sent` | MI.26.2 endpoint enum conversion |
| JSON flags `values` | MI.26.2 |
| `JsonFlagValue.held/sent/number` | MI.26.2 |
| JSON list `item/itemWhenNothing` | MI.26.2 Kinds/OptionalLabels |
| `HttpResponse.status` | MI.23.1; MI.31.5 |
| `HttpResponse.outcome` | MI.31.5 |
| `HttpResponse.headers` | MI.16.2; MI.23.1 |
| `HttpResponse.request` | MI.16.2 sent Referral query |
| `HttpResponse.inputs` | MI.12.2 |
| response body `none` | MI.19.2; every HEAD route |
| response body JSON `value` | MI.11.2 |
| response body text `value` | MI.24.2 |
| response body HTML `value` | MI.1.1 |
| response body file `ResponseFile` | MI.24.1 |
| `ResponseFile.Name/Type/Size` | MI.24.1 saved packet metadata |
| `FormCheck.registration` | MI.17.1 Submit rules |
| `FormCheck.body.format/value` | MI.9.2 URL-encoded body; MI.17.1 JSON |
| `FormCheck.inputs` | MI.12.1 |
| `FormCheck.place` | MI.3.1; MI.7.1 |
| `FormPlace.destination.summary.containerId` | MI.12.2 surviving main summary |
| `FormPlace.destination.notice.separator` | MI.7.1 row request without a sent container |
| request `argument/reply/send/check` | MI.16.1–2 |
| request `layers[].whileLoading/finally` | MI.31.3 |
| request success `kind/do` | MI.17.2 |
| request statuses `status/kind/do` | MI.7.2; MI.11.3 |
| request otherwise `kind/do` | MI.31.5 |
| request `chained` | MI.16.3 |

**TLA+ actions**

At this HEAD, [Gates.g.cs](tests/Alis.Reactive.Conformance.World/Gates.g.cs:3) contains **74 actions**: **69 application actions and five tracing actions**.

An action below names the ordering exercised. A generated enum is not by itself an implemented pause. The application’s endpoint hold is placed at the stated request boundary; synchronous actions are reached through the named gesture or host lifecycle.

| Spec/action | Feature.interaction and boundary |
|---|---|
| Request.Check | MI.3.1; MI.31.3, captured body validation |
| Request.DefaultLoading | MI.31.1–3, session renewal |
| Request.Loading | MI.31.3, request loading state |
| Request.Guard | MI.31.6, contracts and target checked before send |
| Request.Attempt | MI.31.3–4, actual HTTP attempt |
| Request.Route | MI.16.2, one selected route |
| Request.Finally | MI.31.3, request cleanup |
| Request.DefaultFinally | MI.31.3, session flush |
| Request.Chained | MI.16.3, receipt after cleanup |
| Request.After | MI.31.3, final Ready state |
| Composition.Begin | MI.13.1, acquire request holds |
| Composition.Stop | MI.13.3, quote validation stops only its branch |
| Composition.Answer | MI.13.2, each response separately |
| Composition.Route | MI.13.2, catalogue and quote outcomes |
| Composition.DefaultFinally | MI.7.3; MI.13.3, release each hold |
| Composition.Unload | MI.28.3, card leaves during request |
| Lanes.FireFirst | MI.1.1, first selection |
| Lanes.FireSecond | MI.1.1; MI.17.3, replace or skip |
| Lanes.Respond | MI.1.1, release old/new lookup answers |
| Lanes.Route | MI.1.1, only current lookup routes |
| Lanes.SettleTheTail | MI.11.3; MI.13.2, nested follow-up settles |
| Lanes.Throws | MI.14.4, awaited component failure |
| Lanes.Finally | MI.14.4; MI.28.3, owed cleanup |
| Lanes.After | MI.13.3, continuation after complete branches |
| Lanes.Dispose | MI.28.3, owner closes |
| Lanes.Release | MI.28.3, after cleanup |
| Lanes.Settle | MI.13.3, each parallel branch |
| Lanes.AllSettled | MI.13.3, complete join |
| Run.Fire | MI.2.1, care confirmation |
| Run.FirstReturns | MI.9.4, first close handler yields or ends |
| Run.SecondReturns | MI.9.4, second handler starts in the same event turn |
| Run.VendorReads | MI.9.4, drawer reads Cancel |
| Run.Step | MI.2.1, ordered synchronous step |
| Run.Settle | MI.2.1; MI.14.3, awaited answer |
| Run.Abort | MI.19.3, owner ends during confirmation |
| Run.CutInItsStep | MI.28.3, a card’s own close unloads its owner before its next step |
| Ownership.LoadA | MI.28.3, assessment owner |
| Ownership.LoadB | MI.28.3, nested payer owner |
| Ownership.UnloadA | MI.28.3, parent release |
| Ownership.UnloadB | MI.8.3; MI.28.3, nested release |
| Ownership.HostDestroys | MI.28.4, component restart |
| Ownership.ServerHangsUp | MI.21.3, failed held connection |
| Ownership.Retry | MI.21.3, reconnect offer retry |
| Ownership.SendsSlotInput | MI.12.1, original medication identity captured |
| Ownership.ShowsFailures | MI.12.2, late400 placement |
| Ownership.Raise | MI.28.2, live owners hear once |
| Ownership.UnsubscribeThrows | MI.28.5, release continues past vendor exception |
| Ownership.ListenA | MI.28.1–2, same object listened once |
| Ownership.Collect | MI.28.4, dropped peer collected |
| Ownership.HostDestroysListened | MI.28.4, peer’s element no longer holds it |
| Realtime.BeginPage | MI.20.3, page snapshot begins |
| Realtime.Message | MI.20.1–3, pushed update begins work |
| Realtime.Answer | MI.20.3, refresh reply |
| Realtime.DefaultFinally | MI.20.3, request hold released |
| Realtime.Connect | MI.21.2, first start/retry result |
| Realtime.Drop | MI.21.2, connection becomes reconnecting |
| Realtime.Reconnected | MI.21.1–2, refresh after recovery |
| Realtime.GiveUp | MI.21.2, exhausted connection |
| Realtime.Retry | MI.21.2, all failed held connections |
| Realtime.Unload | MI.21.3, card connection released |
| CallbackProperty.Step | MI.29.2, install/release one listener |
| CallbackProperty.Script | MI.29.4, vendor replaces callback |
| CallbackProperty.Freeze | MI.29.4, assignment refused by locked object |
| CallbackProperty.Restart | MI.29.4, new component generation |
| CallbackProperty.LoadA | MI.29.2, admission owner joins |
| CallbackProperty.LoadB | MI.29.2, payer owner joins |
| CallbackProperty.UnloadA | MI.29.3, parent owner leaves |
| CallbackProperty.UnloadB | MI.29.3, nested owner leaves |
| CallbackProperty.Raise | MI.29.1–2, prior callback then current listeners |
| Tracing.Trigger | Excluded: needs the tracing host |
| Tracing.Attempt | Excluded: needs the tracing host |
| Tracing.Respond | Excluded: needs the tracing host |
| Tracing.Route | Excluded: needs the tracing host |
| Tracing.Abort | Excluded: needs the tracing host |

The remaining named specifications govern these application steps even though they have no gate enum:

| Spec/action or invariant | Feature.interaction |
|---|---|
| Partials.Render | MI.5.3; MI.10.3, attached rows and nested prefixes |
| Partials.LoadSlot | MI.6.1; MI.8.2 |
| Partials.AttachedInputBindsItsMember | MI.5.3 |
| Partials.PageReadFindsEachAttachedInput | MI.16.1 |
| SlotHolds.Load | MI.2.3 |
| SlotHolds.Unload | MI.2.3; MI.28.3 |
| SlotHolds.Write | MI.4.2 status elements outside held slots |
| SlotHolds.Add | MI.24.2 receipt/history append |
| ContainerRead.Read | MI.1.1; MI.16.1 |
| ContainerRead.Type | MI.3.1; MI.10.3 |
| ContainerRead.LoadSlot | MI.5.3 |
| ContainerRead.UnloadSlot | MI.6.3 |
| Validation.Init / stuttering Next | Each browser/server check starts from its sent body; MI.3.1, MI.9.2, MI.17.1 |
| Validation.Agreement | MI.17.1 |
| Validation.ConditionAgrees | MI.8.3 |
| Validation.ObjectStartAgrees / NewHolderAgrees | MI.3.1; MI.8.3 |
| Validation.ChildSkipAgrees | MI.6.3 |
| Validation.ListStartAgrees | MI.10.3 |
| Validation.EmptyTextAgrees | MI.30.1 |
| Validation.UnsentInputsUnread | MI.9.2; MI.30.1 |
| Validation.OnlyTheNamedValidator | MI.9.2 |
| Validation.NoTypedValueLost | MI.10.3 |
| Validation.ItemPeerAgrees | MI.10.3 |

These assignments follow the action bodies in [Request.tla](spec/RequestsAndResponses/Request.tla:178), [Composition.tla](spec/RequestsAndResponses/Composition.tla:43), [Lanes.tla](spec/Execution/Lanes.tla:59), [Run.tla](spec/Execution/Run.tla:381), [Ownership.tla](spec/OwnershipAndState/Ownership.tla:206), [Realtime.tla](spec/Realtime/Realtime.tla:46), and [CallbackProperty.tla](spec/Vendors/CallbackProperty.tla:127).

**Native component surface**

| Surface | Use or permitted exclusion |
|---|---|
| NativeTextBox, NativeTextArea, NativeDropDown, NativeRadioGroup | MI.1–3, MI.6, MI.8 |
| NativeCheckBox, NativeCheckList, NativeHiddenField | MI.8, MI.10, MI.13, MI.16 |
| NativeButton and Click | MI.3.1; MI.16.1 |
| NativeDrawer Open, Close, Size, BeforeClose, Cancel, ClosedBy, Closed | MI.9.1–4 |
| NativeConfirm Ask | MI.2.1; MI.19.1 |
| NativeLoader, NativeToast, NativeReconnectOffer | MI.31.1; MI.19.2; MI.21.2 |
| NativeActionButton, NativeActionLink, Row, Element, Render | MI.7.1–3 |
| Remaining Native component-specific pointer coordinates, pressure, tilt and implementation members | Excluded: a Native component’s own member that no screen author calls |

**8. GAPS**

1. **Building one list from separate sources.** MI.24 needs a packet containing saved admission documents and generated arrival pages; there is no public list-construction door for an arbitrary collection of sources, as the supplied skill states at line122; the nearest doors are `Object`, `Select` and `Set`, so E36 assembles the typed packet list on the server.

2. **Binding one input to a whole domain object or dictionary.** MI.11 and MI.14 need complex review values; whole-object input binding is expressly refused at this HEAD, with `PartialFor`, one input per member, or JSON `Set` as the stated alternatives; these features use the alternatives rather than inventing a compound input binding. The restriction is recorded in [the owner’s decision](spec/CLAUDE.md:1905).

3. **Reading a sent JSON member through `Response.Request`.** MI.16 needs to identify the saved submission; that door refuses JSON-body member reads at render, so the receipt returns its authoritative version and reference, while `Response.Request.Read` reads the sent Referral query only. This follows [the recorded decision](spec/CLAUDE.md:1858).

4. **A shipped Native file-picker helper.** MI.22 and MI.23 need live file metadata and explicit file sources; Native PublicAPI has no file-picker helper, while the core supplies `BrowserFile`, live-object declarations, native vendor lookup and multipart/file bodies; the app declares `AdmissionFileInput` and its iterable file list on that public surface.

5. **Application page and print objects.** MI.24 needs a readable page URL and a print operation; the core supplies `Plugin`, `ReadOnly`, `Method`, `MethodAsync` and typed arguments, while the application supplies the browser object and its declaration; no new core statement or expression is required.

6. **A browser cancellation is not a transaction rollback.** MI.17, MI.22 and MI.25 need certainty after a lost reply; no browser door reverses an already committed server mutation, and `Replace`, `Skip` and `Unload` do not express that capability; operation IDs, atomic endpoint rules and the next normal read resolve the outcome.

7. **The new application’s hold placements and domain fixtures do not exist merely because the gate enums exist.** MI.1–31 specify their endpoint and lifecycle boundaries here; implementation must add those application placements through the existing harness contract before any claim of behavioral proof.

**9. WHERE THE BUILD CORRECTED THIS SPEC**

Parts 1 to 8 are the spec as it was derived, before any of it was built. Each unit that builds a feature adds here, in its own words, each place the source won over those parts and each gap it met, so this part is what a reader holds beside them.

**The foundation and MI.1 (M9.X10 to M9.X14)**

Decided, and where the source won:

1. One handler looks the buildings up, under replace, on an app event (`MoveInLocationEvents.CommunityChosen`) that the page's start and the Community drop-down's change both dispatch. MI.1.1 says DomReady and Change "start a Replace handler"; in the source a lane is one handler's (`Alis.Reactive.Assets/runtime/Execution/start.ts:58`, `lane.ts:16`), and `Trigger.DomReady` takes no policy, so two triggers with a handler each would let the start's late answer land over a newer choice. M9.X14 proves the one lane.
2. That handler unloads `buildings`, `apartments` and `beds` before it asks: unloading a level's plan cuts the lookups its own handlers hold, so no answer of an older building lands under a newer community.
3. "Choose a community." is the container's live check: `MoveInRules` is registered for `MoveInPage`, so the drop-down's commit checks its own rule and shows the message beside it. The page calls no `Validate` for it, which would show every section's failures at once.
4. Addresses. The spec gives E1 to E4 as `/move-ins/{DraftId}...`. Landed: `/care/move-ins/D100` and `/care/move-ins/D200` as the page's two routes, and `/care/move-ins/buildings`, `/apartments`, `/beds` with `DraftId` in the query, read from the page's address by `PageUrl<MoveInPageAddress>(MoveInEndpoints.Page)`. Why: `WhenAnAppRendersItsPages` opens every GET route as a document and refuses one with a parameter no page links to (`tests/Alis.Reactive.Conformance.App/CLAUDE.md:188`). A PUT keeps `{DraftId}` in its path (E22).
5. A lookup's answer is rendered at its envelope, `MoveInPage`, with the input at `m => m.Location.BuildingId`, not by `PartialFor` over `LocationInput`: the apartments' list needs `Care.Level`, which `LocationInput` does not hold. The names are the same (`Location.BuildingId`) and no prefix is written in the browser.
6. `X-Community` on E2 is read when sent: 403 only where it names a community CO1 does not serve, so the bare GET the sweep sends answers 200. The 403 is not proven by a test.
7. `MoveInAddress` is `MoveInPageAddress`: `Care/Admission/AddressedMoveInController.cs` holds a `MoveInAddress` in the same namespace.
8. The save. E22 as landed: `[ValidateAntiForgeryToken]`, the body, `If-Match`, `X-Coordinator` and `X-Operation` (part 2's common facts), 400 for a body that does not bind, 403 for a coordinator the world does not know, 410 closed, 409 `VersionConflict`, 404, then `ETag` and `SaveReply`. It reads no Referral and no Rehearsal query: the spec gives them no server meaning. The page's Save is the least MI.1's situation 5 needs: the container's inputs as JSON with the three headers, `NoValidation()`, and MI.16.2's own literal, "Draft saved: version 8", so MI.16 changes no landed promise. MI.16's interactions (the Skip command, `NoRetry`, the "Draft" rule set, Referral, the ETag read, the chained receipt, the 409's comparison) are its unit's, in the Draft section's lines and the Draft files. Proven here: the 200 path alone.
9. Enums. Every enum of the model but the flags one carries `[JsonConverter(typeof(JsonStringEnumConverter<T>))]`, and `CareInput.Level` starts at `CareLevel.Independent`. Why: MVC's radio helper checks an option by the enum's name, and the framework shapes an input from how the app's JSON writes the member's start (`Alis.Reactive/InputsAndIds/InputShapes.cs:69`). With numbers in JSON the page's console, read by hand, said "The request's member 'Level' holds 0; its enum forms are 1, 2, 3, 4."; with names and a start of 0, which no name holds, "The request's member 'Level' holds 0; its enum forms are "Independent", "Assisted", "Memory", "Skilled"." See gap 2. D200 so opens at Independent; the spec states no level for it.
10. A save keeps the sections whose area implemented `KeepThe<Section>`, Location alone today, so a page that does not yet send a section does not blank it in the saved draft.
11. M9.X14 holds one answer, the page's own at its start, as the landed `WhenACascadesFirstAnswerArrivesAfterItsSecond` does; Maple Court's answers unheld. The spec's order, C2's answer then C1's, is that order.
12. The dropped GET leaves four times: its first attempt and the three the host's `Retry(3, ...)` sends again (`Alis.Reactive/TriggersAndPipelines/RequestDefaults.cs`, "How many times a request is sent again after its first attempt").
13. The allocation panel starts from part 1's row (Text "Service review", Value "DIN", Count 2, Amount 240.00, the three items and records, the record Q3) with `ReadOnlyText` the draft's community. Not started: `Outer.Middle.Inner.Text` "North allocation desk" and the records' multiword values and ratios, which the component's start class holds no member for (`Components/ConformanceComponent/live-objects.ts:81` starts the middle from a fixed text; `RecordStart.cs` holds Name, Code, Count, Day, Detail). MI.4.1 posts that text: its unit meets this first.
14. The saved line reads ids, "C1 / B1 / A7 / BED4 revision 12", the form part 6 reads ("lists BED4 revision12").

Gaps of the framework the unit met, with no framework code written for either:

1. A date in a Native input. A Native text box over a `DateTime` the server renders writes it as MVC's `TextBoxFor` does, in the server's culture ("11/18/2026 12:00:00 AM", read on the page; the hidden field, which renders through MVC's own helper too, I did not look at), and the runtime reads a date from ISO text alone (`Alis.Reactive.Assets/runtime/ValuesAndSources/conversion.ts:428`), so the input reads as nothing: the save's body held `"today":"0001-01-01T00:00:00"`. `[DisplayFormat]` changes nothing, since `TextBoxFor` does not read it; I had assumed it did, added it to fifteen members and removed it. No door gives a Native input its date as ISO text. Use: the main container's `Today`, which MI.3's rules compare with (part 5: `Identity.BirthDate` `LessThan(Today)`, `Resident.MoveInDate` `GreaterThanOrEqualTo(Today)`), and any date a saved draft shows again in an input. The page is written without `Today`.
2. An input over an enum whose zero no member names. The input's shape is how JSON writes the member's start, and a start of 0 that no name holds is written as a number even where the app's JSON names the enum, so the names the input holds read as nothing. Decision 9 gives the model a named start, a choice of the app's; the framework's line is yours to judge. Use: MI.1.1's `Care.Level`; any input a later unit binds to a `CareLevel` member meets it.
3. Not a gap for MI.1, a fact for MI.2 on: Native's change carries no value, as MI.1.1 itself says ("read no Native payload member").

**MI.2, MI.3 and MI.4 (M9.X15 to M9.X23, M9.X25 to M9.X29)**

Where the source won:

1. MI.4.1's one condition. `Location.BedId.NotEmpty().And(component.Check()).And(component.SettleCheck())` is refused where the view renders: "An async method is called in a term that runs only when it is reached (the right term of And, Or or IfNull, or a branch of Choose), and the pipeline waits for an async method at its line: call it in a When of its own, in the branch where its term is reached." So `SettleCheck()` has a `When` of its own under the first, and a false earlier term still prevents the awaited call (M9.X26).
2. Addresses, by the foundation's decision 4 and the page sweep: E5 for care is GET `/care/move-ins/sections/care` with `SectionQuery` in its query; E23 is GET `/care/move-ins/receipts` with the draft and the reference in its query; the two desks are GET `/care/move-ins/desks/resident` and `/desks/allocation`. Each answers 200 with no query. E6 and E7 keep `{DraftId}` in their paths.
3. E48 is a POST, `/care/move-ins/{DraftId}/review/check`: the conformance component's code awaits its endpoints by POST (`Components/ConformanceComponent/conformance-component.ts`, `fetch(endpoint, { method: "POST" })`). It reaches `Run.tla`'s Settle gate with its own address, answers true while the draft is open and its coordinator admits, 409 false otherwise.
4. E6's body is the main container's inputs, a `MoveInPage`, of which the endpoint reads `Resident`: a container's JSON body is typed by the container's model (`BodyParts<TRequest>.Json(RequestContainer<TRequest>, ...)`), and only a sent container places a failure beside its input.
5. `DependentRules` is refused in a rule the browser reads (`Alis.Reactive/Validation/BrowserRules.cs`: "declares DependentRules, which the endpoint checks only while the rule passes, a condition no browser rule reads"), so the assessment date's rule stands alone, server-only.
6. A nested validator's rules run under the rule set the check selects (`Alis.Reactive/Validation/Rules.cs`, `Selects`), so `ResidentRules`, its identity's child rules and `AddressRules` are declared in "default, Identity". My first version missed it and M9.X21 caught it in the full run.

Decided:

1. The buttons and handlers of MI.3 and MI.4 are desks, `ResidentDesk.cshtml` and `AllocationDesk.cshtml`, each a partial of its own model (`DeskPage.cs`) its view GETs into a slot at its start; the fields stay rendered with the page, so a save never meets a section that has not loaded. Two reasons: a desk's plan is then no part of the page's first render (fetched after the page, each desk took 0.026 s and 0.028 s on its first request; I did not time the page with and without the desks like for like), and one observation, not followed up: with Review resident details in the attached partial's own plan, one run showed the failure beside its input and stopped, with the container's summary left empty; from the desk's plan the summary fills (M9.X21). I did not isolate that.
2. `ReviewKind` gains `Identity`, at its end (`Model/MoveInEnums.cs`): part 2's E6 "adds passed review" and part 1's enum names no kind for it. This is the one model edit; say if it is past the brief's exception.
3. The component's start gains one member: `OuterStart.InnerText` ("innerText"), which `live-objects.ts` writes to `outer.middle.inner.text` and `start-options.ts` types. The allocation panel starts it from the community's site, "North allocation desk"; its `Enabled` is whether the draft's coordinator admits, and its check endpoint is the draft's.
4. Memory care is closed where no open building of the draft's community holds a Memory apartment: the model holds no member for a secure unit, and the foundation's seed closes B1.
5. An identity review's receipt is required where the details sent differ from the saved draft's (name, birth date, move-in date, street). An allocation's instructions are required where the apartment holds another bed that is not free: apartment 7 yes, apartment 21 no.
6. Situation 5 of MI.4 is read as: a review asked for a newer selection replaces the one still awaiting. The bed's drop-down is the foundation's view, so its change raises nothing of mine.
7. A save keeps the resident's texts and two dates as sent and keeps `ArrivalAt` and `Allergies` as saved, since the page sends neither; it keeps the chosen level's answers alone.
8. Literals the spec leaves open: "Helen Ward already holds admission ADM101.", "Bed held by James Reed; review reference Unassigned", "A waitlist place can be offered", "North allocation desk: prepare BED4 revision 12 for Helen Ward.", "Identity review ID100: ...", the saved care lines, and each rule's message but the four part 5 gives.

Gaps of the framework the unit met, with no framework code written for any:

1. A date a person enters (the foundation's gap 1). MI.3.1 and the care questions: `Identity.BirthDate`, `Resident.MoveInDate`, `Assisted.AssessmentDate` and `Skilled.OrderDate` are each the conformance component bound to the date member, its options ISO days; a pick reaches the server as a date (M9.X20's "moving in 2026-11-25", M9.X15's and M9.X19's saved care). The page still holds no `Today`, so `BirthDate.LessThan(Today)` and `MoveInDate.GreaterThanOrEqualTo(Today)` ("Move-in cannot precede today.") are `ServerOnly()`, the endpoint setting `Today` from the world. `Resident.ArrivalAt`, an instant, has no input: I did not try the component over a `DateTimeOffset`, and its rule is not written.
2. A rule set for a pipeline's check: `pipeline.Validate(container)` takes none, so MI.3.1 checks the whole container under the default set, the location's rules with it; M9.X21 chooses a bed first.
3. An enum's named start (the foundation's gap 2): met nowhere new.

Waiting on a unit that is not built, with nothing standing in:

- MI.2.2, on the Contacts unit: its condition reads the contact rows' roles, and the page holds no contacts at this base. Nothing stands in: Memory's questions load on the confirm alone. Its age term reads the birth date the day picker holds and is not written either, since the condition is one sentence.
- MI.3, situation 5, on MI.9: closing the payer verification drawer with failures of its own.
- MI.4.3, "then enters MI.18", on MI.18; MI.4, situation 4, "quote remains unaccepted", on MI.13 to MI.15; "availability read" (E30) on MI.20, read here as Bed on file.
- `Care.Assisted`'s "Otherwise rules for inactive branches" are not written: the page cannot send an inactive branch.

A fact of the harness the unit met: the navigation wait, measured before the move-in page existed, was 204 ms and the page's first render passed it, so no move-in test ran alone; the waits were measured again over the core pool and the navigation wait is 704 ms.

**MI.5, MI.6 and MI.7 (M9.X40 to M9.X54)**

Where the source won, and what was decided where the spec leaves a form open:

1. Addresses (the foundation's decisions 4 and 5). The spec gives E5 as GET `/move-ins/{DraftId}/sections/{Section}` and E10 as GET `/move-ins/{DraftId}/payer-explanation`. A GET the page sends has no placeholder in its route and answers 200 to a request with no query, since `WhenAnAppRendersItsPages` opens every GET address as a document. So: E5 Contacts is `/care/move-ins/contact-rows`, E5 Authority `/care/move-ins/contact-authority`, the authority on file `/care/move-ins/authority-on-file` and E10 `/care/move-ins/payer-explanation`, each with the draft's id in its query. E8 is POST `/care/move-ins/{DraftId}/contacts`. E9 is DELETE `/care/move-ins/{DraftId}/contacts/{RemoveId}`: the spec names the placeholder `{ContactId}`, and a request's member named like a placeholder fills the path, so it is named for `ContactEdit.RemoveId`, the member part 1 gives the request.
2. The shared command. The spec has Add and Remove dispatch "the typed shared draft command", one skip lane with the Draft's save. At this base the Draft's save is `MoveInDraftEvents.Save`, an event with no payload whose handler stands in the landed page's Draft section, which is MI.16's and no line of mine. So the command is the contacts' own, `MoveInContactEvents.Command`, an `AppEvent<ContactCommand>` of `Kind`, `NewContact`, `RemoveId` and `Version`; Save contacts, Add contact and Remove share its one handler under `HandlerPolicy.Skip`. MI.5 situation 5, "Add during save", is proven as Add contact during Save contacts (M9.X44). When MI.16's unit makes the Draft's command typed, these three kinds join it; nothing of this unit's tests changes with that but the handler's place.
3. E8 and E9 send no container. Their model is `ContactEdit` and the container `move-in` is rendered for `MoveInPage`; a request that reaches a container under a model it was not rendered for stops before it sends (X9.17, which my first form met: "Container 'move-in' is rendered for model ..."). So each body is written member by member: `Set(sent => sent.Contacts, moveIn.Read(m => m.Contacts))`, the whole section read from the main container. What follows from D16.13: a failed check of E8 or E9 and the server's 400 show in the layout's notice ("Notices: error: " and the message), and beside an input only by the main container's live check. Part 7's row "`FormPlace.destination.notice.separator` | MI.7.1 row request without a sent container" agrees.
4. MI.6 interaction 2. Written as its rule of part 5, "Authority consent | `Equal(true).Unless(Role==Guardian)`", inside the group `When(Role!=Ordinary)`: `AuthorityConsentRules` set on the row's authority `Unless` the role is Guardian, its message "Authority consent is required", which refuses a save before it is sent. No view condition is written for it: at a role's change the consent is an input of the section that change loads, so a branch on it before the load has nothing to read, and a power of attorney's section would never load. `And` and `Not` stand in interaction 1's condition (`Contact.cshtml`, line 31); `Or` stands in no view of this unit. Part 6 gives MI.6 no situation for consent, and no test of mine reads its message.
5. The rules a nullable holder would break. On the server a validator reads a member by the compiled expression (read at the source, not run), so `RuleFor(contact => contact.Authority!.Reference)` would throw for a row with no authority. The authority's rules are a validator of their own, `AuthorityRules`, set by `SetValidator`, which skips a null child; the revoked reference is its `Must`, the server's alone, given the world's set through `MoveInReads.HoldsRevoked`, which `ContactEditRules` takes by its constructor.
6. One validator a model. `ContactEdit` has one, `ContactEditRules`: its default rule set checks the sent rows and the new contact, and its rule set "Remove" holds "Choose a contact.". E9 names that set in the browser; its endpoint checks the set, then the default one.
7. The desk is a view of its own. A handler that reloads a slot stands outside the slot (D12.3), so the command's handler, the lines it writes and the slots `payer-explanation` and `authority-on-file` are `ContactDesk.cshtml`'s, a plan the page owns, and the rows it reloads are `ContactRows.cshtml`'s, inside the slot `contact-rows`.
8. The new contact's inputs are a request container, `new-contact`, of the model `ContactEdit`, inside the main container. An outer container sends its inner container's inputs too (X9.16), so the Draft's save carries a member `newContact` that `MoveInPage` does not hold and the endpoint's binder skips. M9.X45 saves the draft with that member in its body.
9. "Next read". The spec's "next read" is the page opened again, which renders the saved draft (the foundation's note 11). The list Contacts on file is what a person reads of it: a line a contact, with the role, the authority's reference and the payer's mark, written by `MoveInReads.ContactsOnFile` from the draft. The same list read before the page is opened again is the chained GET's read of the model.
10. A contact's days. `Authority.cshtml` offers five days of the world's clock and the day the authority holds; a seeded world's clock reads 2026-11-18, so the tests' days are literals.
11. The Draft's save keeps the contacts as sent. The landed save, the foundation's, checks no rule in the browser (`NoValidation()`) and none at its endpoint, so `KeepTheContacts` stores the rows it is sent; the contacts' rules refuse at E8 and E9 and, live, beside an input. M9.X45's planted failure shows it: with the authority's inputs left out of the body, the save stored a guardian with no authority. Whether a draft's save checks the rules is MI.16's.

A planted defect that escaped, and what it showed:

M9.X43's first planted defect was `Alis.Reactive.Assets/runtime/RequestsAndResponses/retry.ts`, `if (sent > policy.attempts || !idempotent(this.#method) || !retryable(reply)) return undefined;` to `if (sent > policy.attempts || !retryable(reply)) return undefined;`, a POST retried as a GET is. It printed "[plant] ESCAPED: M9_F2.M9_X43_0 passed with the defect in place (1 of 1)." The cause, read at the source and not run: the builders write a POST's retry as the policy none (`Alis.Reactive/TriggersAndPipelines/RequestWriter.cs`, `var retried = TakesDefaults && !_noRetry && _method != RequestMethod.Post;`), so the runtime's check of the method never decides a POST, and with the builder's term taken out alone the runtime's check stands in its place. One meaning, "a POST is never sent again", is stated in two places, each hides the other's removal, and `scripts/plant-defect.sh` plants one replacement. I changed neither place: which one holds the meaning is the lead's to say. The defect M9.X43 is proven on is the policy none sending a second time, quoted with its situation above.

Gaps of the framework the unit met, with no framework code written for any:

1. A date a person enters (the foundation's gap 1), at MI.6 interactions 1 and 3, `AuthorityInput.StartsOn` and `EndsOn`. Native holds no date input. Each day is the conformance component bound to its date member, which lets a person pick one of the days its start offers, and the date's rule runs in the browser on what it holds (M9.X46). Its use: a coordinator enters any day an authority starts or ends on; here only a day the list offers.
2. A row's own busy for a request another handler sends, at MI.7 interaction 3 ("row-local busy ownership"). `Busy(params string[] elementIds)` of a request takes ids the developer chose; a row control's id is the framework's, and the command's handler, which sends the DELETE, holds no reference to the row that raised it: `RowActivated.Element` is the row handler's alone and an app event's payload is data. Its use: the action of the row being removed shows busy until its request and what is chained after it settle. The page's Busy status covers the request; the class "removing" the action gains (interaction 1) is the row's own mark, read in the held state of M9.X54.
3. Taking that class back, at MI.7 interaction 3 (part 7: "`Remove` | MI.7.3, row action cleanup"). After a removal the server refuses or the connection drops, the row's action keeps "removing": the handler that learns the outcome is the command's, which cannot reach the row's element, for gap 2's reason. A removed row's action leaves with its row. `Classes.Remove` and `Classes.Toggle` of part 7 stand in no view of this unit. Its use: a refused removal's action reads as before the click.
4. An enum an input holds (the foundation's gap 2) did not bind here: `Relationship` and `Role` are drop-downs whose items name the enum's members, and `ContactCommandKind` carries its own `JsonStringEnumConverter`, so the command's kind is its name in the payload a condition compares.

What waits:

Nothing of MI.5, MI.6 or MI.7 is blocked by it. Four places meet it:
- `MoveIn.cshtml`: this unit changed the one line inside "move-in-contacts"; the other unit's lines are inside its own three elements.
- `spec/Reference/page-doors.json`: `spec.py` adds this unit's nine views to every entry that renders `/Care/MoveIn/MoveIn.cshtml` as it finds the file. If this unit lands first, the other unit's entries need these nine views too; `scripts/page-doors.cs` writes the table either way.
- `tests/Alis.Reactive.Conformance.PlaywrightTests/Harness/measured.json`: the new waits land with the other unit. Under the base's 204 ms navigation wait a test of this unit run alone fails at its first `GotoAsync`.
- The branch of MI.6 interaction 1 for a resident who holds an active admission reads the main container's `ResidentId`. No seeded draft's resident holds one (R203 alone does, and no draft is his), and a draft's resident changes in the Resident section, MI.3's. The branch is written (`Contact.cshtml`, line 39) and no test reaches it.

### MI.18, MI.28 and MI.29 — the waitlist, assessment owners and room handoff

1. MI.18.1. The existing E7 409 response tests `Conflict.Waitlist.Allowed` and uses the offered terms ID and the draft ID as E27 path values. E27 reads the selected bed from its community and renders the holder and the expiry as a local time. The Native and core public surface has no formatter that renders a response instant as `HH:mm` in the community's time zone. Opening the published terms without a bed shows the terms alone. E27 refuses an unknown record or a bed without a current hold with 404, and an inaccessible community with 403.
2. MI.18.2–3. E28 captures the waitlist's own reason and agreement and the resident, selected community and care from the main form. Its receipt opens the saved entry. Position counts the entries for this community and care level. The saved entry, open draft and holder are independent. MI.17 still owns the E25 conflict entry. MI.20 supplies the E30 availability read used by MI.18 situation 5, proven by M9.X69.
3. MI.28.1–4. The allocation owner listens through the existing Handed and Raised.Peer doors. The changed peer contributes its Ratio, Value, PreviousValue and IsInteracted. E44 writes the accepted review before the Respond gate. Closing the card while its response is held clears busy and releases the listener. The dismissed owner displays no receipt and starts no receipt-history read. The server keeps its accepted review. A dropped connection before the write stores no review. The restarted component's new peer has its own listener.
4. MI.28.5. A and nested B subscribe to the same room console. B is the last owner of its search callback and A the last owner of its raise callback. The vendor's Freeze control freezes its callback properties. On replacement both restorations throw at the vendor assignment. X75 reads the release diagnostic naming B's onSearch and A's onRaised, then the surviving page records exactly one change from its new assessment owner.
5. MI.28.6. The external full-assessment and ratio filing controls post real E44 reviews while the card exists. Once it leaves, the two controls encounter a missing assessment container and a missing individual Ratio input before transport. The authority-specific case in part 7 has no E44 destination. ContactInput.Authority.Reference belongs to E8. E44's MobilityRequest holds DraftId, Ratio, Description and PreviousDescription. Guarding an absent authority reference on an assessment save waits for a request that gives that reference a domain meaning.
6. MI.28.7. The registered capacity gauge starts with the available bed count in the draft's community. The page sets Threshold to two, and Alarmed reads the registered object's Reading and Threshold to choose Raise or Lower.
7. MI.29.1–6. The console starts with Desk. A appends Admission and submits E42. Nested B appends Payer and loads E10 with the saved payer contact and policy. B's search calls E43 and hands the returned name list to SearchArgs.Show. HTTP 503 and a dropped network connection each retain the last list. A fresh three-character search after recovery reads the directory again. Closing each owner, replacing the callback, restarting the console, freezing its properties, and raising with a throwing prior callback use the vendor's existing controls.
8. Owner sections and the room directory retain the concrete GET addresses of the part 9 decisions for E5/E10, with typed draft and community query members. E42 and E44 use the draft path member. For a JSON request, a typed Path already carries its member into the request body. The explicit body writes the other members. No component declaration or framework member changes.

### MI.20 and MI.21 — bed availability and its connections

1. MI.20. The selected bed opens an availability owner. Changing the community, building, apartment or bed unloads it. E31 publishes the bed summary and the current service price revision from the same world that applied the hold or release. Its page and card have separate URL-qualified connections to that community. Each rejects another community, another bed and a revision at or below its displayed revision.
2. E30 reads the selected bed under the world's lock. Its body, revision, holder and bed state come from that snapshot. The body in part 4 contains no holder or distinction between held, occupied and closed. The response carries those facts in the app's X-Bed-Holder and X-Bed-State headers. Access is checked before the captured response leaves. A later response with an older revision cannot replace either the displayed bed or its holder. The request for a revision ahead of the world's bed returns 503. E30 can return 404 for an unknown record or 403 for a coordinator outside the community.
3. The page reads the selected bed from the returned list with the typed Find predicate and projection. Reading a List index in a response member lambda is refused at render. At on a reached component's live list applies to that declaration. An accepted message reports its availability flag while the page checks it. The page starts E30 and E18 in Parallel. E18 reads the world's available ServiceDefinition records and the page shows their count beside the change's PriceRevision. MI.13 owns service selection, pricing, quote calculation and the related workflow. The service catalogue accepts the draft's saved payer kind, the current care level and the view culture. The independent card reads an opening snapshot and updates directly from BedChanged. With only its Available flag it can report that a bed is unavailable. The page obtains the holder when E30 answers.
4. E18, E30 and E31 use the established concrete GET routes under /care/move-ins with the draft ID in their query. The routes are services, availability and availability-stream. A bare route says which selection opens it. The stream address carries CommunityId and the independent card adds Card=true. The typed ServerPushStream.At door takes the complete URL of that owner. An E31 connection requires access on admission and a world change publishes only to a draft still authorised for its community.
5. MI.21. Losing a connection marks its data unconfirmed without request busy. Recovery reads E30 before confirming the page. Deliberately closing the page stream blocks its refresh. A failed page or card owns its share of the layout's retry offer until it opens or leaves. Closing the nested card unloads its owner. Closing the final failed page stream releases the offer. Reopening the selected bed reads the world's current record.
6. MI.18 situation 5 now goes through E27, E28 and E30. Releasing the recorded hold while its terms are open advances BED4 to revision 14. Joining saves W100 for Helen Ward and the next availability read still reports BED4 available at revision 14. M9.X69 holds this situation. MI.17 owns the admission submission and its E25 conflict.

### MI.8, MI.9 and MI.13 — payer coverage, verification and prices

1. M9.X115–119. The funding source belongs to the main draft. E5 renders the primary policy and its nested secondary slot. Secondary coverage requires the move-in date inside the primary dates and the live allocation panel’s `Check()`. The two policies at 80 and 20 percent save together. A contribution of 100 stops in the browser. Contributions totalling 110 produce both the server field and summary messages before any draft change. A late response belongs to its former insurance section. An E22 body from an earlier page can omit Payer. An explicit Private selection carries Payer.Kind and replaces the former policy.
2. M9.X120–124 and M9.X24. The existing NativeDrawer owns its independent URL-encoded verification form. The allocation panel supplies the query reference from Value and the community header from ReadOnlyText. Picking its “none” option omits the reference and the conditional header. The server authorizes the draft’s actual community in either case. A saved benefits payer requires an allocation reference, which the endpoint checks against the submitted query. A successful review records the submitted policy, the allocation reference and the community. E12 resolves that verification’s policy details. The URL-encoded drawer posts its opening funding source as `PayerVerificationPage.FundingSource`, alongside ContactId and Policy.PolicyNumber. E11 compares that source, policy and contact with the current saved payer under the draft lock before it records the clearance. Changing the saved funding source, contact or policy releases its clearance. The close handler reads ClosedBy and writes Cancel during a pending verification. The main draft can save while E11 waits, and closing a drawer with a consent failure leaves the resident’s own review usable.
3. The E5 payer, secondary and verification GETs, E12 details and E20 terms follow part 9’s existing concrete GET route decision. E11 and E19 retain the draft identifier in the path. E12 and E20 render MVC partials into their respective slots. The application uses date-bound inputs from the conformance component for coverage dates, following the date-input gap already recorded for MI.3. The server’s membership, active policy, hold and authorized representative facts exist. Part 5 also names document-specific payer predicates and authorized exemptions. The policy and verification models in parts 1 and 2 have neither a payer document nor an exemption. Those rules need their domain facts.
4. M9.X125–129. E18 and E19 both hold the quote panel. E18 reads the existing service catalogue. E19 reads the selected codes, current visit rows, care and funding, and the separate service component’s readiness, count and review record. The current care rate and service prices determine the monthly total. Current capacity can refuse a selection. E20 reads the persisted quote and its assistance revision. E19 carries the numeric MonthlyTotal and a display header for its two-decimal currency text. The final status waits for both branches and the E20 tail.
5. D100 already holds the reference quote in part 1. A world that tests its first calculation explicitly clears that quote before the gesture. A failed calculation leaves that world unquoted, and the successful next read shows Q100, revision 3, 3040.00 USD, assistance 2432.00 and the count of recorded calculations. MI.4 situation 4 now reads the persisted acceptance after a failed readiness check. Repricing the Ready seed retains its existing acceptance when the terms match. Removing pharmacy support changes the quote to 2980.00 USD and clears that acceptance. MI.14 owns the awaited service-review inputs. MI.15 owns price acceptance, and MI.16 owns the shared draft command and its broader rule set.

### MI.14 and MI.15 — reviewed services and exact quote acceptance

1. M9.X140–144. The service panel belongs to the current working service codes and visit rows. Editing a row or its selection loads a new MVC partial with its own component and handlers. The component’s count and record POSTs carry that panel’s MVC-bound visit codes and counts. The service desk reads the catalogue for their names and returns its numeric visit total and typed record. The current review read supplies the world timestamp for Review.ReviewedAt. E19 receives the component’s Total as SelectedServiceCount, SettleCount as VisitCount, SettleRecord as Result and Check as Ready. The record’s Code holds the ordered service codes, Name the ordered labels, Count the number of services and Day the review date. E19 checks the current selection and capacity under the draft lock before storing the review. The reopened quote reads those stored facts.
2. The component’s selection change reports its own current item and the allocation desk. Its first Ask takes the Answer for each live item. Review installs Pair for subsequent visit labels. The page reads the active items, ordered service records and visit totals. A refused record unwinds the request that holds the quote panel. Replacing the service owner stops a held continuation before it can price the old selection.
3. M9.X145–149. E21 binds QuoteId, Revision, Version and AcceptAssistance. Its locked decision compares the displayed quote with the open draft, its funding, care level, expiry and current catalogue charges. Required assistance must be agreed. A successful receipt carries ACCEPT-Q followed by the accepted revision and the assistance reference for E20. A 409 carries the current QuoteReply and the two-decimal price header. The coordinator explicitly agrees to the replacement's terms. A lost connection before execution leaves the agreement open for retry.
4. An otherwise unchanged catalogue can price a different selection. Changed priced facts advance the quote revision and clear its acceptance. An identical recalculation retains both the revision and assistance agreement. A pharmacy price change updates the catalogue, quoted lines, monthly total and assistance under the same world lock before the existing bed stream publishes PriceRevision. The availability stream forwards price changes only from the selected community, independently of the bed revision. The displayed offer ignores catalogue notifications at or below its loaded catalogue revision. On a newer price the surviving quote section releases the old offer owner immediately and requests the current offer with the previous quote revision. The replacement reads the latest saved snapshot after the hold and displays its price and acceptance. A held E21 request cannot accept a superseded offer, and a held success for the old revision cannot accept the new one. E19 and E21 return request-owned snapshots.
