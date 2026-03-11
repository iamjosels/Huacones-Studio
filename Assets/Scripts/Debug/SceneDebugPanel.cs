using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDebugPanel : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    // Atajos: F1 panel, F2 refrescar, F5 recargar, PgUp/PgDown navegar.
    private static SceneDebugPanel instance;

    private readonly List<SceneEntry> scenes = new List<SceneEntry>();
    private Vector2 scroll;
    private bool isVisible = true;

    private const float PanelWidth = 340f;
    private const float PanelHeight = 520f;
    private const float Margin = 12f;

    private struct SceneEntry
    {
        public string Name;
        public int BuildIndex;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        EnsureInstance();
    }

    public static SceneDebugPanel EnsureInstance()
    {
        if (instance != null)
        {
            return instance;
        }

        GameObject debugObject = new GameObject(nameof(SceneDebugPanel));
        instance = debugObject.AddComponent<SceneDebugPanel>();
        DontDestroyOnLoad(debugObject);
        return instance;
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
        RefreshScenes();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            isVisible = !isVisible;
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            RefreshScenes();
        }

        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            LoadAdjacentScene(+1);
        }

        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            LoadAdjacentScene(-1);
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            SceneTransitionManager.EnsureInstance().LoadSceneSafe(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void RefreshScenes()
    {
        scenes.Clear();

        int sceneCount = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < sceneCount; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = Path.GetFileNameWithoutExtension(path);
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            scenes.Add(new SceneEntry
            {
                Name = name,
                BuildIndex = i
            });
        }
    }

    private void LoadAdjacentScene(int step)
    {
        int current = SceneManager.GetActiveScene().buildIndex;
        if (current < 0 || scenes.Count == 0)
        {
            return;
        }

        int target = Mathf.Clamp(current + step, 0, scenes.Count - 1);
        if (target == current)
        {
            return;
        }

        LoadByBuildIndex(target);
    }

    private void LoadByBuildIndex(int buildIndex)
    {
        foreach (SceneEntry entry in scenes)
        {
            if (entry.BuildIndex != buildIndex)
            {
                continue;
            }

            if (entry.Name == "CutsceneViewer")
            {
                CutsceneLoader.EnsureDebugFallback("MainMenu");
            }

            SceneTransitionManager.EnsureInstance().LoadSceneSafe(entry.Name);
            return;
        }
    }

    private void OnGUI()
    {
        if (!isVisible)
        {
            return;
        }

        float panelX = Screen.width - PanelWidth - Margin;
        float panelY = Margin;

        GUILayout.BeginArea(new Rect(panelX, panelY, PanelWidth, PanelHeight), GUI.skin.box);
        GUILayout.Label("Scene Debug Panel");

        Scene active = SceneManager.GetActiveScene();
        GUILayout.Label($"Actual: {active.name} ({active.buildIndex})");
        GUILayout.Label("F1: mostrar/ocultar");
        GUILayout.Label("F2: refrescar lista");
        GUILayout.Label("F5: recargar escena");
        GUILayout.Label("PgUp/PgDown: siguiente/anterior");

        GUILayout.Space(6f);
        scroll = GUILayout.BeginScrollView(scroll);
        foreach (SceneEntry entry in scenes)
        {
            bool isCurrent = entry.BuildIndex == active.buildIndex;
            string label = isCurrent
                ? $"[{entry.BuildIndex}] {entry.Name} (actual)"
                : $"[{entry.BuildIndex}] {entry.Name}";

            if (GUILayout.Button(label, GUILayout.Height(28f)))
            {
                if (entry.Name == "CutsceneViewer")
                {
                    CutsceneLoader.EnsureDebugFallback("MainMenu");
                }

                SceneTransitionManager.EnsureInstance().LoadSceneSafe(entry.Name);
            }
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
#endif
}
