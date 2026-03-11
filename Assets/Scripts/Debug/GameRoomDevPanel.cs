using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRoomDevPanel : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private static GameRoomDevPanel instance;

    private bool isVisible = true;
    private int selectedRound = 1;

    private const float PanelWidth = 320f;
    private const float PanelHeight = 330f;
    private const float Margin = 12f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        EnsureInstance();
    }

    public static GameRoomDevPanel EnsureInstance()
    {
        if (instance != null)
        {
            return instance;
        }

        GameObject panelObject = new GameObject(nameof(GameRoomDevPanel));
        instance = panelObject.AddComponent<GameRoomDevPanel>();
        DontDestroyOnLoad(panelObject);
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
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            isVisible = !isVisible;
        }
    }

    private void OnGUI()
    {
        if (!isVisible || !IsGameRoomActive())
        {
            return;
        }

        GameManager manager = Object.FindFirstObjectByType<GameManager>();
        if (manager == null)
        {
            return;
        }

        selectedRound = Mathf.Clamp(selectedRound, 1, Mathf.Max(1, manager.maxRounds));

        float panelX = Margin;
        float panelY = Margin;

        GUILayout.BeginArea(new Rect(panelX, panelY, PanelWidth, PanelHeight), GUI.skin.box);
        GUILayout.Label("GameRoom Dev Panel");
        GUILayout.Label("F3: mostrar/ocultar");
        GUILayout.Space(4f);

        GUILayout.Label($"Ronda actual: {manager.DebugCurrentRound}");
        GUILayout.Label($"Mini actual: {manager.DebugCurrentMiniName}");
        GUILayout.Label($"Corriendo: {(manager.DebugIsSequenceRunning ? "SI" : "NO")}");

        GUILayout.Space(8f);
        GUILayout.Label($"Ronda para test: {selectedRound}");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("-", GUILayout.Width(36f)))
        {
            selectedRound = Mathf.Max(1, selectedRound - 1);
        }

        if (GUILayout.Button("+", GUILayout.Width(36f)))
        {
            selectedRound = Mathf.Min(manager.maxRounds, selectedRound + 1);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(8f);
        if (GUILayout.Button("Iniciar run completo", GUILayout.Height(28f)))
        {
            manager.StartFullRun();
        }

        if (GUILayout.Button("Forzar WIN actual", GUILayout.Height(28f)))
        {
            manager.DebugForceCurrentWin();
        }

        if (GUILayout.Button("Forzar LOSS actual", GUILayout.Height(28f)))
        {
            manager.DebugForceCurrentLoss();
        }

        GUILayout.Space(8f);
        if (GUILayout.Button("Test Simon", GUILayout.Height(28f)))
        {
            manager.DebugPlaySingleMini(GameManager.MiniType.Simon, selectedRound);
        }

        if (GUILayout.Button("Test Orden", GUILayout.Height(28f)))
        {
            manager.DebugPlaySingleMini(GameManager.MiniType.Order, selectedRound);
        }

        if (GUILayout.Button("Test Dalgona", GUILayout.Height(28f)))
        {
            manager.DebugPlaySingleMini(GameManager.MiniType.Dalgona, selectedRound);
        }

        GUILayout.EndArea();
    }

    private static bool IsGameRoomActive()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        return activeScene.IsValid() && activeScene.name == "GameRoomScene";
    }
#endif
}
