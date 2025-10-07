using System.Collections.Generic;
using Assets.Scripts.Inventory__Items__Pickups.Chests;
using Assets.Scripts.Inventory__Items__Pickups.Items;

namespace MegabonkBetterMinimap
{
    public static class Statistics
    {
        private static readonly Dictionary<
            string,
            Dictionary<EItemRarity, int>
        > defaultInteractableCounter = new()
        {
            {
                typeof(InteractableChest).Name,
                new()
                {
                    { EItemRarity.Common, 0 },
                    { EItemRarity.Epic, 0 },
                    { EItemRarity.Legendary, 0 },
                }
            },
            {
                typeof(InteractableMicrowave).Name,
                new()
                {
                    { EItemRarity.Common, 0 },
                    { EItemRarity.Rare, 0 },
                    { EItemRarity.Epic, 0 },
                    { EItemRarity.Legendary, 0 },
                }
            },
            {
                typeof(InteractableShadyGuy).Name,
                new()
                {
                    { EItemRarity.Common, 0 },
                    { EItemRarity.Rare, 0 },
                    { EItemRarity.Epic, 0 },
                    { EItemRarity.Legendary, 0 },
                }
            },
            {
                typeof(ChargeShrine).Name,
                new() { { EItemRarity.Common, 0 }, { EItemRarity.Legendary, 0 } }
            },
            {
                typeof(InteractableShrineChallenge).Name,
                new() { { EItemRarity.Common, 0 } }
            },
            {
                typeof(InteractableShrineCursed).Name,
                new() { { EItemRarity.Common, 0 } }
            },
            {
                typeof(InteractableShrineMagnet).Name,
                new() { { EItemRarity.Common, 0 } }
            },
            {
                typeof(InteractableShrineMoai).Name,
                new() { { EItemRarity.Common, 0 } }
            },
        };

        private static Dictionary<string, Dictionary<EItemRarity, int>> interactableCounter =
            CopyDefault();

        public static void ResetCounter()
        {
            interactableCounter = CopyDefault();
        }

        public static void AddInteractable(string type, EItemRarity rarity)
        {
            interactableCounter[type][rarity] += 1;
            StatsUI.Instance?.UpdateUI();
        }

        public static void RemoveInteractable(string type, EItemRarity rarity)
        {
            interactableCounter[type][rarity] -= 1;
            StatsUI.Instance?.UpdateUI();
        }

        public static Dictionary<string, Dictionary<EItemRarity, int>> GetCounters()
        {
            return interactableCounter;
        }

        private static Dictionary<string, Dictionary<EItemRarity, int>> CopyDefault()
        {
            Dictionary<string, Dictionary<EItemRarity, int>> copy = [];

            foreach (
                KeyValuePair<string, Dictionary<EItemRarity, int>> kvp in defaultInteractableCounter
            )
            {
                Dictionary<EItemRarity, int> innerCopy = [];
                foreach (KeyValuePair<EItemRarity, int> inner in kvp.Value)
                {
                    innerCopy[inner.Key] = inner.Value;
                }

                copy[kvp.Key] = innerCopy;
            }

            return copy;
        }
    }
}
