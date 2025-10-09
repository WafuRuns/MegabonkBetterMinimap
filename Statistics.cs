using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Inventory__Items__Pickups.Chests;
using Assets.Scripts.Inventory__Items__Pickups.Items;
using HarmonyLib;

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

        private static bool _isReset;
        private static List<int> _runStats;
        private static int _kills = 0;

        public static void ResetCounter()
        {
            interactableCounter = CopyDefault();
        }

        public static bool IsReset()
        {
            return _isReset;
        }

        public static void SetDefaultRunFlags()
        {
            _isReset = true;
            _kills = 0;
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

            foreach (var kvp in defaultInteractableCounter)
            {
                Dictionary<EItemRarity, int> innerCopy = [];
                foreach (var inner in kvp.Value)
                {
                    innerCopy[inner.Key] = inner.Value;
                }

                copy[kvp.Key] = innerCopy;
            }

            return copy;
        }

        // This needs to be backed by science. Currently this is very much a guesstimate.
        // What we need is logging every run so that we have the number of each interactable
        // and kills + level at the end of stage 1 and kills at the end of the run.

        // Template for tracking your runs if we wanna make the rating accurate:
        // https://docs.google.com/spreadsheets/d/1g9hjzonZ7EyAw2dnt1n__JMNKmneXhfFAec5CB_US2U/edit?usp=sharing
        // When you die, line of numbers is printed into the console, if you clone this spreadsheet,
        // you can paste it into the first column, go to Data -> Split text to columns

        // Share you spreadsheet on Discord: wafuruns

        public static float GetRating()
        {
            Dictionary<(string, EItemRarity), float> weights = new()
            {
                { (typeof(InteractableChest).Name, EItemRarity.Legendary), 1f },
                { (typeof(InteractableShadyGuy).Name, EItemRarity.Epic), 3f },
                { (typeof(InteractableShadyGuy).Name, EItemRarity.Legendary), 10f },
                { (typeof(ChargeShrine).Name, EItemRarity.Legendary), 5f },
                { (typeof(InteractableShrineChallenge).Name, EItemRarity.Common), 1f },
                { (typeof(InteractableShrineCursed).Name, EItemRarity.Common), 3f },
                { (typeof(InteractableShrineMoai).Name, EItemRarity.Common), 1.5f },
                { (typeof(InteractableShrineMagnet).Name, EItemRarity.Common), 0.25f },
            };

            float rating = 0f;

            foreach (var interactablePair in interactableCounter)
            {
                string interactableType = interactablePair.Key;

                foreach (var rarityEntry in interactablePair.Value)
                {
                    (string interactableType, EItemRarity Key) key = (
                        interactableType,
                        rarityEntry.Key
                    );
                    if (weights.TryGetValue(key, out var weight))
                        rating += rarityEntry.Value * weight;
                }
            }

            return rating;
        }

        public static void PrintStats()
        {
            _runStats.AddRange(new int[16 - _runStats.Count]);
            Plugin.Log.LogInfo($"Stats for map rating research: {string.Join(",", _runStats)}");
        }

        public static void ResetRunStats()
        {
            _isReset = false;
            (string, EItemRarity)[] order =
            [
                (nameof(InteractableChest), EItemRarity.Legendary),
                (nameof(InteractableShadyGuy), EItemRarity.Epic),
                (nameof(InteractableShadyGuy), EItemRarity.Legendary),
                (nameof(ChargeShrine), EItemRarity.Legendary),
                (nameof(InteractableShrineChallenge), EItemRarity.Common),
                (nameof(InteractableShrineCursed), EItemRarity.Common),
                (nameof(InteractableShrineMoai), EItemRarity.Common),
                (nameof(InteractableShrineMagnet), EItemRarity.Common),
            ];

            _runStats = [.. order.Select(k => interactableCounter[k.Item1][k.Item2])];
        }

        public static void AddRunStat(int value)
        {
            _runStats.Add(value);
        }

        public static void AddKill()
        {
            _kills += 1;
        }

        public static int GetKills()
        {
            return _kills;
        }
    }
}
