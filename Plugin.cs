using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Actors.Enemies;
using Assets.Scripts.Camera;
using Assets.Scripts.Inventory__Items__Pickups.Chests;
using Assets.Scripts.Inventory__Items__Pickups.Interactables;
using Assets.Scripts.Inventory__Items__Pickups.Items;
using Assets.Scripts.Inventory__Items__Pickups.Weapons.Projectiles;
using Assets.Scripts.Managers;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace MegabonkBetterMinimap;

[BepInPlugin("com.wafuruns.megabonkbetterminimap", "MegabonkBetterMinimap", "1.3.0")]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log;
    private static float _currentScale = 1.0f;
    private const float ScaleIncrement = 0.10f;
    private const float MaxScale = 4.5f;
    private static int _currentZoom = 100;
    private static int _currentFullZoom = 300;
    private const int ZoomIncrement = 5;
    private const int MaxZoom = 500;
    private static bool _onMinimap = false;
    private static readonly Dictionary<EItemRarity, Color> RarityColors = new()
    {
        { EItemRarity.Common, new Color(0.225f, 1, 0, 1) },
        { EItemRarity.Rare, new Color(0, 0.317f, 0.965f, 1) },
        { EItemRarity.Epic, new Color(0.965f, 0, 0.691f, 1) },
        { EItemRarity.Legendary, new Color(0.951f, 0.965f, 0, 1) },
    };
    private static bool _hideJunk = false;
    private static ConfigEntry<float> CurrentScaleConfig;
    private static ConfigEntry<int> CurrentZoomConfig;
    private static ConfigEntry<int> CurrentFullZoomConfig;

    public override void Load()
    {
        Log = base.Log;

        CurrentScaleConfig = Config.Bind("Minimap", "CurrentScale", 1.0f, "Current minimap scale");
        CurrentZoomConfig = Config.Bind("Minimap", "CurrentZoom", 100, "Current normal zoom");
        CurrentFullZoomConfig = Config.Bind(
            "Minimap",
            "CurrentFullZoom",
            300,
            "Current full/minimap zoom"
        );

        _currentScale = CurrentScaleConfig.Value;
        _currentZoom = CurrentZoomConfig.Value;
        _currentFullZoom = CurrentFullZoomConfig.Value;

        Harmony harmony = new("com.wafuruns.megabonkbetterminimap");
        harmony.PatchAll();
        Log.LogInfo("Loaded MegabonkBetterMinimap");

        if (!ClassInjector.IsTypeRegisteredInIl2Cpp(typeof(StatsUI)))
            ClassInjector.RegisterTypeInIl2Cpp<StatsUI>();

        GameObject stats = new("StatsUI");
        stats.AddComponent<StatsUI>();
        UnityEngine.Object.DontDestroyOnLoad(stats);
    }

    [HarmonyPatch(typeof(MinimapUi), "Awake")]
    public static class MinimapUi_Awake_Patch
    {
        static void Postfix(MinimapUi __instance)
        {
            __instance.UpdateScale(_currentScale);
        }
    }

    [HarmonyPatch(typeof(MinimapUi), "Update")]
    public static class MinimapUi_Update_Patch
    {
        private static Vector3? originalLocalPosition = null;
        private static Vector2 originalAnchorMin,
            originalAnchorMax,
            originalPivot;

        static void Postfix(MinimapUi __instance)
        {
            if (KeyHelper.IsKeyPressedOnce(KeyCode.F1) && !_onMinimap)
            {
                _currentScale += ScaleIncrement;
                if (_currentScale > MaxScale)
                    _currentScale = 1.0f;

                __instance.UpdateScale(_currentScale);
                CurrentScaleConfig.Value = _currentScale;
            }

            if (KeyHelper.IsKeyPressedOnce(KeyCode.M))
            {
                Time.timeScale = Time.timeScale == 0 ? 1f : 0f;
                RectTransform rect = __instance.GetComponent<RectTransform>();
                if (rect == null)
                    return;

                if (Time.timeScale == 0)
                {
                    if (originalLocalPosition == null)
                    {
                        originalLocalPosition = rect.localPosition;
                        originalAnchorMin = rect.anchorMin;
                        originalAnchorMax = rect.anchorMax;
                        originalPivot = rect.pivot;
                    }

                    rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.localPosition = Vector3.zero;

                    __instance.border.gameObject.active = false;
                    __instance.UpdateScale(5f);

                    Transform mapRenderer = __instance.transform.Find("MapRenderer");
                    if (mapRenderer != null)
                    {
                        UnityEngine.UI.Mask mask = mapRenderer.GetComponent<UnityEngine.UI.Mask>();
                        if (mask != null)
                            mask.enabled = false;
                    }
                    _onMinimap = true;
                }
                else
                {
                    if (originalLocalPosition != null)
                    {
                        rect.anchorMin = originalAnchorMin;
                        rect.anchorMax = originalAnchorMax;
                        rect.pivot = originalPivot;
                        rect.localPosition = originalLocalPosition.Value;
                    }

                    __instance.border.gameObject.active = true;
                    __instance.UpdateScale(_currentScale);

                    Transform mapRenderer = __instance.transform.Find("MapRenderer");
                    if (mapRenderer != null)
                    {
                        UnityEngine.UI.Mask mask = mapRenderer.GetComponent<UnityEngine.UI.Mask>();
                        if (mask != null)
                            mask.enabled = true;
                    }
                    _onMinimap = false;
                }
            }

            if (KeyHelper.IsKeyPressedOnce(KeyCode.F3))
            {
                _hideJunk = !_hideJunk;
            }
        }
    }

    [HarmonyPatch(typeof(GameManager), "Update")]
    class RestartRunPatch
    {
        static void Postfix()
        {
            if (KeyHelper.IsKeyPressedOnce(KeyCode.P))
            {
                MapController.RestartRun();
            }
        }
    }

    [HarmonyPatch(typeof(MinimapCamera), "Update")]
    public static class MinimapCamera_Update_Patch
    {
        private static Vector3? originalPosition = null;
        private static readonly Quaternion CenterRotation = Quaternion.Euler(90f, 0f, 0f);
        private static readonly Vector3 CenterPosition = new(0f, 1000f, 0f);

        static void Postfix(MinimapCamera __instance)
        {
            RenderTexture texture = __instance.minimapCamera.targetTexture;
            if (texture != null)
            {
                texture.Release();
                texture.width = 1024;
                texture.height = 1024;
                texture.Create();
            }

            if (_onMinimap == true)
            {
                if (originalPosition == null)
                {
                    originalPosition = __instance.minimapCamera.transform.position;
                }

                __instance.minimapCamera.transform.SetPositionAndRotation(
                    CenterPosition,
                    CenterRotation
                );

                if (__instance.minimapCamera.orthographicSize != _currentFullZoom)
                {
                    __instance.minimapCamera.orthographicSize = _currentFullZoom;
                }
            }
            else
            {
                if (originalPosition != null)
                {
                    __instance.minimapCamera.transform.position = originalPosition.Value;
                    originalPosition = null;
                }

                if (__instance.minimapCamera.orthographicSize != _currentZoom)
                {
                    __instance.minimapCamera.orthographicSize = _currentZoom;
                }
            }

            if (KeyHelper.IsKeyPressedOnce(KeyCode.F2))
            {
                if (__instance?.minimapCamera == null)
                    return;

                if (_onMinimap)
                {
                    _currentFullZoom += ZoomIncrement;
                    if (_currentFullZoom > MaxZoom)
                        _currentFullZoom = 100;
                    CurrentZoomConfig.Value = _currentZoom;
                }
                else
                {
                    _currentZoom += ZoomIncrement;
                    if (_currentZoom > MaxZoom)
                        _currentZoom = 100;
                    CurrentZoomConfig.Value = _currentZoom;
                }
            }
        }
    }

    [HarmonyPatch(typeof(InteractableChest), "Start")]
    class InteractableChest_Start_Patch
    {
        static void Postfix(InteractableChest __instance)
        {
            // TODO: Chests from elites and challenges don't work?
            EItemRarity rarity = __instance.chestType switch
            {
                EChest.Normal => EItemRarity.Common,
                EChest.Free => EItemRarity.Legendary,
                _ => EItemRarity.Common,
            };
            Statistics.AddInteractable(__instance.GetType().Name, rarity);

            if (__instance.chestType == EChest.Free)
            {
                ChangeMinimapIcon(__instance.icon, "ChestFree");
            }
        }
    }

    [HarmonyPatch(typeof(OpenChest), "Awake")]
    class OpenChest_Awake_Patch
    {
        static void Postfix(OpenChest __instance)
        {
            Statistics.AddInteractable(typeof(InteractableChest).Name, EItemRarity.Epic);
        }
    }

    [HarmonyPatch(typeof(OpenChest), "OnTriggerStay")]
    class OpenChest_OnTriggerStay_Patch
    {
        static void Postfix(OpenChest __instance)
        {
            Statistics.RemoveInteractable(typeof(InteractableChest).Name, EItemRarity.Epic);
        }
    }

    [HarmonyPatch(typeof(InteractableChest), "OnDestroy")]
    class InteractableChest_OnDestroy_Patch
    {
        static void Postfix(InteractableChest __instance)
        {
            EItemRarity rarity = __instance.chestType switch
            {
                EChest.Normal => EItemRarity.Common,
                EChest.Free => EItemRarity.Legendary,
                _ => EItemRarity.Common,
            };
            Statistics.RemoveInteractable(__instance.GetType().Name, rarity);
        }
    }

    [HarmonyPatch(typeof(ChargeShrine), "Start")]
    class ChargeShrine_Start_Patch
    {
        static void Postfix(ChargeShrine __instance)
        {
            Statistics.AddInteractable(
                __instance.GetType().Name,
                __instance.isGolden ? EItemRarity.Legendary : EItemRarity.Common
            );

            if (__instance.isGolden)
            {
                ChangeMinimapIcon(
                    __instance.minimapIcon.transform,
                    "Shrine",
                    EItemRarity.Legendary
                );
            }
            else
            {
                ChangeMinimapIcon(__instance.minimapIcon.transform, "Shrine", EItemRarity.Common);
            }
        }
    }

    [HarmonyPatch(typeof(ChargeShrine), "Complete")]
    class ChargeShrine_Complete_Patch
    {
        static void Postfix(ChargeShrine __instance)
        {
            Statistics.RemoveInteractable(
                __instance.GetType().Name,
                __instance.isGolden ? EItemRarity.Legendary : EItemRarity.Common
            );
        }
    }

    [HarmonyPatch(typeof(InteractableMicrowave), "Start")]
    class InteractableMicrowave_Start_Patch
    {
        static void Postfix(InteractableMicrowave __instance)
        {
            if (__instance.usesLeft > 0)
            {
                Statistics.AddInteractable(__instance.GetType().Name, __instance.rarity);
                ChangeMinimapIcon(__instance.minimapIcon.transform, "Microwave", __instance.rarity);
            }
        }
    }

    [HarmonyPatch(typeof(InteractableMicrowave), "Explode")]
    class InteractableMicrowave_Explode_Patch
    {
        static void Postfix(InteractableMicrowave __instance)
        {
            Statistics.RemoveInteractable(__instance.GetType().Name, __instance.rarity);
        }
    }

    [HarmonyPatch(typeof(InteractableShadyGuy), "Start")]
    class InteractableShadyGuy_Start_Patch
    {
        static void Postfix(InteractableShadyGuy __instance)
        {
            Statistics.AddInteractable(__instance.GetType().Name, __instance.rarity);
            ChangeMinimapIcon(
                __instance.hideAfterPurchase.First().transform,
                "ShadyGuy",
                __instance.rarity
            );
        }
    }

    [HarmonyPatch(typeof(InteractableShadyGuy), "OnDestroy")]
    class InteractableShadyGuy_OnDestroy_Patch
    {
        static void Postfix(InteractableShadyGuy __instance)
        {
            Statistics.RemoveInteractable(__instance.GetType().Name, __instance.rarity);
        }
    }

    [HarmonyPatch(typeof(InteractableShrineChallenge), "Awake")]
    class InteractableShrineChallenge_Awake_Patch
    {
        static void Postfix(InteractableShrineChallenge __instance)
        {
            if (!__instance.done)
            {
                Statistics.AddInteractable(__instance.GetType().Name, EItemRarity.Common);
                ChangeMinimapIcon(__instance.minimapIcon.transform, "Challenge");
            }
        }
    }

    [HarmonyPatch(typeof(InteractableShrineChallenge), "OnDestroy")]
    class InteractableShrineChallenge_OnDestroy_Patch
    {
        static void Postfix(InteractableShrineChallenge __instance)
        {
            Statistics.RemoveInteractable(__instance.GetType().Name, EItemRarity.Common);
        }
    }

    [HarmonyPatch(typeof(BaseInteractable), "Start")]
    public static class BaseInteractable_Start_Patch
    {
        static void Postfix(BaseInteractable __instance)
        {
            string typeName = __instance.GetIl2CppType().Name;
            if (typeName == "InteractableShrineCursed")
            {
                InteractableShrineCursed shrine =
                    __instance.GetComponent<InteractableShrineCursed>();
                if (shrine != null)
                {
                    Statistics.AddInteractable(shrine.GetType().Name, EItemRarity.Common);
                    ChangeMinimapIcon(shrine.minimapIcon.transform, "BossCurse");
                }
                return;
            }
            if (typeName == "InteractableShrineMagnet")
            {
                InteractableShrineMagnet shrine =
                    __instance.GetComponent<InteractableShrineMagnet>();
                if (shrine != null)
                {
                    Statistics.AddInteractable(shrine.GetType().Name, EItemRarity.Common);
                    ChangeMinimapIcon(shrine.minimapIcon.transform, "Magnet");
                }
                return;
            }
            if (typeName == "InteractableShrineMoai")
            {
                InteractableShrineMoai shrine = __instance.GetComponent<InteractableShrineMoai>();
                if (shrine != null)
                {
                    Statistics.AddInteractable(shrine.GetType().Name, EItemRarity.Common);
                    ChangeMinimapIcon(shrine.minimapIcon.transform, "Moai");
                }
                return;
            }
        }
    }

    [HarmonyPatch(typeof(BaseInteractable), "OnDestroy")]
    public static class BaseInteractable_OnDestroy_Patch
    {
        static void Postfix(BaseInteractable __instance)
        {
            string typeName = __instance.GetIl2CppType().Name;
            if (typeName == "InteractableShrineCursed")
            {
                InteractableShrineCursed shrine =
                    __instance.GetComponent<InteractableShrineCursed>();
                if (shrine != null)
                {
                    Statistics.RemoveInteractable(shrine.GetType().Name, EItemRarity.Common);
                }
                return;
            }
            if (typeName == "InteractableShrineMagnet")
            {
                InteractableShrineMagnet shrine =
                    __instance.GetComponent<InteractableShrineMagnet>();
                if (shrine != null)
                {
                    Statistics.RemoveInteractable(shrine.GetType().Name, EItemRarity.Common);
                }
                return;
            }
            if (typeName == "InteractableShrineMoai")
            {
                InteractableShrineMoai shrine = __instance.GetComponent<InteractableShrineMoai>();
                if (shrine != null)
                {
                    Statistics.RemoveInteractable(shrine.GetType().Name, EItemRarity.Common);
                }
                return;
            }
        }
    }

    [HarmonyPatch(typeof(PauseHandler), "Start")]
    public static class PauseHandler_Start_Patch
    {
        static void Postfix()
        {
            Statistics.ResetCounter();
        }
    }

    public static void ChangeMinimapIcon(
        Transform icon,
        string iconName,
        EItemRarity? rarity = null
    )
    {
        MeshRenderer meshRenderer = icon.GetComponent<MeshRenderer>();

        if (meshRenderer == null)
            return;

        meshRenderer.transform.localScale *= 1.3f;

        Texture2D tex = TextureManager.Load(iconName);

        if (tex != null)
        {
            meshRenderer.material.mainTexture = tex;
            meshRenderer.material.color =
                rarity != null ? RarityColors[(EItemRarity)rarity] : Color.white;
        }
    }

    [HarmonyPatch(typeof(Enemy), "InitEnemy")]
    public static class Enemy_InitEnemy_Patch
    {
        static void Postfix(Enemy __instance)
        {
            if (!_hideJunk)
                return;
            foreach (Renderer renderer in __instance.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }
        }
    }

    [HarmonyPatch(typeof(ProjectileBase), "Set")]
    public static class ProjectileBase_Set_Patch
    {
        static void Postfix(ProjectileBase __instance)
        {
            if (!_hideJunk)
                return;
            foreach (Renderer renderer in __instance.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }
        }
    }
}
