using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
#if UNITY_XR_INTERACTION_TOOLKIT
using UnityEngine.XR.Interaction.Toolkit;
#endif

[System.Serializable]
public class ConditionCheck
{
    [Tooltip("Description of this condition (for debugging)")]
    public string conditionName = "Condition";

    [Tooltip("Type of check to perform")]
    public ConditionType conditionType = ConditionType.GameObjectActive;

    [Tooltip("GameObject to check (for GameObject checks)")]
    public GameObject targetObject;

    [Tooltip("Script component to check (for Script checks)")]
    public MonoBehaviour targetScript;

    [Tooltip("Method name to call on script (must return bool)")]
    public string methodName = "";

    [Tooltip("Expected state (for GameObject checks)")]
    public bool expectedState = true;

    [Tooltip("Tag to check for (for Tag checks)")]
    public string tagToCheck = "";

    [Tooltip("Number of objects with tag that should exist")]
    public int expectedTagCount = 1;
}

public enum ConditionType
{
    GameObjectActive,      // Check if GameObject is active/inactive
    GameObjectInactive,    // Check if GameObject is inactive
    ScriptMethod,          // Call a method on a script that returns bool
    TagExists,             // Check if objects with a tag exist
    TagCount,              // Check if specific number of objects with tag exist
    FoodItemsComplete,     // Check if FoodItems script has completed all items
    Custom                 // Use custom check method
}

public class HummusBowlChecker : MonoBehaviour
{
    [Header("FoodItems Integration")]
    [Tooltip("FoodItems script to monitor (used for condition checking)")]
    public FoodItems foodItemsScript;

    [Tooltip("Automatically check conditions when FoodItems completes all items (disabled by default - use button instead)")]
    public bool autoCheckOnFoodItemsComplete = false;

    [Header("Button Settings")]
    [Tooltip("UI Button to trigger check (optional - use XR Interactable instead for VR)")]
    public Button checkButton;

    [Tooltip("XR Simple Interactable GameObject to trigger check (for VR/XR interactions)")]
    public GameObject xrInteractableButton;

    [Tooltip("Key to press to trigger check (leave empty to disable keyboard input)")]
    public KeyCode triggerKey = KeyCode.None;

    [Tooltip("Auto-find button in scene if not assigned")]
    public bool autoFindButton = false;

    [Tooltip("Auto-find XR Simple Interactable in scene if not assigned")]
    public bool autoFindXRInteractable = false;

    [Header("Conditions to Check")]
    [Tooltip("List of conditions that must all pass for success")]
    public List<ConditionCheck> conditions = new List<ConditionCheck>();

    [Header("Success Settings")]
    [Tooltip("Hummus bowl GameObject to show on success")]
    public GameObject hummusBowl;

    [Tooltip("Hide hummus bowl at start")]
    public bool hideBowlAtStart = true;

    [Tooltip("Sound to play on success (optional)")]
    public AudioClip successSound;

    [Header("Failure Settings")]
    [Tooltip("Canvas to show on failure (will be created if not assigned)")]
    public Canvas failCanvas;

    [Tooltip("Text to display on failure screen")]
    public string failMessage = "Fail! Try again from this scene";

    [Tooltip("Automatically create fail canvas if not assigned")]
    public bool autoCreateFailCanvas = true;

    [Tooltip("Reload scene on failure (after showing fail message)")]
    public bool reloadSceneOnFailure = true;

    [Tooltip("Delay before reloading scene (seconds)")]
    public float reloadDelay = 3f;

    [Tooltip("Sound to play on failure (optional)")]
    public AudioClip failSound;

    [Header("Timer Settings")]
    [Tooltip("Enable timer countdown")]
    public bool enableTimer = true;

    [Tooltip("Time limit in seconds (5 minutes = 300)")]
    public float timeLimit = 300f;

    [Tooltip("Automatically create timer UI if not assigned")]
    public bool autoCreateTimerUI = true;

    [Tooltip("Text component to display the timer (leave empty to auto-create)")]
    public Text timerText;

    [Tooltip("Canvas to use for timer UI (leave empty to auto-find or create)")]
    public Canvas timerCanvas;

    [Tooltip("Timer display format: 0 = Seconds only, 1 = MM:SS, 2 = Minutes and seconds text")]
    public int timerFormat = 1;

    [Tooltip("Position of timer on screen (0-1, where 0.5 is center)")]
    public Vector2 timerPosition = new Vector2(0.5f, 0.95f);

    [Tooltip("Font size for timer text")]
    public int timerFontSize = 36;

    [Tooltip("Color of timer text")]
    public Color timerColor = Color.white;

    [Tooltip("Show fail canvas when timer runs out")]
    public bool failOnTimerExpire = true;

    [Header("Debug")]
    [Tooltip("Show debug messages")]
    public bool showDebugMessages = true;

    private Text failText;
    private bool hasChecked = false;
    private float timerStartTime;
    private bool timerRunning = false;
    private Coroutine timerCoroutine;

    void Start()
    {
        // Set up FoodItems integration
        if (foodItemsScript == null)
        {
            foodItemsScript = FindObjectOfType<FoodItems>();
        }

        if (foodItemsScript != null && autoCheckOnFoodItemsComplete)
        {
            // Subscribe to the FoodItems completion event
            foodItemsScript.onAllItemsCompleted.AddListener(OnFoodItemsCompleted);

            if (showDebugMessages)
            {
                Debug.Log("HummusBowlChecker: Connected to FoodItems script - will auto-check when items complete");
            }
        }
        else if (foodItemsScript == null && showDebugMessages)
        {
            Debug.LogWarning("HummusBowlChecker: No FoodItems script found. Auto-check on completion disabled.");
        }

        // Set up UI button
        if (checkButton == null && autoFindButton)
        {
            checkButton = FindObjectOfType<Button>();
            if (checkButton != null && showDebugMessages)
            {
                Debug.Log($"HummusBowlChecker: Auto-found UI button: {checkButton.name}");
            }
        }

        if (checkButton != null)
        {
            checkButton.onClick.AddListener(OnButtonPressed);
            if (showDebugMessages)
            {
                Debug.Log($"HummusBowlChecker: UI Button connected: {checkButton.name}");
            }
        }

        // Set up XR Interactable
        SetupXRInteractable();

        // Warn if no button/interactable found
        if (checkButton == null && xrInteractableButton == null && showDebugMessages)
        {
            Debug.LogWarning("HummusBowlChecker: No button or XR interactable assigned! Assign one or enable auto-find options.");
        }

        // Hide hummus bowl at start
        if (hummusBowl != null && hideBowlAtStart)
        {
            hummusBowl.SetActive(false);
        }

        // Set up fail canvas
        if (autoCreateFailCanvas && failCanvas == null)
        {
            SetupFailCanvas();
        }
        else if (failCanvas != null)
        {
            failCanvas.gameObject.SetActive(false);
        }

        // Set up timer
        if (enableTimer)
        {
            if (autoCreateTimerUI && timerText == null)
            {
                SetupTimerUI();
            }
            StartTimer();
        }
    }

    void SetupXRInteractable()
    {
#if UNITY_XR_INTERACTION_TOOLKIT
        // Auto-find XR Simple Interactable if enabled
        if (xrInteractableButton == null && autoFindXRInteractable)
        {
            XRSimpleInteractable[] interactables = FindObjectsOfType<XRSimpleInteractable>();
            if (interactables.Length > 0)
            {
                xrInteractableButton = interactables[0].gameObject;
                if (showDebugMessages)
                {
                    Debug.Log($"HummusBowlChecker: Auto-found XR Simple Interactable: {xrInteractableButton.name}");
                }
            }
        }
        
        // Set up XR Interactable if assigned
        if (xrInteractableButton != null)
        {
            XRSimpleInteractable interactable = xrInteractableButton.GetComponent<XRSimpleInteractable>();
            if (interactable != null)
            {
                // Subscribe to select entered event (when button is pressed/selected)
                interactable.onSelectEntered.AddListener(OnXRButtonPressed);
                
                if (showDebugMessages)
                {
                    Debug.Log($"HummusBowlChecker: XR Simple Interactable connected: {xrInteractableButton.name}");
                }
            }
            else
            {
                if (showDebugMessages)
                {
                    Debug.LogWarning($"HummusBowlChecker: GameObject '{xrInteractableButton.name}' does not have XRSimpleInteractable component!");
                }
            }
        }
#else
        if (xrInteractableButton != null && showDebugMessages)
        {
            Debug.LogWarning("HummusBowlChecker: XR Interaction Toolkit not available. Install XR Interaction Toolkit package to use XR interactables.");
        }
#endif
    }

    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (foodItemsScript != null && autoCheckOnFoodItemsComplete)
        {
            foodItemsScript.onAllItemsCompleted.RemoveListener(OnFoodItemsCompleted);
        }

#if UNITY_XR_INTERACTION_TOOLKIT
        if (xrInteractableButton != null)
        {
            XRSimpleInteractable interactable = xrInteractableButton.GetComponent<XRSimpleInteractable>();
            if (interactable != null)
            {
                interactable.onSelectEntered.RemoveListener(OnXRButtonPressed);
            }
        }
#endif
    }

#if UNITY_XR_INTERACTION_TOOLKIT
    void OnXRButtonPressed(SelectEnterEventArgs args)
    {
        if (showDebugMessages)
        {
            Debug.Log("HummusBowlChecker: XR Button pressed!");
        }
        CheckConditions();
    }
#endif

    void OnFoodItemsCompleted()
    {
        // Only auto-check if enabled
        if (!autoCheckOnFoodItemsComplete)
        {
            if (showDebugMessages)
            {
                Debug.Log("HummusBowlChecker: FoodItems completed, but auto-check is disabled. Waiting for button press.");
            }
            return;
        }

        if (showDebugMessages)
        {
            Debug.Log("HummusBowlChecker: FoodItems completed! Checking conditions...");
        }

        // Automatically check conditions when FoodItems completes
        CheckConditions();
    }

    void Update()
    {
        // Check for keyboard input
        if (triggerKey != KeyCode.None && Input.GetKeyDown(triggerKey))
        {
            CheckConditions();
        }

        // Update timer display
        if (timerRunning && timerText != null && enableTimer)
        {
            UpdateTimerDisplay();
        }
    }

    void OnButtonPressed()
    {
        CheckConditions();
    }

    /// <summary>
    /// Check all conditions and show success or failure accordingly
    /// </summary>
    public void CheckConditions()
    {
        // Allow checking multiple times if needed (removed the hasChecked check)
        // This allows it to work with FoodItems completion which might happen multiple times

        if (showDebugMessages)
        {
            Debug.Log("HummusBowlChecker: Checking conditions...");
        }

        bool allConditionsMet = true;
        List<string> failedConditions = new List<string>();

        // If no conditions are set, assume success (all items disappeared = success)
        if (conditions.Count == 0)
        {
            if (showDebugMessages)
            {
                Debug.Log("HummusBowlChecker: No conditions set - assuming success since FoodItems completed");
            }
            ShowSuccess();
            return;
        }

        // Check each condition
        foreach (ConditionCheck condition in conditions)
        {
            bool conditionMet = EvaluateCondition(condition);

            if (!conditionMet)
            {
                allConditionsMet = false;
                failedConditions.Add(condition.conditionName);

                if (showDebugMessages)
                {
                    Debug.LogWarning($"HummusBowlChecker: Condition failed: {condition.conditionName}");
                }
            }
            else
            {
                if (showDebugMessages)
                {
                    Debug.Log($"HummusBowlChecker: Condition passed: {condition.conditionName}");
                }
            }
        }

        // Stop timer when checking (success or failure)
        StopTimer();

        // Show result
        if (allConditionsMet)
        {
            ShowSuccess();
        }
        else
        {
            ShowFailure(failedConditions);
        }

        hasChecked = true;
    }

    bool EvaluateCondition(ConditionCheck condition)
    {
        switch (condition.conditionType)
        {
            case ConditionType.GameObjectActive:
                if (condition.targetObject == null)
                {
                    if (showDebugMessages)
                        Debug.LogWarning($"HummusBowlChecker: Target object is null for condition: {condition.conditionName}");
                    return false;
                }
                return condition.targetObject.activeSelf == condition.expectedState;

            case ConditionType.GameObjectInactive:
                if (condition.targetObject == null)
                {
                    if (showDebugMessages)
                        Debug.LogWarning($"HummusBowlChecker: Target object is null for condition: {condition.conditionName}");
                    return false;
                }
                return !condition.targetObject.activeSelf;

            case ConditionType.ScriptMethod:
                if (condition.targetScript == null || string.IsNullOrEmpty(condition.methodName))
                {
                    if (showDebugMessages)
                        Debug.LogWarning($"HummusBowlChecker: Script or method name is null for condition: {condition.conditionName}");
                    return false;
                }

                // Use reflection to call the method
                var method = condition.targetScript.GetType().GetMethod(condition.methodName);
                if (method != null && method.ReturnType == typeof(bool))
                {
                    return (bool)method.Invoke(condition.targetScript, null);
                }
                else
                {
                    if (showDebugMessages)
                        Debug.LogWarning($"HummusBowlChecker: Method '{condition.methodName}' not found or doesn't return bool");
                    return false;
                }

            case ConditionType.TagExists:
                GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(condition.tagToCheck);
                return objectsWithTag.Length > 0;

            case ConditionType.TagCount:
                GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(condition.tagToCheck);
                return taggedObjects.Length == condition.expectedTagCount;

            case ConditionType.FoodItemsComplete:
                FoodItems foodItems = condition.targetScript as FoodItems;
                if (foodItems == null && foodItemsScript != null)
                {
                    foodItems = foodItemsScript;
                }
                if (foodItems == null)
                {
                    foodItems = FindObjectOfType<FoodItems>();
                }
                if (foodItems == null)
                {
                    if (showDebugMessages)
                        Debug.LogWarning($"HummusBowlChecker: FoodItems script not found for condition: {condition.conditionName}");
                    return false;
                }
                // Use the public IsComplete() method
                return foodItems.IsComplete();

            default:
                return false;
        }
    }

    void ShowSuccess()
    {
        if (showDebugMessages)
        {
            Debug.Log("HummusBowlChecker: All conditions met! Showing hummus bowl.");
        }

        // Show hummus bowl
        if (hummusBowl != null)
        {
            hummusBowl.SetActive(true);
        }

        // Play success sound
        if (successSound != null)
        {
            AudioSource.PlayClipAtPoint(successSound, transform.position);
        }
    }

    void ShowFailure(List<string> failedConditions)
    {
        if (showDebugMessages)
        {
            Debug.Log($"HummusBowlChecker: Conditions failed! Failed conditions: {string.Join(", ", failedConditions)}");
        }

        // Show fail canvas
        if (failCanvas != null)
        {
            failCanvas.gameObject.SetActive(true);

            // Update fail text
            if (failText != null)
            {
                string message = failMessage;
                if (showDebugMessages && failedConditions.Count > 0)
                {
                    message += $"\n\nFailed: {string.Join(", ", failedConditions)}";
                }
                failText.text = message;
            }
        }

        // Play fail sound
        if (failSound != null)
        {
            AudioSource.PlayClipAtPoint(failSound, transform.position);
        }

        // Reload scene if enabled
        if (reloadSceneOnFailure)
        {
            StartCoroutine(ReloadSceneAfterDelay());
        }
    }

    System.Collections.IEnumerator ReloadSceneAfterDelay()
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
            failCanvas.sortingOrder = 200; // Very high to be on top

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

        // Position text in center of screen
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(800, 200);

        // Hide canvas initially
        failCanvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// Reset the checker (allows checking again)
    /// </summary>
    public void ResetChecker()
    {
        hasChecked = false;

        if (hummusBowl != null)
        {
            hummusBowl.SetActive(false);
        }

        if (failCanvas != null)
        {
            failCanvas.gameObject.SetActive(false);
        }

        // Restart timer if enabled
        if (enableTimer)
        {
            StopTimer();
            StartTimer();
        }
    }

    /// <summary>
    /// Manually trigger success (for testing)
    /// </summary>
    public void ForceSuccess()
    {
        ShowSuccess();
    }

    /// <summary>
    /// Manually trigger failure (for testing)
    /// </summary>
    public void ForceFailure()
    {
        ShowFailure(new List<string> { "Manual failure trigger" });
    }

    // ========== TIMER METHODS ==========

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

    System.Collections.IEnumerator TimerCountdown()
    {
        while (timerRunning)
        {
            float elapsed = Time.time - timerStartTime;
            float remaining = Mathf.Max(0f, timeLimit - elapsed);

            if (remaining <= 0f)
            {
                // Timer expired
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
            Debug.Log("HummusBowlChecker: Timer expired! Showing fail canvas.");
        }

        StopTimer();

        if (failOnTimerExpire)
        {
            ShowFailure(new List<string> { "Time limit exceeded" });
        }
    }

    void UpdateTimerDisplay()
    {
        if (timerText == null || !timerRunning) return;

        float elapsed = Time.time - timerStartTime;
        float remaining = Mathf.Max(0f, timeLimit - elapsed);

        timerText.text = FormatTime(remaining);

        // Change color as time runs out
        if (remaining < 60f) // Less than 1 minute
        {
            timerText.color = Color.Lerp(Color.red, timerColor, remaining / 60f);
        }
        else
        {
            timerText.color = timerColor;
        }

        // Ensure text is visible
        if (!timerText.gameObject.activeSelf)
        {
            timerText.gameObject.SetActive(true);
        }
    }

    string FormatTime(float time)
    {
        int totalSeconds = Mathf.CeilToInt(time);

        switch (timerFormat)
        {
            case 0: // Seconds only
                return $"{totalSeconds}";

            case 1: // MM:SS
                int minutes = totalSeconds / 60;
                int seconds = totalSeconds % 60;
                return $"{minutes:D2}:{seconds:D2}";

            case 2: // Minutes and seconds text
                int mins = totalSeconds / 60;
                int secs = totalSeconds % 60;
                if (mins > 0)
                {
                    return $"{mins}m {secs}s";
                }
                else
                {
                    return $"{secs}s";
                }

            default:
                return $"{totalSeconds}";
        }
    }

    void SetupTimerUI()
    {
        if (showDebugMessages)
        {
            Debug.Log("HummusBowlChecker: Setting up timer UI...");
        }

        // Find or create canvas
        if (timerCanvas == null)
        {
            // Look for a ScreenSpaceOverlay canvas first
            Canvas[] allCanvases = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in allCanvases)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    timerCanvas = canvas;
                    break;
                }
            }

            // If no ScreenSpaceOverlay canvas found, create one
            if (timerCanvas == null)
            {
                GameObject canvasObj = new GameObject("Timer Canvas");
                timerCanvas = canvasObj.AddComponent<Canvas>();
                timerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                timerCanvas.sortingOrder = 150; // High sorting order

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                canvasObj.AddComponent<GraphicRaycaster>();
            }
        }

        // Create timer text if it doesn't exist
        if (timerText == null)
        {
            GameObject textObj = new GameObject("Timer Text");
            textObj.transform.SetParent(timerCanvas.transform, false);

            timerText = textObj.AddComponent<Text>();
            timerText.text = FormatTime(timeLimit);

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont != null)
            {
                timerText.font = defaultFont;
            }
            else
            {
                timerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            timerText.fontSize = timerFontSize;
            timerText.color = timerColor;
            timerText.alignment = TextAnchor.UpperCenter;
            timerText.horizontalOverflow = HorizontalWrapMode.Overflow;
            timerText.verticalOverflow = VerticalWrapMode.Overflow;

            // Set position using RectTransform
            RectTransform rectTransform = textObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = timerPosition;
            rectTransform.anchorMax = timerPosition;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(400, 100);

            // Ensure the text object is active
            textObj.SetActive(true);
            timerText.enabled = true;

            // Force update the canvas
            Canvas.ForceUpdateCanvases();

            if (showDebugMessages)
            {
                Debug.Log($"HummusBowlChecker: Timer text created at position {timerPosition}");
            }
        }
    }
}
