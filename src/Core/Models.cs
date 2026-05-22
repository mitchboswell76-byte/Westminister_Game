using System.Text.Json;
using System.Text.Json.Serialization;

namespace Westminster.Core;

public record CharacterName(string First, string Last, string? Honorific);
public record Relationship(string TargetId, int Value, string Kind);
public record CharacterAttributes(int Charisma,int Oratory,int Negotiation,int Tactics,int Vision,int Intelligence,int Judgement,int Composure,int Discipline,int Concentration,int Empathy,int Manipulation,int Networking,int Likeability,int Authority,int Stamina,int Health,int Presence,int Resilience);
public record CharacterHidden(int Loyalty,int Greed,int Pragmatism,int Ambition,int Honour,int IdeologyDrift,int Sociopathy,int Fear,int RiskAppetite,int SexDrive,int AddictionSusceptibility,int Spirituality,int CoupPlotterQuotient,int PressCharm,int WorkingClassAuthenticity,int EstablishmentAcceptance);
public record Character(string Id, CharacterName Name, DateOnly BirthDate, DateOnly? DeathDate, string Gender, string Ethnicity, string Religion, string Sexuality, string? ConstituencyId, string? PartyId, int CareerRank, string CurrentPosition, CharacterAttributes Attributes, CharacterHidden Hidden, List<string> Traits, string IdeologyId, int IdeologyPurity, int Stress, int Energy, long MoneyPersonalGbp, List<Relationship> Relationships, List<string> HooksHeld, List<string> HooksAgainstMe, List<string> Secrets, List<string> PerksUnlocked, Dictionary<string,int> PerkXp, string LifestyleFocus, List<string> SchemesActive, int Fame, bool IsPlayer, string SpawnSource);
public record ConstituencyDemographics(int MedianAge,int MedianHouseholdIncomeGbp,double UniGradPct,double WhitePct,double NonUkBornPct,double OwnerOccupiedPct,double SocialHousingPct,double PrivateRentPct);
public record ConstituencyResult(int Lab,int Grn,int Con,int Ld,int Ref,int IndCorbyn,int Other);
public record Constituency(string Id,string Name,string Country,string Region,int Electorate,ConstituencyResult Result2024,string WinnerCharId,int Majority,string MarginalClass,ConstituencyDemographics Demographics,string TopojsonObjectId,string LadId);
public record CountryRelationship(int Value,List<string> Treaties,double TradeVolumeGbpBn);
public record Country(string Id,string IsoA2,string IsoA3,string NameEn,string NameNative,string HeadOfStateId,string HeadOfGovernmentId,string GovernmentType,string IdeologyId,List<string> RulingParties,double GdpUsdBn2025,long Population,int MilitaryScore,int SoftPowerScore,bool NuclearArmed,List<string> Memberships,CountryRelationship RelationshipWithUk,string TopojsonObjectId,int AiTier);
public record Hook(string Id,string HolderId,string TargetId,string Strength,string Source,string? SourceSecretId,DateOnly? ExpiresDate,bool Used);
public record Secret(string Id,string SubjectId,List<string> KnowerIds,string Kind,string Severity,string Description,DateOnly DiscoveredDate,bool Public);
// PRD §8.7 choice: EnumValues is included to represent enum lever labels while numeric CurrentValue remains persisted.
public record PolicyLever(string Id,string Name,string Category,string Subcategory,string Type,double Min,double Max,double Step,double Default,double CurrentValue,List<string> EnumValues,string Unit,string DisplayFormat,List<string> Dependencies,List<Westminster.Policy.PolicyEffect> Effects,Dictionary<string,double> FactionReactions,List<string> EnabledByIdeologies,List<string> DisabledByGovernmentTypes,string PhaseTag);
public record Ideology(string Id,string Name,Dictionary<string,double> AxisVector,Dictionary<string,JsonElement> Defaults,Dictionary<string,List<JsonElement>> ToleranceBands,Dictionary<string,int> FactionAffinity,string UiPaletteHex,List<string> TrueBelieverPerks,string PhaseTag);
public record Scheme(string Id,string TemplateId,string OwnerId,string TargetId,int Progress,int Secrecy,List<string> Agents,DateOnly StartedDate,DateOnly ExpectedCompletionDate,int MonthlyProgressGain,double ExposureChanceMonthly,string ExpectedOutcome,string FallbackOutcome,string State);
public record StageMilestone(int At,string Name,bool Fired);
public record Situation(string Id,string Scope,string? ScopeTarget,int Progress,int Stage,double MonthlyDrift,string Approach,List<string> ApproachesAvailable,List<StageMilestone> StageMilestones,Dictionary<string,double> ContributingCountries);
public record EventConsequence(string Target,double Delta);
public record GameEvent(string Id,DateOnly FiredDate,string Scope,string Headline,string Category,bool DecisionRequired,List<JsonElement> Options,List<EventConsequence> Consequences);
public record InitiativeReward(string Type,string? Value,string? Target,double? Delta);
public record StrategicInitiative(string Id,string Name,string Tree,List<string> Prerequisites,int DurationDays,int OrderCostPerYear,Dictionary<string,string> IdeologyRequirements,List<InitiativeReward> Rewards,Dictionary<string,int> UiPosition,string PhaseTag);
public record CoreIssue(string PolicyId,JsonElement PreferredValue,int Weight);
public record Faction(string Id,string Name,int MembersPopCount,int ApprovalOfGovernment,int PowerScore,string IdeologyLean,List<CoreIssue> CoreIssues,string LeaderId,string? PetitionActiveId,string? InGovernmentPartyId);
public record Pop(string Id,string RegionId,long Size,string Stratum,string Profession,Dictionary<string,double> IdeologyVector,string Ethnicity,string Religion,string AgeCohort,string Education,double Engagement);
public record SaveSettings(int Speed,bool AutopauseEvents,bool Ironman);
public record SaveGameStructure(int SaveVersion,string GameVersion,DateOnly GameDate,ulong RngSeed,ulong RngCallCount,string PlayerCharacterId,string WorldStateDb,List<JsonElement> CharactersDirty,SaveSettings Settings);

public static class JsonSupport
{
    public static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, WriteIndented = true, Converters = { new JsonStringEnumConverter() } };
}
