using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-900)]
public sealed class TestSceneHotkeys : MonoBehaviour
{
    private static TestSceneHotkeys instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateRuntimeInstance()
    {
        if (instance != null)
        {
            return;
        }

        var hotkeysObject = new GameObject("Test Scene Hotkeys");
        DontDestroyOnLoad(hotkeysObject);
        hotkeysObject.AddComponent<TestSceneHotkeys>();
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
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            LoadScene("Town");
        }
        else if (keyboard.digit2Key.wasPressedThisFrame)
        {
            LoadScene("Field");
        }
        else if (keyboard.digit3Key.wasPressedThisFrame)
        {
            LoadScene("Dungeon");
        }
    }

    private static void LoadScene(string sceneName)
    {
        if (SceneManager.GetActiveScene().name == sceneName)
        {
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
