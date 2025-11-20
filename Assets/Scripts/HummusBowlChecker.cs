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
        failCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        failCanvas.worldCamera = Camera.main;
        failCanvas.planeDistance = 1f;
        failCanvas.sortingOrder = 200;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
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
        timerCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        timerCanvas.worldCamera = Camera.main;
        timerCanvas.planeDistance = 1f;
        timerCanvas.sortingOrder = 150;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject textObj = new GameObject("Timer Text");
        textObj.transform.SetParent(timerCanvas.transform, false);

        timerText = textObj.AddComponent<Text>();
        timerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ??
                         Resources.GetBuiltinResource<Font>("Arial.ttf");
        timerText.fontSize = 36;
        timerText.color = Color.white;
        timerText.alignment = TextAnchor.UpperCenter;

        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.95f);
        rt.sizeDelta = new Vector2(400, 100);
    }
    
    /// <summary>
    /// Find VR camera (XR Origin or Main Camera with XR components)
    /// </summary>
    Camera FindVRCamera()
    {
        // Try to find XR Origin camera
        Camera[] allCameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in allCameras)
        {
            // Check if it's tagged as MainCamera (common for VR)
            if (cam.CompareTag("MainCamera"))
            {
                return cam;
            }
            // Check if camera name suggests VR
            if (cam.name.Contains("XR") || cam.name.Contains("VR") || cam.name.Contains("Camera"))
            {
                return cam;
            }
        }
        
        // Fallback: try Camera.main
        if (Camera.main != null)
        {
            return Camera.main;
        }
        
        // Last resort: find any active camera
        foreach (Camera cam in allCameras)
        {
            if (cam.gameObject.activeInHierarchy && cam.enabled)
            {
                return cam;
            }
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
        float remaining = Mathf.Max(0f, timeLimit - (Time.time - timerStartTime));
        int minutes = Mathf.FloorToInt(remaining / 60);
        int seconds = Mathf.FloorToInt(remaining % 60);
        timerText.text = $"{minutes:D2}:{seconds:D2}";
        
    }
}
