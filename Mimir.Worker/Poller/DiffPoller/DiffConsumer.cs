using System.Threading.Channels;
using Bencodex;
using Libplanet.Crypto;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Client;
using Mimir.Worker.Handler;
using Mimir.Worker.Services;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.Poller;

public class DiffConsumer
{
    protected readonly MongoDbService _dbService;
    protected readonly ILogger _logger;
    private readonly Codec Codec = new();

    public DiffConsumer(MongoDbService dbService, Address accountAddress)
    {
        _dbService = dbService;
        _logger = Log.ForContext<DiffConsumer>()
            .ForContext("AccountAddress", accountAddress.ToHex());
    }

    public async Task ConsumeAsync(
        ChannelReader<DiffContext> reader,
        CancellationToken stoppingToken
    )
    {
        await foreach (var diffContext in reader.ReadAllAsync(stoppingToken))
        {
            if (diffContext.DiffResponse.AccountDiffs.Count() == 0)
            {
                _logger.Information("{CollectionName}: No diffs", diffContext.CollectionName);
                await _dbService.UpdateLatestBlockIndexAsync(
                    new MetadataDocument
                    {
                        PollerType = nameof(DiffPoller),
                        CollectionName = diffContext.CollectionName,
                        LatestBlockIndex = diffContext.TargetBlockIndex
                    }
                );
                continue;
            }

            if (
                AddressHandlerMappings.HandlerMappings.TryGetValue(
                    diffContext.AccountAddress,
                    out var handler
                )
            )
            {
                await ProcessStateDiff(
                    handler,
                    diffContext.AccountAddress,
                    diffContext.DiffResponse,
                    stoppingToken
                );

                await _dbService.UpdateLatestBlockIndexAsync(
                    new MetadataDocument
                    {
                        PollerType = nameof(DiffPoller),
                        CollectionName = diffContext.CollectionName,
                        LatestBlockIndex = diffContext.TargetBlockIndex
                    },
                    null,
                    stoppingToken
                );
            }
            else
            {
                _logger.Error(
                    "No handler for {AccountAddress} address",
                    diffContext.AccountAddress
                );
            }
        }
    }

    private async Task ProcessStateDiff(
        IStateDiffHandler handler,
        Address accountAddress,
        GetAccountDiffsResponse diffResponse,
        CancellationToken stoppingToken
    )
    {
        List<MimirBsonDocument> documents = new List<MimirBsonDocument>();
        foreach (var diff in diffResponse.AccountDiffs)
        {
            if (diff.ChangedState is not null)
            {
                var address = new Address(diff.Path);

                var document = handler.ConvertToDocument(
                    new()
                    {
                        Address = address,
                        RawState = Codec.Decode(Convert.FromHexString(diff.ChangedState))
                    }
                );

                documents.Add(document);
            }
        }

        _logger.Information(
            "{DiffCount} Handle in {Handler} Converted {Count} States",
            diffResponse.AccountDiffs.Count(),
            handler.GetType().Name,
            documents.Count
        );

        if (documents.Count > 0)
        {
            await _dbService.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName(accountAddress),
                documents,
                null,
                stoppingToken
            );
        }
    }
}
