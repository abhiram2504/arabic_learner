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
    [Tooltip("Drag your blender GameObject that has the FoodItems script")]
    public FoodItems foodItemsScript;

    [Tooltip("Drag your button GameObject that has XR Simple Interactable component")]
    public GameObject xrButton;

    [Tooltip("Drag your hummus bowl GameObject (will be shown on success)")]
    public GameObject hummusBowl;

    [Header("Timer Settings")]
    [Tooltip("Enable 5-minute countdown timer")]
    public bool enableTimer = true;

    [Tooltip("Time limit in seconds (5 minutes = 300)")]
    public float timeLimit = 300f;

    [Header("Failure Settings")]
    [Tooltip("Message to show on failure screen")]
    public string failMessage = "Fail! Try again from this scene";

    [Tooltip("Reload scene after showing fail message")]
    public bool reloadSceneOnFailure = true;

    [Tooltip("Seconds to wait before reloading scene")]
    public float reloadDelay = 3f;

    [Header("Debug")]
    [Tooltip("Show debug messages in console")]
    public bool showDebugMessages = true;

    // Private variables
    private Canvas failCanvas;
    private Text failText;
    private Text timerText;
    private Canvas timerCanvas;
    private float timerStartTime;
    private bool timerRunning = false;
    private Coroutine timerCoroutine;

    void Start()
    {
        // Auto-find FoodItems if not assigned
        if (foodItemsScript == null)
        {
            foodItemsScript = FindObjectOfType<FoodItems>();
            if (foodItemsScript != null && showDebugMessages)
            {
                Debug.Log("HummusBowlChecker: Auto-found FoodItems script");
            }
        }

        // Auto-find XR button if not assigned
        if (xrButton == null)
        {
#if UNITY_XR_INTERACTION_TOOLKIT
            XRSimpleInteractable[] interactables = FindObjectsOfType<XRSimpleInteractable>();
            if (interactables.Length > 0)
            {
                xrButton = interactables[0].gameObject;
                if (showDebugMessages)
                {
                    Debug.Log($"HummusBowlChecker: Auto-found XR button: {xrButton.name}");
                }
            }
#endif
        }

        // Set up XR button
        SetupXRButton();

        // Hide hummus bowl at start
        if (hummusBowl != null)
        {
            hummusBowl.SetActive(false);
        }
        else if (showDebugMessages)
        {
            Debug.LogWarning("HummusBowlChecker: Hummus bowl not assigned!");
        }

        // Set up fail canvas
        SetupFailCanvas();

        // Set up timer
        if (enableTimer)
        {
            SetupTimer();
            StartTimer();
        }

        // Warn if required references are missing
        if (foodItemsScript == null && showDebugMessages)
        {
            Debug.LogWarning("HummusBowlChecker: FoodItems script not found! Assign it in the Inspector.");
        }
        if (xrButton == null && showDebugMessages)
        {
            Debug.LogWarning("HummusBowlChecker: XR button not found! Assign it in the Inspector.");
        }
    }

    void SetupXRButton()
    {
#if UNITY_XR_INTERACTION_TOOLKIT
        if (xrButton != null)
        {
            XRSimpleInteractable interactable = xrButton.GetComponent<XRSimpleInteractable>();
            if (interactable != null)
            {
                interactable.onSelectEntered.AddListener(OnButtonPressed);
                if (showDebugMessages)
                {
                    Debug.Log($"HummusBowlChecker: XR button connected: {xrButton.name}");
                }
            }
            else if (showDebugMessages)
            {
                Debug.LogWarning($"HummusBowlChecker: {xrButton.name} does not have XRSimpleInteractable component!");
            }
        }
#endif
    }

    void OnDestroy()
    {
#if UNITY_XR_INTERACTION_TOOLKIT
        if (xrButton != null)
        {
            XRSimpleInteractable interactable = xrButton.GetComponent<XRSimpleInteractable>();
            if (interactable != null)
            {
                interactable.onSelectEntered.RemoveListener(OnButtonPressed);
            }
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
        // Update timer display
        if (timerRunning && timerText != null)
        {
            UpdateTimerDisplay();
        }
    }

    /// <summary>
    /// Check if FoodItems is complete and show result
    /// If ingredients are not all added, shows fail screen and reloads scene
    /// </summary>
    public void CheckConditions()
    {
        if (showDebugMessages)
        {
            Debug.Log("HummusBowlChecker: Button pressed - Checking if all ingredients are added...");
        }

        // Stop timer
        StopTimer();

        // Check if FoodItems is complete
        bool success = false;
        string failureReason = "";

        if (foodItemsScript == null)
        {
            failureReason = "FoodItems script not found!";
            if (showDebugMessages)
            {
                Debug.LogWarning("HummusBowlChecker: FoodItems script is null! Cannot check conditions.");
            }
        }
        else
        {
            success = foodItemsScript.IsComplete();
            if (!success)
            {
                failureReason = "Not all ingredients have been added to the blender!";
                if (showDebugMessages)
                {
                    Debug.LogWarning("HummusBowlChecker: Ingredients not complete - some ingredients are missing!");
                }
            }
        }

        // Show result - ALWAYS show either success or failure
        if (success)
        {
            ShowSuccess();
        }
        else
        {
            ShowFailure(failureReason);
        }
    }

    void ShowSuccess()
    {
        if (showDebugMessages)
        {
            Debug.Log("HummusBowlChecker: SUCCESS! Showing hummus bowl.");
        }

        // Hide fail canvas
        if (failCanvas != null)
        {
            failCanvas.gameObject.SetActive(false);
        }

        // Show hummus bowl
        if (hummusBowl != null)
        {
            hummusBowl.SetActive(true);
        }
    }

    void ShowFailure(string reason = "")
    {
        if (showDebugMessages)
        {
            Debug.Log($"HummusBowlChecker: FAILURE! Reason: {reason}");
        }

        // Hide hummus bowl
        if (hummusBowl != null)
        {
            hummusBowl.SetActive(false);
        }

        // Show fail canvas
        if (failCanvas != null)
        {
            failCanvas.gameObject.SetActive(true);

            // Update fail text with reason if provided
            if (failText != null)
            {
                string message = failMessage;
                if (!string.IsNullOrEmpty(reason))
                {
                    message += $"\n\n{reason}";
                }
                failText.text = message;
            }
        }

        // ALWAYS reload scene on failure (restart from beginning)
        StartCoroutine(ReloadSceneAfterDelay());
    }

    IEnumerator ReloadSceneAfterDelay()
    {
        yield return new WaitForSeconds(reloadDelay);

        if (showDebugMessages)
        {
            Debug.Log("HummusBowlChecker: Reloading scene...");
        }

        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    void SetupFailCanvas()
    {
        // Find or create canvas
        Canvas existingCanvas = FindObjectOfType<Canvas>();
        if (existingCanvas != null && existingCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            failCanvas = existingCanvas;
        }
        else
        {
            GameObject canvasObj = new GameObject("Fail Canvas");
            failCanvas = canvasObj.AddComponent<Canvas>();
            failCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            failCanvas.sortingOrder = 200;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create fail text
        GameObject textObj = new GameObject("Fail Text");
        textObj.transform.SetParent(failCanvas.transform, false);

        failText = textObj.AddComponent<Text>();
        failText.text = failMessage;
        failText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (failText.font == null)
        {
            failText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        failText.fontSize = 48;
        failText.color = Color.red;
        failText.alignment = TextAnchor.MiddleCenter;
        failText.horizontalOverflow = HorizontalWrapMode.Overflow;
        failText.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(800, 200);

        // Hide canvas initially
        failCanvas.gameObject.SetActive(false);
    }

    void SetupTimer()
    {
        // Find or create canvas for timer
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                timerCanvas = canvas;
                break;
            }
        }

        if (timerCanvas == null)
        {
            GameObject canvasObj = new GameObject("Timer Canvas");
            timerCanvas = canvasObj.AddComponent<Canvas>();
            timerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            timerCanvas.sortingOrder = 150;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create timer text
        GameObject textObj = new GameObject("Timer Text");
        textObj.transform.SetParent(timerCanvas.transform, false);

        timerText = textObj.AddComponent<Text>();
        timerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (timerText.font == null)
        {
            timerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        timerText.fontSize = 36;
        timerText.color = Color.white;
        timerText.alignment = TextAnchor.UpperCenter;
        timerText.horizontalOverflow = HorizontalWrapMode.Overflow;
        timerText.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.95f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.95f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(400, 100);

        textObj.SetActive(true);
    }

    void StartTimer()
    {
        if (!enableTimer) return;

        timerStartTime = Time.time;
        timerRunning = true;

        if (showDebugMessages)
        {
            Debug.Log($"HummusBowlChecker: Timer started - {timeLimit} seconds");
        }

        timerCoroutine = StartCoroutine(TimerCountdown());
    }

    void StopTimer()
    {
        timerRunning = false;
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    IEnumerator TimerCountdown()
    {
        while (timerRunning)
        {
            float elapsed = Time.time - timerStartTime;
            float remaining = Mathf.Max(0f, timeLimit - elapsed);

            if (remaining <= 0f)
            {
                timerRunning = false;
                OnTimerExpired();
                yield break;
            }

            yield return null;
        }
    }

    void OnTimerExpired()
    {
        if (showDebugMessages)
        {
            Debug.Log("HummusBowlChecker: Timer expired!");
        }

        StopTimer();
        ShowFailure("Time limit exceeded!");
    }

    void UpdateTimerDisplay()
    {
        if (timerText == null || !timerRunning) return;

        float elapsed = Time.time - timerStartTime;
        float remaining = Mathf.Max(0f, timeLimit - elapsed);

        // Format as MM:SS
        int minutes = Mathf.FloorToInt(remaining / 60);
        int seconds = Mathf.FloorToInt(remaining % 60);
        timerText.text = $"{minutes:D2}:{seconds:D2}";

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
