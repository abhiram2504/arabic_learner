using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
#if UNITY_XR_INTERACTION_TOOLKIT
using UnityEngine.XR.Interaction.Toolkit;
#endif

public class HummusBowlChecker : MonoBehaviour
{
    [Header("Required References")]
    public FoodItems foodItemsScript;
    public GameObject xrButton;
    public GameObject hummusBowl;

    [Header("Timer Settings")]
    public bool enableTimer = true;
    public float timeLimit = 300f;

    [Header("Failure Settings")]
    public string failMessage = "Fail! Try again from this scene";
    public bool reloadSceneOnFailure = true;
    public float reloadDelay = 3f;

    [Header("Debug")]
    public bool showDebugMessages = true;

    private Canvas failCanvas;
    private Text failText;
    private Text timerText;
    private Canvas timerCanvas;
    private float timerStartTime;
    private bool timerRunning = false;
    private Coroutine timerCoroutine;

    void Start()
    {
        if (foodItemsScript == null)
            foodItemsScript = FindObjectOfType<FoodItems>();

#if UNITY_XR_INTERACTION_TOOLKIT
        if (xrButton == null)
        {
            XRSimpleInteractable interactable = FindObjectOfType<XRSimpleInteractable>();
            if (interactable != null)
                xrButton = interactable.gameObject;
        }
#endif

        SetupXRButton();

        if (hummusBowl != null)
            hummusBowl.SetActive(false);

        SetupFailCanvas();

        if (enableTimer)
        {
            SetupTimer();
            StartTimer();
        }
    }

    void SetupXRButton()
    {
#if UNITY_XR_INTERACTION_TOOLKIT
        if (xrButton != null)
        {
            XRSimpleInteractable interactable = xrButton.GetComponent<XRSimpleInteractable>();
            if (interactable != null)
                interactable.onSelectEntered.AddListener(OnButtonPressed);
        }
#endif
    }

#if UNITY_XR_INTERACTION_TOOLKIT
    void OnButtonPressed(SelectEnterEventArgs args)
    {
        CheckConditions();
    }
#endif

    void Update()
    {
        if (timerRunning && timerText != null)
            UpdateTimerDisplay();
    }

    public void CheckConditions()
    {
        StopTimer();

        bool success = foodItemsScript != null && foodItemsScript.IsComplete();

        if (success)
            ShowSuccess();
        else
            ShowFailure("Not all ingredients have been added!");
    }

    void ShowSuccess()
    {
        if (failCanvas != null)
            failCanvas.gameObject.SetActive(false);

        if (hummusBowl != null)
            hummusBowl.SetActive(true);
    }

    void ShowFailure(string reason)
    {
        if (hummusBowl != null)
            hummusBowl.SetActive(false);

        if (failCanvas != null)
        {
            failCanvas.gameObject.SetActive(true);
            if (failText != null)
                failText.text = failMessage + "\n\n" + reason;
        }

        if (reloadSceneOnFailure)
            StartCoroutine(ReloadSceneAfterDelay());
    }

    IEnumerator ReloadSceneAfterDelay()
    {
        yield return new WaitForSeconds(reloadDelay);
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }

    // ---------------- VR SAFE UI ----------------

    void SetupFailCanvas()
    {
        GameObject canvasObj = new GameObject("Fail Canvas");
        failCanvas = canvasObj.AddComponent<Canvas>();

        // Find VR camera
        Camera vrCamera = FindVRCamera();
        if (vrCamera == null)
        {
            vrCamera = Camera.main;
        }

        if (vrCamera != null)
        {
            failCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            failCanvas.worldCamera = vrCamera;
            failCanvas.planeDistance = 1.2f; // Closer for fail screen
        }
        else
        {
            failCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        failCanvas.sortingOrder = 200;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject textObj = new GameObject("Fail Text");
        textObj.transform.SetParent(failCanvas.transform, false);

        failText = textObj.AddComponent<Text>();
        failText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ??
                        Resources.GetBuiltinResource<Font>("Arial.ttf");
        failText.fontSize = 48;
        failText.color = Color.red;
        failText.alignment = TextAnchor.MiddleCenter;

        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(800, 200);

        failCanvas.gameObject.SetActive(false);
    }

    void SetupTimer()
    {
        GameObject canvasObj = new GameObject("Timer Canvas");
        timerCanvas = canvasObj.AddComponent<Canvas>();

        // Find VR camera - try multiple methods
        Camera vrCamera = FindVRCamera();
        if (vrCamera == null)
        {
            vrCamera = Camera.main; // Fallback
        }

        if (vrCamera != null)
        {
            timerCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            timerCanvas.worldCamera = vrCamera;
            timerCanvas.planeDistance = 1.5f; // Distance from camera for visibility
            if (showDebugMessages)
            {
                Debug.Log($"HummusBowlChecker: Timer using camera: {vrCamera.name}, distance: {timerCanvas.planeDistance}");
            }
        }
        else
        {
            // Fallback to overlay if no camera found
            timerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            if (showDebugMessages)
            {
                Debug.LogWarning("HummusBowlChecker: No camera found for timer! Using ScreenSpaceOverlay");
            }
        }

        timerCanvas.sortingOrder = 150;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject textObj = new GameObject("Timer Text");
        textObj.transform.SetParent(timerCanvas.transform, false);

        timerText = textObj.AddComponent<Text>();
        timerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (timerText.font == null)
        {
            timerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        // Larger font for VR visibility
        timerText.fontSize = 56; // Increased from 36 for better VR visibility
        timerText.color = Color.white;
        timerText.alignment = TextAnchor.UpperCenter;
        timerText.horizontalOverflow = HorizontalWrapMode.Overflow;
        timerText.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.95f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(600, 150); // Larger size for VR

        textObj.SetActive(true);
        timerText.enabled = true;

        // Force canvas update
        Canvas.ForceUpdateCanvases();

        if (showDebugMessages)
        {
            Debug.Log($"HummusBowlChecker: Timer created - Canvas: {timerCanvas.name}, Camera: {timerCanvas.worldCamera?.name}, Mode: {timerCanvas.renderMode}");
        }
    }

    /// <summary>
    /// Find VR camera (XR Origin or Main Camera with XR components)
    /// </summary>
    Camera FindVRCamera()
    {
        // First try Camera.main (most common in VR setups)
        if (Camera.main != null && Camera.main.gameObject.activeInHierarchy)
        {
            if (showDebugMessages)
            {
                Debug.Log($"HummusBowlChecker: Using Camera.main: {Camera.main.name}");
            }
            return Camera.main;
        }

        // Try to find XR Origin camera or any active camera
        Camera[] allCameras = FindObjectsOfType<Camera>();

        // Priority 1: MainCamera tag
        foreach (Camera cam in allCameras)
        {
            if (cam.CompareTag("MainCamera") && cam.gameObject.activeInHierarchy && cam.enabled)
            {
                if (showDebugMessages)
                {
                    Debug.Log($"HummusBowlChecker: Found MainCamera tagged camera: {cam.name}");
                }
                return cam;
            }
        }

        // Priority 2: Camera with XR/VR in name
        foreach (Camera cam in allCameras)
        {
            if ((cam.name.Contains("XR") || cam.name.Contains("VR") || cam.name.Contains("Camera"))
                && cam.gameObject.activeInHierarchy && cam.enabled)
            {
                if (showDebugMessages)
                {
                    Debug.Log($"HummusBowlChecker: Found VR camera by name: {cam.name}");
                }
                return cam;
            }
        }

        // Priority 3: Any active enabled camera
        foreach (Camera cam in allCameras)
        {
            if (cam.gameObject.activeInHierarchy && cam.enabled)
            {
                if (showDebugMessages)
                {
                    Debug.Log($"HummusBowlChecker: Using first active camera: {cam.name}");
                }
                return cam;
            }
        }

        if (showDebugMessages)
        {
            Debug.LogWarning("HummusBowlChecker: No camera found! Timer may not be visible.");
        }
        return null;
    }

    void StartTimer()
    {
        timerStartTime = Time.time;
        timerRunning = true;
        timerCoroutine = StartCoroutine(TimerCountdown());
    }

    void StopTimer()
    {
        timerRunning = false;
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
    }

    IEnumerator TimerCountdown()
    {
        while (timerRunning)
        {
            float remaining = Mathf.Max(0f, timeLimit - (Time.time - timerStartTime));
            if (remaining <= 0)
            {
                timerRunning = false;
                ShowFailure("Time limit exceeded!");
            }
            yield return null;
        }
    }

    void UpdateTimerDisplay()
    {
        if (timerText == null || !timerRunning) return;

        float remaining = Mathf.Max(0f, timeLimit - (Time.time - timerStartTime));
        int minutes = Mathf.FloorToInt(remaining / 60);
        int seconds = Mathf.FloorToInt(remaining % 60);
        timerText.text = $"{minutes:D2}:{seconds:D2}";

        // Ensure timer is visible
        if (!timerText.gameObject.activeSelf)
        {
            timerText.gameObject.SetActive(true);
        }
        if (timerCanvas != null && !timerCanvas.gameObject.activeSelf)
        {
            timerCanvas.gameObject.SetActive(true);
        }

        // Change color as time runs out
        if (remaining < 60f)
        {
            timerText.color = Color.Lerp(Color.red, Color.white, remaining / 60f);
        }
        else
        {
            timerText.color = Color.white;
        }
    }
}
