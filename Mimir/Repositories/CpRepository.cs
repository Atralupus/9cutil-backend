using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Battle;
using Nekoyume.Model.Item;
using Nekoyume.Model.Stat;
using Nekoyume.Model.State;
using Nekoyume.TableData;
using Mimir.Models.Agent;
using Mimir.Services;
using Mimir.Util;

namespace Mimir.Repositories;

public class CpRepository
{
    private StateGetter _stateGetter;

    public CpRepository(IStateService stateService)
    {
        _stateGetter = new StateGetter(stateService);
    }

    public async Task<int?> CalculateCp(
        Avatar avatar,
        int characterId,
        IEnumerable<string> equipmentIds,
        IEnumerable<string> costumeIds,
        IEnumerable<(int id, int level)> runeOption
    )
    {
        try
        {
            var avatarAddress = new Address(avatar.AvatarAddress);

            var characterRow = await GetCharacterRow(characterId);
            var costumeStatSheet = await _stateGetter.GetSheetAsync<CostumeStatSheet>();
            var collectionSheets = await _stateGetter.GetSheetAsync<CollectionSheet>();

            var inventoryState = await _stateGetter.GetInventoryStateAsync(avatarAddress);

            List<Equipment> equipments = inventoryState
                .Equipments
                .Where(x => x.Equipped).ToList();
            List<Costume> costumes = inventoryState
                .Costumes
                .Where(x => x.Equipped).ToList();
            List<RuneOptionSheet.Row.RuneOptionInfo> runes = runeOption.Select(x => GetRuneOptionInfo(x.id, x.level).Result).ToList();
            var collectionStates = await _stateGetter.GetCollectionStatesAsync([avatarAddress]);
            var modifiers = new Dictionary<Address, List<StatModifier>>
            {
                [avatarAddress] = new(),
            };
            if (collectionStates.Count > 0)
            {
                foreach (var (address, state) in collectionStates)
                {
                    var modifier = modifiers[address];
                    foreach (var collectionId in state.Ids)
                    {
                        modifier.AddRange(collectionSheets[collectionId].StatModifiers);
                    }
                }
            }

            return CPHelper.TotalCP(
                equipments,
                costumes,
                runes,
                avatar.Level,
                characterRow,
                costumeStatSheet,
                modifiers[avatarAddress]
            );
        }
        catch (NullReferenceException)
        {
            return null;
        }
    }

    private async Task<RuneOptionSheet.Row.RuneOptionInfo> GetRuneOptionInfo(
        int id,
        int level
    )
    {
        var sheets = await _stateGetter.GetSheetAsync<RuneOptionSheet>();

        if (!sheets.TryGetValue(id, out var optionRow))
        {
            throw new SheetRowNotFoundException("RuneOptionSheet", id);
        }

        if (!optionRow.LevelOptionMap.TryGetValue(level, out var option))
        {
            throw new SheetRowNotFoundException("RuneOptionSheet", level);
        }

        return option;
    }

    private async Task<CharacterSheet.Row> GetCharacterRow(int characterId)
    {
        var sheets = await _stateGetter.GetSheetAsync<CharacterSheet>();

        if (!sheets.TryGetValue(characterId, out var row))
        {
            throw new SheetRowNotFoundException("CharacterSheet", characterId);
        }

        return row;
    }
}
