using Lib9c.Models.Items;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.Handler;

public class InventoryStateHandler : IStateDiffHandler
{
    public MimirBsonDocument ConvertToDocument(StateDiffContext context)
    {
        return new InventoryDocument(context.Address, new Inventory(context.RawState));
    }
}
