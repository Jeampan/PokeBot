#region using directives

using Google.Protobuf.Collections;
using POGOProtos.Networking.Responses;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace PokemonGo.RocketAPI.Logic.Utils
{
    public static class StringUtils
    {
        public static string GetSummedFriendlyNameOfItemAwardList(FortSearchResponse items)
        {
            var enumerable = items.ItemsAwarded as RepeatedField<POGOProtos.Inventory.Item.ItemAward>;

            if (!enumerable.Any())
                return string.Empty;

            return
                enumerable.GroupBy(i => i.ItemId)
                    .Select(kvp => new {ItemName = kvp.Key.ToString(), Amount = kvp.Sum(x => x.ItemCount)})
                    .Select(y => $"{y.Amount} x {y.ItemName}")
                    .Aggregate((a, b) => $"{a}, {b}");
        }
    }
}