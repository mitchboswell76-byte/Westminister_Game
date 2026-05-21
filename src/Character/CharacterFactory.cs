using Westminster.Core;

namespace Westminster.Character;

public static class CharacterFactory
{
    public record PlayerCreationRequest(
        string CharacterId,
        string FirstName,
        string LastName,
        DateOnly BirthDate,
        string ConstituencyId,
        string PartyId,
        string Gender,
        string Ethnicity,
        string Religion,
        string Sexuality,
        string IdeologyId);

    public static Character CreatePlayer(string id, string firstName, string lastName, DateOnly birthDate, string constituencyId, string partyId)
    {
        return new Character(
            id,
            new CharacterName(firstName, lastName, null),
            birthDate,
            null,
            "unspecified",
            "unknown",
            "none",
            "unspecified",
            constituencyId,
            partyId,
            0,
            "backbencher",
            new CharacterAttributes(10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10),
            new CharacterHidden(50, 50, 50, 50, 50, 50, 0, 50, 50, 50, 50, 50, 0, 50, 50, 50),
            [],
            "ideology_social_democracy",
            50,
            0,
            100,
            0,
            [],
            [],
            [],
            [],
            [],
            new Dictionary<string, int>(),
            "none",
            [],
            0,
            true,
            "player_created"
        );
    }

    public static Character CreatePlayer(PlayerCreationRequest request)
    {
        var baseCharacter = CreatePlayer(
            request.CharacterId,
            request.FirstName,
            request.LastName,
            request.BirthDate,
            request.ConstituencyId,
            request.PartyId);

        return baseCharacter with
        {
            Gender = request.Gender,
            Ethnicity = request.Ethnicity,
            Religion = request.Religion,
            Sexuality = request.Sexuality,
            IdeologyId = request.IdeologyId
        };
    }
}
