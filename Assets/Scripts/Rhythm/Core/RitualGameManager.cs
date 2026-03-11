using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RitualGameManager : MonoBehaviour
{
    public static RitualGameManager Instance;

    public ProgressBarController progressBar;
    public AudioSource musicSource;

    [Header("Cutscene de transición")]
    public CutsceneData ritmoToRoomCutscene;

    private bool hasCheckedResult = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        WarmUpCutsceneAssets(ritmoToRoomCutscene);
    }

    void Update()
    {
        if (!hasCheckedResult && musicSource != null && !musicSource.isPlaying && musicSource.time > 0.1f)
        {
            hasCheckedResult = true;
            CheckResult();
        }
    }

    public void RegisterHit(string accuracy)
    {
        Debug.Log("Hit: " + accuracy);
        progressBar?.AddProgress(accuracy);
    }

    private void CheckResult()
    {
        float progress = progressBar.progressValue;

        if (progress >= progressBar.maxProgress * 0.5f)
        {
            Debug.Log("✅ ¡Ronda superada!");
            LoadNextRound();
        }
        else
        {
            Debug.Log("❌ No se logró el mínimo, reiniciando...");
            ReloadLevel();
        }
    }

    private void LoadNextRound()
    {
        // Mostrar cutscene antes de la escena "Room"
        CutsceneLoader.cutsceneToLoad = ritmoToRoomCutscene;
        SceneTransitionManager.EnsureInstance().LoadSceneSafe("CutsceneViewer");
    }

    private void ReloadLevel()
    {
        SceneTransitionManager.EnsureInstance().LoadSceneSafe(SceneManager.GetActiveScene().buildIndex);
    }

    private static void WarmUpCutsceneAssets(CutsceneData data)
    {
        if (data == null || data.images == null)
        {
            return;
        }

        foreach (Sprite sprite in data.images)
        {
            if (sprite == null)
            {
                continue;
            }

            _ = sprite.texture;
        }
    }
}

