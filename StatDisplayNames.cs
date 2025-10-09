using System.Collections.Generic;
using Assets.Scripts.Inventory__Items__Pickups.Chests;
using Assets.Scripts.Inventory__Items__Pickups.Items;

namespace MegabonkBetterMinimap
{
    public static class StatDisplayNames
    {
        public static readonly Dictionary<string, string> Interactable = new()
        {
            { typeof(InteractableChest).Name, "Chest" },
            { typeof(InteractableMicrowave).Name, "Microwave" },
            { typeof(InteractableShadyGuy).Name, "Shady Guy" },
            { typeof(ChargeShrine).Name, "Shrine" },
            { typeof(InteractableShrineChallenge).Name, "Challenge" },
            { typeof(InteractableShrineCursed).Name, "Cursed" },
            { typeof(InteractableShrineMagnet).Name, "Magnet" },
            { typeof(InteractableShrineMoai).Name, "Moai" },
        };

        public static readonly Dictionary<string, Dictionary<EItemRarity, string>> RarityOverrides =
            new()
            {
                {
                    typeof(InteractableChest).Name,
                    new() { { EItemRarity.Epic, "Elite" }, { EItemRarity.Legendary, "Free" } }
                },
                {
                    typeof(ChargeShrine).Name,
                    new() { { EItemRarity.Legendary, "Golden" } }
                },
            };

        public static string GetInteractableName(string type) =>
            Interactable.TryGetValue(type, out var name) ? name : type;

        public static string GetRarityName(string interactableType, EItemRarity rarity)
        {
            if (RarityOverrides.TryGetValue(interactableType, out var overrides))
            {
                if (overrides.TryGetValue(rarity, out var custom))
                    return custom;
            }

            return rarity.ToString();
        }
    }
}
