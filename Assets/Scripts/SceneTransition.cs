using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    [Header("Scene Transition Settings")]
    [Tooltip("Name of the scene to transition to")]
    public string targetSceneName = "Kitchen_scene";
    
    [Tooltip("Time in seconds before transitioning to the next scene")]
    public float transitionDelay = 45f;
    
    [Tooltip("Transition automatically when scene starts")]
    public bool transitionOnStart = true;
    
    [Header("Timer UI Settings")]
    [Tooltip("Automatically create timer UI if not assigned")]
    public bool autoCreateTimerUI = true;
    
    [Tooltip("Text component to display the countdown timer (leave empty to auto-create)")]
    public Text timerText;
    
    [Tooltip("Canvas to use for timer UI (leave empty to auto-find or create)")]
    public Canvas timerCanvas;
    
    [Tooltip("Timer display format: 0 = Seconds only (45), 1 = MM:SS (00:45), 2 = Minutes and seconds text (0m 45s)")]
    public int timerFormat = 1;
    
    [Tooltip("Position of timer on screen (0-1, where 0.5 is center)")]
    public Vector2 timerPosition = new Vector2(0.5f, 0.95f);
    
    [Tooltip("Font size for timer text")]
    public int fontSize = 36;
    
    [Tooltip("Color of timer text")]
    public Color timerColor = Color.white;
    
    [Header("Debug")]
    [Tooltip("Show debug messages")]
    public bool showDebugMessages = true;
    
    private Coroutine transitionCoroutine;
    private float startTime;
    private float remainingTime;
    private bool isTransitioning = false;
    
    void Start()
    {
        if (showDebugMessages)
        {
            Debug.Log("SceneTransition: Start() called");
        }
        
        // Set up timer UI
        if (autoCreateTimerUI && timerText == null)
        {
            SetupTimerUI();
        }
        
        if (transitionOnStart)
        {
            StartTransition();
        }
        
        // Ensure timer is visible if it exists
        if (timerText != null && !timerText.gameObject.activeSelf)
        {
            timerText.gameObject.SetActive(true);
            if (showDebugMessages)
            {
                Debug.Log("SceneTransition: Timer text was inactive, activating it");
            }
        }
    }
    
    void Update()
    {
        // Update timer display
        if (timerText != null)
        {
            if (isTransitioning)
            {
                UpdateTimerDisplay();
            }
            else if (autoCreateTimerUI)
            {
                // Show initial time even if not transitioning yet
                timerText.text = FormatTime(transitionDelay);
            }
        }
    }
    
    void SetupTimerUI()
    {
        if (showDebugMessages)
        {
            Debug.Log("SceneTransition: Setting up timer UI...");
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
                if (showDebugMessages)
                {
                    Debug.Log("SceneTransition: No ScreenSpaceOverlay Canvas found, creating new one...");
                }
                
                GameObject canvasObj = new GameObject("Timer Canvas");
                timerCanvas = canvasObj.AddComponent<Canvas>();
                timerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                timerCanvas.sortingOrder = 100; // High sorting order to ensure it's on top
                
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            else
            {
                if (showDebugMessages)
                {
                    Debug.Log($"SceneTransition: Found existing ScreenSpaceOverlay Canvas: {timerCanvas.name}");
                }
                
                // Ensure the canvas has a high sorting order
                if (timerCanvas.sortingOrder < 100)
                {
                    timerCanvas.sortingOrder = 100;
                }
            }
        }
        
        // Create timer text if it doesn't exist
        if (timerText == null)
        {
            if (showDebugMessages)
            {
                Debug.Log("SceneTransition: Creating timer text...");
            }
            
            GameObject textObj = new GameObject("Timer Text");
            textObj.transform.SetParent(timerCanvas.transform, false);
            
            timerText = textObj.AddComponent<Text>();
            timerText.text = FormatTime(transitionDelay);
            
            // Try to get the font
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont != null)
            {
                timerText.font = defaultFont;
            }
            else
            {
                // Fallback: try Arial
                timerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            
            timerText.fontSize = fontSize;
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
            
            // Ensure the text object is active and visible
            textObj.SetActive(true);
            timerText.enabled = true;
            
            // Set initial text
            timerText.text = FormatTime(transitionDelay);
            
            // Force update the canvas
            Canvas.ForceUpdateCanvases();
            
            if (showDebugMessages)
            {
                Debug.Log($"SceneTransition: Timer text created at position {timerPosition}, size {rectTransform.sizeDelta}");
                Debug.Log($"SceneTransition: Timer text active: {textObj.activeSelf}, enabled: {timerText.enabled}");
                Debug.Log($"SceneTransition: Timer text: '{timerText.text}', color: {timerText.color}");
                Debug.Log($"SceneTransition: Canvas renderMode: {timerCanvas.renderMode}, sortingOrder: {timerCanvas.sortingOrder}");
            }
        }
        else
        {
            if (showDebugMessages)
            {
                Debug.Log("SceneTransition: Timer text already exists");
            }
        }
    }
    
    void UpdateTimerDisplay()
    {
        if (timerText == null) return;
        
        remainingTime = Mathf.Max(0f, transitionDelay - (Time.time - startTime));
        string timeString = FormatTime(remainingTime);
        
        if (timerText.text != timeString)
        {
            timerText.text = timeString;
        }
        
        // Optional: Change color as time runs out
        if (remainingTime < 10f)
        {
            timerText.color = Color.Lerp(Color.red, timerColor, remainingTime / 10f);
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
    
    /// <summary>
    /// Start the transition countdown
    /// </summary>
    public void StartTransition()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        
        startTime = Time.time;
        isTransitioning = true;
        remainingTime = transitionDelay;
        
        transitionCoroutine = StartCoroutine(TransitionAfterDelay());
    }
    
    /// <summary>
    /// Cancel the transition
    /// </summary>
    public void CancelTransition()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }
        
        isTransitioning = false;
        
        // Hide timer if exists
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }
        
        if (showDebugMessages)
        {
            Debug.Log("SceneTransition: Transition cancelled");
        }
    }
    
    /// <summary>
    /// Transition immediately to the target scene
    /// </summary>
    public void TransitionImmediately()
    {
        if (showDebugMessages)
        {
            Debug.Log($"SceneTransition: Transitioning immediately to {targetSceneName}");
        }
        
        LoadTargetScene();
    }
    
    IEnumerator TransitionAfterDelay()
    {
        if (showDebugMessages)
        {
            Debug.Log($"SceneTransition: Will transition to {targetSceneName} in {transitionDelay} seconds");
        }
        
        yield return new WaitForSeconds(transitionDelay);
        
        if (showDebugMessages)
        {
            Debug.Log($"SceneTransition: Transitioning to {targetSceneName}");
        }
        
        LoadTargetScene();
    }
    
    void LoadTargetScene()
    {
        // Check if scene exists
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("SceneTransition: Target scene name is empty!");
            return;
        }
        
        // Load the scene asynchronously (better for performance)
        SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Single);
    }
    
    /// <summary>
    /// Set the transition delay dynamically
    /// </summary>
    public void SetTransitionDelay(float delay)
    {
        transitionDelay = delay;
        
        // Restart transition if already running
        if (transitionCoroutine != null)
        {
            CancelTransition();
            StartTransition();
        }
    }
    
    /// <summary>
    /// Get remaining time until transition
    /// </summary>
    public float GetRemainingTime()
    {
        if (isTransitioning)
        {
            return Mathf.Max(0f, transitionDelay - (Time.time - startTime));
        }
        return transitionDelay;
    }
    
    /// <summary>
    /// Show or hide the timer UI
    /// </summary>
    public void SetTimerVisible(bool visible)
    {
        if (timerText != null)
        {
            timerText.gameObject.SetActive(visible);
        }
    }
}

