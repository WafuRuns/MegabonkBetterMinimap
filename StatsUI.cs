using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Inventory__Items__Pickups.Chests;
using Assets.Scripts.Inventory__Items__Pickups.Items;
using UnityEngine;
using UnityEngine.UI;

namespace MegabonkBetterMinimap
{
    public class StatsUI : MonoBehaviour
    {
        public static StatsUI Instance { get; private set; }
        private Canvas _canvas;
        private RectTransform _panel;
        private VerticalLayoutGroup _layoutGroup;
        private readonly Dictionary<string, GameObject> _statLines = new();
        private static Font gameFont;
        private static Sprite borderSprite;
        private bool _initialized = false;
        private bool _visible = true;
        private const int VK_T = 0x54;

        private void Update()
        {
            if (_initialized)
            {
                if (Plugin.IsKeyPressedOnce(VK_T))
                {
                    _visible = !_visible;
                    _canvas.gameObject.SetActive(_visible);
                }
                return;
            }

            InteractableChest obj = FindObjectOfType<InteractableChest>();

            if (obj != null)
            {
                InitializeResources();
                CreateCanvas();
                UpdateUI();
                _initialized = true;
            }
        }

        public static void InitializeResources()
        {
            if (gameFont == null)
            {
                gameFont = Resources
                    .FindObjectsOfTypeAll<Font>()
                    .FirstOrDefault(f => f.name == "alagard");

                if (gameFont != null && gameFont.material?.mainTexture != null)
                    gameFont.material.mainTexture.filterMode = FilterMode.Point;
            }

            if (borderSprite == null)
            {
                borderSprite = Resources
                    .FindObjectsOfTypeAll<Sprite>()
                    .FirstOrDefault(s => s.name == "Border2_Gray");
            }
        }

        private void CreateCanvas()
        {
            Instance = this;

            GameObject canvas = new("StatsCanvas");
            canvas.transform.SetParent(transform, false);

            _canvas = canvas.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.pixelPerfect = true;

            CanvasScaler scaler = canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;

            canvas.AddComponent<GraphicRaycaster>();

            GameObject panel = new("StatsPanel");
            panel.transform.SetParent(canvas.transform, false);
            _panel = panel.AddComponent<RectTransform>();

            Image image = panel.AddComponent<Image>();
            image.sprite = borderSprite;
            image.type = Image.Type.Sliced;
            image.color = Color.white;

            _panel.anchorMin = new Vector2(0f, 0f);
            _panel.anchorMax = new Vector2(0f, 0f);
            _panel.pivot = new Vector2(0f, 0f);
            _panel.anchoredPosition = new Vector2(30f, 60f);
            _panel.sizeDelta = new Vector2(380f, 600f);

            _layoutGroup = panel.AddComponent<VerticalLayoutGroup>();
            _layoutGroup.childAlignment = TextAnchor.UpperLeft;
            _layoutGroup.spacing = 3;
            _layoutGroup.padding = new RectOffset
            {
                left = 32,
                right = 32,
                top = 24,
                bottom = 24,
            };

            UpdateUI();

            _visible = !_visible;
            _canvas.gameObject.SetActive(_visible);
        }

        private GameObject CreateLine(string content, int indent = 0)
        {
            GameObject line = new("Line");
            line.transform.SetParent(_panel, false);
            RectTransform rectTransform = line.AddComponent<RectTransform>();
            rectTransform.localScale = Vector3.one;
            rectTransform.sizeDelta = new Vector2(0, 24);

            GameObject text = new("Text");
            text.transform.SetParent(line.transform, false);
            Text textComponent = text.AddComponent<Text>();
            textComponent.font = gameFont;
            textComponent.fontSize = 22;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.MiddleLeft;
            textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
            textComponent.verticalOverflow = VerticalWrapMode.Overflow;
            textComponent.text = content;

            RectTransform textRectTransform = text.GetComponent<RectTransform>();
            textRectTransform.anchorMin = new Vector2(0, 1);
            textRectTransform.anchorMax = new Vector2(0, 1);
            textRectTransform.pivot = new Vector2(0, 1);
            textRectTransform.anchoredPosition = new Vector2(indent * 24, 0);
            textRectTransform.sizeDelta = new Vector2(_panel.sizeDelta.x - indent * 24, 24);

            return line;
        }

        public void UpdateUI()
        {
            foreach (KeyValuePair<string, GameObject> kvp in _statLines)
                Destroy(kvp.Value);
            _statLines.Clear();

            foreach (
                KeyValuePair<string, Dictionary<EItemRarity, int>> kvp in Statistics.GetCounters()
            )
            {
                string typeName = kvp.Key;
                Dictionary<EItemRarity, int> rarityDict = kvp.Value;

                string displayType = StatDisplayNames.GetInteractableName(typeName);

                if (rarityDict.Count == 1 && rarityDict.ContainsKey(EItemRarity.Common))
                {
                    string content = $"{displayType}: {rarityDict[EItemRarity.Common]}";
                    _statLines[typeName] = CreateLine(content, 0);
                }
                else
                {
                    _statLines[typeName] = CreateLine(displayType, 0);

                    foreach (KeyValuePair<EItemRarity, int> r in rarityDict)
                    {
                        string content =
                            $"{StatDisplayNames.GetRarityName(typeName, r.Key)}: {r.Value}";
                        _statLines[$"{typeName}_{r.Key}"] = CreateLine(content, 1);
                    }
                }
            }
        }
    }
}
