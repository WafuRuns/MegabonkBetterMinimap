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
                    new Dictionary<EItemRarity, string>
                    {
                        { EItemRarity.Epic, "Elite" },
                        { EItemRarity.Legendary, "Free" },
                    }
                },
                {
                    typeof(ChargeShrine).Name,
                    new Dictionary<EItemRarity, string> { { EItemRarity.Legendary, "Golden" } }
                },
            };

        public static string GetInteractableName(string type) =>
            Interactable.TryGetValue(type, out string name) ? name : type;

        public static string GetRarityName(string interactableType, EItemRarity rarity)
        {
            if (
                RarityOverrides.TryGetValue(
                    interactableType,
                    out Dictionary<EItemRarity, string> overrides
                )
            )
            {
                if (overrides.TryGetValue(rarity, out string custom))
                    return custom;
            }

            return rarity.ToString();
        }
    }
}
