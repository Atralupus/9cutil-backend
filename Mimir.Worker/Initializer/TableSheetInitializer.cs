using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.ActionHandler;
using Mimir.Worker.Services;
using Mimir.Worker.Util;
using MongoDB.Bson;
using Serilog;

namespace Mimir.Worker.Initializer;

public class TableSheetInitializer(IStateService service, MongoDbService store)
    : BaseInitializer(service, store, Log.ForContext<TableSheetInitializer>())
{
    public override async Task RunAsync(CancellationToken stoppingToken)
    {
        var handler = new TableSheetStateHandler(_stateService, _store);
        var sheetTypes = TableSheetUtil.GetTableSheetTypes();

        foreach (var sheetType in sheetTypes)
        {
            _logger.Information("Init sheet, table: {TableName} ", sheetType.Name);

            // using (var session = await _store.GetMongoClient().StartSessionAsync())
            // {
                // session.StartTransaction();
            await handler.SyncSheetStateAsync(sheetType.Name, sheetType);
                // session.CommitTransaction();
            // }
        }
    }

    public override async Task<bool> IsInitialized()
    {
        var sheetTypes = TableSheetUtil.GetTableSheetTypes();

        var collection = _store.GetCollection(CollectionNames.GetCollectionName<SheetDocument>());
        var count = await collection.CountDocumentsAsync(new BsonDocument());
        var sheetTypesCount = sheetTypes.Count() - 4;

        return count >= sheetTypesCount;
    }
}
