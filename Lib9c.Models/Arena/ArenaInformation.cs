using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Arena;

[BsonIgnoreExtraElements]
public record ArenaInformation : IBencodable
{
    public Address Address { get; init; }
    public int Win { get; init; }
    public int Lose { get; init; }
    public int Ticket { get; init; }
    public int TicketResetCount { get; init; }
    public int PurchasedTicketCount { get; init; }

    [BsonIgnore, GraphQLIgnore]
    public IValue Bencoded => List.Empty
        .Add(Address.Serialize())
        .Add(Win)
        .Add(Lose)
        .Add(Ticket)
        .Add(TicketResetCount)
        .Add(PurchasedTicketCount);

    public ArenaInformation(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind);
        }

        Address = l[0].ToAddress();
        Win = (Integer)l[1];
        Lose = (Integer)l[2];
        Ticket = (Integer)l[3];
        TicketResetCount = (Integer)l[4];
        PurchasedTicketCount = (Integer)l[5];
    }
}
