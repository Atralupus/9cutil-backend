using Lib9c.GraphQL.Enums;
using Libplanet.Crypto;
using Mimir.Models;
using Mimir.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Repositories;

public class AvatarRepository(MongoDBCollectionService mongoDbCollectionService)
    : BaseRepository<BsonDocument>(mongoDbCollectionService)
{
    public Avatar? GetAvatar(string network, Address avatarAddress) =>
        GetAvatar(GetCollection<BsonDocument>(network), avatarAddress);

    public Avatar? GetAvatar(PlanetName planetName, Address avatarAddress) =>
        GetAvatar(GetCollection<BsonDocument>(planetName), avatarAddress);

    private static Avatar? GetAvatar(
        IMongoCollection<BsonDocument> collection,
        Address avatarAddress)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("Address", avatarAddress.ToHex());
        var document = collection.Find(filter).FirstOrDefault();
        if (document is null)
        {
            return null;
        }

        try
        {
            var avatarDoc = document["State"];
            return new Avatar(
                avatarDoc["agentAddress"].AsString,
                avatarDoc["address"].AsString,
                avatarDoc["name"].AsString,
                avatarDoc["level"].AsInt32,
                avatarDoc["actionPoint"].AsInt32,
                avatarDoc["dailyRewardReceivedIndex"].ToInt64()
            );
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    protected override string GetCollectionName() => "avatar";
}
