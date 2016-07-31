#region using directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PokemonGo.RocketAPI.Enums;
using POGOProtos.Enums;
using POGOProtos.Networking.Responses;
using POGOProtos.Data;
using POGOProtos.Settings.Master;
using POGOProtos.Data.Player;
using POGOProtos.Inventory.Item;

#endregion

namespace PokemonGo.RocketAPI.Logic
{
    public class Inventory
    {
        private readonly Client _client;
        private GetInventoryResponse _cachedInventory;
        private DateTime _lastRefresh;

        public Inventory(Client client)
        {
            _client = client;
        }

        public async void DeletePokemonFromInvById(ulong id)
        {
            var inventory = await GetCachedInventory();
            var pokemon =
                inventory.InventoryDelta.InventoryItems.FirstOrDefault(
                    i => i.InventoryItemData.PokemonData != null && i.InventoryItemData.PokemonData.Id == id);
            if (pokemon != null)
                inventory.InventoryDelta.InventoryItems.Remove(pokemon);
        }

        private async Task<GetInventoryResponse> GetCachedInventory()
        {
            var now = DateTime.UtcNow;
            var ss = new SemaphoreSlim(10);

            if (_lastRefresh.AddSeconds(30).Ticks > now.Ticks)
            {
                return _cachedInventory;
            }
            await ss.WaitAsync();
            try
            {
                _lastRefresh = now;
                _cachedInventory = await _client.Inventory.GetInventory();
                return _cachedInventory;
            }
            finally
            {
                ss.Release();
            }
        }

        public async Task<IEnumerable<PokemonData>> GetDuplicatePokemonToTransfer(
            bool keepPokemonsThatCanEvolve = false, bool prioritizeIVoverCp = false,
            IEnumerable<PokemonId> filter = null, bool WhereInList = false)
        {
            var myPokemon = await GetPokemons();

            var pokemonList =
                myPokemon.Where(p => p.DeployedFortId == "" && p.Favorite == 0 && p.Cp < _client.Settings.KeepMinCP)
                    .ToList();
            if (filter != null && WhereInList)
            {
                pokemonList = pokemonList.Where(p => filter.Contains(p.PokemonId)).ToList();
            }
            else if(filter != null && !WhereInList)
            {
                pokemonList = pokemonList.Where(p => !filter.Contains(p.PokemonId)).ToList();
            }

            /*
            if (keepPokemonsThatCanEvolve)
            {
                var results = new List<PokemonData>();
                var pokemonsThatCanBeTransfered = pokemonList.GroupBy(p => p.PokemonId)
                    .Where(x => x.Count() > 2).ToList();

                var myPokemonSettings = await GetPokemonSettings();
                var pokemonSettings = myPokemonSettings.ToList();
                
                var myPokemonFamilies = await GetPokemonFamilies();
                var pokemonFamilies = myPokemonFamilies.ToArray();

                foreach (var pokemon in pokemonsThatCanBeTransfered)
                {
                    var settings = pokemonSettings.Single(x => x.PokemonId == pokemon.Key);
                    var familyCandy = pokemonFamilies.Single(x => settings.FamilyId == x.FamilyId);
                    if (settings.CandyToEvolve == 0)
                        continue;

                    var amountToSkip = familyCandy.Candy/settings.CandyToEvolve;
                    amountToSkip = amountToSkip > _client.Settings.KeepMinDuplicatePokemon
                        ? amountToSkip
                        : _client.Settings.KeepMinDuplicatePokemon;
                    if (prioritizeIVoverCp)
                    {
                        results.AddRange(pokemonList.Where(x => x.PokemonId == pokemon.Key)
                            .OrderByDescending(PokemonInfo.CalculatePokemonPerfection)
                            .ThenBy(n => n.StaminaMax)
                            .Skip(amountToSkip)
                            .ToList());
                    }
                    else
                    {
                        results.AddRange(pokemonList.Where(x => x.PokemonId == pokemon.Key)
                            .OrderByDescending(x => x.Cp)
                            .ThenBy(n => n.StaminaMax)
                            .Skip(amountToSkip)
                            .ToList());
                    }
                }

                return results;
            }
            */
            if (prioritizeIVoverCp)
            {
                return pokemonList.GroupBy(p => p.PokemonId).Where(x => x.Count() > 1).SelectMany(p => p.OrderByDescending(PokemonInfo.CalculatePokemonPerfection).ThenBy(n => n.StaminaMax).Skip(_client.Settings.KeepMinDuplicatePokemon).ToList());
            }
            return pokemonList
                .GroupBy(p => p.PokemonId)
                .Where(x => x.Count() > 1)
                .SelectMany(
                    p =>
                        p.OrderByDescending(x => x.Cp)
                            .ThenBy(n => n.StaminaMax)
                            .Skip(_client.Settings.KeepMinDuplicatePokemon)
                            .ToList());
        }

        public async Task<PokemonData> GetHighestPokemonOfTypeByCp(PokemonData pokemon)
        {
            var myPokemon = await GetPokemons();
            var pokemons = myPokemon.ToList();
            return pokemons.Where(x => x.PokemonId == pokemon.PokemonId)
                .OrderByDescending(x => x.Cp)
                .FirstOrDefault();
        }

        public async Task<PokemonData> GetHighestPokemonOfTypeByIv(PokemonData pokemon)
        {
            var myPokemon = await GetPokemons();
            var pokemons = myPokemon.ToList();
            return pokemons.Where(x => x.PokemonId == pokemon.PokemonId)
                .OrderByDescending(PokemonInfo.CalculatePokemonPerfection)
                .FirstOrDefault();
        }

        public async Task<IEnumerable<PokemonData>> GetHighestsCp(int limit, List<PokemonData> SessionPokemon = null)
        {
            var myPokemon = SessionPokemon ?? await GetPokemons();
            var pokemons = myPokemon.ToList();
            return pokemons.OrderByDescending(x => x.Cp).ThenBy(n => n.StaminaMax).Take(limit);
        }

        public async Task<IEnumerable<PokemonData>> GetHighestsPerfect(int limit, List<PokemonData> SessionPokemon = null)
        {
            var myPokemon = SessionPokemon ?? await GetPokemons();
            var pokemons = myPokemon.ToList();
            return pokemons.OrderByDescending(PokemonInfo.CalculatePokemonPerfection).Take(limit);
        }

        public async Task<int> GetItemAmountByType(MiscEnums.Item type)
        {
            var pokeballs = await GetItems();
            return pokeballs.FirstOrDefault(i => (MiscEnums.Item) i.ItemId == type)?.Count ?? 0;
        }

        public async Task<IEnumerable<POGOProtos.Inventory.Item.ItemData>> GetItems()
        {
            var inventory = await GetCachedInventory();
            return inventory.InventoryDelta.InventoryItems
                .Select(i => i.InventoryItemData?.Item)
                .Where(p => p != null);
        }

        public async Task<IEnumerable<POGOProtos.Inventory.Item.ItemData>> GetItemsToRecycle(ISettings settings)
        {
            var myItems = await GetItems();

            return myItems
                .Where(x => settings.ItemRecycleFilter.Any(f => f.Key == (ItemId) x.ItemId && x.Count > f.Value))
                .Select(
                    x =>
                        new POGOProtos.Inventory.Item.ItemData
                        {
                            ItemId = x.ItemId,
                            Count = x.Count - settings.ItemRecycleFilter.Single(f => f.Key == (ItemId) x.ItemId).Value,
                            Unseen = x.Unseen
                        });
        }

        public async Task<IEnumerable<PlayerStats>> GetPlayerStats()
        {
            var inventory = await GetCachedInventory();
            return inventory.InventoryDelta.InventoryItems
                .Select(i => i.InventoryItemData?.PlayerStats)
                .Where(p => p != null);
        }

        //public async Task<IEnumerable<POGOProtos.Data.PokemonData>> GetPokemonFamilies()
        //{
        //    var inventory = await GetCachedInventory();
        //    return
        //        inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.PokemonData.Family)
        //            .Where(p => p != null && p.FamilyId != PokemonFamilyId.FamilyUnset);
        //}

        public async Task<IEnumerable<PokemonData>> GetPokemons()
        {
            var inventory = await GetCachedInventory();
            return
                inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.PokemonData)
                    .Where(p => p != null && p.PokemonId > 0);
        }

        public async Task<IEnumerable<PokemonSettings>> GetPokemonSettings()
        {
            var templates = await _client.Download.GetItemTemplates();
            return
                templates.ItemTemplates.Select(i => i.PokemonSettings)
                    .Where(p => p != null && p.FamilyId != PokemonFamilyId.FamilyUnset);
        }

        public async Task SetFavouritePerPokemon()
        {
            try
            {

            var allpokemon = await GetPokemons();
            var distinctIds = allpokemon.GroupBy(pokemon => pokemon.PokemonId).Select(grp => grp.First());

            foreach (var pokemon in distinctIds)
            {
                var highestPercent = await GetHighestPokemonOfTypeByIv(pokemon);

                await _client.Inventory.SetFavoritePokemon(highestPercent.Id, true);

                var notBest = allpokemon.Select(p => p).Where(p => p.PokemonId == highestPercent.PokemonId && p.Id != highestPercent.Id);

                foreach (var poke in notBest)
                {
                    await _client.Inventory.SetFavoritePokemon(poke.Id, false);
                    await Task.Delay(300);
                }

                    Logger.Write($"Best {pokemon.PokemonId} set");
            }

            Logger.Write($"Best percentage for each pokemon set as favourite", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Logger.Write($"Error setting favourites by percentage");
                throw;
            }
        }


        //public async Task<IEnumerable<PokemonData>> GetPokemonToEvolve(IEnumerable<PokemonId> filter = null)
        //{
        //    var myPokemons = await GetPokemons();
        //    myPokemons = myPokemons.Where(p => p.DeployedFortId == "0").OrderByDescending(p => p.Cp);
        //        //Don't evolve pokemon in gyms
        //    if (filter != null)
        //    {
        //        myPokemons = myPokemons.Where(p => filter.Contains(p.PokemonId));
        //    }
        //    var pokemons = myPokemons.ToList();

        //    var myPokemonSettings = await GetPokemonSettings();
        //    var pokemonSettings = myPokemonSettings.ToList();

        //    var myPokemonFamilies = await GetPokemonFamilies();
        //    var pokemonFamilies = myPokemonFamilies.ToArray();

        //    var pokemonToEvolve = new List<PokemonData>();
        //    foreach (var pokemon in pokemons)
        //    {
        //        var settings = pokemonSettings.Single(x => x.PokemonId == pokemon.PokemonId);
        //        var familyCandy = pokemonFamilies.Single(x => settings.FamilyId == x.FamilyId);

        //        //Don't evolve if we can't evolve it
        //        if (settings.EvolutionIds.Count == 0)
        //            continue;

        //        var pokemonCandyNeededAlready =
        //            pokemonToEvolve.Count(
        //                p => pokemonSettings.Single(x => x.PokemonId == p.PokemonId).FamilyId == settings.FamilyId)*
        //            settings.CandyToEvolve;

        //        if (_client.Settings.EvolveAllPokemonAboveIV)
        //            if (PokemonInfo.CalculatePokemonPerfection(pokemon) >= _client.Settings.EvolveAboveIVValue &&
        //                familyCandy.Candy - pokemonCandyNeededAlready > settings.CandyToEvolve)
        //            {
        //                pokemonToEvolve.Add(pokemon);
        //            }
        //            else
        //            {
        //            }
        //        else
        //        {
        //            if (familyCandy.Candy - pokemonCandyNeededAlready > settings.CandyToEvolve)
        //            {
        //                pokemonToEvolve.Add(pokemon);
        //            }
        //        }
        //    }
            
        //    return pokemonToEvolve;
        //}
    }
}