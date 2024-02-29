using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Libplanet.Action.State;
using Libplanet.Types.Blocks;
using NineChroniclesUtilBackend.Store.Client;

namespace NineChroniclesUtilBackend.Store.Services;

public class EmptyChronicleStateService : IStateService
{
    private readonly EmptyChronicleClient client;
    private static readonly Codec Codec = new();

    public EmptyChronicleStateService(EmptyChronicleClient client)
    {
        this.client = client;
    }

    public async Task<IValue?> GetState(Address address)
    {
        return await GetState(address, ReservedAddresses.LegacyAccount);
    }

    public async Task<IValue?> GetState(Address address, Address accountAddress)
    {
        var result = await client.GetStateByAddressAsync(address.ToString(), accountAddress.ToString());

        if (result.Value is null)
        {
            return null;
        }

        return Codec.Decode(Convert.FromHexString(result.Value));
    }
}
