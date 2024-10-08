using System.Text.Json.Serialization;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Libplanet.Crypto;
using Nekoyume.TableData;
using ValueKind = Bencodex.Types.ValueKind;
using static Lib9c.SerializeKeys;
using Lib9c.Models.Extensions;
using MongoDB.Bson.Serialization.Attributes;

namespace Lib9c.Models.AttachmentActionResults;

/// <summary>
/// <see cref="Nekoyume.Model.State.MonsterCollectionResult"/>
/// </summary>
[BsonIgnoreExtraElements]
public record MonsterCollectionResult : AttachmentActionResult
{
    public Guid Id { get; init; }
    public Address AvatarAddress { get; init; }
    public List<MonsterCollectionRewardSheet.RewardInfo> Rewards { get; init; }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public override IValue Bencoded => ((Dictionary)base.Bencoded)
        .Add("id", Id.Serialize())
        .Add(AvatarAddressKey, AvatarAddress.Serialize())
        .Add(MonsterCollectionResultKey, new List(Rewards.Select(r => r.Serialize())));

    public MonsterCollectionResult()
    {
    }

    public MonsterCollectionResult(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind);
        }

        Id = d["id"].ToGuid();
        AvatarAddress = d[AvatarAddressKey].ToAddress();
        Rewards = d[MonsterCollectionResultKey]
            .ToList(s => new MonsterCollectionRewardSheet.RewardInfo((Dictionary)s));
    }
}
