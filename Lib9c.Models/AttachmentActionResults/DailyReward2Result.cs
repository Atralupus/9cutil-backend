using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using Lib9c.Models.Factories;
using Lib9c.Models.Items;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.AttachmentActionResults;

/// <summary>
/// <see cref="Nekoyume.Action.DailyReward2.DailyRewardResult"/>
/// </summary>
[BsonIgnoreExtraElements]
public record DailyReward2Result : AttachmentActionResult
{
    public Dictionary<Material, int> Materials { get; init; }
    public Guid Id { get; init; }

    [BsonIgnore, GraphQLIgnore]
    public override IValue Bencoded => ((Dictionary)base.Bencoded)
        .Add("materials", new List(Materials
            .OrderBy(kv => kv.Key.Id)
            .Select(pair => (IValue)Dictionary.Empty
                .Add("material", pair.Key.Bencoded)
                .Add("count", pair.Value.Serialize()))))
        .Add("id", Id.Serialize());

    public DailyReward2Result()
    {
    }

    public DailyReward2Result(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind);
        }

        Materials = ((List)d["materials"])
            .Cast<Dictionary>()
            .ToDictionary(
                value => (Material)ItemFactory.Deserialize(value["material"]),
                value => value["count"].ToInteger());

        Id = d["id"].ToGuid();
    }
}
