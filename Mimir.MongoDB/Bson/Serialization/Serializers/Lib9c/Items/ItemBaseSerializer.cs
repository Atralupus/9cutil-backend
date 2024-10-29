using Lib9c.Models.Items;
using Mimir.MongoDB.Bson.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.MongoDB.Bson.Serialization.Serializers.Lib9c.Items;

public class ItemBaseSerializer : ClassSerializerBase<ItemBase>
{
    public static readonly ItemBaseSerializer Instance = new();

    public static ItemBase Deserialize(BsonDocument doc)
    {
        if (!doc.TryGetValue("ItemType", out var itemTypeBsonValue))
        {
            throw new BsonSerializationException("Missing ItemType in document.");
        }

        if (!doc.TryGetValue("ItemSubType", out var itemSubTypeBsonValue))
        {
            throw new BsonSerializationException("Missing itemSubTypeValue in document.");
        }

        var itemType = itemTypeBsonValue.ToEnum<Nekoyume.Model.Item.ItemType>();
        var itemSubType = itemSubTypeBsonValue.ToEnum<Nekoyume.Model.Item.ItemSubType>();
        return itemType switch
        {
            Nekoyume.Model.Item.ItemType.Consumable or
                Nekoyume.Model.Item.ItemType.Equipment => ItemUsableSerializer.Deserialize(doc),
            Nekoyume.Model.Item.ItemType.Costume => CostumeSerializer.Deserialize(doc),
            Nekoyume.Model.Item.ItemType.Material => MaterialSerializer.Deserialize(doc),
            _ => throw new BsonSerializationException(
                $"Unsupported ItemType: {itemType} or ItemSubType: {itemSubType}"),
        };
    }

    public override ItemBase Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var doc = BsonDocumentSerializer.Instance.Deserialize(context, args);
        return Deserialize(doc);
    }

    // DO NOT OVERRIDE Serialize METHOD: Currently objects will be serialized to Json first.
    // public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, ItemBase value)
    // {
    //     base.Serialize(context, args, value);
    // }
}
