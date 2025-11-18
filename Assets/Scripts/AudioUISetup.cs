using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Helper script to automatically create UI sliders for volume and pitch control
/// </summary>
public class AudioUISetup : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool autoCreateUI = false;

    [Header("UI References")]
    public Canvas audioUICanvas;
    public Slider volumeSlider;
    public Slider pitchSlider;
    public Text volumeLabel;
    public Text pitchLabel;
    public Text volumeValueText;
    public Text pitchValueText;

    void Start()
    {
        if (autoCreateUI)
        {
            CreateAudioUI();
        }

        ConnectToAudioManager();
    }

    void ConnectToAudioManager()
    {
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            // Connect sliders to AudioManager
            if (volumeSlider != null)
            {
                audioManager.masterVolumeSlider = volumeSlider;
            }

            if (pitchSlider != null)
            {
                audioManager.masterPitchSlider = pitchSlider;
            }

            if (volumeValueText != null)
            {
                audioManager.volumeValueText = volumeValueText;
            }

            if (pitchValueText != null)
            {
                audioManager.pitchValueText = pitchValueText;
            }
        }
    }

    void CreateAudioUI()
    {
        // Find or create canvas
        if (audioUICanvas == null)
        {
            audioUICanvas = FindObjectOfType<Canvas>();
            if (audioUICanvas == null)
            {
                GameObject canvasObj = new GameObject("Audio UI Canvas");
                audioUICanvas = canvasObj.AddComponent<Canvas>();
                audioUICanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }
        }

        // Create volume slider
        if (volumeSlider == null)
        {
            CreateVolumeSlider();
        }

        // Create pitch slider
        if (pitchSlider == null)
        {
            CreatePitchSlider();
        }
    }

    void CreateVolumeSlider()
    {
        GameObject sliderObj = new GameObject("Volume Slider");
        sliderObj.transform.SetParent(audioUICanvas.transform, false);

        RectTransform rectTransform = sliderObj.AddComponent<RectTransform>();
        // Position on right side, smaller width
        rectTransform.anchorMin = new Vector2(0.85f, 0.65f);
        rectTransform.anchorMax = new Vector2(0.98f, 0.7f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;

        volumeSlider = sliderObj.AddComponent<Slider>();
        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;
        volumeSlider.value = 1f;

        // Create background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform, false);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        volumeSlider.targetGraphic = bgImage;

        // Create fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;

        // Create fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.8f, 0.2f, 1f);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.sizeDelta = Vector2.zero;
        volumeSlider.fillRect = fillRect;

        // Create handle (smaller)
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(sliderObj.transform, false);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = Color.white;
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(12, 12);
        volumeSlider.handleRect = handleRect;

        // Create label (smaller)
        GameObject labelObj = new GameObject("Volume Label");
        labelObj.transform.SetParent(sliderObj.transform, false);
        volumeLabel = labelObj.AddComponent<Text>();
        volumeLabel.text = "Volume";
        volumeLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        volumeLabel.fontSize = 10;
        volumeLabel.color = Color.white;
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 1f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.anchoredPosition = new Vector2(0, -15);
        labelRect.sizeDelta = new Vector2(0, 12);

        // Create value text (smaller)
        GameObject valueObj = new GameObject("Volume Value");
        valueObj.transform.SetParent(sliderObj.transform, false);
        volumeValueText = valueObj.AddComponent<Text>();
        volumeValueText.text = "100%";
        volumeValueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        volumeValueText.fontSize = 9;
        volumeValueText.color = Color.white;
        volumeValueText.alignment = TextAnchor.MiddleRight;
        RectTransform valueRect = valueObj.GetComponent<RectTransform>();
        valueRect.anchorMin = new Vector2(0f, 0f);
        valueRect.anchorMax = new Vector2(1f, 0f);
        valueRect.anchoredPosition = new Vector2(0, 3);
        valueRect.sizeDelta = new Vector2(0, 12);
    }

    void CreatePitchSlider()
    {
        GameObject sliderObj = new GameObject("Pitch Slider");
        sliderObj.transform.SetParent(audioUICanvas.transform, false);

        RectTransform rectTransform = sliderObj.AddComponent<RectTransform>();
        // Position on right side, below volume slider, smaller width
        rectTransform.anchorMin = new Vector2(0.85f, 0.55f);
        rectTransform.anchorMax = new Vector2(0.98f, 0.6f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;

        pitchSlider = sliderObj.AddComponent<Slider>();
        pitchSlider.minValue = 0.1f;
        pitchSlider.maxValue = 3f;
        pitchSlider.value = 1f;

        // Create background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform, false);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        pitchSlider.targetGraphic = bgImage;

        // Create fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;

        // Create fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.8f, 0.2f, 0.8f, 1f);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.sizeDelta = Vector2.zero;
        pitchSlider.fillRect = fillRect;

        // Create handle (smaller)
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(sliderObj.transform, false);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = Color.white;
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(12, 12);
        pitchSlider.handleRect = handleRect;

        // Create label (smaller)
        GameObject labelObj = new GameObject("Pitch Label");
        labelObj.transform.SetParent(sliderObj.transform, false);
        pitchLabel = labelObj.AddComponent<Text>();
        pitchLabel.text = "Pitch";
        pitchLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        pitchLabel.fontSize = 10;
        pitchLabel.color = Color.white;
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 1f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.anchoredPosition = new Vector2(0, -15);
        labelRect.sizeDelta = new Vector2(0, 12);

        // Create value text (smaller)
        GameObject valueObj = new GameObject("Pitch Value");
        valueObj.transform.SetParent(sliderObj.transform, false);
        pitchValueText = valueObj.AddComponent<Text>();
        pitchValueText.text = "1.00x";
        pitchValueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        pitchValueText.fontSize = 9;
        pitchValueText.color = Color.white;
        pitchValueText.alignment = TextAnchor.MiddleRight;
        RectTransform valueRect = valueObj.GetComponent<RectTransform>();
        valueRect.anchorMin = new Vector2(0f, 0f);
        valueRect.anchorMax = new Vector2(1f, 0f);
        valueRect.anchoredPosition = new Vector2(0, 3);
        valueRect.sizeDelta = new Vector2(0, 12);
    }

#if UNITY_EDITOR
    [ContextMenu("Create Audio UI")]
    void CreateAudioUIMenu()
    {
        autoCreateUI = true;
        CreateAudioUI();
        ConnectToAudioManager();
        EditorUtility.SetDirty(this);
    }
#endif
}
