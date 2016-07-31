using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using PokemonGo.RocketAPI.Enums;
using System.Collections.Generic;

namespace PokemonGo.RocketAPI
{
    public interface ISettings
    {
        AuthType AuthType { get; }
        double DefaultLatitude { get; }
        double DefaultLongitude { get; }
        double DefaultAltitude { get; }
        //string GoogleRefreshToken { get; set; }
        //string PtcPassword { get; set; }
       // string PtcUsername { get; set; }
        //string GoogleUsername { get; }
        //string GooglePassword { get; }

        float KeepMinIVPercentage { get; }
        int KeepMinCP { get; }
        double WalkingSpeedInKilometerPerHour { get; }
        bool EvolveAllPokemonWithEnoughCandy { get; }
        bool TransferDuplicatePokemon { get; }
        int DelayBetweenPokemonCatch { get; }
        bool UsePokemonToNotCatchFilter { get; }
        int KeepMinDuplicatePokemon { get; }
        bool PrioritizeIVOverCP { get; }
        int MaxTravelDistanceInMeters { get; }
        bool UseGPXPathing { get; }
        string GPXFile { get; }
        bool useLuckyEggsWhileEvolving { get; }
        bool EvolveAllPokemonAboveIV { get; }
        float EvolveAboveIVValue { get; }
        bool RecycleItems { get; }

        bool DontCatchPokemon { get; set; }
        int NarratorVolume { get; }
        int NarratorSpeed { get; }
        bool OnlyTransferDuplicateShit { get; }

        ICollection<KeyValuePair<ItemId, int>> ItemRecycleFilter { get; }

        ICollection<PokemonId> PokemonsToEvolve { get; }

        ICollection<PokemonId> PokemonsNotToTransfer { get; }

        ICollection<PokemonId> PokemonsNotToCatch { get; }
        ICollection<PokemonId> ShitPokemonsToTransfer { get; }

        bool PurePokemonMode { get; }
    }
}