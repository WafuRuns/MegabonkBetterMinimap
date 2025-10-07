using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Assets.Scripts.Camera;
using Assets.Scripts.Inventory__Items__Pickups.Chests;
using Assets.Scripts.Inventory__Items__Pickups.Items;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;

namespace MegabonkBetterMinimap;

[BepInPlugin("com.wafuruns.megabonkbetterminimap", "MegabonkBetterMinimap", "1.1.0")]
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
    private static DateTime _lastKeyPressTime = DateTime.MinValue;
    private static readonly TimeSpan KeyCooldown = TimeSpan.FromMilliseconds(100);
    private static readonly Dictionary<EItemRarity, Color> RarityColors = new()
    {
        { EItemRarity.Common, new Color(0.225f, 1, 0, 1) },
        { EItemRarity.Rare, new Color(0, 0.317f, 0.965f, 1) },
        { EItemRarity.Epic, new Color(0.965f, 0, 0.691f, 1) },
        { EItemRarity.Legendary, new Color(0.951f, 0.965f, 0, 1) }
    };

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    public override void Load()
    {
        Log = base.Log;

        Harmony harmony = new("com.wafuruns.megabonkbetterminimap");
        harmony.PatchAll();
        Log.LogInfo("Loaded MegabonkBetterMinimap");
    }

    [HarmonyPatch(typeof(MinimapUi), "Update")]
    public static class MinimapUi_Update_Patch
    {
        private static Vector3? originalLocalPosition = null;
        private static Vector2 originalAnchorMin, originalAnchorMax, originalPivot;

        static void Postfix(MinimapUi __instance)
        {
            const int VK_F1 = 0x70;
            const int VK_M = 0x4D;

            if (IsKeyPressedOnce(VK_F1) && !_onMinimap)
            {
                try
                {
                    _currentScale += ScaleIncrement;
                    if (_currentScale > MaxScale)
                        _currentScale = 1.0f;

                    __instance.UpdateScale(_currentScale);
                }
                catch (Exception ex)
                {
                    Log.LogWarning($"Couldn't find MinimapUi instance! {ex}");
                }
            }

            if (IsKeyPressedOnce(VK_M))
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
        }
    }

    [HarmonyPatch(typeof(MinimapCamera), "Update")]
    public static class MinimapCamera_Update_Patch
    {
        private static Vector3? originalPosition = null;
        private static readonly Quaternion CenterRotation = new(0.50000f, 0.50000f, -0.50000f, 0.50000f);
        private static readonly Vector3 CenterPosition = new(0f, 1000f, 0f);
        static void Postfix(MinimapCamera __instance)
        {
            if (_onMinimap == true)
            {
                if (originalPosition == null)
                {
                    originalPosition = __instance.minimapCamera.transform.position;
                }

                __instance.minimapCamera.transform.SetPositionAndRotation(CenterPosition, CenterRotation);

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

            const int VK_F2 = 0x71;

            if (IsKeyPressedOnce(VK_F2))
            {
                if (__instance?.minimapCamera == null)
                    return;

                if (_onMinimap)
                {
                    _currentFullZoom += ZoomIncrement;
                    if (_currentFullZoom > MaxZoom)
                        _currentFullZoom = 100;
                }
                else
                {
                    _currentZoom += ZoomIncrement;
                    if (_currentZoom > MaxZoom)
                        _currentZoom = 100;
                }
            }
        }
    }

    [HarmonyPatch(typeof(InteractableChest), "Start")]
    class InteractableChest_Start_Patch
    {
        static void Postfix(InteractableChest __instance)
        {
            if (__instance.chestType == Assets.Scripts.Inventory__Items__Pickups.Interactables.EChest.Free)
            {
                ChangeMinimapIcon(__instance.icon, "ChestFree");
            }
        }
    }

    [HarmonyPatch(typeof(ChargeShrine), "Start")]
    class ChargeShrine_Start_Patch
    {
        static void Postfix(ChargeShrine __instance)
        {
            if (__instance.isGolden)
            {
                ChangeMinimapIcon(__instance.minimapIcon.transform, "Shrine", EItemRarity.Legendary);
            }
            else
            {
                ChangeMinimapIcon(__instance.minimapIcon.transform, "Shrine", EItemRarity.Common);
            }
        }
    }

    [HarmonyPatch(typeof(InteractableMicrowave), "Start")]
    class InteractableMicrowave_Start_Patch
    {
        static void Postfix(InteractableMicrowave __instance)
        {
            if (__instance.usesLeft > 0)
            {
                ChangeMinimapIcon(__instance.minimapIcon.transform, "Microwave", __instance.rarity);
            }
        }
    }

    [HarmonyPatch(typeof(InteractableShadyGuy), "Start")]
    class InteractableShadyGuy_Start_Patch
    {
        static void Postfix(InteractableShadyGuy __instance)
        {
            ChangeMinimapIcon(__instance.hideAfterPurchase.First().transform, "ShadyGuy", __instance.rarity);
        }
    }

    [HarmonyPatch(typeof(InteractableShrineChallenge), "Awake")]
    class InteractableShrineChallenge_Awake_Patch
    {
        static void Postfix(InteractableShrineChallenge __instance)
        {
            if (!__instance.done)
            {
                ChangeMinimapIcon(__instance.minimapIcon.transform, "Challenge");
            }
        }
    }

    [HarmonyPatch(typeof(BaseInteractable), "Start")]
    public static class BaseInteractableStartPatch
    {
        static void Postfix(BaseInteractable __instance)
        {
            string typeName = __instance.GetIl2CppType().Name;
            if (typeName == "InteractableShrineCursed")
            {
                InteractableShrineCursed shrine = __instance.GetComponent<InteractableShrineCursed>();
                if (shrine != null)
                {
                    ChangeMinimapIcon(shrine.minimapIcon.transform, "BossCurse");
                }
                return;
            }
            if (typeName == "InteractableShrineMagnet")
            {
                InteractableShrineMagnet shrine = __instance.GetComponent<InteractableShrineMagnet>();
                if (shrine != null)
                {
                    ChangeMinimapIcon(shrine.minimapIcon.transform, "Magnet");
                }
                return;
            }
            if (typeName == "InteractableShrineMoai")
            {
                InteractableShrineMoai shrine = __instance.GetComponent<InteractableShrineMoai>();
                if (shrine != null)
                {
                    ChangeMinimapIcon(shrine.minimapIcon.transform, "Moai");
                }
                return;
            }
        }
    }

    public static bool IsKeyPressedOnce(int vKey)
    {
        if ((GetAsyncKeyState(vKey) & 0x8000) == 0)
            return false;

        if (DateTime.Now - _lastKeyPressTime < KeyCooldown)
            return false;

        _lastKeyPressTime = DateTime.Now;
        return true;
    }

    public static void ChangeMinimapIcon(Transform icon, string iconName, EItemRarity? rarity = null)
    {
        MeshRenderer meshRenderer = icon.GetComponent<MeshRenderer>();

        if (meshRenderer == null)
            return;

        Texture2D tex = TextureManager.Load(iconName);

        if (tex != null)
        {
            meshRenderer.material.mainTexture = tex;
            meshRenderer.material.color = rarity != null ? RarityColors[(EItemRarity)rarity] : Color.white;
        }
    }
}
