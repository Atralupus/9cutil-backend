using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using Libplanet.Types.Assets;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Mails;

/// <summary>
/// <see cref="Nekoyume.Model.Mail.GrindingMail"/>
/// </summary>
[BsonIgnoreExtraElements]
public record GrindingMail : Mail
{
    public int ItemCount { get; init; }
    public FungibleAssetValue Asset { get; init; }

    public override IValue Bencoded => ((Dictionary)base.Bencoded)
        .Add("ic", ItemCount.Serialize())
        .Add("a", Asset.Serialize());

    public GrindingMail(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind);
        }

        ItemCount = d["ic"].ToInteger();
        Asset = d["a"].ToFungibleAssetValue();
    }
}
