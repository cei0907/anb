using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-1000)]
public sealed class GlobalUIManager : MonoBehaviour
{
    private const string InstanceName = "Global UI Manager";

    [SerializeField] private Key inventoryKey = Key.I;
    [SerializeField] private Key characterKey = Key.C;
    [SerializeField] private bool closeOtherWindowsOnOpen = true;
    [SerializeField] private List<string> disabledSceneNames = new();

    private static GlobalUIManager instance;

    private Canvas canvas;
    private GameObject inventoryWindow;
    private GameObject characterWindow;
    private Text sceneStatusLabel;
    private bool uiAllowedInCurrentScene = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateRuntimeInstance()
    {
        if (instance != null)
        {
            return;
        }

        var managerObject = new GameObject(InstanceName);
        DontDestroyOnLoad(managerObject);
        managerObject.AddComponent<GlobalUIManager>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureEventSystem();
        BuildUI();
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
        RefreshSceneAccess(SceneManager.GetActiveScene());
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            instance = null;
        }
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null || !uiAllowedInCurrentScene)
        {
            return;
        }

        if (keyboard[inventoryKey].wasPressedThisFrame)
        {
            ToggleInventory();
        }

        if (keyboard[characterKey].wasPressedThisFrame)
        {
            ToggleCharacter();
        }
    }

    public void ToggleInventory()
    {
        SetWindowVisible(inventoryWindow, !inventoryWindow.activeSelf);
    }

    public void ToggleCharacter()
    {
        SetWindowVisible(characterWindow, !characterWindow.activeSelf);
    }

    public void CloseAll()
    {
        inventoryWindow.SetActive(false);
        characterWindow.SetActive(false);
    }

    private void SetWindowVisible(GameObject window, bool visible)
    {
        if (visible && closeOtherWindowsOnOpen)
        {
            CloseAll();
        }

        window.SetActive(visible);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureEventSystem();
        RefreshSceneAccess(SceneManager.GetActiveScene());
    }

    private void OnActiveSceneChanged(Scene previousScene, Scene newScene)
    {
        RefreshSceneAccess(newScene);
    }

    private void RefreshSceneAccess(Scene scene)
    {
        uiAllowedInCurrentScene = !disabledSceneNames.Contains(scene.name);

        var sceneAccess = FindObjectsByType<SceneUIAccess>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var access in sceneAccess)
        {
            if (access.gameObject.scene == scene)
            {
                uiAllowedInCurrentScene = access.AllowGlobalWindows;
                break;
            }
        }

        if (!uiAllowedInCurrentScene)
        {
            CloseAll();
        }

        UpdateSceneStatusLabel(scene.name);
    }

    private void BuildUI()
    {
        var canvasObject = new GameObject("Global UI Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        inventoryWindow = CreateWindow("Inventory", new Vector2(-360f, 0f), new Vector2(520f, 640f));
        CreateLabel(inventoryWindow.transform, "Inventory", 28, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(32f, -56f), new Vector2(-32f, -16f));
        CreateLabel(inventoryWindow.transform, "No items yet.", 20, TextAnchor.UpperLeft, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(32f, 32f), new Vector2(-32f, -108f));

        characterWindow = CreateWindow("Character", new Vector2(360f, 0f), new Vector2(520f, 640f));
        CreateLabel(characterWindow.transform, "Character", 28, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(32f, -56f), new Vector2(-32f, -16f));
        CreateLabel(characterWindow.transform, "Level 1\nHP 100 / 100\nAttack 10\nDefense 5", 20, TextAnchor.UpperLeft, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(32f, 32f), new Vector2(-32f, -108f));

        sceneStatusLabel = CreateLabel(canvas.transform, string.Empty, 18, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -112f), new Vector2(620f, -24f));

        CloseAll();
    }

    private GameObject CreateWindow(string title, Vector2 anchoredPosition, Vector2 size)
    {
        var window = new GameObject(title + " Window", typeof(Image));
        window.transform.SetParent(canvas.transform, false);

        var rect = window.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        var image = window.GetComponent<Image>();
        image.color = new Color(0.08f, 0.09f, 0.11f, 0.94f);

        return window;
    }

    private static Text CreateLabel(Transform parent, string text, int fontSize, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var labelName = string.IsNullOrEmpty(text) ? "Scene Status Label" : text.Split('\n')[0] + " Label";
        var labelObject = new GameObject(labelName, typeof(Text));
        labelObject.transform.SetParent(parent, false);

        var rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        var label = labelObject.GetComponent<Text>();
        label.text = text;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.fontSize = fontSize;
        label.alignment = alignment;
        label.color = Color.white;

        return label;
    }

    private void UpdateSceneStatusLabel(string sceneName)
    {
        if (sceneStatusLabel == null)
        {
            return;
        }

        sceneStatusLabel.text = $"Scene: {sceneName}\n1 Town  2 Field  3 Dungeon\nI Inventory  C Character\nGlobal UI: {(uiAllowedInCurrentScene ? "Enabled" : "Disabled")}";
        sceneStatusLabel.color = uiAllowedInCurrentScene ? new Color(0.7f, 1f, 0.75f, 1f) : new Color(1f, 0.62f, 0.62f, 1f);
    }

    private static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
        DontDestroyOnLoad(eventSystemObject);
    }
}
